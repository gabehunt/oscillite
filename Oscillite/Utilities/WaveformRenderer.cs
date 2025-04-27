using System;

namespace Oscillite.Utilities
{
    using System.Diagnostics;
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.Mathematics.Interop;

    namespace Oscillite
    {
        public class WaveformRenderer
        {
            public void Render(WaveformRenderContext context)
            {
                if (context.RenderTarget == null) return;

                var rt = context.RenderTarget;
                rt.BeginDraw();
                rt.Clear(new RawColor4(0, 0, 0, 1));

                DrawGrid(context);

                // ⛔ Start Clip Region for Zoomed Waveform Rendering
                if (!context.Zoom.IsEmpty)
                {
                    rt.PushAxisAlignedClip(context.DrawingArea, AntialiasMode.Aliased);
                }

                DrawWaveforms(context);

                if (!context.Zoom.IsEmpty)
                {
                    rt.PopAxisAlignedClip();
                }

                DrawZoomSelection(context);
                DrawPhaseRulers(context);
                DrawAxes(context);
                DrawRulers(context);

                if (context.CursorOverlay != null)
                {
                    context.CursorOverlay.Draw(
                        context.RenderTarget,
                        context.D2DFactory,
                        context.TextFormat);
                }

                rt.EndDraw();
            }

            private void DrawZoomSelection(WaveformRenderContext context)
            {
                if (!context.IsZoomingSelectionActive)
                    return;

                using (var selectionBrush = new SolidColorBrush(context.RenderTarget, new RawColor4(1f, 1f, 1f, 0.3f)))
                {
                    float x1 = Math.Min(context.ZoomStart.X, context.ZoomEnd.X);
                    float x2 = Math.Max(context.ZoomStart.X, context.ZoomEnd.X);
                    float y1 = Math.Min(context.ZoomStart.Y, context.ZoomEnd.Y);
                    float y2 = Math.Max(context.ZoomStart.Y, context.ZoomEnd.Y);

                    var rect = new RawRectangleF(x1, y1, x2, y2);

                    context.RenderTarget.FillRectangle(rect, selectionBrush);
                    context.RenderTarget.DrawRectangle(rect, selectionBrush, 1.0f);
                }
            }


            private void DrawGrid(WaveformRenderContext context)
            {
                var rt = context.RenderTarget;
                var area = context.DrawingArea;

                const int divisionsX = 10;
                const int divisionsY = 8;

                float xSpacing = area.Width / divisionsX;
                float ySpacing = area.Height / divisionsY;

                using (var gridBrush = new SolidColorBrush(rt, new RawColor4(0.2f, 0.2f, 0.2f, 1)))
                using (var gridStyle = new StrokeStyle(context.D2DFactory, new StrokeStyleProperties { DashStyle = DashStyle.Solid }))
                {
                    for (int i = 0; i <= divisionsX; i++)
                    {
                        float x = area.Left + i * xSpacing;
                        rt.DrawLine(
                            new RawVector2(x, area.Top),
                            new RawVector2(x, area.Bottom),
                            gridBrush, 0.5f, gridStyle);
                    }

                    for (int i = 0; i <= divisionsY; i++)
                    {
                        float y = area.Top + i * ySpacing;
                        rt.DrawLine(
                            new RawVector2(area.Left, y),
                            new RawVector2(area.Right, y),
                            gridBrush, 0.5f, gridStyle);
                    }
                }
            }


            private void DrawWaveforms(WaveformRenderContext context)
            {
                var rt = context.RenderTarget;
                var zoom = context.Zoom;
                var area = context.DrawingArea;
                float fileDuration = context.FileTimespan;
                bool isZoomedIn = context.IsZoomedIn;

                for (int i = 0; i < context.Channels.Count; i++)
                {
                    var channel = context.Channels[i];
                    if (!channel.Visible || channel.Data == null || channel.Data.Length < 2)
                        continue;

                    var data = channel.Data;
                    (float vMin, float vMax) = channel.GetVisibleVoltageRange();

                    float tMin = zoom.TimeStart;
                    float tMax = zoom.TimeEnd;

                    var world = new RectangleF(tMin, vMin, zoom.TimeSpan, vMax - vMin);
                    Debug.WriteLine($"World: ({tMin}, {vMin}) to ({tMax}, {vMax})");
                    Debug.WriteLine($"Area: ({area.Left},{area.Top}) - ({area.Width},{area.Height})");
                    var transform = new ViewportTransform(world, area);

                    using (var geometry = new PathGeometry(context.D2DFactory))
                    using (var sink = geometry.Open())
                    {
                        Vector2 first = data[0];
                        Vector2 screenFirst = transform.WorldToScreen(first);

                        Debug.WriteLine($"First World point: {first}");
                        Debug.WriteLine($"First Screen point: {screenFirst}");

                        if (!isZoomedIn)
                            screenFirst.Y += channel.Offset;

                        sink.BeginFigure(new RawVector2(screenFirst.X, screenFirst.Y), FigureBegin.Hollow);

                        for (int j = 0; j < data.Length; j++)
                        {
                            Vector2 pt = transform.WorldToScreen(data[j]);
                            if (!isZoomedIn)
                                pt.Y += channel.Offset;

                            sink.AddLine(new RawVector2(pt.X, pt.Y));

                            if(j < 20)
                            {
                                Debug.WriteLine($"Channel: {i+1}, pt.X: {pt.X}, pt.Y: {pt.Y}");
                            }
                        }

                        sink.EndFigure(FigureEnd.Open);
                        sink.Close();

                        var bounds = geometry.GetBounds();
                        Console.WriteLine($"Channel {i + 1}: Geometry Bounds = {bounds.Left},{bounds.Top} to {bounds.Right},{bounds.Bottom}");

                        rt.DrawGeometry(geometry, channel.Brush, 1.0f);
                    }
                }
            }


            private void DrawAxes(WaveformRenderContext context)
            {
                var rt = context.RenderTarget;
                var area = context.DrawingArea;
                var channels = context.Channels;

                using (var axisBrush = new SolidColorBrush(rt, new RawColor4(1f, 1f, 1f, 1f)))
                {
                    DrawTimeAxis(context, axisBrush);
                    DrawTimeRulers(context, axisBrush);

                    float currentX = context.RenderTarget.Size.Width - 60; // Start at the right edge with some margin

                    for (int i = 0; i < channels.Count; i++)
                    {
                        var channel = channels[i];
                        if (!channel.Visible)
                            continue;

                        DrawVoltageAxis(context, channel, currentX, axisBrush);
                        currentX -= 60; // Space between channels
                    }
                }
            }
            private void DrawTimeRulers(WaveformRenderContext context, SolidColorBrush brush)
            {
                var rt = context.RenderTarget;
                var area = context.DrawingArea;
                var zoom = context.Zoom;
                var transform = new ViewportTransform(new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1), area);

                using (var dashed = new StrokeStyle(context.D2DFactory, new StrokeStyleProperties { DashStyle = DashStyle.Dash }))
                {
                    for (int i = 0; i < context.TimeRulers.Count; i++)
                    {
                        var ruler = context.TimeRulers[i];
                        if (!ruler.Active)
                            continue;

                        float absoluteTime = ruler.Time * context.FileTimespan;
                        bool isInsideZoom = absoluteTime >= zoom.TimeStart && absoluteTime <= zoom.TimeEnd;

                        float screenX = isInsideZoom
                            ? transform.WorldToScreenX(absoluteTime)
                            : (absoluteTime < zoom.TimeStart ? area.Left : area.Right);

                        if (screenX < area.Left - 1 || screenX > area.Right + 1)
                            continue;

                        // Draw the dashed line
                        rt.DrawLine(
                            new RawVector2(screenX, area.Top),
                            new RawVector2(screenX, area.Bottom),
                            brush, 1.0f, dashed);

                        // Phase angle detection
                        bool useDegrees = false;
                        float px1 = transform.WorldToScreenX(context.PhaseRuler1.Time * context.FileTimespan);
                        float px2 = transform.WorldToScreenX(context.PhaseRuler2.Time * context.FileTimespan);
                        if (px1 > px2) { float tmp = px1; px1 = px2; px2 = tmp; }
                        useDegrees = context.ShowPhaseRulers && (screenX >= px1 && screenX <= px2);

                        string label;
                        if (isInsideZoom)
                        {
                            label = useDegrees
                                ? string.Format("{0:F2}°", ((ruler.Time - context.PhaseRuler1.Time) / (context.PhaseRuler2.Time - context.PhaseRuler1.Time) * 720f))
                                : string.Format("{0:F3}s", absoluteTime);
                        }
                        else
                        {
                            label = "⤴ Drag In";
                        }

                        var layout = new TextLayout(context.WriteFactory, label, context.TextFormat, 60, 20);
                        var labelBox = new RawRectangleF(screenX - 30, area.Bottom + 5, screenX + 30, area.Bottom + 25);

                        // Background for label
                        using (var bgBrush = new SolidColorBrush(rt, isInsideZoom
                            ? new RawColor4(0f, 0f, 0f, 1f)
                            : new RawColor4(0.2f, 0.2f, 0.2f, 0.7f))) // Ghost ones a little transparent
                        {
                            rt.FillRectangle(labelBox, bgBrush);
                        }

                        rt.DrawRectangle(labelBox, brush, 1.0f);
                        rt.DrawTextLayout(new RawVector2(labelBox.Left + 5, labelBox.Top + 2), layout, brush);

                        layout.Dispose();
                    }
                }
            }


            private void DrawTimeAxis(WaveformRenderContext context, SolidColorBrush brush)
            {
                var rt = context.RenderTarget;
                var zoom = context.Zoom;
                var area = context.DrawingArea;
                float y = area.Bottom;

                var transform = new ViewportTransform(new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1), area);

                rt.DrawLine(
                    new RawVector2(area.Left, y),
                    new RawVector2(area.Right, y),
                    brush, 1.0f);

                for (int i = 0; i <= 10; i++)
                {
                    float time = zoom.TimeStart + (i / 10f) * zoom.TimeSpan;
                    float x = transform.WorldToScreenX(time);

                    rt.DrawLine(new RawVector2(x, y - 5), new RawVector2(x, y + 5), brush, 1.0f);

                    string label = time.ToString("F2") + "s";
                    var layout = new TextLayout(context.WriteFactory, label, context.TextFormat, 60, 20);

                    rt.DrawTextLayout(new RawVector2(x - 25, y + 5), layout, brush);
                    layout.Dispose();
                }
            }

            private void DrawVoltageAxis(WaveformRenderContext context, WaveformChannel channel, float x, SolidColorBrush axisBrush)
            {
                var rt = context.RenderTarget;
                var area = context.DrawingArea;
                var transform = GetTransform(context, channel); // Uses Helpers.CreateTransformForChannel(...)
                bool isZoomedIn = context.IsZoomedIn;

                using (var channelBrush = new SolidColorBrush(rt, channel.Color))
                {
                    // Draw vertical axis line
                    rt.DrawLine(
                        new RawVector2(x, area.Top),
                        new RawVector2(x, area.Bottom),
                        channelBrush, 1.0f);

                    // Tick marks + labels
                    var (vMin, vMax) = channel.GetVisibleVoltageRange();
                    float step = (vMax - vMin) / 8f;

                    for (int i = 0; i <= 8; i++)
                    {
                        float v = vMax - i * step;
                        float y = transform.WorldToScreenY(v);
                        if (!isZoomedIn) y += channel.Offset;

                        // Tick
                        rt.DrawLine(new RawVector2(x - 5, y), new RawVector2(x + 5, y), channelBrush, 1.0f);

                        // Label
                        string label =$"{v:F3} {channel.Unit}";
                        var layout = new TextLayout(context.WriteFactory, label, context.TextFormat, 50, 20);

                        rt.DrawTextLayout(new RawVector2(x + 6, y - 10), layout, channelBrush);
                        layout.Dispose();
                    }

                    //// Draw channel name (e.g., "CH1") at the top
                    //var chLabel = channel.Name;
                    //var nameLayout = new TextLayout(context.WriteFactory, chLabel, context.TextFormat, 50, 20);
                    //rt.DrawTextLayout(new RawVector2(x + 5, area.Top - 25), nameLayout, channelBrush);
                    //nameLayout.Dispose();
                }
            }

            private void DrawPhaseRulers(WaveformRenderContext context)
            {
                if (!context.ShowPhaseRulers)
                    return;

                var rt = context.RenderTarget;
                var area = context.DrawingArea;
                var zoom = context.Zoom;
                var transform = new ViewportTransform(
                    new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1),
                    area);

                float x1 = transform.WorldToScreenX(context.PhaseRuler1.Time * context.FileTimespan);
                float x2 = transform.WorldToScreenX(context.PhaseRuler2.Time * context.FileTimespan);

                if (x1 > x2)
                {
                    float tmp = x1;
                    x1 = x2;
                    x2 = tmp;
                }

                float segmentWidth = (x2 - x1) / 4f;

                using (var phaseBrush = new SolidColorBrush(rt, new RawColor4(0.4f, 0.8f, 1f, 0.25f)))
                using (var edgeBrush = new SolidColorBrush(rt, new RawColor4(0.4f, 0.8f, 1f, 0.8f)))
                using (var backgroundBrush = new SolidColorBrush(rt, new RawColor4(0.4f, 0.8f, 1f, 0.25f)))
                using (var borderBrush = new SolidColorBrush(rt, new RawColor4(0.4f, 0.8f, 1f, 0.8f)))
                using (var textBrush = new SolidColorBrush(rt, new RawColor4(1f, 1f, 1f, 1f)))
                {
                    // Alternate shaded segments
                    rt.FillRectangle(new RawRectangleF(x1, area.Top, x1 + segmentWidth, area.Bottom), phaseBrush);
                    rt.FillRectangle(new RawRectangleF(x1 + 2 * segmentWidth, area.Top, x1 + 3 * segmentWidth, area.Bottom), phaseBrush);

                    // Left and right edge lines
                    rt.DrawLine(new RawVector2(x1, area.Top), new RawVector2(x1, area.Bottom), edgeBrush, 1.0f);
                    rt.DrawLine(new RawVector2(x2, area.Top), new RawVector2(x2, area.Bottom), edgeBrush, 1.0f);

                    // Drag handles and labels
                    float[] positions = { x1, x2 };
                    string[] labels = { "0°", "720°" };

                    for (int i = 0; i < 2; i++)
                    {
                        float x = positions[i];
                        var rect = new RawRectangleF(x - 30, area.Bottom + 30, x + 30, area.Bottom + 50);

                        rt.FillRectangle(rect, backgroundBrush);
                        rt.DrawRectangle(rect, borderBrush, 1.0f);

                        using (var layout = new TextLayout(context.WriteFactory, labels[i], context.TextFormat, 60, 20))
                        {
                            rt.DrawTextLayout(new RawVector2(x - 15, area.Bottom + 30), layout, textBrush);
                        }
                    }
                }
            }

            private void DrawRulers(WaveformRenderContext context)
            {
                using (var axisBrush = new SolidColorBrush(context.RenderTarget, new RawColor4(1f, 1f, 1f, 1f)))
                {
                    DrawTimeRulers(context, axisBrush);
                    DrawVoltageRulers(context);
                }
            }

            private ViewportTransform GetTransform(WaveformRenderContext context, WaveformChannel channel)
            {
                var zoom = context.Zoom;
                float timeStart = zoom.TimeStart;
                float timeSpan = zoom.TimeSpan;

                var range = channel.GetVisibleVoltageRange();
                float vMin = range.Min;
                float vMax = range.Max;

                var world = new RectangleF(timeStart, vMin, timeSpan, vMax - vMin);
                return Helpers.CreateTransformForChannel(world, context.DrawingArea);
            }

            private void DrawVoltageRulers(WaveformRenderContext context)
            {
                var rt = context.RenderTarget;
                var area = context.DrawingArea;
                bool isZoomedIn = context.IsZoomedIn;

                float axisX = context.RenderTarget.Size.Width - 60;

                foreach (var channel in context.Channels)
                {
                    if (!channel.Visible) continue;

                    var rulers = channel.Rulers;
                    var transform = GetTransform(context, channel);

                    using (var brush = new SolidColorBrush(rt, channel.Color))
                    using (var dashed = new StrokeStyle(context.D2DFactory, new StrokeStyleProperties { DashStyle = DashStyle.Dash }))
                    {
                        foreach (var ruler in rulers)
                        {
                            if (!ruler.Active) continue;

                            float y = transform.WorldToScreenY(ruler.Voltage);
                            if (!isZoomedIn) y += channel.Offset;

                            bool isDocked = y <= area.Top + 1;
                            if (isDocked) y = area.Top + 1;

                            if (!isDocked)
                            {
                                rt.DrawLine(
                                    new RawVector2(area.Left, y),
                                    new RawVector2(area.Right, y),
                                    brush, 1.0f, dashed);
                            }

                            string label = $"{ruler.Voltage:F3}";
                            var layout = new TextLayout(context.WriteFactory, label, context.TextFormat, 50, 20);
                            var labelBox = new RawRectangleF(axisX + 6, y - 10, axisX + 56, y + 10);

                            using (var bg = new SolidColorBrush(rt, new RawColor4(0f, 0f, 0f, 1f)))
                            {
                                rt.FillRectangle(labelBox, bg);
                            }

                            rt.DrawRectangle(labelBox, brush, 1);
                            rt.DrawTextLayout(new RawVector2(labelBox.Left + 2, labelBox.Top + 2), layout, brush);
                            layout.Dispose();
                        }
                    }

                    axisX -= 60;
                }
            }

        }
    }

}
