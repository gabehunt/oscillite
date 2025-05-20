using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oscillite.CleanRoom.VSM;
using System.Security.Policy;
using System.Xml.Serialization;
using System.Reflection;

namespace Oscillite.Utilities
{
    public static class WaveformFileLoader
    {
        public static WaveformFileResult Load(string filePath, int defaultChannelCount = 4, float totalDurationSeconds = 10f)
        {
            if (filePath.EndsWith(".ocsv", StringComparison.OrdinalIgnoreCase))
            {
                return LoadCsv(filePath, defaultChannelCount, totalDurationSeconds);
            }
            else if (filePath.EndsWith(".lsm", StringComparison.OrdinalIgnoreCase))
            {
                return LoadLSM(filePath, defaultChannelCount);
            }
            else if (filePath.EndsWith(".vsm", StringComparison.OrdinalIgnoreCase))
            {
                return LoadVSM(filePath, defaultChannelCount);
            }
            else if (filePath.EndsWith(".tdms", StringComparison.OrdinalIgnoreCase))
            {
                return LoadTDMS(filePath, 8);
            }
            else
            {
                return new WaveformFileResult();
            }
        }

        private static WaveformFileResult LoadTDMS(string filePath, int channelCount)
        {
            var file = new NationalInstruments.Tdms.File(filePath);
            file.Open();

            var channels = new List<WaveformChannelData>();
            float totalDurationSeconds = 0;
            foreach (var group in file.Groups)
            {
                int i = 0;
                foreach (var channel in group.Value.Channels)
                {
                    var props = channel.Value.Properties;
                    float wfIncrement = props.ContainsKey("wf_increment") ? Convert.ToSingle(props["wf_increment"]) : 0f;
                    float wfSamples = props.ContainsKey("wf_samples") ? Convert.ToSingle(props["wf_samples"]) : 0f;

                    foreach (var prop in channel.Value.Properties)
                    {
                        Console.WriteLine($"{group.Key}\t{channel.Value.Name}\t{prop.Key} = {prop.Value}");
                    }
                    //wf_increment
                    //wf_samples

                    Vector2[] data = null;
                    List<float> voltages = null;

                    if (channel.Value.HasData)
                    {
                        var dataType = channel.Value.DataType;
                        Console.WriteLine(dataType.ToString());
                        voltages = channel.Value.GetData<float>().ToList();

                        if (voltages.Count < 2)
                            throw new Exception("CSV must contain at least 2 voltage values.");

                        totalDurationSeconds = (wfSamples * wfIncrement);
                        int totalSamples = voltages.Count;
                        float timeStep = totalDurationSeconds / (totalSamples - 1);

                        data = new Vector2[totalSamples];
                        for (int j = 0; j < totalSamples; j++)
                            data[j] = new Vector2(j * timeStep, voltages[j]);
                    }
                    else
                    {
                        data = new Vector2[0];
                        totalDurationSeconds = 10;
                        voltages = new List<float>() { 2f };
                    }

                    var absVoltages = voltages.Select(v => Math.Abs(v)).OrderBy(v => v).ToArray();
                    var index = (int)(absVoltages.Length * 0.9990); // 99.99% point
                    var reasonableMax = absVoltages[Math.Min(index, absVoltages.Length - 1)];
                    var roundedMax = RoundUpToFixedVoltages(reasonableMax);

                    channels.Add(new WaveformChannelData
                    {
                        ChannelIndex = i,
                        Visible = data.Length > 0,
                        Data = data,
                        FullScale = 2 * roundedMax
                    });

                    i += 1;
                }
            }

            return new WaveformFileResult
            {
                Channels = channels,
                Duration = totalDurationSeconds,
                FileExtension = "tdms"
            };
        }

        private static WaveformFileResult LoadCsv(string filePath, int channelCount, float totalDurationSeconds)
        {
            var voltages = File.ReadAllLines(filePath)
                .Select(line => float.TryParse(line, out float v) ? v : (float?)null)
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .ToList();

            if (voltages.Count < 2)
                throw new Exception("CSV must contain at least 2 voltage values.");

            int totalSamples = voltages.Count;
            float timeStep = totalDurationSeconds / (totalSamples - 1);

            var data = new Vector2[totalSamples];
            for (int i = 0; i < totalSamples; i++)
                data[i] = new Vector2(i * timeStep, voltages[i]);

            var channels = new List<WaveformChannelData>();
            for (int i = 0; i < channelCount; i++)
            {
                channels.Add(new WaveformChannelData
                {
                    ChannelIndex = i,
                    Visible = (i == 0),
                    Data = i == 0 ? data : Array.Empty<Vector2>(),
                    FullScale = i == 0 ? 2 * voltages.Max(v => Math.Abs(v)) : 20.0f
                });
            }

            return new WaveformFileResult
            {
                Channels = channels,
                Duration = totalDurationSeconds,
                FileExtension = "ocsv"
            };
        }

        private static WaveformFileResult LoadLSM(string filePath, int channelCount)
        {

            CleanRoom.LSM.ConfigurationSettingsLoader configurationSettingsLoader = new CleanRoom.LSM.ConfigurationSettingsLoader();
            configurationSettingsLoader.LoadFromFile(filePath);
            var cfg = configurationSettingsLoader.ConfigurationSettings;
            var data = cfg.ScopeSettings;
            double secondsPerFrame = data.Sweep.Seconds;

            var tracesById = data.TraceList
                .ToDictionary(trace => trace.Id);

            var waveformDataByTraceId = cfg.WaveformData
                .GroupBy(wf => wf.TraceId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var channels = Enumerable.Range(0, channelCount)
                .Select(i => new WaveformChannelData { ChannelIndex = i, Visible = false })
                .ToDictionary(c => c.ChannelIndex);

            for (int index = 0; index < tracesById.Count; index++)
            {
                var trace = tracesById[index + 1];

                if (index >= 0 && index < channelCount)
                {
                    channels[index].MaxExpectedVoltage = (float)trace.Scale.FullScaleValue;
                    float fullScale = channels[index].MaxExpectedVoltage * 2.0f;
                    channels[index].Visible = trace.Enabled;
                    channels[index].FullScale = fullScale;
                    channels[index].UnitString = trace.Probe.SelectedScale.Unit.ToString();
                }
            }

            double? duration = null;
            foreach (var kvp in waveformDataByTraceId)
            {
                int channelIindex = kvp.Key - 1;
                var allPoints = kvp.Value.SelectMany(wf => wf.Points).ToArray();
                int totalPoints = allPoints.Length;
                if (!duration.HasValue && totalPoints > 0)
                {
                    duration = (totalPoints * data.Sweep.Seconds) / 1000;
                }

                var vectorPoints = allPoints.Select((v, i) =>
                    new Vector2((float)(i * duration / (totalPoints - 1)), Sanitize((float)v, channels[channelIindex].MaxExpectedVoltage, (float)data.TraceList[channelIindex].Scale.ProbeGain))).ToArray();
                channels[channelIindex].Data = vectorPoints;
            }

            return new WaveformFileResult
            {
                Channels = channels.Values.ToList(),
                Duration = (float)duration,
                FileExtension = "vsm"
            };
        }

        private static WaveformFileResult LoadVSM(string filePath, int channelCount)
        {

            ConfigurationSettings scopedata = ConfigurationSettingsLoader.LoadFromFile(filePath);

            var data = scopedata.ScopeSettings;
            double secondsPerFrame = data.SelectedSweep.Sec;

            var tracesById = scopedata.ScopeSettings.TraceList.Trace
                .ToDictionary(trace => trace.Id.Value);

            var waveformDataByTraceId = scopedata.WaveformDataList.WaveformData
                .GroupBy(wf => wf.TId.Value)
                .ToDictionary(group => group.Key, group => group.ToList());

            var channels = Enumerable.Range(0, channelCount)
                .Select(i => new WaveformChannelData { ChannelIndex = i, Visible = false })
                .ToDictionary(c => c.ChannelIndex);

            for (int index = 0; index < tracesById.Count; index++)
            {
                var trace = tracesById[index+1];

                if (index >= 0 && index < channelCount)
                {
                    channels[index].MaxExpectedVoltage = (float)trace.Scale.FullScaleValue;
                    float fullScale = channels[index].MaxExpectedVoltage * 2.0f;
                    channels[index].Visible = trace.Enabled.Value;
                    channels[index].FullScale = fullScale;
                    channels[index].UnitString = trace.Probe.SelectedScale.Unit;
                }
            }

            double? duration = null;
            foreach (var kvp in waveformDataByTraceId)
            {
                int channelIindex = kvp.Key - 1;
                var allPoints = kvp.Value.SelectMany(wf => wf.PointsList.Points).ToArray();
                int totalPoints = allPoints.Length;
                if (!duration.HasValue && totalPoints > 0)
                {
                    duration = (totalPoints * data.SelectedSweep.Sec) / 1000;
                }

                var vectorPoints = allPoints.Select((v, i) =>
                    new Vector2((float)(i * duration / (totalPoints - 1)), Sanitize((float)v, channels[channelIindex].MaxExpectedVoltage, (float)data.TraceList.Trace[channelIindex].Scale.ProbeGain))).ToArray();
                channels[channelIindex].Data = vectorPoints;
            }

            return new WaveformFileResult
            {
                Channels = channels.Values.ToList(),
                Duration = (float)duration,
                FileExtension = "vsm"
            };
        }

        private static float Sanitize(float v, float maxAbs, float gain = 1.0f)
        {
            if (float.IsNaN(v)) return 0f;
            if (float.IsNegativeInfinity(v)) return -maxAbs;
            if (float.IsPositiveInfinity(v)) return maxAbs;
            return Math.Max(-maxAbs, Math.Min(maxAbs, v * gain));
        }

        private static float RoundUpToFixedVoltages(float value)
        {
            float[] voltages = { 1f, 5f, 10f, 20f, 50f, 100f, 200f };

            foreach (var v in voltages)
            {
                if (value <= v)
                    return v;
            }

            // If value is somehow bigger than the largest voltage, you can handle it like this:
            return voltages.Last(); // or throw, or add more ranges if needed
        }
    }

    public class WaveformFileResult
    {
        public List<WaveformChannelData> Channels { get; set; }
        public float Duration { get; set; }
        public string FileExtension { get; set; }
    }

    public class WaveformChannelData
    {
        public int ChannelIndex { get; set; }
        public bool Visible { get; set; }
        public Vector2[] Data { get; set; } = Array.Empty<Vector2>();
        public float FullScale { get; set; } = 1.0f;
        public float MaxExpectedVoltage { get; set; }
        public string UnitString { get; internal set; }
    }
}
