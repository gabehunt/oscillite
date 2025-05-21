# Clean-Room Analysis: LSM File Format Specification

## Overview

This document outlines a forensic, clean-room analysis of the `.lsm` file format, used in signal acquisition and waveform storage contexts. The findings here are based solely on static and dynamic inspection of real-world `.lsm` file contents and behavior, without access to proprietary file format documentation or source code.

The file format appears to be structured, human-readable at the top, and binary-encoded in the lower regions. Numerous `m_`-prefixed field names are embedded in the data, suggesting a self-describing configuration structure.

---

## File Header

* The first **4 bytes** of the file are consistently structured and appear to represent a **magic number** or file signature, possibly for file type validation.
* These bytes are often followed by recognizable strings in ASCII or UTF-16LE encoding.

---

## Field Layout and Observation

After the initial header, the file transitions into a pattern of repeated structures:

1. **Field Name Section**:

   * Appears as sequences of recognizable ASCII or UTF-16LE-encoded field names.
   * Each field starts with the prefix `m_`, which resembles Hungarian notation typically used to denote module-level or member-level variables in code.
   * Examples include: "m\_TriggerSource", "m\_Scale", "m\_LabelText", etc.

2. **Field Value Section**:

   * Directly after each field name (or at an offset indicated elsewhere), raw binary data is observed.
   * The format of the binary data varies and appears to correlate with the logical type suggested by the field.

---

## Verified m\_ Field Names in LSM Files

The following list includes only the verified string keys read directly from `.lsm` files. These names are passed exactly as string arguments into the file reader and represent the true serialized identifiers. No inferred property names or internal aliases have been used.

### Ignition and Engine Fields

* "m\_IgnitionType"
* "m\_EngineStroke"
* "m\_NumberOfCylinders"
* "m\_CylinderCount"
* "m\_CylinderDisplayed"
* "m\_CylinderPolarities"
* "m\_FiringOrder"

### Trigger Configuration

* "m\_TriggerSource"
* "m\_TriggerMode"
* "m\_TriggerDelay"
* "m\_TriggerLevel"
* "m\_TriggerSlope"
* "m\_TriggerDisplayed"
* "m\_TriggerDelayIncrement"
* "m\_TriggerLevelIncrement"
* "m\_TriggerMinDelay"
* "m\_TriggerMaxDelay"
* "m\_TriggerMinLevel"
* "m\_TriggerMaxLevel"
* "m\_TriggerActiveColor"
* "m\_TriggerCylinder"
* "m\_TriggerSelection"
* "m\_TriggerTextDisplay"

### Cursor Configuration

* "m\_CursorId"
* "m\_CursorPosition"
* "m\_CursorDisplayed"
* "m\_CursorSelected"
* "m\_CursorPositionIncrement"
* "m\_CursorMinPosition"
* "m\_CursorMaxPosition"
* "m\_CursorActiveColor"
* "m\_CursorSelection"
* "m\_CursorInfos\[]"

### Display / Layout Settings

* "m\_ViewType"
* "m\_bGridOn"
* "m\_ScaleLabelsDisplay"
* "m\_ScaleDisplayMode"
* "m\_BackgroundColor"
* "m\_TraceSelection"
* "m\_SweepRate"
* "m\_SweepWidth"
* "m\_SettingsDisplayed"
* "m\_PositionIncrement"
* "m\_MinPosition"
* "m\_MaxPosition"
* "m\_UnitsPerVolt"
* "m\_Precision"
* "m\_UnitsText"
* "m\_AutoScale"
* "m\_RasterSpacing"
* "m\_bDigitalReadOutTracesDisplayed"
* "m\_bDigitalReadOutRPMDisplayed"

### Channel and Probe Fields

* "m\_ChannelId"
* "m\_ChannelProbeType"
* "m\_ChannelCouplingType"
* "m\_ChannelScale"
* "m\_ChannelEnabled"
* "m\_ChannelFilter"
* "m\_ChannelLeadResistance"
* "m\_bPeakDetectByChangeInProbeType"
* "m\_bInvertedByChangeInProbeType"
* "m\_PeakDetectState"
* "m\_PulseScale"
* "m\_ThresholdValue"
* "m\_ThresholdSlope"

### Multimeter Fields

* "m\_MultimeterProbeType"
* "m\_MultimeterCouplingType"
* "m\_MultimeterScale"
* "m\_MultimeterEnabled"
* "m\_MultimeterLeadResistance"

### Trace Fields

* "m\_Id"
* "m\_Displayed"
* "m\_InputType"
* "m\_Position"
* "m\_Scale"
* "m\_Inverted"
* "m\_LabelDisplayed"
* "m\_LabelText"
* "m\_LabelColor"

### Buffer Fields

* "m\_BufferPointIndex"
* "m\_BufferPointCount"
* "m\_FrozenBufferPointIndex"
* "m\_TraceInfos\[]"
* "m\_ChannelInfos\[]"
* "m\_TriggerInfos\[]"

### Other Verified Fields

* "m\_DataPlatformType"
* "m\_MainSettings"
* "m\_TraceSettings"
* "m\_TriggerSettings"
* "m\_CursorSettings"

## Inferred Field Types

Through repeat pattern analysis, test file behavior, and data entropy examination, the following data types have been inferred:

* **Integer (int32)** — Common for ID fields, toggle flags, enumerated types, counters
* **Floating-Point (double)** — Used for positions, scaling factors, increment values
* **String (UTF-16LE)** — Used for labels and text fields
* **Boolean** — Typically encoded as 0/1 integers
* **Arrays / Repeated Structures** — Often indicated with `[]` notation

---

## Structural Observations

* Field data appears grouped into logical sections.
* File layout transitions into \[header → data] structures per trace after metadata.
* Each waveform trace contains its own 42-byte header immediately followed by its data buffer.
* Headers contain values for calibration multiplier, divisor, offset, point count, point size, and statistical fields.
* All waveform headers are followed by raw binary buffers containing per-point waveform samples.
* Multi-byte values are little-endian.

---

## Parsing Strategy

A clean-room parser should:

1. Validate the 4-byte magic header.
2. Locate and decode UTF-16LE field names prefixed with `m_`.
3. Parse each field using its embedded value.
4. Locate waveform header blocks by searching for known patterns like `m_BufferPointCount` and matching counts.
5. Parse each 42-byte header + 2-byte null terminator as a waveform header.
6. Extract waveform buffers and apply calibration formula:

   ```
   calibrated = ((raw - offset) * multiplier) / divisor
   ```

---

## License and Disclaimer

This document is provided for research and interoperability purposes only. No proprietary or confidential information was used. Contributions and corrections are welcome.
