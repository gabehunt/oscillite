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
            else if (filePath.EndsWith(".vsm", StringComparison.OrdinalIgnoreCase))
            {
                return LoadVSM(filePath, defaultChannelCount);
            }
            else
            {
                return new WaveformFileResult();
            }
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
                Duration = totalDurationSeconds
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

            //int frameSize = scopedata.FrameSize?.V ?? 0;
            //int bufferSize = scopedata.WaveformDataList.WaveformData.FirstOrDefault().BufferSize?.V ?? 0;
            //int totalFrames = (int)((bufferSize - frameSize) / frameSize) + 1;
            //float duration = (float)(totalFrames * secondsPerFrame);

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

            //var waveformsByTrace = new Dictionary<int, List<Waveform>>();

            //for (uint i = 0; i <= bufferSize - frameSize; i += frameSize)
            //{
            //    scopedata.BufferPosition = new BufferPosition(i);
            //    var waveforms = scopedata.GetWaveforms(GetWaveformReadType.GetWaveformReadType_Default);
            //    for (int wfi = 0; wfi < waveforms.Count; wfi++)
            //    {
            //        var wf = waveforms[wfi];
            //        //int traceId = wf.TraceId?.Value ?? 0;
            //        int channelIndex = wfi;
            //        if (!channels.ContainsKey(channelIndex)) continue;

            //        float maxExpected = channels[channelIndex].FullScale / 2;
            //        for (int j = 0; j < wf.Points.Length; j++)
            //            wf.Points[j] = Sanitize(wf.Points[j], maxExpected, data.Traces[wfi].Scale.ProbeGain);

            //        if (!waveformsByTrace.ContainsKey(channelIndex))
            //            waveformsByTrace[channelIndex] = new List<Waveform>();

            //        waveformsByTrace[channelIndex].Add(wf);
            //    }
            //}
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
                //        for (int j = 0; j < wf.Points.Length; j++)
                //            wf.Points[j] = Sanitize(wf.Points[j], maxExpected, data.Traces[wfi].Scale.ProbeGain);
                channels[channelIindex].Data = vectorPoints;
            }

            return new WaveformFileResult
            {
                Channels = channels.Values.ToList(),
                Duration = (float)duration
            };
        }

        private static float Sanitize(float v, float maxAbs, float gain = 1.0f)
        {
            if (float.IsNaN(v)) return 0f;
            if (float.IsNegativeInfinity(v)) return -maxAbs;
            if (float.IsPositiveInfinity(v)) return maxAbs;
            return Math.Max(-maxAbs, Math.Min(maxAbs, v * gain));
        }
    }

    public class WaveformFileResult
    {
        public List<WaveformChannelData> Channels { get; set; }
        public float Duration { get; set; }
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
