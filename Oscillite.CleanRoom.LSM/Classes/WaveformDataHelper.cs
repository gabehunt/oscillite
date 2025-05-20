using System;
using System.IO;

namespace Oscillite.CleanRoom.LSM
{
    public class WaveformDataHelper
    {
        private readonly WaveformHeader[] waveformHeader;
        private readonly ConfigurationSettingsLoader lsmFileReader;

        public WaveformDataHelper(ConfigurationSettingsLoader lsmFR)
        {
            waveformHeader = new WaveformHeader[Constants.NUMBER_OF_CHANNELS];
            lsmFileReader = lsmFR;
        }

        public void ReadFrameFromVersion(BinaryReader binaryReader, FileVersion? fileVersion, string fileExt)
        {
            short[][] channelData = new short[Constants.NUMBER_OF_CHANNELS][];
            int numberOfPoints = 0;
            var bufferList = new WaveformBufferList();

            if (fileVersion <= FileVersion.V1_2)
            {
                // Read header fields
                if (fileVersion == FileVersion.V1_0)
                {
                    for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                    {
                        waveformHeader[channelIndex].calibrationMultiplier = binaryReader.ReadInt32();
                        waveformHeader[channelIndex].calibrationDivisor = binaryReader.ReadInt32();
                        waveformHeader[channelIndex].calibrationOffset = binaryReader.ReadInt32();
                        waveformHeader[channelIndex].numberOfPoints = binaryReader.ReadInt32();
                    }
                }
                else
                {
                    for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                    {
                        waveformHeader[channelIndex].averageValue = binaryReader.ReadDouble();
                        waveformHeader[channelIndex].minValue = binaryReader.ReadDouble();
                        waveformHeader[channelIndex].maxValue = binaryReader.ReadDouble();
                    }
                    for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                    {
                        waveformHeader[channelIndex].calibrationMultiplier = binaryReader.ReadInt32();
                        waveformHeader[channelIndex].calibrationDivisor = binaryReader.ReadInt32();
                        waveformHeader[channelIndex].calibrationOffset = binaryReader.ReadInt32();
                        waveformHeader[channelIndex].numberOfPoints = binaryReader.ReadInt32();
                    }
                }

                // Allocate and fill channel arrays
                for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                {
                    if (waveformHeader[channelIndex].numberOfPoints > 0)
                    {
                        numberOfPoints = waveformHeader[channelIndex].numberOfPoints;
                        channelData[channelIndex] = new short[numberOfPoints];
                        FillWithMaxValue(channelData[channelIndex]);
                    }
                }

                if (numberOfPoints == 0)
                {
                    // No data to read, exit early
                    lsmFileReader.ConfigurationSettings.WaveformData = bufferList;
                    lsmFileReader.ConfigurationSettings.FrameSize = new Position<int>(lsmFileReader.SweepWidth);
                    return;
                }

                short defaultSweepWidth = GetDefaultSweepWidth(fileVersion);
                short sweepWidth = SweepWidthHelper.GetSweepWidth(lsmFileReader.ConfigurationSettings.ScopeSettings.Sweep);

                if (defaultSweepWidth == sweepWidth)
                {
                    // Direct read, interleaved channels
                    for (int i = 0; i < numberOfPoints; i++)
                    {
                        for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                        {
                            if (channelData[channelIndex] != null)
                            {
                                channelData[channelIndex][i] = binaryReader.ReadInt16();
                            }
                            else
                            {
                                binaryReader.BaseStream.Position += Constants.ENCODING_NULL_BYTE_COUNT;
                            }
                        }
                    }
                }
                else
                {
                    // Resample logic for legacy files
                    int tempSweepWidth = Constants.DEFAULT_SWEEP_WIDTH;
                    short[][] dataPointA = new short[tempSweepWidth][];
                    short[][] dataPointB = new short[tempSweepWidth][];

                    for (int i = 0; i < tempSweepWidth; i++)
                    {
                        dataPointA[i] = new short[Constants.NUMBER_OF_CHANNELS];
                        dataPointB[i] = new short[Constants.NUMBER_OF_CHANNELS];
                    }

                    bool moreBlocks = true;
                    int resampleBlockCount = 0;
                    int resampleOffset = 0;

                    while (moreBlocks)
                    {
                        resampleBlockCount++;
                        for (int i = 0; i < tempSweepWidth; i++)
                        {
                            for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                            {
                                dataPointA[i][channelIndex] = short.MaxValue;
                                dataPointB[i][channelIndex] = short.MaxValue;
                            }
                        }
                        for (int i = 0; i < tempSweepWidth; i++)
                        {
                            if (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                            {
                                for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                                {
                                    dataPointA[i][channelIndex] = binaryReader.ReadInt16();
                                }
                            }
                            else
                            {
                                moreBlocks = false;
                                break;
                            }
                        }
                        ResampleWaveformData(dataPointA, defaultSweepWidth, dataPointB, sweepWidth);
                        ResampleWaveformData(dataPointB, sweepWidth, dataPointA, defaultSweepWidth);

                        for (int i = 0; i < tempSweepWidth; i++)
                        {
                            for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                            {
                                int outputIndex = i + resampleOffset;
                                if (channelData[channelIndex] != null && outputIndex < channelData[channelIndex].Length)
                                {
                                    channelData[channelIndex][outputIndex] = dataPointA[i][channelIndex];
                                }
                            }
                        }
                        resampleOffset = tempSweepWidth * resampleBlockCount;
                    }
                }
            }
            else // Modern file versions
            {
                var byteData = new byte[Constants.NUMBER_OF_CHANNELS][];
                for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                {
                    waveformHeader[channelIndex].calibrationMultiplier = binaryReader.ReadInt32();
                    waveformHeader[channelIndex].calibrationDivisor = binaryReader.ReadInt32();
                    waveformHeader[channelIndex].calibrationOffset = binaryReader.ReadInt32();
                    waveformHeader[channelIndex].numberOfPoints = binaryReader.ReadInt32();
                    waveformHeader[channelIndex].pointSize = binaryReader.ReadInt16();
                    waveformHeader[channelIndex].averageValue = binaryReader.ReadDouble();
                    waveformHeader[channelIndex].minValue = binaryReader.ReadDouble();
                    waveformHeader[channelIndex].maxValue = binaryReader.ReadDouble();
                    binaryReader.BaseStream.Position += Constants.ENCODING_NULL_BYTE_COUNT;
                    short decimationFactor = binaryReader.ReadInt16();
                    if (decimationFactor == 0) decimationFactor = 1;

                    switch (decimationFactor)
                    {
                        case 1:
                        case 3:
                        case 6:
                        case 10:
                        case 15:
                            lsmFileReader.Decimation = decimationFactor;
                            if (waveformHeader[channelIndex].numberOfPoints != lsmFileReader.BufferPointCount)
                            {
                                waveformHeader[channelIndex].numberOfPoints = 0;
                            }
                            if (waveformHeader[channelIndex].numberOfPoints > 0 && waveformHeader[channelIndex].pointSize > 0)
                            {
                                numberOfPoints = waveformHeader[channelIndex].numberOfPoints * waveformHeader[channelIndex].pointSize;
                                byteData[channelIndex] = binaryReader.ReadBytes(numberOfPoints);
                                channelData[channelIndex] = new short[lsmFileReader.BufferPointCount];
                                FillWithMaxValue(channelData[channelIndex]);
                                for (int i = 0; i < byteData[channelIndex].Length; i += Constants.ENCODING_NULL_BYTE_COUNT)
                                {
                                    int outputIndex = i / 2;
                                    if (outputIndex < channelData[channelIndex].Length)
                                    {
                                        channelData[channelIndex][outputIndex] = BitConverter.ToInt16(byteData[channelIndex], i);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            // Convert raw data to calibrated waveform buffers
            for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
            {
                if (channelData[channelIndex] == null) continue;

                TraceProbeType probeType = TraceProbeType.TRACE_PROBE_VOLTS;
                if (lsmFileReader.ConfigurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_GraphingMeter)
                {
                    probeType = lsmFileReader.ConfigurationSettings.ScopeSettings.TraceList[channelIndex].Probe.Type;
                }

                switch (probeType)
                {
                    case TraceProbeType.TRACE_PROBE_FREQUENCY:
                        GetFrequenciesFromRawData(channelIndex, channelData[channelIndex], bufferList, fileExt);
                        break;
                    case TraceProbeType.TRACE_PROBE_INJECTOR_PULSE_WIDTH:
                    case TraceProbeType.TRACE_PROBE_PULSE_WIDTH:
                        GetCalibratedValuesFromRawData(channelIndex, channelData[channelIndex], bufferList, fileExt, Constants.MILLION_DIVISOR);
                        break;
                    case TraceProbeType.TRACE_PROBE_DUTY_CYCLE:
                    case TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_60:
                    case TraceProbeType.TRACE_PROBE_MIXTURE_CONTROL_DWELL_90:
                        GetCalibratedValuesFromRawData(channelIndex, channelData[channelIndex], bufferList, fileExt, 1);
                        break;
                    default:
                        GetDataValuesFromRaw(channelIndex, channelData[channelIndex], bufferList, fileExt);
                        break;
                }
            }

            lsmFileReader.ConfigurationSettings.WaveformData = bufferList;
            lsmFileReader.ConfigurationSettings.FrameSize = new Position<int>(lsmFileReader.SweepWidth);
        }

        private short GetDefaultSweepWidth(FileVersion? fileVersion)
        {
            if (fileVersion == FileVersion.V1_0)
                return Constants.EXTENDED_SWEEP_WIDTH;
            if (fileVersion == FileVersion.V1_1)
                return Constants.DEFAULT_SWEEP_WIDTH;
            return (short)lsmFileReader.SweepWidth;
        }

        private void FillWithMaxValue(short[] target)
        {
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = short.MaxValue;
            }
        }

        public void ResampleWaveformData(short[][] fromPoint, short fromLength, short[][] toPoint, short toLength)
        {
            int shortOverflow = short.MaxValue + 1;
            int sampleRate = fromLength / toLength;
            int remainderSpread = (int)((long)(fromLength % toLength) * shortOverflow / toLength);
            long fractionalErrorAccumulator = 0L;
            int fromIndex = 0;
            for (int toIndex = 0; toIndex < toLength; toIndex++)
            {
                for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
                {
                    toPoint[toIndex][channelIndex] = fromPoint[fromIndex][channelIndex];
                }
                fractionalErrorAccumulator += remainderSpread;
                if (fractionalErrorAccumulator >= shortOverflow)
                {
                    fractionalErrorAccumulator -= shortOverflow;
                    fromIndex++;
                }
                fromIndex += sampleRate;
            }
        }

        public void GetDataValuesFromRaw(int index, short[] bufferPoints, WaveformBufferList bufferList, string fileExt)
        {
            WaveformBuffer waveformBuffer = new WaveformBuffer
            {
                TraceId = index + 1
            };

            var header = waveformHeader[index];
            bool isSFrame = ScopeFileExtensionMap.SFrameFileExtensions.Contains(fileExt.ToLower());

            if (isSFrame)
            {
                waveformBuffer.Points = new float[lsmFileReader.SweepWidth];
                for (int i = 0; i < lsmFileReader.SweepWidth; i++)
                {
                    int rawIndex = lsmFileReader.BufferPointIndex + i;
                    waveformBuffer.Points[i] = (rawIndex >= bufferPoints.Length || bufferPoints[rawIndex] == short.MaxValue)
                        ? float.NaN
                        : GetDataValueFromRaw(bufferPoints[rawIndex], header);
                }
            }
            else
            {
                waveformBuffer.Points = new float[bufferPoints.Length];
                for (int i = 0; i < bufferPoints.Length; i++)
                {
                    waveformBuffer.Points[i] = (bufferPoints[i] == short.MaxValue)
                        ? float.NaN
                        : GetDataValueFromRaw(bufferPoints[i], header);
                }
            }

            bufferList.Add(waveformBuffer);
        }

        private float GetDataValueFromRaw(short rawValue, WaveformHeader header)
        {
            return ((rawValue - header.calibrationOffset) * header.calibrationMultiplier) / (float)header.calibrationDivisor;
        }

        public void GetFrequenciesFromRawData(int index, short[] bufferPoints, WaveformBufferList bufferList, string fileExt)
        {
            WaveformBuffer waveformBuffer = new WaveformBuffer
            {
                TraceId = index + 1
            };

            var header = waveformHeader[index];
            bool isSFrame = ScopeFileExtensionMap.SFrameFileExtensions.Contains(fileExt.ToLower());

            if (isSFrame)
            {
                waveformBuffer.Points = new float[lsmFileReader.SweepWidth];
                for (int i = 0; i < lsmFileReader.SweepWidth; i++)
                {
                    int rawIndex = lsmFileReader.BufferPointIndex + i;
                    waveformBuffer.Points[i] = (rawIndex >= bufferPoints.Length || bufferPoints[rawIndex] == short.MaxValue)
                        ? float.NaN
                        : FrequencyFromRaw(bufferPoints[rawIndex], header);
                }
            }
            else
            {
                waveformBuffer.Points = new float[bufferPoints.Length];
                for (int i = 0; i < bufferPoints.Length; i++)
                {
                    waveformBuffer.Points[i] = (bufferPoints[i] == short.MaxValue)
                        ? float.NaN
                        : FrequencyFromRaw(bufferPoints[i], header);
                }
            }

            bufferList.Add(waveformBuffer);
        }

        private float FrequencyFromRaw(short rawValue, WaveformHeader header)
        {
            float calibrated = ((rawValue - header.calibrationOffset) * header.calibrationMultiplier) / (float)header.calibrationDivisor;
            float microseconds = calibrated / Constants.MILLION_DIVISOR;
            return 1f / microseconds;
        }

        public void GetCalibratedValuesFromRawData(int index, short[] bufferPoints, WaveformBufferList bufferList, string fileExt, int additionalDivisor)
        {
            WaveformBuffer waveformBuffer = new WaveformBuffer
            {
                TraceId = index + 1
            };

            var header = waveformHeader[index];
            bool isSFrame = ScopeFileExtensionMap.SFrameFileExtensions.Contains(fileExt.ToLower());

            if (isSFrame)
            {
                waveformBuffer.Points = new float[lsmFileReader.SweepWidth];
                for (int i = 0; i < lsmFileReader.SweepWidth; i++)
                {
                    int rawIndex = lsmFileReader.BufferPointIndex + i;
                    waveformBuffer.Points[i] = (rawIndex >= bufferPoints.Length || bufferPoints[rawIndex] == short.MaxValue)
                        ? float.NaN
                        : GetCalibratedValue(bufferPoints[rawIndex], header, additionalDivisor);
                }
            }
            else
            {
                waveformBuffer.Points = new float[bufferPoints.Length];
                for (int i = 0; i < bufferPoints.Length; i++)
                {
                    waveformBuffer.Points[i] = (bufferPoints[i] == short.MaxValue)
                        ? float.NaN
                        : GetCalibratedValue(bufferPoints[i], header, additionalDivisor);
                }
            }

            bufferList.Add(waveformBuffer);
        }

        private float GetCalibratedValue(short raw, WaveformHeader header, int additionalDivisor)
        {
            float calibrated = ((raw - header.calibrationOffset) * header.calibrationMultiplier) / (float)header.calibrationDivisor;
            return calibrated / additionalDivisor;
        }
    }
}
