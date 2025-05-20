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
   * Examples include: `"m_TriggerSource"`, `"m_Scale"`, `"m_LabelText"`, etc.

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

* **Integer (int32)**

  * Common for ID fields, toggle flags, enumerated types, counters.
  * Examples: `"m_Id"`, `"m_Displayed"`, `"m_CursorId"`

* **Floating-Point (double)**

  * Used for positions, scaling factors, increment values, timing.
  * Examples: `"m_Position"`, `"m_TriggerLevel"`, `"m_UnitsPerVolt"`

* **String (UTF-16LE)**

  * Used for labels, text fields, descriptions.
  * Examples: `"m_LabelText"`, `"m_UnitsText"`

* **Boolean**

  * Typically encoded as integer 0/1, inferred from toggle-like behavior in tooling.
  * Examples: `"m_Displayed"`, `"m_LabelDisplayed"`

* **Arrays / Repeated Structures**

  * Some fields appear with `[]` notation, suggesting embedded arrays.
  * Examples: `"m_TraceInfos[]"`, `"m_CursorInfos[]"`

---

## Structural Observations

* Field data appears grouped into logical sections (e.g., traces, cursors, channels), though boundaries are implicit.
* The number of repetitions (e.g., number of traces) may be derived from earlier integer fields or inferred by structure count.
* Some fields appear to be version-dependent — certain values are absent in older files.

---

## Interpretation Strategy

When interpreting `.lsm` files, a clean-room parser should:

1. Validate the magic header.
2. Search for UTF-encoded field name patterns starting with `m_`.
3. At each field name location, parse the subsequent bytes based on known value patterns (e.g., 4-byte int, 8-byte double, null-terminated UTF-16LE string).
4. Maintain a mapping of discovered fields and their offsets.
5. Handle arrays by repeated reads and dynamic sizing based on index patterns.

---

# LSM File Format: Clean-Room Specification

## Introduction

This document provides an independent, clean-room specification of the `.lsm` file format, as used in signal acquisition and waveform storage. All information herein is derived solely from empirical analysis of `.lsm` files—no proprietary documentation, source code, or confidential information has been referenced.

This specification is intended to facilitate interoperability, research, and archival efforts.

---

## Methodology

- **Static and dynamic analysis** of `.lsm` files was performed.
- All field names, types, and structures were inferred from observed file contents and behavior.
- No reverse engineering of proprietary binaries or documentation was conducted.

---

## File Structure Overview

- The file begins with a **4-byte magic number** (file signature) for type validation.
- This is followed by ASCII or UTF-16LE encoded strings, often representing field names.
- The file is organized as a sequence of field name/value pairs, with field names prefixed by `m_`.
- Field values are stored in binary, with type and length inferred from context.

---

## Field Name Conventions

- All verified field names begin with `m_` (e.g., `m_TriggerSource`, `m_Scale`).
- Arrays are denoted with `[]` (e.g., `m_TraceInfos[]`).
- Field names are UTF-16LE or ASCII encoded.

---

## Field Encoding and Value Extraction

Through clean-room analysis, the following conventions for field encoding and value extraction in `.lsm` files have been established:

- **Field Storage Format:**
  - Each field is stored as a UTF-16LE (Unicode) string, immediately preceded by a 4-byte little-endian integer indicating the total byte length of the string (including a 2-byte null terminator at the end).
  - The string itself is formatted as: ```m_FieldName value``` where the field name and value are separated by a single space character.

- **Length Calculation:**
  - The 4-byte integer at the start specifies the total byte length of the field string, including the 2-byte null terminator.
  - To extract the actual string, subtract 2 from the length (to exclude the null terminator).

- **Parsing Steps:**
  1. Read the 4-byte length prefix.
  2. Subtract 2 to determine the length of the UTF-16LE string (excluding the null).
  3. Read the string bytes and decode as UTF-16LE.
  4. Split the resulting string on the first space character:
     - The first token is the field name (e.g., `m_SweepWidth`).
     - The second token is the field value (e.g., `560`).
  5. The field value is always space-delimited after the field name.

- **Example:**
  - If the file contains the following bytes: ```[0x0012][UTF-16LE bytes for "m_SweepWidth 560"][0x0000]``` 
    - `0x0012` (18) is the total byte length (16 bytes for the string, 2 bytes for the null terminator).
    - The string is `"m_SweepWidth 560"`.

- **Notes:**
  - All field names and values are extracted exactly as found in the file, with no inference or renaming.
  - This format was determined solely by binary analysis of `.lsm` files.

---

## Verified Field Names

Below is a non-exhaustive list of field names observed in real `.lsm` files:

<details>
<summary>Click to expand</summary>

### Ignition and Engine
- `m_IgnitionType`
- `m_EngineStroke`
- `m_NumberOfCylinders`
- `m_CylinderCount`
- `m_CylinderDisplayed`
- `m_CylinderPolarities`
- `m_FiringOrder`

### Trigger Configuration
- `m_TriggerSource`
- `m_TriggerMode`
- `m_TriggerDelay`
- `m_TriggerLevel`
- `m_TriggerSlope`
- `m_TriggerDisplayed`
- `m_TriggerDelayIncrement`
- `m_TriggerLevelIncrement`
- `m_TriggerMinDelay`
- `m_TriggerMaxDelay`
- `m_TriggerMinLevel`
- `m_TriggerMaxLevel`
- `m_TriggerActiveColor`
- `m_TriggerCylinder`
- `m_TriggerSelection`
- `m_TriggerTextDisplay`

### Cursor Configuration
- `m_CursorId`
- `m_CursorPosition`
- `m_CursorDisplayed`
- `m_CursorSelected`
- `m_CursorPositionIncrement`
- `m_CursorMinPosition`
- `m_CursorMaxPosition`
- `m_CursorActiveColor`
- `m_CursorSelection`
- `m_CursorInfos[]`

### Display / Layout
- `m_ViewType`
- `m_bGridOn`
- `m_ScaleLabelsDisplay`
- `m_ScaleDisplayMode`
- `m_BackgroundColor`
- `m_TraceSelection`
- `m_SweepRate`
- `m_SweepWidth`
- `m_SettingsDisplayed`
- `m_PositionIncrement`
- `m_MinPosition`
- `m_MaxPosition`
- `m_UnitsPerVolt`
- `m_Precision`
- `m_UnitsText`
- `m_AutoScale`
- `m_RasterSpacing`
- `m_bDigitalReadOutTracesDisplayed`
- `m_bDigitalReadOutRPMDisplayed`

### Channel and Probe
- `m_ChannelId`
- `m_ChannelProbeType`
- `m_ChannelCouplingType`
- `m_ChannelScale`
- `m_ChannelEnabled`
- `m_ChannelFilter`
- `m_ChannelLeadResistance`
- `m_bPeakDetectByChangeInProbeType`
- `m_bInvertedByChangeInProbeType`
- `m_PeakDetectState`
- `m_PulseScale`
- `m_ThresholdValue`
- `m_ThresholdSlope`

### Multimeter
- `m_MultimeterProbeType`
- `m_MultimeterCouplingType`
- `m_MultimeterScale`
- `m_MultimeterEnabled`
- `m_MultimeterLeadResistance`

### Trace
- `m_Id`
- `m_Displayed`
- `m_InputType`
- `m_Position`
- `m_Scale`
- `m_Inverted`
- `m_LabelDisplayed`
- `m_LabelText`
- `m_LabelColor`

### Buffer
- `m_BufferPointIndex`
- `m_BufferPointCount`
- `m_FrozenBufferPointIndex`
- `m_TraceInfos[]`
- `m_ChannelInfos[]`
- `m_TriggerInfos[]`

### Other
- `m_DataPlatformType`
- `m_MainSettings`
- `m_TraceSettings`
- `m_TriggerSettings`
- `m_CursorSettings`

</details>

---

## Inferred Field Types

| Type         | Description                                      | Example Fields                |
|--------------|--------------------------------------------------|-------------------------------|
| Integer      | 4 bytes, little-endian                           | `m_Id`, `m_Displayed`         |
| Double       | 8 bytes, little-endian                           | `m_Position`, `m_TriggerLevel`|
| String       | UTF-16LE, null-terminated                        | `m_LabelText`, `m_UnitsText`  |
| Boolean      | Integer 0/1                                      | `m_Displayed`, `m_LabelDisplayed` |
| Array        | Repeated structure, denoted by `[]`              | `m_TraceInfos[]`, `m_CursorInfos[]` |

---

## Structural Notes

- Fields are grouped by logical sections (e.g., traces, cursors, channels).
- The number of repeated structures (e.g., traces) is typically defined by a preceding integer field.
- Some fields are version-dependent and may not appear in all files.
- All multi-byte values are little-endian.

---

## Parsing Strategy

A clean-room parser should:

1. Validate the 4-byte magic header.
2. Locate and decode field names (UTF-16LE or ASCII, prefixed with `m_`).
3. Parse field values according to inferred type and context.
4. Map field names to offsets and values.
5. Handle arrays by reading repeated structures as indicated by count fields.

---

## Limitations

- Field presence and order may vary by file version.
- Some offsets may be relative or indirect.
- This specification is based solely on observed file samples; further research may refine these findings.

---

## License and Disclaimer

This document is provided for research and interoperability purposes only. No proprietary or confidential information was used. Contributions and corrections are welcome.
## Limitations

* The byte alignment or endianness has been observed as little-endian.
* Field presence may vary between file versions or use cases.
* Structure is mostly sequential; however, some offsets may be relative or indirect.

---

## Purpose of Documentation

This specification is intended solely for research, interoperability, and archival purposes. No internal documentation or proprietary data structures have been referenced. All insights were derived empirically from file content.

Further reverse discovery and data mining are ongoing to refine field typing and structural inference.


