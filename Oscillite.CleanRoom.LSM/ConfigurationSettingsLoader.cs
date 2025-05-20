using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Oscillite.CleanRoom.LSM
{
    /// <summary>
    /// Loads and parses configuration settings from LSM files.
    /// Field names with 'm_' are preserved to match the original file format as discovered during clean room binary analysis.
    /// </summary>
    public class ConfigurationSettingsLoader
	{
        private const double TRIGGER_DELAY_DIVISOR = 600.0;

        private Dictionary<int, string> allTraceScales;

		private Dictionary<int, TraceProbeType> allTraceProbeTypes;

		private Dictionary<int, object> allSweepTimes;

		private ConfigurationSettings configurationSettings;

		private ASCIIEncoding asciiEncoding = new ASCIIEncoding();

		private string[] scale;

		private double[] position;

		private float[] unitsGain;

		private VacuumUnits[] vacuumUnits;

		private PressureUnits[] pressureUnits;

		private TemperatureUnits[] temperatureUnits;

		private BinaryReader binaryReader;

        private Stream stream;

        private WaveformDataHelper waveformDataHelper;

        /// <summary>
        /// Gets the parsed configuration settings.
        /// </summary>
        public ConfigurationSettings ConfigurationSettings => configurationSettings;

        /// <summary>
        /// Gets or sets the buffer point index for waveform data.
        /// </summary>
        public int BufferPointIndex { get; set; }

        /// <summary>
        /// Gets or sets the buffer point count for waveform data.
        /// </summary>
        public int BufferPointCount { get; set; }

        /// <summary>
        /// Gets or sets the sweep width for the current configuration.
        /// </summary>
        public int SweepWidth { get; set; }

        /// <summary>
        /// Gets or sets the decimation factor for waveform data.
        /// </summary>
        public int Decimation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsLoader"/> class.
        /// Sets up default mappings and arrays for trace, channel, and unit information.
        /// </summary>
        public ConfigurationSettingsLoader()
		{
            allTraceScales = TraceScaleMap.GetDefaultTraceScales();
            allTraceProbeTypes = TraceProbeTypeMap.GetDefaultTraceProbes();
            allSweepTimes = SweepTimeMap.GetDefaultSweepTimes();
            Decimation = 1;
            waveformDataHelper = new WaveformDataHelper(this);
            position = new double[Constants.NUMBER_OF_CHANNELS];
            pressureUnits = new PressureUnits[Constants.NUMBER_OF_CHANNELS];
            scale = new string[Constants.NUMBER_OF_CHANNELS];
            temperatureUnits = new TemperatureUnits[Constants.NUMBER_OF_CHANNELS];
            unitsGain = new float[Constants.NUMBER_OF_CHANNELS];
			vacuumUnits = new VacuumUnits[Constants.NUMBER_OF_CHANNELS];
        }

        /// <summary>
        /// Loads configuration settings from the specified LSM file.
        /// </summary>
        /// <param name="fileName">The path to the LSM file.</param>
        public void LoadFromFile(string fileName)
		{
			if ((stream = File.Open(fileName, FileMode.Open)) != null)
			{
				GetConfigurationSettings(fileName);
				stream.Close();
			}
		}

        /// <summary>
        /// Parses the configuration settings from the open file stream.
        /// Handles header, version, and extension-specific logic.
        /// </summary>
        /// <param name="fileName">The file name (used for extension checks).</param>
        protected void GetConfigurationSettings(string fileName)
        {
            configurationSettings = new ConfigurationSettings();
            binaryReader = new BinaryReader(stream);

            // Try to parse header
            try
            {
                GetLsmHeaderByMagic();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCScopeModuleValueFromFile Error: {ex}");
                configurationSettings.ScopeSettings = null;
                return;
            }

            // Initialize settings by file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            InitializeScopeSettings(extension);

            int fileVersionTemp = GetScopeHeader();
            FileVersion? fileVersion = FileVersionMap.GetFileVersion(fileVersionTemp);
            bool isKnown = FileVersionMap.IsKnownFileVersion(fileVersionTemp);

            if (!isKnown)
            {
                throw new Exception($"Unsupported file version: {fileVersionTemp}");
            }

            // Load version-specific settings
            try
            {
                GetSettingsFromFileByVersion(fileVersion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSettingsFromFileByVersion Error: {ex}");
                configurationSettings.ScopeSettings = null;
                return;
            }

            // Only certain extensions support frames, others gave us null reference or index bound exceptions (possibly config only files?)
            var frameExtensions = ScopeFileExtensionMap.FrameFileExtensions;
            if (frameExtensions.Contains(extension))
            {
                try
                {
                    waveformDataHelper.ReadFrameFromVersion(binaryReader, fileVersion, extension);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"LSMFileReader.ReadFrameFromVersion Error: {ex}");
                    configurationSettings.ScopeSettings = null;
                }
            }
        }

        /// <summary>
        /// Initializes scope settings based on the file extension.
        /// </summary>
        /// <param name="fileExt">The file extension (e.g., ".lsm").</param>
        public void InitializeScopeSettings(string fileExt)
        {
            if (string.IsNullOrEmpty(fileExt))
                return;

            string ext = fileExt.ToLowerInvariant();

            if (ScopeFileExtensionMap.IsLabScope(ext))
            {
                configurationSettings.ScopeSettings = new ScopeSettings(
                    ScopeType.ScopeType_LabScope,
                    TraceCount.Four,
                    SweepListMap.GetSweepList(0.00005, 20.0)
                );
            }
            else if (ScopeFileExtensionMap.IsGraphingMeter(ext))
            {
                configurationSettings.ScopeSettings = new ScopeSettings(
                    ScopeType.ScopeType_GraphingMeter,
                    TraceCount.Two,
                    SweepListMap.GetSweepList(1.0, 3600.0)
                );
            }
        }

        /// <summary>
        /// Reads and validates the LSM file header using the known magic value.
        /// Advances the stream to the data section if the header matches.
        /// </summary>
        public void GetLsmHeaderByMagic()
        {
            var headerBytes = new byte[Constants.LSM_MAGIC.Length];
            stream.Read(headerBytes, 0, headerBytes.Length);

            var header = asciiEncoding.GetString(headerBytes).ToUpperInvariant();

            stream.Position = (header == Constants.LSM_MAGIC)
                ? Constants.LSM_HEADER_LENGTH
                : 0L;
        }

        /// <summary>
        /// Reads the required module value from the file using the discovered field names.
        /// </summary>
        public int GetScopeHeader()
        {
            return BinaryFileReadHelper.ReadFieldValue(binaryReader, stream, ScopeModuleRequiredFieldNamesMap.GetFieldNames(), int.Parse);
        }

        /// <summary>
        /// Reads settings from the file based on the discovered file version.
        /// </summary>
        /// <param name="fileVersion">The file version, as determined from the file.</param>
        public void GetSettingsFromFileByVersion(FileVersion? fileVersion)
        {
            if (fileVersion == FileVersion.V1_0)
            {
                int m_BackgroundColor = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_BackgroundColor");
            }

            if (fileVersion >= FileVersion.V1_7)
            {
                int m_DataPlatformType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_DataPlatformType");
            }

            int m_TraceSelection = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TraceSelection");

            ReadTraceInfos(fileVersion);

            if (fileVersion != FileVersion.V1_0)
            {
                ReadMultimeterSettings(fileVersion);
            }

            ReadChannelInfos(fileVersion);

            int m_SweepRate = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_SweepRate");
            if (IsSweepRateInRange(m_SweepRate))
            {
                //We found two object types in here
                var swTime = allSweepTimes[m_SweepRate];
                if (swTime is double d)
                {
                    configurationSettings.ScopeSettings.Sweep = configurationSettings.ScopeSettings.Sweeps[d];
                }
                else if (swTime is SweepType st)
                {
                    configurationSettings.ScopeSettings.Sweep = configurationSettings.ScopeSettings.Sweeps[st];
                }
            }

            int m_TriggerSelection = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerSelection");
            if (configurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_LabScope)
            {
                configurationSettings.ScopeSettings.SelectedTriggers = configurationSettings.ScopeSettings.TriggerList[m_TriggerSelection];
            }
            else
            {
                configurationSettings.ScopeSettings.SelectedTriggers = new Trigger();
            }
            ReadTriggerInfos(fileVersion);

            int m_CursorSelection = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CursorSelection");
            ReadCursorInfos(fileVersion);

            if (fileVersion == FileVersion.V1_0)
            {
                string m_bDigitalReadOutTracesDisplayed = BinaryFileReadHelper.ReadStringValueFromFile(binaryReader, stream, "m_bDigitalReadOutTracesDisplayed");
                string m_bDigitalReadOutRPMDisplayed = BinaryFileReadHelper.ReadStringValueFromFile(binaryReader, stream, "m_bDigitalReadOutRPMDisplayed");
            }

            int m_ViewType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ViewType");
            int m_bGridOn = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_bGridOn");
            string m_TriggerTextDisplay = BinaryFileReadHelper.ReadStringValueFromFile(binaryReader, stream, "m_TriggerTextDisplay");
            int m_ScaleLabelsDisplay = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ScaleLabelsDisplay");
            int m_ScaleDisplayMode = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ScaleDisplayMode");

            int m_BufferPointIndex = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_BufferPointIndex");
            BufferPointIndex = m_BufferPointIndex;

            int m_BufferPointCount = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_BufferPointCount");
            BufferPointCount = m_BufferPointCount;

            if (fileVersion != FileVersion.V1_0)
            {
                int m_FrozenBufferPointIndex = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_FrozenBufferPointIndex");

                int m_SweepWidth = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_SweepWidth");
                SweepWidth = m_SweepWidth;
            }

            if (fileVersion == FileVersion.V1_0)
            {
                SweepWidth = Constants.EXTENDED_SWEEP_WIDTH;
                BufferPointIndex = BufferPointIndex * Constants.DEFAULT_SWEEP_WIDTH / SweepWidth;
                BufferPointCount = BufferPointCount * Constants.DEFAULT_SWEEP_WIDTH / SweepWidth;
            }
            else if (fileVersion == FileVersion.V1_1)
            {
                short sweepWidth = SweepWidthHelper.GetSweepWidth(configurationSettings.ScopeSettings.Sweep);
                BufferPointIndex = BufferPointIndex * sweepWidth / SweepWidth;
                BufferPointCount = BufferPointCount * sweepWidth / SweepWidth;
            }
            configurationSettings.CurrentBufferPosition = new Position<int>(BufferPointIndex);

            if (fileVersion == FileVersion.V1_0)
            {
                return;
            }

            if (IsModernVersion(fileVersion))
			{
				int m_IgnitionType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_IgnitionType");
			}
            
			if (fileVersion == FileVersion.V1_3)
			{
				int m_NumberOfCylinders = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_NumberOfCylinders");
			}

			if (IsModernVersion(fileVersion))
			{
				ReadCylinderSettings();
			}

			if (IsModernVersion(fileVersion))
			{
				int m_EngineStroke = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_EngineStroke");
			}
		}

        private bool IsSweepRateInRange(int rate) => rate >= 0 && rate < allSweepTimes.Count + Constants.DEFAULT_SWEEP_SETTING_COUNT;


        /// <summary>
        /// Reads all trace information blocks from the file for the current version.
        /// Each trace's settings are parsed using <see cref="ReadTraceSettings"/>.
        /// </summary>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void ReadTraceInfos(FileVersion? fileVersion)
		{
			for (int traceIndex = 0; traceIndex < Constants.NUMBER_OF_TRACES; traceIndex++)
			{
                int m_TraceInfos = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TraceInfos[]");

				if (Is_V1_3_Or_Lower(fileVersion))
				{
                    int TraceInfo = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "TraceInfo");
				}

                ReadTraceSettings(traceIndex, fileVersion);
			}
		}

        /// <summary>
        /// Reads all channel information blocks from the file for the current version.
        /// Each channel's settings are parsed using <see cref="ReadChannelSettings"/>.
        /// </summary>
        public void ReadChannelInfos(FileVersion? fileVersion)
		{
			for (int channelIndex = 0; channelIndex < Constants.NUMBER_OF_CHANNELS; channelIndex++)
			{
				int m_ChannelInfos = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ChannelInfos[]");

				if (Is_V1_3_Or_Lower(fileVersion))
				{
					int ChannelInfo = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "ChannelInfo");
				}

				ReadChannelSettings(channelIndex, fileVersion);
			}
		}

        private bool Is_V1_3_Or_Lower(FileVersion? fileVersion)
        {
			return fileVersion <= FileVersion.V1_3;
        }

        /// <summary>
        /// Reads all trigger information blocks from the file for the current version.
        /// Handles both modern and legacy (v1.0) trigger formats as discovered in the file structure.
        /// </summary>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void ReadTriggerInfos(FileVersion? fileVersion)
		{
			for (int triggerIndex = 0; triggerIndex < Constants.NUMBER_OF_TRIGGERS; triggerIndex++)
			{
				int m_TriggerInfos = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerInfos[]");

				if (Is_V1_3_Or_Lower(fileVersion))
				{
					int TriggerInfo = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "TriggerInfo");
				}
				if (fileVersion == FileVersion.V1_0)
				{
					GetTriggerSettingsFromFileVersion_1_0(triggerIndex);
				}
				else
				{
					GetTriggerSettingsFromFile(triggerIndex, fileVersion);
				}
			}
		}

        /// <summary>
        /// Reads all cursor information blocks from the file for the current version.
        /// Throws if the cursor index does not match the expected value in the file.
        /// </summary>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void ReadCursorInfos(FileVersion? fileVersion)
		{
			for (int cursorIndex = 0; cursorIndex < Constants.NUMBER_OF_CURSORS; cursorIndex++)
			{
                int m_CursorInfos = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CursorInfos[]");

				if (fileVersion == FileVersion.V1_0 || fileVersion == FileVersion.V1_1 || fileVersion == FileVersion.V1_2 || fileVersion == FileVersion.V1_3)
				{
					int CursorInfo = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "CursorInfo");
				}

				GetCursorSettingsFromFile(cursorIndex, fileVersion);
			}
		}

        /// <summary>
        /// Reads cylinder-related settings from the file, including firing order and polarity.
        /// This method uses field names as discovered during binary analysis.
        /// </summary>
        public void ReadCylinderSettings()
		{
            // we ended up not supporting these becasue they were not in many files
            int m_FiringOrder = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_FiringOrder");
			int m_NumberOfCylinders = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_NumberOfCylinders");
			int m_CylinderPolarities = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CylinderPolarities");
		}

        /// <summary>
        /// Reads and applies settings for a specific trace, including display, scale, and units.
        /// Handles both modern and legacy file versions as discovered in the file format.
        /// </summary>
        /// <param name="index">The trace index.</param>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void ReadTraceSettings(int index, FileVersion? fileVersion)
        {
			if (fileVersion != FileVersion.V1_0)
			{
                // V1_0 files do not appear to have id which seems suspicious
                int m_id = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_Id");
				configurationSettings.ScopeSettings.TraceList[index].Id = m_id + 1;
			}

            int m_Displayed = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_Displayed");
            configurationSettings.ScopeSettings.TraceList[index].Enabled = Convert.ToBoolean(m_Displayed);

            int m_InputType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_InputType");

			double m_Position = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_Position");
            position[index] = m_Position;

            int m_Scale = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_Scale");
            if (m_Scale >= 0 && m_Scale < allTraceScales.Count)
            {
                scale[index] = (string)allTraceScales[m_Scale];
            }

            int m_Inverted = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_Inverted");
            if (IsLabScopeOrChannelAB(index))
            {
                configurationSettings.ScopeSettings.TraceList[index].Inverted = Convert.ToBoolean(m_Inverted);
            }

			if(fileVersion == FileVersion.V1_0)
			{
                // m_Color only appears to be in V1_0, we saw very few of these
                int m_Color = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_Color");
            }

            int m_LabelDisplayed = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_LabelDisplayed");

            string m_LabelText = BinaryFileReadHelper.ReadStringValueFromFile(binaryReader, stream, "m_LabelText");

            if (fileVersion == FileVersion.V1_0)
            {
                // m_LabelColor only appears to be in V1_0, we saw very few of these
                int m_LabelColor = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_LabelColor");
            }

            int m_SettingsDisplayed = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_SettingsDisplayed");

            double m_PositionIncrement = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_PositionIncrement");

            double m_MinPosition = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_MinPosition");

            double m_MaxPosition = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_MaxPosition");

            double m_UnitsPerVolt = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_UnitsPerVolt");
            SetUnitGain(index, m_UnitsPerVolt);

            int m_Precision = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_Precision");

            string m_UnitsText = BinaryFileReadHelper.ReadStringValueFromFile(binaryReader, stream, "m_UnitsText");
            GetVacPresTempUnit(index, m_UnitsText);

            int m_bInvertedByChangeInProbeType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_bInvertedByChangeInProbeType");

            int m_AutoScale = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_AutoScale");

            if (IsModernVersion(fileVersion))
            {
				// these settings only appeared in V1_3, V1_7, V1_8 (note: we received no v1_4, v1_5, v1_6 sample files though)
                double m_RasterSpacing = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_RasterSpacing");
                int m_CylinderCount = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CylinderCount");
                int m_CylinderDisplayed = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CylinderDisplayed");
            }
        }

        // some settings like inverting don't seem to apply to all channels, example resistance channel which seems to always be >2
		private bool IsLabScopeOrChannelAB(int index)
        {
            return (configurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_LabScope || index < 2);
        }

        /// <summary>
        /// Reads multimeter-related settings from the file for the current version.
        /// These settings are not used in the application logic but must be read to maintain file position.
        /// </summary>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void ReadMultimeterSettings(FileVersion? fileVersion)
		{
            //these Multimeter Settings don't appear to be used, but needed to be read to move to the next items in the file

            if (fileVersion == FileVersion.V1_1 || fileVersion == FileVersion.V1_2 || fileVersion == FileVersion.V1_3)
			{
				// this was an odd one is was in v1_1, 1_2 and 1_3.  Usually 1_3 seemed to be first version to follow later standards.
				int MultimeterInfo = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "MultimeterInfo");
			}

            int m_MultimeterProbeType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_MultimeterProbeType");
            int m_MultimeterCouplingType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_MultimeterCouplingType");
            int m_MultimeterScale = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_MultimeterScale");
            int m_MultimeterEnabled = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_MultimeterEnabled");
            double m_MultimeterLeadResistance = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_MultimeterLeadResistance");
		}

        /// <summary>
        /// Reads and applies settings for a specific channel, including probe type, coupling, and filtering.
        /// Handles both modern and legacy file versions as discovered in the file format.
        /// </summary>
        /// <param name="index">The channel index.</param>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void ReadChannelSettings(int index, FileVersion? fileVersion)
		{
			int m_ChannelId = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ChannelId");
			int m_ChannelProbeType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ChannelProbeType");
			if (IsLabScopeOrChannelAB(index))
			{
				if (m_ChannelProbeType >= 0 && m_ChannelProbeType < allTraceProbeTypes.Count)
				{
					configurationSettings.ScopeSettings.TraceList[index].Probe = configurationSettings.ScopeSettings.Probes[allTraceProbeTypes[m_ChannelProbeType]];
				}
				if (configurationSettings.ScopeSettings.TraceList[index].Probe.IsPressureProbe(configurationSettings.ScopeSettings.TraceList[index].Probe.Type))
				{
					configurationSettings.ScopeSettings.TraceList[index].Probe.SetPressureUnits(pressureUnits[index]);
					configurationSettings.ScopeSettings.PressureUnits = pressureUnits[index];
				}
				else if (configurationSettings.ScopeSettings.TraceList[index].Probe.IsVacuumProbe(configurationSettings.ScopeSettings.TraceList[index].Probe.Type))
				{
					configurationSettings.ScopeSettings.TraceList[index].Probe.SetVacuumUnits(vacuumUnits[index]);
					configurationSettings.ScopeSettings.VacuumUnits = vacuumUnits[index];
				}
				else if (configurationSettings.ScopeSettings.TraceList[index].Probe.IsTemperatureProbe(configurationSettings.ScopeSettings.TraceList[index].Probe.Type))
				{
					configurationSettings.ScopeSettings.TraceList[index].Probe.SetTemperatureUnits(temperatureUnits[index]);
					configurationSettings.ScopeSettings.TemperatureUnits = temperatureUnits[index];
				}

                // TODO: Scale has been a very problematic lookup
                configurationSettings.ScopeSettings.TraceList[index].Scale = configurationSettings.ScopeSettings.TraceList[index].Probe.TraceScaleList[scale[index]];

				configurationSettings.ScopeSettings.TraceList[index].Probe.SelectedScale = configurationSettings.ScopeSettings.TraceList[index].Probe.TraceScaleList[scale[index]];
				configurationSettings.ScopeSettings.TraceList[index].Position.Value = position[index] * unitsGain[index];
			}
			int m_ChannelCouplingType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ChannelCouplingType");
			if (IsLabScopeOrChannelAB(index))
			{
                TraceCouplingType traceCoupling = GetTraceCoupling(m_ChannelCouplingType);
                configurationSettings.ScopeSettings.TraceList[index].Coupling = traceCoupling;
            }

            int m_ChannelScale = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ChannelScale");
			int m_ChannelEnabled = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ChannelEnabled");

			if (fileVersion != FileVersion.V1_0)
			{
                //V1_0 files do not appear to have any of these properties, but we had very limited samples of this version
                int m_PeakDetectState = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_PeakDetectState");
                PeakDetectStatus traceDetect = GetTraceDetect(m_PeakDetectState);
				if (IsLabScopeOrChannelAB(index))
				{
					configurationSettings.ScopeSettings.TraceList[index].PeakDetect = traceDetect;
				}

				int m_ChannelFilter = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ChannelFilter");
				if (IsLabScopeOrChannelAB(index))
				{
                    FilterState traceFilter = GetTraceFilter(m_ChannelFilter);
                    configurationSettings.ScopeSettings.TraceList[index].Filter = traceFilter;
				}

				double m_ThresholdValue = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_ThresholdValue");
				int m_ThresholdSlope = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_ThresholdSlope");
				ThresholdSlope traceThresholdSlope = GetTraceThresholdSlope(m_ThresholdSlope);
				if (IsLabScopeOrChannelAB(index))
				{
					configurationSettings.ScopeSettings.TraceList[index].ThresholdSlope = traceThresholdSlope;
				}
				int m_PulseScale = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_PulseScale");
				int m_bPeakDetectByChangeInProbeType = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_bPeakDetectByChangeInProbeType");
			}

            if (fileVersion == FileVersion.V1_7 || fileVersion == FileVersion.V1_8)
            {
                // ignoring "m_ChannelLeadResistance" only seems to appear in 1.7 and 1.8 possibly?
                double m_ChannelLeadResistance = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_ChannelLeadResistance");
            }
        }

        /// <summary>
        /// Reads and applies trigger settings for a specific trigger index and file version.
        /// Handles both modern and legacy file versions as discovered in the file format.
        /// </summary>
        /// <param name="index">The trigger index.</param>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void GetTriggerSettingsFromFile(int index, FileVersion? fileVersion)
        {
            int mappedIndex = GetTriggerIndex(index);

            // Source
            int m_TriggerSource = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerSource");
            configurationSettings.ScopeSettings.TriggerList[mappedIndex].Source = GetTriggerSource(index, m_TriggerSource);

            // Mode
            int m_TriggerMode = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerMode");
            configurationSettings.ScopeSettings.TriggerList[mappedIndex].Mode = GetTriggerMode(m_TriggerMode);

            // Delay
            double m_TriggerDelay = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerDelay");
            configurationSettings.ScopeSettings.TriggerList[mappedIndex].Delay.Value = m_TriggerDelay;

            // Level
            double m_TriggerLevel = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerLevel");
            configurationSettings.ScopeSettings.TriggerList[mappedIndex].Level.Value = m_TriggerLevel;

            // Adjust level for selected trigger
            if (configurationSettings.ScopeSettings.TriggerList[mappedIndex] == configurationSettings.ScopeSettings.TriggerList.SelectedTrigger)
            {
                // Apply gain to level to match known voltage values
                configurationSettings.ScopeSettings.TriggerList[mappedIndex].Level.Value *= unitsGain[index];
                if (configurationSettings.ScopeSettings.TraceList[index].Inverted)
                {
                    configurationSettings.ScopeSettings.TriggerList[mappedIndex].Level.Value *= -1;
                }
            }

            // Slope
            int m_TriggerSlope = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerSlope");
            configurationSettings.ScopeSettings.TriggerList[mappedIndex].Slope = GetTriggerSlope(m_TriggerSlope);

            // Misc fields (likely padding or extended metadata)
            int m_TriggerDisplayed = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerDisplayed");

            double m_TriggerDelayIncrement = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerDelayIncrement");
            double m_TriggerLevelIncrement = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerLevelIncrement");
            double m_TriggerMinDelay = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMinDelay");
            double m_TriggerMaxDelay = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMaxDelay");
            double m_TriggerMinLevel = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMinLevel");
            double m_TriggerMaxLevel = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMaxLevel");

            int m_TriggerActiveColor = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerActiveColor");

            // Cylinder Number handling
			if (IsModernVersion(fileVersion))
            {
                int m_TriggerCylinder = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerCylinder");

                if (configurationSettings.ScopeSettings.TriggerList[mappedIndex] == configurationSettings.ScopeSettings.TriggerList.SelectedTrigger)
                {
                    configurationSettings.ScopeSettings.TriggerList[mappedIndex].CylinderNumber = m_TriggerCylinder;
                }
            }
       }

        private int GetTriggerIndex(int inputIndex)
        {
            return inputIndex;
        }

        /// <summary>
        /// Reads and applies trigger settings for a specific trigger index for legacy v1.0 files.
        /// Uses field names as discovered during binary analysis.
        /// </summary>
        /// <param name="index">The trigger index.</param>
        public void GetTriggerSettingsFromFileVersion_1_0(int index)
		{
            int m_TriggerSource = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerSource");
            if (configurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_LabScope)
			{
				TriggerSource triggerSource = GetTriggerSource(index, m_TriggerSource);
				configurationSettings.ScopeSettings.TriggerList[index].Source = triggerSource;
			}

            int m_TriggerMode = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerMode");

            double m_TriggerDelay = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerDelay");

            double m_TriggerLevel = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerLevel");
            if (configurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_LabScope)
			{
				configurationSettings.ScopeSettings.TriggerList[index].Level.Value = m_TriggerLevel;
			}

			if (configurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_LabScope)
			{
				if (index < 4 && configurationSettings.ScopeSettings.TriggerList[index] == configurationSettings.ScopeSettings.TriggerList.SelectedTrigger)

                {
                    //apply unit gain to level to match known voltage values
                    configurationSettings.ScopeSettings.TriggerList[index].Level.Value *= unitsGain[index];
					if (configurationSettings.ScopeSettings.TraceList[index].Inverted)
					{
						configurationSettings.ScopeSettings.TriggerList[index].Level.Value = 0.0 - configurationSettings.ScopeSettings.TriggerList[index].Level.Value;
					}
				}
			}

            int m_TriggerSlope = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerSlope");
			if (configurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_LabScope)
			{
                SlopeType triggerSlope = GetTriggerSlope(m_TriggerSlope);
                configurationSettings.ScopeSettings.TriggerList[index].Slope = triggerSlope;
			}

            int m_TriggerDisplayed = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerDisplayed");

            double m_TriggerDelayIncrement = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerDelayIncrement");
            double m_TriggerLevelIncrement = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerLevelIncrement");
            double m_TriggerMinDelay = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMinDelay");
            
			double m_TriggerMaxDelay = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMaxDelay");
			if (configurationSettings.ScopeSettings.ScopeType == ScopeType.ScopeType_LabScope)
			{
                m_TriggerDelay = m_TriggerDelay * (m_TriggerMaxDelay - m_TriggerMinDelay) / TRIGGER_DELAY_DIVISOR;
                configurationSettings.ScopeSettings.TriggerList[index].Delay.Value = m_TriggerDelay;
			}

            double m_TriggerMinLevel = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMinLevel");
            double m_TriggerMaxLevel = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_TriggerMaxLevel");

            int m_TriggerActiveColor = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_TriggerActiveColor");
		}

        /// <summary>
        /// Reads and applies cursor settings for a specific cursor index and file version.
        /// Handles both modern and legacy file versions as discovered in the file format.
        /// </summary>
        /// <param name="index">The cursor index.</param>
        /// <param name="fileVersion">The file version, as determined from the file header.</param>
        public void GetCursorSettingsFromFile(int index, FileVersion? fileVersion)
		{
            int m_CursorId = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CursorId");
			configurationSettings.ScopeSettings.Cursors[index].Id = m_CursorId + 1;

            double m_CursorPosition = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_CursorPosition");
			
			int m_CursorDisplayed = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CursorDisplayed");
			configurationSettings.ScopeSettings.Cursors[index].Enabled = Convert.ToBoolean(m_CursorDisplayed);
			
			int m_CursorSelected = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CursorSelected");
			double m_CursorPositionIncrement = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_CursorPositionIncrement");

			double m_CursorMinPosition = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_CursorMinPosition");

			double m_CursorMaxPosition = BinaryFileReadHelper.ReadDoubleValueFromFile(binaryReader, stream, "m_CursorMaxPosition");

			if (fileVersion == FileVersion.V1_0)
			{
				// this was not lining up with the default 560, so it appeared to be a wider value like 600?
				m_CursorPosition = m_CursorPosition * (m_CursorMaxPosition - m_CursorMinPosition) / Constants.EXTENDED_SWEEP_WIDTH;
			}
			configurationSettings.ScopeSettings.Cursors[index].Position.Value = m_CursorPosition;

			int m_CursorActiveColor = BinaryFileReadHelper.ReadIntegerValueFromFile(binaryReader, stream, "m_CursorActiveColor");
		}

        /// <summary>
        /// Sets the gain factor for a trace channel based on its scale and units per volt.
        /// This is used to normalize values for different measurement units as discovered in the file format.
        /// </summary>
        /// <param name="index">The trace channel index.</param>
        /// <param name="m_UnitsPerVolt">The units-per-volt value read from the file.</param>
        public void SetUnitGain(int index, double m_UnitsPerVolt)
		{
            switch (scale[index].Split(' ')[1].ToLower())
			{
				case "ma":
				case "mv":
				case "ms":
                    m_UnitsPerVolt /= Constants.DEFAULT_UNIT_ADJUSTMENT_FACTOR;
					break;
				case "kv":
				case "khz":
                    m_UnitsPerVolt *= Constants.DEFAULT_UNIT_ADJUSTMENT_FACTOR;
					break;
				default:
                    unitsGain[index] = (float)m_UnitsPerVolt;
					break;
            }
		}

        /// <summary>
        /// Determines the trace coupling type (AC/DC) for a given index as discovered in the file format.
        /// </summary>
        /// <param name="traceCouplingIndex">The index value read from the file.</param>
        /// <returns>A <see cref="TraceCoupling"/> instance representing the coupling type.</returns>
        public TraceCouplingType GetTraceCoupling(int traceCouplingIndex)
		{
			return (TraceCouplingType)traceCouplingIndex;
		}

        /// <summary>
        /// Determines the peak detect status for a trace channel as discovered in the file format.
        /// </summary>
        /// <param name="tracePeakDetectIndex">The index value read from the file.</param>
        /// <returns>A <see cref="TracePeakDetect"/> instance representing the peak detect status.</returns>
        public PeakDetectStatus GetTraceDetect(int tracePeakDetectIndex)
		{
            return (PeakDetectStatus)tracePeakDetectIndex;
		}

        /// <summary>
        /// Determines the filter state for a trace channel as discovered in the file format.
        /// </summary>
        /// <param name="traceFilterIndex">The index value read from the file.</param>
        /// <returns>A <see cref="TraceFilter"/> instance representing the filter state.</returns>
        public FilterState GetTraceFilter(int traceFilterIndex)
		{
			return (FilterState)traceFilterIndex;
		}

        /// <summary>
        /// Determines the threshold slope (up/down) for a trace channel as discovered in the file format.
        /// </summary>
        /// <param name="thresholdSlopeIndex">The index value read from the file.</param>
        /// <returns>A <see cref="ThresholdSlope"/> instance representing the threshold slope.</returns>
        public ThresholdSlope GetTraceThresholdSlope(int thresholdSlopeIndex)
		{
			ThresholdSlope thresholdSlope = new ThresholdSlope();
			if (thresholdSlopeIndex != 0)
			{
				thresholdSlope.Slope = ThresholdSlopeType.Up;
			}
			return thresholdSlope;
		}

        /// <summary>
        /// Determines the trigger slope (up/down) for a trigger as discovered in the file format.
        /// </summary>
        /// <param name="triggerSlopeIndex">The slope index value read from the file.</param>
        /// <returns>A <see cref="TriggerSlope"/> instance representing the trigger slope.</returns>
        public SlopeType GetTriggerSlope(int triggerSlopeIndex)
        {
            return (SlopeType)triggerSlopeIndex;
        }

        /// <summary>
        /// Determines the trigger source for a given trigger index and value as discovered in the file format.
        /// </summary>
        /// <param name="index">The trigger index.</param>
        /// <param name="triggerSourceIndex">The source index value read from the file.</param>
        /// <returns>A <see cref="TriggerSource"/> instance representing the trigger source.</returns>
        public TriggerSource GetTriggerSource(int index, int triggerSourceIndex)
		{
			TriggerSource result = new TriggerSource(SourceType.None);
            switch (triggerSourceIndex)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    result = new TriggerSource(configurationSettings.ScopeSettings.TraceList[index]);
                    break;
                case 4:
                    result = new TriggerSource(SourceType.Cylinder);
                    break;
                case 5:
                    result = new TriggerSource(SourceType.None);
                    break;
            }
			return result;
		}

        /// <summary>
        /// Determines and sets the vacuum, pressure, or temperature unit for a channel based on the provided unit text.
        /// This method uses field names and logic as discovered during binary analysis.
        /// </summary>
        /// <param name="index">The channel index.</param>
        /// <param name="unitText">The unit text string read from the file.</param>
        public void GetVacPresTempUnit(int index, string unitText)
        {
            if (string.IsNullOrWhiteSpace(unitText))
                return;

            string label = unitText.ToLowerInvariant();

            switch (label)
            {
                case "°f":
                    temperatureUnits[index] = TemperatureUnits.degF;
                    break;
                case "°c":
                    temperatureUnits[index] = TemperatureUnits.degC;
                    break;
                case "kpa":
                    vacuumUnits[index] = VacuumUnits.kPa;
                    pressureUnits[index] = PressureUnits.kPa;
                    break;
                case "mpa":
                    pressureUnits[index] = PressureUnits.kPa;
                    break;
                case "psi":
                    pressureUnits[index] = PressureUnits.psi;
                    break;
                case "bar":
                    pressureUnits[index] = PressureUnits.bar;
                    break;
                case "inhg":
                    vacuumUnits[index] = VacuumUnits.inHg;
                    break;
                case "mmhg":
                    vacuumUnits[index] = VacuumUnits.mmHg;
                    break;
                case "kg/cm²":
                    pressureUnits[index] = PressureUnits.kgcm2;
                    break;
            }
        }

        /// <summary>
        /// Determines the trigger mode (auto/normal) for a trigger as discovered in the file format.
        /// </summary>
        /// <param name="triggerModeIndex">The mode index value read from the file.</param>
        /// <returns>A <see cref="TriggerMode"/> instance representing the trigger mode.</returns>
        public TriggerModeType GetTriggerMode(int triggerModeIndex)
		{
			return (TriggerModeType)triggerModeIndex;
		}

        private bool IsModernVersion(FileVersion? v) => v >= FileVersion.V1_3;
	}
}