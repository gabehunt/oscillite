using System;
using System.Diagnostics;
using System.Windows.Forms;
using Oscillite.Utilities;
using SharpDX;

namespace Oscillite.Input
{
    public class WaveformInputHandler
    {
        private readonly WaveformViewer viewer;
        private DateTime lastAxisDragUpdate = DateTime.MinValue;
        private readonly TimeSpan axisDragThrottle = TimeSpan.FromMilliseconds(8);
        private float dragAxisStartVoltage;
        private float dragStartOffset;
        private float dragStartY;
        private ViewportTransform dragAxisTransform;
        private float dragAxisStartOffset;
        private float dragZoomStartMin;
        private float dragZoomStartMax;
        private float timeRulerDragStartX;
        private float timeRulerStartTime;
        private float phaseRulerStartTime;
        private DateTime lastCursorUpdate = DateTime.MinValue;
        private readonly TimeSpan cursorUpdateInterval = TimeSpan.FromMilliseconds(100);

        public WaveformInputHandler(WaveformViewer viewer)
        {
            this.viewer = viewer;
        }

        public void HandleMouseDown(MouseEventArgs e)
        {
            var mousePos = new Point(e.X, e.Y);

            if (e.Button == MouseButtons.Right)
            {
                viewer.ResetZoom();
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            if (TryHandleCursorPanelDrag(mousePos)) return;
            if (TryHandleVoltageRulerDrag(mousePos)) return;
            if (TryHandleAxisDrag(mousePos)) return;
            if (TryHandlePhaseRulerDrag(e)) return;
            if (TryHandleTimeRulerDrag(e)) return;

            viewer.ToolController?.OnMouseDown(e);
        }

        private bool TryHandleCursorPanelDrag(Point mousePos)
        {
            if (viewer.CursorPanel.HitTest(mousePos))
            {
                viewer.CursorPanel.OnMouseDown(mousePos);
                viewer.Invalidate();
                return true;
            }
            return false;
        }

        private bool TryHandleVoltageRulerDrag(Point mousePos)
        {
            var area = viewer.DrawingArea;
            float currentX = viewer.Width - 60; // RIGHT_MARGIN

            for (int i = 0; i < viewer.Channels.Count; i++)
            {
                if (!viewer.Channels[i].Visible)
                    continue;

                var rulers = viewer.Channels[i].Rulers;
                var transform = Helpers.CreateTransformForChannel(viewer.GetWorldBounds(viewer.Channels[i]), area);

                for (int r = 0; r < rulers.Count; r++)
                {
                    float screenY = transform.WorldToScreenY(rulers[r].Voltage);
                    if (!viewer.IsZoomedIn())
                        screenY += viewer.Channels[i].Offset;

                    float clampedY = Math.Max(area.Top + 1, screenY);
                    var handleBox = new RectangleF(currentX + 6, clampedY - 10, 50, 20);

                    if (handleBox.Contains(new Vector2(mousePos.X, mousePos.Y)))
                    {
                        StartDraggingRuler(i, r, mousePos.Y);
                        return true;
                    }
                }

                currentX -= 60; // RIGHT_MARGIN
            }

            return false;
        }

        private bool TryHandleAxisDrag(Point mousePos)
        {
            int index = viewer.HitTestYAxis(mousePos);
            if (index == -1) return false;

            BeginAxisDrag(index, mousePos.Y);
            return true;
        }

        private bool TryHandlePhaseRulerDrag(MouseEventArgs e)
        {
            if (!viewer.ShowPhaseRulers) return false;

            var zoom = viewer.GetEffectiveZoom();
            var transform = Helpers.CreateTimeTransform(zoom, viewer.DrawingArea);
            float[] times = { viewer.PhaseRuler1.Time, viewer.PhaseRuler2.Time };

            for (int i = 0; i < 2; i++)
            {
                float x = transform.WorldToScreenX(times[i] * viewer.CurrentFileTimespan);
                var rect = new RectangleF(x - 30, viewer.DrawingArea.Bottom + 30, 60, 20);
                if (rect.Contains(new Vector2(e.X, e.Y)))
                {
                    StartDraggingPhaseRuler(i, transform.ScreenToWorldX(e.X));
                    return true;
                }
            }

            return false;
        }

        private bool TryHandleTimeRulerDrag(MouseEventArgs e)
        {
            var zoom = viewer.GetEffectiveZoom();
            var transform = Helpers.CreateTimeTransform(zoom, viewer.DrawingArea);
            var area = viewer.DrawingArea;

            for (int i = 0; i < viewer.TimeRulers.Count; i++)
            {
                var ruler = viewer.TimeRulers[i];
                if (!ruler.Active)
                    continue;

                float absoluteTime = ruler.Time * viewer.CurrentFileTimespan;
                float screenX;
                float startDragTime; // NEW

                bool isInsideZoom = absoluteTime >= zoom.TimeStart && absoluteTime <= zoom.TimeEnd;

                if (isInsideZoom)
                {
                    screenX = transform.WorldToScreenX(absoluteTime);
                    startDragTime = ruler.Time; // use real normalized time
                }
                else
                {
                    if (absoluteTime < zoom.TimeStart)
                    {
                        screenX = area.Left;
                        startDragTime = zoom.TimeStart / viewer.CurrentFileTimespan; // normalize it
                    }
                    else
                    {
                        screenX = area.Right;
                        startDragTime = zoom.TimeEnd / viewer.CurrentFileTimespan; // normalize it
                    }
                }

                var handleBox = new RectangleF(screenX - 30, area.Bottom + 5, 60, 20);

                if (handleBox.Contains(new Vector2(e.X, e.Y)))
                {
                    StartDraggingTimeRuler(i, e.X, startDragTime); // 👈 use adjusted start time
                    return true;
                }
            }

            return false;
        }

        internal void BeginAxisDrag(int channelIndex, float mouseY)
        {
            var channel = viewer.Channels[channelIndex];
            viewer.DragState.IsDraggingAxis = true;
            viewer.DragState.DraggedAxisIndex = channelIndex;
            dragStartY = mouseY;
            dragStartOffset = viewer.IsZoomedIn() ? 0 : channel.Offset;
            viewer.Cursor = Cursors.SizeNS;

            if (viewer.IsZoomedIn())
            {
                dragAxisTransform = Helpers.CreateTransformForChannel(viewer.GetWorldBounds(channel), viewer.DrawingArea);
                dragAxisStartVoltage = dragAxisTransform.ScreenToWorldY(mouseY);
                dragZoomStartMin = channel.ZoomedVoltageMin ?? 0f;
                dragZoomStartMax = channel.ZoomedVoltageMax ?? 0f;
                dragAxisStartOffset = channel.Offset;
            }
        }


        public void HandleMouseMove(MouseEventArgs e)
        {
            var pos = new Point(e.X, e.Y);
            viewer.ToolController?.OnMouseMove(e);

            if (viewer.CursorPanel.IsDragging)
            {
                viewer.Cursor = Cursors.SizeAll;
                viewer.CursorPanel.OnMouseMove(pos);
                viewer.Invalidate();
                return;
            }
            else if (viewer.CursorPanel.HitTest(pos))
            {
                viewer.Cursor = Cursors.SizeAll;
                return;
            }

            if (viewer.IsDraggingVoltageRuler)
            {
                ContinueDraggingRuler(e);
                return;
            }

            if (viewer.IsDraggingPhaseRuler)
            {
                ContinueDraggingPhaseRuler(e);
                return;
            }

            if (viewer.IsDraggingTimeRuler)
            {
                ContinueDraggingTimeRuler(e);
                return;
            }

            if (viewer.IsDraggingVoltageAxis)
            {
                ContinueDraggingAxis(e);
                return;
            }

            // Channel offset dragging
            if (viewer.IsDraggingChannel)
            {
                ContinueDraggingChannel(e);
                return;
            }

            UpdateCursorHover(pos);
        }

        public void HandleMouseUp(MouseEventArgs e)
        {
            var pos = new Point(e.X, e.Y);
            viewer.ToolController?.OnMouseUp(e);

            viewer.CursorPanel.OnMouseUp(pos);

            if (viewer.IsDraggingVoltageRuler)
            {
                EndDraggingRuler();
                return;
            }

            if (viewer.IsDraggingPhaseRuler)
            {
                EndDraggingPhaseRuler();
                return;
            }

            if (viewer.IsDraggingTimeRuler)
            {
                EndDraggingTimeRuler();
                return;
            }

            if (viewer.IsDraggingVoltageAxis)
            {
                EndDraggingAxis();
                return;
            }

            if (viewer.IsDraggingChannel)
            {
                EndDraggingChannel();
                return;
            }

            viewer.Cursor = Cursors.Default;
        }

        internal void ContinueDraggingRuler(MouseEventArgs e) => DragRuler(e);
        internal void ContinueDraggingAxis(MouseEventArgs e)
        {
            var now = DateTime.Now;
            if (now - lastAxisDragUpdate < axisDragThrottle)
                return;

            lastAxisDragUpdate = now;

            var channel = viewer.Channels[viewer.DragState.DraggedAxisIndex];
            float deltaY = e.Y - dragStartY;

            var transform = Helpers.CreateTransformForChannel(viewer.GetWorldBounds(channel), viewer.DrawingArea);

            if (viewer.IsZoomedIn())
            {
                float voltsNow = dragAxisTransform.ScreenToWorldY(e.Y);
                float deltaVolts = dragAxisStartVoltage - voltsNow;

                if (channel.ZoomedVoltageMin.HasValue && channel.ZoomedVoltageMax.HasValue)
                {
                    float vMin = dragZoomStartMin + deltaVolts;
                    float vMax = dragZoomStartMax + deltaVolts;
                    channel.ZoomedVoltageMin = vMin;
                    channel.ZoomedVoltageMax = vMax;

                    float baseRange = channel.EffectiveVoltsPerDivision * WaveformViewer.DIVISIONS_Y;
                    float pixelsPerVolt = viewer.DrawingArea.Height / baseRange;
                    float newOffset = dragStartOffset + (deltaVolts * pixelsPerVolt);
                    channel.Offset = dragAxisStartOffset + newOffset;
                }
            }
            else
            {
                float voltsStart = transform.ScreenToWorldY(dragStartY);
                float voltsNow = transform.ScreenToWorldY(e.Y);
                float deltaVolts = voltsStart - voltsNow;

                float baseRange = channel.EffectiveVoltsPerDivision * WaveformViewer.DIVISIONS_Y;
                float pixelsPerVolt = viewer.DrawingArea.Height / baseRange;

                float newOffset = dragStartOffset + (deltaVolts * pixelsPerVolt);
                channel.Offset = newOffset;
            }

            viewer.Invalidate();
        }

        internal void ContinueDraggingTimeRuler(MouseEventArgs e) => DragTimeRuler(e);
        internal void ContinueDraggingPhaseRuler(MouseEventArgs e) => DragPhaseRuler(e);
        internal void ContinueDraggingChannel(MouseEventArgs e)
        {
            float deltaY = e.Y - viewer.DragState.LastMousePosition.Y;
            viewer.Channels[viewer.DragState.DragChannelIndex].Offset += deltaY;
            viewer.DragState.LastMousePosition = new Point(e.X, e.Y);
            viewer.Invalidate();
        }

        internal void EndDraggingRuler()
        {
            viewer.DragState.DraggingVoltageRuler = null;
            viewer.Cursor = Cursors.Default;
            viewer.Invalidate();
        }

        internal void EndDraggingPhaseRuler()
        {
            viewer.DragState.DraggingPhaseRuler = null;
            viewer.Cursor = Cursors.Default;
            viewer.Invalidate();
        }

        internal void EndDraggingTimeRuler()
        {
            viewer.DragState.DraggingTimeRuler = null;
            viewer.Cursor = Cursors.Default;
            viewer.Invalidate();
        }

        internal void EndDraggingAxis()
        {
            viewer.DragState.IsDraggingAxis = false;
            viewer.DragState.DraggedAxisIndex = -1;
            viewer.Cursor = Cursors.Default;
            viewer.Invalidate();
        }

        internal void EndDraggingChannel()
        {
            viewer.DragState.IsDraggingChannel = false;
            viewer.DragState.DragChannelIndex = -1;
            viewer.Cursor = Cursors.Default;
            viewer.Invalidate();
        }

        internal void StartDraggingPhaseRuler(int rulerIndex, float worldTime)
        {
            viewer.DragState.DraggingPhaseRuler = rulerIndex;
            phaseRulerStartTime = worldTime;
            viewer.Cursor = Cursors.SizeWE;
        }

        internal void StartDraggingTimeRuler(int index, float startX, float startTimeNorm)
        {
            viewer.DragState.DraggingTimeRuler = index;
            timeRulerDragStartX = startX;
            timeRulerStartTime = startTimeNorm;
            viewer.Cursor = Cursors.SizeWE;
        }

        internal void StartDraggingRuler(int channelIndex, int rulerIndex, float mouseY)
        {
            viewer.DragState.DraggingVoltageRuler = (channelIndex, rulerIndex);
            viewer.DragState.RulerDragStartY = mouseY;
            viewer.Cursor = Cursors.SizeNS;
        }


        private void DragRuler(MouseEventArgs e)
        {
            if (viewer.DragState.DraggingVoltageRuler == null)
                return;

            int i = viewer.DragState.DraggingVoltageRuler.Value.channelIndex;
            int r = viewer.DragState.DraggingVoltageRuler.Value.rulerIndex;

            var channel = viewer.Channels[i];
            var ruler = channel.Rulers[r];

            var transform = Helpers.CreateTransformForChannel(viewer.GetWorldBounds(channel), viewer.DrawingArea);

            (float vMin, float vMax) = channel.GetVisibleVoltageRange();

            float pixelsPerVolt = viewer.DrawingArea.Height / (vMin - vMax);

            float deltaY = e.Y - viewer.DragState.RulerDragStartY;

            float newVoltage;

            if (viewer.IsZoomedIn())
            {
                newVoltage = transform.ScreenToWorldY(e.Y);
            }
            else
            {
                float adjustedY = e.Y - channel.Offset;
                newVoltage = transform.ScreenToWorldY(adjustedY);
            }
            ruler.Voltage = newVoltage;

            float screenY = transform.WorldToScreenY(newVoltage);
            ruler.IsAtTop = Math.Abs(screenY - (viewer.DrawingArea.Top + 1)) < 0.01f;

            if ((DateTime.Now - lastCursorUpdate) > cursorUpdateInterval)
            {
                viewer.UpdateCursorPanel();
                lastCursorUpdate = DateTime.Now;
            }

            viewer.Invalidate();
        }

        private void DragTimeRuler(MouseEventArgs e)
        {
            if (viewer.DragState.DraggingTimeRuler == null) return;

            int index = viewer.DragState.DraggingTimeRuler.Value;
            var zoom = viewer.GetEffectiveZoom();
            var transform = Helpers.CreateTimeTransform(zoom, viewer.DrawingArea);

            float zoomStartSeconds = zoom.Left * viewer.CurrentFileTimespan;
            float zoomEndSeconds = (zoom.Left + zoom.Width) * viewer.CurrentFileTimespan;
            float zoomTimeSpan = zoomEndSeconds - zoomStartSeconds;

            float startX = timeRulerDragStartX;
            float currentX = e.X;

            float startTime = transform.ScreenToWorldX(startX);
            float currentTime = transform.ScreenToWorldX(currentX);
            float deltaTime = currentTime - startTime;

            float newAbsoluteTime = (timeRulerStartTime * viewer.CurrentFileTimespan) + deltaTime;

            // Convert back to normalized time over full 10s range
            viewer.TimeRulers[index].Time = viewer.ClampTime(newAbsoluteTime / viewer.CurrentFileTimespan);

            if ((DateTime.Now - lastCursorUpdate) > cursorUpdateInterval)
            {
                viewer.UpdateCursorPanel();
                lastCursorUpdate = DateTime.Now;
            }

            viewer.Invalidate();
        }

        private void DragPhaseRuler(MouseEventArgs e)
        {
            if (viewer.DragState.DraggingPhaseRuler == null) return;

            var zoom = viewer.GetEffectiveZoom();
            var transform = Helpers.CreateTimeTransform(zoom, viewer.DrawingArea);

            // Get current time under mouse
            float currentTime = transform.ScreenToWorldX(e.X);

            // Get delta time in world units
            float deltaTime = currentTime - phaseRulerStartTime;

            // Add that to whichever ruler was selected
            if (viewer.DragState.DraggingPhaseRuler == 0)
                viewer.PhaseRuler1.Time = viewer.ClampTime(viewer.PhaseRuler1.Time + deltaTime / viewer.CurrentFileTimespan);
            else
                viewer.PhaseRuler2.Time = viewer.ClampTime(viewer.PhaseRuler2.Time + deltaTime / viewer.CurrentFileTimespan);

            // Update reference point for next move
            phaseRulerStartTime = currentTime;

            viewer.Invalidate();
        }

        private void UpdateCursorHover(Point mousePosition)
        {
            if (viewer.HitTestYAxis(mousePosition) != -1)
            {
                viewer.Cursor = Cursors.SizeNS;
                return;
            }

            var area = viewer.DrawingArea;
            if (mousePosition.X >= area.Left && mousePosition.X <= area.Right &&
                mousePosition.Y >= area.Top && mousePosition.Y <= area.Bottom)
            {
                viewer.Cursor = (viewer.ToolController?.CurrentToolMode == ToolManager.ToolMode.Zoom)
                    ? Cursors.Cross
                    : Cursors.Hand;
            }
            else
            {
                viewer.Cursor = Cursors.Default;
            }
        }
    }
}