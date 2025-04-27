using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace Oscillite.Utilities
{
    public class WaveformRenderContext
    {
        public RenderTarget RenderTarget { get; set; }
        public SharpDX.Direct2D1.Factory D2DFactory { get; set; }
        public SharpDX.DirectWrite.Factory WriteFactory { get; set; }
        public TextFormat TextFormat { get; set; }
        public RectangleF DrawingArea { get; set; }
        public ZoomRegion Zoom { get; set; }
        public bool IsZoomedIn { get; set; }

        public float FileTimespan { get; set; }
        public List<WaveformChannel> Channels { get; set; }
        public List<TimeRuler> TimeRulers { get; set; }
        public bool ShowPhaseRulers { get; set; }
        public TimeRuler PhaseRuler1 { get; set; }
        public TimeRuler PhaseRuler2 { get; set; }
        public bool IsZoomingSelectionActive { get; set; }
        public SharpDX.Point ZoomStart { get; set; }
        public SharpDX.Point ZoomEnd { get; set; }
        public CursorSummaryOverlay CursorOverlay { get; set; }

    }
}
