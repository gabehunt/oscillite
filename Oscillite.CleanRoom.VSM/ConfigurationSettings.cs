using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Oscillite.CleanRoom.VSM
{
    // Clean room open spec transformed to c# class via https://json2csharp.com/code-converters/xml-to-csharp
    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationSettings));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (ConfigurationSettings)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "Sweep")]
    public class Sweep
    {

        [XmlAttribute(AttributeName = "T")]
        public string T { get; set; }

        [XmlAttribute(AttributeName = "Sec")]
        public double Sec { get; set; }

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "U")]
        public string U { get; set; }

        [XmlAttribute(AttributeName = "G")]
        public int G { get; set; }
    }

    [XmlRoot(ElementName = "SweepList")]
    public class SweepList
    {

        [XmlElement(ElementName = "Sweep")]
        public List<Sweep> Sweep { get; set; }
    }

    [XmlRoot(ElementName = "SelectedSweep")]
    public class SelectedSweep
    {

        [XmlAttribute(AttributeName = "T")]
        public string T { get; set; }

        [XmlAttribute(AttributeName = "Sec")]
        public double Sec { get; set; }

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "U")]
        public string U { get; set; }

        [XmlAttribute(AttributeName = "G")]
        public int G { get; set; }
    }

    [XmlRoot(ElementName = "Id")]
    public class Id
    {

        [XmlAttribute(AttributeName = "V")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "E")]
    public class E
    {

        [XmlAttribute(AttributeName = "V")]
        public bool Value { get; set; }
    }

    [XmlRoot(ElementName = "I")]
    public class I
    {

        [XmlAttribute(AttributeName = "V")]
        public bool V { get; set; }
    }

    [XmlRoot(ElementName = "C")]
    public class C
    {

        [XmlAttribute(AttributeName = "V")]
        public string V { get; set; }
    }
    

    [XmlRoot(ElementName = "PD")]
    public class PD
    {

        [XmlAttribute(AttributeName = "V")]
        public string V { get; set; }
    }

    [XmlRoot(ElementName = "F")]
    public class F
    {

        [XmlAttribute(AttributeName = "V")]
        public string V { get; set; }
    }

    [XmlRoot(ElementName = "TS")]
    public class TS
    {

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "FSV")]
        public double FSV { get; set; }

        [XmlAttribute(AttributeName = "U")]
        public string U { get; set; }

        [XmlAttribute(AttributeName = "PG")]
        public double PG { get; set; }

        [XmlAttribute(AttributeName = "PO")]
        public double PO { get; set; }

        [XmlAttribute(AttributeName = "UG")]
        public double UG { get; set; }

        [XmlAttribute(AttributeName = "PA")]
        public double PA { get; set; }

        [XmlAttribute(AttributeName = "V")]
        public string V { get; set; }
    }

    [XmlRoot(ElementName = "TSL")]
    public class TSL
    {

        [XmlElement(ElementName = "TS")]
        public List<TS> TS { get; set; }
    }

    [XmlRoot(ElementName = "SS")]
    public class SS
    {

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "FSV")]
        public int FSV { get; set; }

        [XmlAttribute(AttributeName = "U")]
        public string Unit { get; set; }

        [XmlAttribute(AttributeName = "PG")]
        public double PG { get; set; }

        [XmlAttribute(AttributeName = "PO")]
        public int PO { get; set; }

        [XmlAttribute(AttributeName = "UG")]
        public double UG { get; set; }

        [XmlAttribute(AttributeName = "PA")]
        public int PA { get; set; }
    }

    [XmlRoot(ElementName = "Pr")]
    public class Pr
    {

        [XmlElement(ElementName = "TSL")]
        public TSL TSL { get; set; }

        [XmlElement(ElementName = "SS")]
        public SS SelectedScale { get; set; }

        [XmlAttribute(AttributeName = "T")]
        public string T { get; set; }

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "G")]
        public int G { get; set; }

        [XmlAttribute(AttributeName = "O")]
        public int O { get; set; }

        [XmlAttribute(AttributeName = "HAS")]
        public bool HAS { get; set; }

        [XmlAttribute(AttributeName = "IASS")]
        public bool IASS { get; set; }
    }

    [XmlRoot(ElementName = "S")]
    public class S
    {

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "FSV")]
        public int FullScaleValue { get; set; }

        [XmlAttribute(AttributeName = "U")]
        public string U { get; set; }

        [XmlAttribute(AttributeName = "PG")]
        public double ProbeGain { get; set; }

        [XmlAttribute(AttributeName = "PO")]
        public int PO { get; set; }

        [XmlAttribute(AttributeName = "UG")]
        public double UG { get; set; }

        [XmlAttribute(AttributeName = "PA")]
        public int PA { get; set; }
    }

    [XmlRoot(ElementName = "P")]
    public class P
    {

        [XmlAttribute(AttributeName = "V")]
        public double Value { get; set; }
    }

    [XmlRoot(ElementName = "RS")]
    public class RS
    {

        [XmlAttribute(AttributeName = "V")]
        public double V { get; set; }
    }

    [XmlRoot(ElementName = "TV1")]
    public class TV1
    {

        [XmlAttribute(AttributeName = "V")]
        public double V { get; set; }
    }

    [XmlRoot(ElementName = "TV2")]
    public class TV2
    {

        [XmlAttribute(AttributeName = "V")]
        public double V { get; set; }
    }

    [XmlRoot(ElementName = "Trace")]
    public class Trace
    {

        [XmlElement(ElementName = "Id")]
        public Id Id { get; set; }

        [XmlElement(ElementName = "E")]
        public E Enabled { get; set; }

        [XmlElement(ElementName = "I")]
        public I I { get; set; }

        [XmlElement(ElementName = "C")]
        public List<C> Couplings { get; set; }

        [XmlElement(ElementName = "PD")]
        public PD PD { get; set; }

        [XmlElement(ElementName = "F")]
        public F F { get; set; }

        [XmlElement(ElementName = "Pr")]
        public Pr Probe { get; set; }

        [XmlElement(ElementName = "S")]
        public S Scale { get; set; }

        [XmlElement(ElementName = "P")]
        public P P { get; set; }

        [XmlElement(ElementName = "RS")]
        public RS RS { get; set; }

        [XmlElement(ElementName = "TV1")]
        public TV1 TV1 { get; set; }

        [XmlElement(ElementName = "TV2")]
        public TV2 TV2 { get; set; }

        [XmlElement(ElementName = "TS")]
        public TS TS { get; set; }

        [XmlAttribute(AttributeName = "CT")]
        public DateTime CT { get; set; }

        [XmlAttribute(AttributeName = "FPDC")]
        public bool FPDC { get; set; }

        [XmlAttribute(AttributeName = "FIC")]
        public bool FIC { get; set; }
    }

    [XmlRoot(ElementName = "TraceList")]
    public class TraceList
    {

        [XmlElement(ElementName = "Trace")]
        public List<Trace> Trace { get; set; }
    }

    [XmlRoot(ElementName = "TraceProbe")]
    public class TraceProbe
    {

        [XmlElement(ElementName = "TSL")]
        public TSL TSL { get; set; }

        [XmlElement(ElementName = "SS")]
        public SS SS { get; set; }

        [XmlAttribute(AttributeName = "T")]
        public string T { get; set; }

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "G")]
        public double G { get; set; }

        [XmlAttribute(AttributeName = "O")]
        public double O { get; set; }

        [XmlAttribute(AttributeName = "HAS")]
        public bool HAS { get; set; }

        [XmlAttribute(AttributeName = "IASS")]
        public bool IASS { get; set; }
    }

    [XmlRoot(ElementName = "TraceProbeList")]
    public class TraceProbeList
    {

        [XmlElement(ElementName = "TraceProbe")]
        public List<TraceProbe> TraceProbe { get; set; }
    }

    [XmlRoot(ElementName = "Source")]
    public class Source
    {

        [XmlAttribute(AttributeName = "V")]
        public string V { get; set; }

        [XmlAttribute(AttributeName = "ST")]
        public string ST { get; set; }

        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }

        [XmlAttribute(AttributeName = "C")]
        public string C { get; set; }
    }

    [XmlRoot(ElementName = "M")]
    public class M
    {

        [XmlAttribute(AttributeName = "V")]
        public string V { get; set; }
    }

    [XmlRoot(ElementName = "L")]
    public class L
    {

        [XmlAttribute(AttributeName = "V")]
        public double V { get; set; }
    }

    [XmlRoot(ElementName = "D")]
    public class D
    {

        [XmlAttribute(AttributeName = "V")]
        public double V { get; set; }
    }

    [XmlRoot(ElementName = "Sl")]
    public class Sl
    {

        [XmlAttribute(AttributeName = "V")]
        public string V { get; set; }
    }

    [XmlRoot(ElementName = "Trigger")]
    public class Trigger
    {

        [XmlElement(ElementName = "Source")]
        public Source Source { get; set; }

        [XmlElement(ElementName = "M")]
        public M M { get; set; }

        [XmlElement(ElementName = "L")]
        public L L { get; set; }

        [XmlElement(ElementName = "D")]
        public D D { get; set; }

        [XmlElement(ElementName = "Sl")]
        public Sl Sl { get; set; }

        [XmlElement(ElementName = "C")]
        public C Cylinder { get; set; }
    }

    [XmlRoot(ElementName = "TriggerList")]
    public class TriggerList
    {

        [XmlElement(ElementName = "Trigger")]
        public List<Trigger> Trigger { get; set; }
    }

    [XmlRoot(ElementName = "SelectedTrigger")]
    public class SelectedTrigger
    {

        [XmlElement(ElementName = "Source")]
        public Source Source { get; set; }

        [XmlElement(ElementName = "M")]
        public M M { get; set; }

        [XmlElement(ElementName = "L")]
        public L L { get; set; }

        [XmlElement(ElementName = "D")]
        public D D { get; set; }

        [XmlElement(ElementName = "Sl")]
        public Sl Sl { get; set; }

        [XmlElement(ElementName = "C")]
        public C Cylinder { get; set; }
    }

    [XmlRoot(ElementName = "Cursor")]
    public class Cursor
    {

        [XmlElement(ElementName = "Id")]
        public Id CId { get; set; }

        [XmlElement(ElementName = "E")]
        public E CE { get; set; }

        [XmlElement(ElementName = "P")]
        public P CP { get; set; }
    }

    [XmlRoot(ElementName = "CursorList")]
    public class CursorList
    {

        [XmlElement(ElementName = "Cursor")]
        public List<Cursor> Cursor { get; set; }
    }

    [XmlRoot(ElementName = "Ignition")]
    public class Ignition
    {

        [XmlAttribute(AttributeName = "IT")]
        public string IT { get; set; }

        [XmlAttribute(AttributeName = "NOC")]
        public int NOC { get; set; }

        [XmlAttribute(AttributeName = "FO")]
        public string FO { get; set; }

        [XmlAttribute(AttributeName = "NOT")]
        public string NOT { get; set; }

        [XmlAttribute(AttributeName = "P")]
        public string P { get; set; }

        [XmlAttribute(AttributeName = "RPMF")]
        public string RPMF { get; set; }

        [XmlAttribute(AttributeName = "ES")]
        public int ES { get; set; }
    }

    [XmlRoot(ElementName = "ScopeSettings")]
    public class ScopeSettings
    {

        [XmlElement(ElementName = "ScopeType")]
        public string ScopeType { get; set; }

        [XmlElement(ElementName = "SweepList")]
        public SweepList SweepList { get; set; }

        [XmlElement(ElementName = "SelectedSweep")]
        public SelectedSweep SelectedSweep { get; set; }

        [XmlElement(ElementName = "TraceList")]
        public TraceList TraceList { get; set; }

        [XmlElement(ElementName = "TraceProbeList")]
        public TraceProbeList TraceProbeList { get; set; }

        [XmlElement(ElementName = "TriggerList")]
        public TriggerList TriggerList { get; set; }

        [XmlElement(ElementName = "SelectedTrigger")]
        public SelectedTrigger SelectedTrigger { get; set; }

        [XmlElement(ElementName = "CursorList")]
        public CursorList CursorList { get; set; }

        [XmlElement(ElementName = "Ignition")]
        public Ignition Ignition { get; set; }

        [XmlElement(ElementName = "PressureUnits")]
        public string PressureUnits { get; set; }

        [XmlElement(ElementName = "VacuumUnits")]
        public string VacuumUnits { get; set; }

        [XmlElement(ElementName = "TemperatureUnits")]
        public string TemperatureUnits { get; set; }

        [XmlElement(ElementName = "ScopeHardwareType")]
        public string ScopeHardwareType { get; set; }
    }

    [XmlRoot(ElementName = "CurrentBufferPosition")]
    public class CurrentBufferPosition
    {

        [XmlAttribute(AttributeName = "V")]
        public int V { get; set; }
    }

    [XmlRoot(ElementName = "FrameSize")]
    public class FrameSize
    {

        [XmlAttribute(AttributeName = "V")]
        public int V { get; set; }
    }

    [XmlRoot(ElementName = "TId")]
    public class TId
    {

        [XmlAttribute(AttributeName = "V")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "PL")]
    public class PL
    {

        [XmlElement(ElementName = "P")]
        public List<double> Points { get; set; }
    }

    [XmlRoot(ElementName = "PPS")]
    public class PPS
    {

        [XmlAttribute(AttributeName = "V")]
        public int V { get; set; }
    }

    [XmlRoot(ElementName = "PWI")]
    public class PWI
    {

        [XmlAttribute(AttributeName = "V")]
        public int V { get; set; }
    }

    [XmlRoot(ElementName = "WaveformData")]
    public class WaveformData
    {

        [XmlElement(ElementName = "TId")]
        public TId TId { get; set; }

        [XmlElement(ElementName = "PL")]
        public PL PointsList { get; set; }

        [XmlElement(ElementName = "PPS")]
        public PPS PPS { get; set; }

        [XmlElement(ElementName = "PWI")]
        public PWI PWI { get; set; }

        [XmlAttribute(AttributeName = "HL")]
        public string HL { get; set; }

        [XmlAttribute(AttributeName = "LL")]
        public string LL { get; set; }

        [XmlAttribute(AttributeName = "Prec")]
        public int Prec { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "WaveformDataList")]
    public class WaveformDataList
    {

        [XmlElement(ElementName = "WaveformData")]
        public List<WaveformData> WaveformData { get; set; }
    }

    [XmlRoot(ElementName = "ConfigurationSettings")]
    public class ConfigurationSettings
    {

        [XmlElement(ElementName = "ScopeSettings")]
        public ScopeSettings ScopeSettings { get; set; }

        [XmlElement(ElementName = "CurrentBufferPosition")]
        public CurrentBufferPosition CurrentBufferPosition { get; set; }

        [XmlElement(ElementName = "FrameSize")]
        public FrameSize FrameSize { get; set; }

        [XmlElement(ElementName = "WaveformDataList")]
        public WaveformDataList WaveformDataList { get; set; }

        [XmlAttribute(AttributeName = "xsi")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd")]
        public string Xsd { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
