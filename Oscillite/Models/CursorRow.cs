using System;

namespace Oscillite
{
    public class CursorRow
    {
        public string Label { get; set; }
        public float? Value1 { get; set; }
        public float? Value2 { get; set; }
        public string Unit { get; set; }
        public SharpDX.Color4 LabelColor { get; set; }

        public string GetValue1Text() => Value1.HasValue ? Value1.Value.ToString("F2") : "—";
        public string GetValue2Text() => Value2.HasValue ? Value2.Value.ToString("F2") : "—";
        public string GetDeltaText() =>
            (Value1.HasValue && Value2.HasValue)
            ? $"{Math.Abs(Value2.Value - Value1.Value):F2} {Unit}"
            : "—";
    }

}
