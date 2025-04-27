using System.Windows.Forms;
using Oscillite.Utilities;

namespace Oscillite.Input
{
    public class WaveformToolController
    {
        private readonly WaveformViewer viewer;
        private readonly ToolManager toolManager;
        private ZoomRegion currentZoom = new ZoomRegion();

        public ToolManager.ToolMode CurrentToolMode => toolManager.CurrentTool;

        public bool IsZoomingSelectionActive => toolManager.IsZoomingSelectionActive;
        public SharpDX.Point ZoomSelectionStartPoint => toolManager.ZoomSelectionStartPoint;
        public SharpDX.Point ZoomSelectionEndPoint => toolManager.ZoomSelectionEndPoint;

        public ToolManager ToolManager => toolManager;

        public WaveformToolController(WaveformViewer viewer)
        {
            this.viewer = viewer;

            toolManager = new ToolManager(
                () => viewer.DrawingArea,
                () => GetEffectiveZoom(),
                zoom =>
                {
                    currentZoom = zoom;
                    viewer.SetZoomRegion(zoom); // ✅ update the actual viewer's zoom reference
                },    // ✅ updates internal zoom
                viewer.Invalidate,
                () => viewer.CurrentFileTimespan,
                () => viewer.Channels
            );

            toolManager.OnToolChanged += OnToolChanged;
        }

        public void ToggleToolMode()
        {
            toolManager?.ToggleToolMode();
        }

        public void HookModeToggleButton(Button button)
        {
            button.Click += (s, e) => ToggleToolMode();
            UpdateButtonText(button, toolManager.CurrentTool);
        }

        private void OnToolChanged(ToolManager.ToolMode mode)
        {
            UpdateCursor(mode);
            if (viewer.ModeToggleButton != null)
                UpdateButtonText(viewer.ModeToggleButton, mode);
        }

        private void UpdateCursor(ToolManager.ToolMode mode)
        {
            viewer.Cursor = (mode == ToolManager.ToolMode.Zoom)
                ? Cursors.Cross
                : Cursors.Hand;
        }

        public void ResetToZoomMode()
        {
            if (toolManager.CurrentTool != ToolManager.ToolMode.Zoom)
                toolManager.ToggleToolMode(); // Switch back to Zoom if not already
        }

        private void UpdateButtonText(Button button, ToolManager.ToolMode mode)
        {
            button.Text = (mode == ToolManager.ToolMode.Zoom)
                ? "\uE8A4 Zoom Mode"
                : "\uE762 Pan Mode";
        }

        public void OnMouseDown(MouseEventArgs e) => toolManager?.OnMouseDown(e);
        public void OnMouseMove(MouseEventArgs e) => toolManager?.OnMouseMove(e);
        public void OnMouseUp(MouseEventArgs e) => toolManager?.OnMouseUp(e);

        internal void ResetZoom()
        {
            currentZoom = new ZoomRegion
            {
                TimeStart = 0f,
                TimeEnd = viewer.CurrentFileTimespan,
                ViewportTopNorm = 0f,
                ViewportHeightNorm = 1f
            };
        }

        internal void ResetVerticalZooms()
        {
            foreach (var channel in viewer.Channels)
            {
                channel.ZoomedVoltageMin = null;
                channel.ZoomedVoltageMax = null;
            }
        }

        internal ZoomRegion GetEffectiveZoom()
        {
            if (currentZoom == null || currentZoom.IsEmpty)
            {
                return new ZoomRegion
                {
                    TimeStart = 0,
                    TimeEnd = viewer.CurrentFileTimespan,  // 👈 full timespan
                    ViewportTopNorm = 0,
                    ViewportHeightNorm = 1
                };
            }

            return currentZoom;
        }
    }
}
