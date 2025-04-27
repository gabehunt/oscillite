using System;
using SharpDX;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;

namespace Oscillite.Utilities
{
    public class ToolManager
    {
        public enum ToolMode { Zoom, Pan }
        private ToolMode currentTool = ToolMode.Zoom;
        public ToolMode CurrentTool => currentTool;
        public event Action<ToolMode> OnToolChanged;
        private bool isZoomingSelectionActive = false;
        private SharpDX.Point zoomSelectionStartPoint;
        private SharpDX.Point zoomSelectionEndPoint;
        private bool isPanning = false;
        private SharpDX.Point panStartMousePos;
        private ZoomRegion panStartZoom;
        private readonly Func<SharpDX.RectangleF> getDrawingArea;
        private readonly Func<ZoomRegion> getCurrentZoom;
        private readonly Action<ZoomRegion> setZoom;
        private readonly Action invalidate;
        private readonly Func<float> getTotalTimeSpan;
        private readonly Func<List<WaveformChannel>> getChannels;

        public ToolManager(
            Func<SharpDX.RectangleF> getDrawingArea,
            Func<ZoomRegion> getCurrentZoom,
            Action<ZoomRegion> setZoom,
            Action invalidate,
            Func<float> getTotalTimeSpan,
            Func<List<WaveformChannel>> getChannels)
        {
            this.getDrawingArea = getDrawingArea;
            this.getCurrentZoom = getCurrentZoom;
            this.setZoom = setZoom;
            this.invalidate = invalidate;
            this.getTotalTimeSpan = getTotalTimeSpan;
            this.getChannels = getChannels;
        }

        public void ToggleToolMode()
        {
            currentTool = (currentTool == ToolMode.Zoom) ? ToolMode.Pan : ToolMode.Zoom;
            OnToolChanged?.Invoke(currentTool);
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            var drawingArea = getDrawingArea();
            var pos = new SharpDX.Vector2(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {
                if (currentTool == ToolMode.Zoom && drawingArea.Contains(pos))
                {
                    isZoomingSelectionActive = true;
                    zoomSelectionStartPoint = new SharpDX.Point(e.X, e.Y);
                    zoomSelectionEndPoint = new SharpDX.Point(e.X, e.Y);
                    invalidate();
                }
                else if (currentTool == ToolMode.Pan && drawingArea.Contains(pos))
                {
                    isPanning = true;
                    panStartMousePos = new SharpDX.Point(e.X, e.Y);
                    panStartZoom = getCurrentZoom();
                }
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (isZoomingSelectionActive)
            {
                zoomSelectionEndPoint = new SharpDX.Point(e.X, e.Y);
                invalidate();
            }
            else if (isPanning)
            {
                PanFromMouse(new SharpDX.Point(e.X, e.Y));
                invalidate();
            }
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            if (isZoomingSelectionActive)
            {
                isZoomingSelectionActive = false;
                var zoom = GetZoomRegionFromScreenPoints(zoomSelectionStartPoint, zoomSelectionEndPoint);
                if (zoom != null && !zoom.IsEmpty)
                {
                    setZoom(zoom);
                }
                //LogZoomSummary(zoomStartPoint, zoomEndPoint, zoom, totalTimeSpan);
                //LogZoomVoltageSummary(zoom, channels);
                invalidate();
            }

            if (isPanning)
            {
                isPanning = false;
                invalidate();
            }
        }

        private void LogZoomSummary(SharpDX.Point start, SharpDX.Point end, ZoomRegion zoom, float totalTimeSpan)
        {
            if(zoom == null) return;

            float timeSpan = zoom.TimeEnd - zoom.TimeStart;
            float timePct = (timeSpan / totalTimeSpan) * 100f;
            float vertPct = zoom.ViewportHeightNorm * 100f;

            Debug.WriteLine("🟦 Zoom Selection:");
            Debug.WriteLine($"   Screen Box:   ({start.X},{start.Y}) → ({end.X},{end.Y})");
            Debug.WriteLine($"   Time Range:   {zoom.TimeStart:F3}s → {zoom.TimeEnd:F3}s");
            Debug.WriteLine($"   ⏱ Span:       {timeSpan:F3}s ({timePct:F1}% of full)");
        }

        private void LogZoomVoltageSummary(ZoomRegion zoom, List<WaveformChannel> channels, int divisionsY = 8)
        {
            foreach (var channel in channels.Where(c => c.Visible))
            {
                Debug.WriteLine($"   📈 {channel.Name} Voltage Zoom: {channel.ZoomedVoltageMin:F3}V → {channel.ZoomedVoltageMax:F3}V");
            }
        }


        private void PanFromMouse(SharpDX.Point currentMousePos)
        {
            var drawingArea = getDrawingArea();
            var deltaX = currentMousePos.X - panStartMousePos.X;
            var deltaY = currentMousePos.Y - panStartMousePos.Y;

            var zoom = panStartZoom;

            float timeDelta = -(deltaX / drawingArea.Width) * zoom.TimeSpan;
            float viewportTopDelta = deltaY / drawingArea.Height;

            float newTimeStart = Math.Max(0, Math.Min(getTotalTimeSpan() - zoom.TimeSpan, zoom.TimeStart + timeDelta));

            var newZoom = new ZoomRegion
            {
                TimeStart = newTimeStart,
                TimeEnd = newTimeStart + zoom.TimeSpan,
                ViewportTopNorm = zoom.ViewportTopNorm + viewportTopDelta,
                ViewportHeightNorm = zoom.ViewportHeightNorm
            };

            setZoom(newZoom);
        }

        private ZoomRegion GetZoomRegionFromScreenPoints(SharpDX.Point start, SharpDX.Point end)
        {
            var drawingArea = getDrawingArea();
            int x1 = Helpers.ClampToRange(start.X, drawingArea.Left, drawingArea.Right);
            int x2 = Helpers.ClampToRange(end.X, drawingArea.Left, drawingArea.Right);
            int y1 = Helpers.ClampToRange(start.Y, drawingArea.Top, drawingArea.Bottom);
            int y2 = Helpers.ClampToRange(end.Y, drawingArea.Top, drawingArea.Bottom);

            if (x2 - x1 < 2 || y2 - y1 < 2)
                return null;

            float sx = Math.Min(x1, x2);
            float ex = Math.Max(x1, x2);
            float sy = Math.Min(y1, y2);
            float ey = Math.Max(y1, y2);

            var zoom = getCurrentZoom();
            var transform = new ViewportTransform(
                new SharpDX.RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1),
                drawingArea);

            float tStart = transform.ScreenToWorldX(sx);
            float tEnd = transform.ScreenToWorldX(ex);

            float vTopNorm = (sy - drawingArea.Top) / drawingArea.Height;
            float vHeightNorm = (ey - sy) / drawingArea.Height;

            // Apply vertical zoom to channels
            foreach (var channel in getChannels())
            {
                // Use the actual transform based on existing world bounds
                var worldBounds = new RectangleF(zoom.TimeStart, channel.GetVisibleVoltageRange().Min, zoom.TimeSpan, channel.GetVisibleVoltageRange().Max - channel.GetVisibleVoltageRange().Min);
                var transform2 = new ViewportTransform(worldBounds, drawingArea);

                // Clamp screen coordinates
                float sy2 = Math.Min(start.Y, end.Y);
                float ey2 = Math.Max(start.Y, end.Y);

                float voltsTop = transform2.ScreenToWorldY(sy2 - channel.Offset);
                float voltsBottom = transform2.ScreenToWorldY(ey2 - channel.Offset);

                channel.ZoomedVoltageMin = Math.Min(voltsTop, voltsBottom);
                channel.ZoomedVoltageMax = Math.Max(voltsTop, voltsBottom);
            }

            return new ZoomRegion
            {
                TimeStart = tStart,
                TimeEnd = tEnd,
                ViewportTopNorm = vTopNorm,
                ViewportHeightNorm = vHeightNorm
            };
        }

        public bool IsZoomingSelectionActive => isZoomingSelectionActive;
        public SharpDX.Point ZoomSelectionStartPoint => zoomSelectionStartPoint;
        public SharpDX.Point ZoomSelectionEndPoint => zoomSelectionEndPoint;
    }
}
