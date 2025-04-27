# VSM Open Format CleanRoom Specification

## Objective
This document captures independently observed forensic information about VSM file structures based on cleanroom analysis techniques. No prior source code, file format documentation, or proprietary knowledge was used during this investigation.

---

## Summary of Forensic Findings

### 1. Initial File Composition
- **XML Segment:**
  - Detected between byte 0 and approximately byte 73.
  - Structure identified: `<Vehicle year="" make="" model=""><Description></Description></Vehicle>`
  - Terminates with unusual characters (`@@`) before transition into next data type.

### 2. Compression Artifact
- **Magic Bytes:** `0x1F 0x8B` (standard GZip magic number) detected immediately after XML segment.
- **Offset:** Observed at byte position 73.

### 3. Decompressed Data Characteristics
- **Decompression:** Successful using GZip standard decompression.
- **Encoding:** utf-8 interpreted cleanly.

### 4. Discovered XML Elements (Post-Decompression)

#### Top-Level
- `<ConfigurationSettings>`
  - Attributes: *(none explicitly discovered)*
  - See [Sample VSM XML file](sample.xml) for an example decompressed structure.

#### Child Elements under `<ConfigurationSettings>`
- `<ScopeSettings>`
- `<CurrentBufferPosition>`: Attribute `V`
- `<FrameSize>`: Attribute `V`
- `<WaveformDataList>`

#### Child Elements under `<ScopeSettings>`
- `<ScopeType>`
- `<SweepList>`
- `<SelectedSweep>`
- `<TraceList>`
- `<TraceProbeList>`
- `<TriggerList>`
- `<SelectedTrigger>`
- `<CursorList>`
- `<Ignition>`
- `<PressureUnits>`
- `<VacuumUnits>`
- `<TemperatureUnits>`
- `<ScopeHardwareType>`

#### Element Attributes Discovered

| Element | Known Attributes |
|:---|:---|
| `<Sweep>` | `T`, `Sec`, `N`, `U`, `G` |
| `<SelectedSweep>` | `T`, `Sec`, `N`, `U`, `G` |
| `<Trace>` | `C`, `CT`, `FPDC`, `FIC` |
| `<Id>` | `V` |
| `<E>` | `V` |
| `<I>` | `V` |
| `<C>` | `V` |
| `<PD>` | `V` |
| `<F>` | `V` |
| `<Pr>` (Probe?) | `T`, `N`, `G`, `O`, `HAS`, `IASS` |
| `<TS>` (Trace Scale?) | `N`, `FSV`, `U`, `PG`, `PO`, `UG`, `PA` |
| `<SS>` (Selected Scale?) | `N`, `FSV`, `U`, `PG`, `PO`, `UG`, `PA` |
| `<P>` | `V` |
| `<RS>` | `V` |
| `<TV1>` | `V` |
| `<TV2>` | `V` |
| `<TS>` | `V` |
| `<Trigger>` | *(nested child elements only, see below)* |
| `<Source>` | `V`, `ST`, `N`, `C` |
| `<M>` (Mode?  Values like "Auto") | `V` |
| `<L>` | `V` |
| `<D>` | `V` |
| `<Sl>` (Slope? Values [U,D] May be Up/Down?) | `V` |
| `<C>` | `V` |
| `<Ignition>` | `IT`, `NOC`, `FO`, `NOT`, `P`, `RPMF`, `ES` |

### 5. Discovered List Values

#### `<ScopeType>` Values
- `LS` (presumed Lab Scope, also known as an Oscilloscope)
- `DM` (presumed Digital Meter, DM short for DMM industry acronym for Digital Multimeter)

#### `<PressureUnits>` Values
- `psi`

#### `<VacuumUnits>` Values
- `inHg`

#### `<TemperatureUnits>` Values
- `degF`

#### `<ScopeHardwareType>` Values
- `M5`
- `M6`
- `No_HW` (No Hardware?)

#### TraceProbe Types (`<TraceProbe T="...">`)
- `V` (Volts)
- `V100` (Vacuum) (Inferred once we knew P### was Pressure)
- `P100` (Pressure)
- `P500` (Pressure -- found in Waveform file "GOLF W TRANS 500PSI.vsm")
- `P5000` (Pressure)
- `LA20` (Low Amps 20)
- `LA40` (Low Amps 40)
- `LA60` (Low Amps 60) (We must not have discovered the high amp clamps yet)
- `I` (Ignition)
- `EEDM506D_TEMP` (Temperature)
- `MT5030_V` (Vacuum assumed by V suffix)
- `MT5030_P` (Pressure assumed by V suffix)

### 6. Decompressed File Size Range
- Full decompressed XML was observed with several kilobytes of text beyond initial configuration settings.

---

## Methodology
- File content inspection was performed using custom-built forensic tooling (`Oscillite.CleanRoomFileDiscovery.exe`).
- No proprietary readers, SDKs, or leaked documentation were referenced.
- Hex editors and open RFCs for GZip (RFC 1952) were used.
- XML structures were detected based on open W3C standards.

## Notes
- All discovered structures and values are purely observational and do not assume completeness.
- Some values such as units (`µs`, `mV`, `psi`) appear to use common metric prefixes.

## Future Research Suggestions
- Deeper exploration of WaveformData structures (e.g., how "P" point lists are encoded).
- Statistical analysis of buffer positions for waveform data alignment.

---

*This spec is cleanroom and independently created for compatibility, not for duplication of proprietary formats.*

