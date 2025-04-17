using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Snapon.Scope.Common.Library;
using Factory = SharpDX.Direct2D1.Factory;

namespace Oscillite
{
    public partial class WaveformViewer : UserControl
    {
        private RenderTarget d2dRenderTarget;
        private Factory d2dFactory;
        private SwapChain swapChain;
        private List<WaveformChannel> channels;
        private bool isDragging;
        private int dragChannelIndex;
        private SharpDX.Point lastMousePosition;
        private int CHANNEL_COUNT = 4;
        private List<Axis> yAxes = new List<Axis>();
        private SharpDX.DirectWrite.Factory writeFactory;
        private TextFormat textFormat;
        private bool isDraggingAxis = false;
        private int draggedAxisIndex = -1;
        private float dragStartY;
        private float dragStartOffset;
        private const float LEFT_MARGIN = 50f;    // Space for left labels
        private const float RIGHT_MARGIN = 60f;   // Space for each Y-axis
        private const float TOP_MARGIN = 10f;     // Space for top labels
        private const float BOTTOM_MARGIN = 60f;  // Space for time axis
        private const int DIVISIONS_Y = 8;
        private const int DIVISIONS_X = 10;
        private const float DEFAULT_TIME_SPAN = 10.0f;
        private bool isZoomSelecting = false;
        private Point zoomStartPoint;
        private Point zoomEndPoint;
        private ZoomRegion currentZoom = null; // current zoom state
        private (int channelIndex, int rulerIndex)? draggingRuler = null;
        private float rulerDragStartY;
        private float rulerStartVoltage;
        private FlowLayoutPanel channelPanel = new FlowLayoutPanel();
        private List<TimeRuler> timeRulers = new List<TimeRuler>
        {
            new TimeRuler { Time = 0.3f },
            new TimeRuler { Time = 0.7f }
        };
        private int? draggingTimeRuler = null;
        private float timeRulerDragStartX;
        private float timeRulerStartTime;
        private CursorSummaryPanel cursorPanel;
        private DateTime lastCursorUpdate = DateTime.MinValue;
        private readonly TimeSpan cursorUpdateInterval = TimeSpan.FromMilliseconds(100); // 10 FPS is smooth
        private bool showPhaseRulers = false;
        private TimeRuler phaseRuler1 = new TimeRuler { Time = 0.2f }; // normalized
        private TimeRuler phaseRuler2 = new TimeRuler { Time = 0.8f };
        private int? draggingPhaseRuler = null;
        private float phaseRulerDragStartX;
        private float phaseRulerStartTime;
        private bool isPanning = false;
        private Point panStartMousePos;
        private ZoomRegion panStartZoom;
        private enum ToolMode { Zoom, Pan }
        private ToolMode currentTool = ToolMode.Zoom;
        private Button modeToggleButton;
        public WaveformViewer()
        {
            InitializeComponent();

            // Tell Windows Forms not to handle any drawing
            SetStyle(
                ControlStyles.Opaque |          // Don't draw background
                ControlStyles.UserPaint |       // We'll handle all painting
                ControlStyles.AllPaintingInWmPaint, // No background painting
                true);

            // Explicitly disable double buffering
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);

            var leftPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10),
                WrapContents = false,
                BackColor = System.Drawing.Color.Black
            };
            this.Controls.Add(leftPanel);

            // Load your embedded logo
            var logoPictureBox = new PictureBox
            {
                Size = new System.Drawing.Size(125, 125),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Helpers.LoadEmbeddedImage("Oscillite.Oscillite.png"), // match exact resource name
                Margin = new Padding(5),
                BackColor = System.Drawing.Color.Transparent
            };
            leftPanel.Controls.Add(logoPictureBox);


            var openButton = new Button
            {
                Size = new System.Drawing.Size(120, 40), // Make it big enough to fit both icon and text
                Text = "\uE8B7 Open File", // Folder icon + text ( is U+E8B7 in Segoe MDL2 Assets)
                Font = new System.Drawing.Font("Segoe MDL2 Assets", 12), // Use readable font
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                ForeColor = System.Drawing.Color.White,
                FlatAppearance = { BorderSize = 1, BorderColor = System.Drawing.Color.Gray },
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 10, 10, 10), // Adds space around it
                Anchor = AnchorStyles.None // Let FlowLayoutPanel center it
            };


            // Add event handler
            openButton.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Snap-on Scope Files|*.vsm;*.vss";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        ImportSnaponScopeFile(ofd.FileName);
                    }
                }
            };

            // Add to control
            leftPanel.Controls.Add(openButton);

            modeToggleButton = new Button
            {
                Size = new System.Drawing.Size(120, 40),
                Text = "\uE8A4 Zoom Mode", // U+E762 is the hand icon in Segoe MDL2 Assets
                Font = new System.Drawing.Font("Segoe MDL2 Assets", 12),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                ForeColor = System.Drawing.Color.White,
                FlatAppearance = { BorderSize = 1, BorderColor = System.Drawing.Color.Gray },
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 10, 10)
            };

            modeToggleButton.Click += (s, e) =>
            {
                if (currentTool == ToolMode.Zoom)
                {
                    currentTool = ToolMode.Pan;
                    modeToggleButton.Text = "\uE762 Pan Mode";
                    Cursor = Cursors.Hand;
                }
                else
                {
                    currentTool = ToolMode.Zoom;
                    modeToggleButton.Text = "\uE8A4 Zoom Mode"; // U+E8A4 is a magnifying glass icon
                    Cursor = Cursors.Cross;
                }
            };

            leftPanel.Controls.Add(modeToggleButton);

            var phaseButton = new Button
            {
                Size = new System.Drawing.Size(120, 40),
                Text = "\uE713 Phase Rulers", // U+E713 is the gear icon in Segoe MDL2 Assets
                Font = new System.Drawing.Font("Segoe MDL2 Assets", 12),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                ForeColor = System.Drawing.Color.White,
                FlatAppearance = { BorderSize = 1, BorderColor = System.Drawing.Color.Gray },
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 10, 10)
            };

            // Optional: Add click handler
            phaseButton.Click += (s, e) =>
            {
                showPhaseRulers = !showPhaseRulers;
                Invalidate(); // triggers re-draw
            };

            leftPanel.Controls.Add(phaseButton);

            channels = new List<WaveformChannel>();

            channelPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                
                Dock = DockStyle.Left,
                Width = 70,
                BackColor = System.Drawing.Color.Black,
                Padding = new Padding(5),
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,

            };
            leftPanel.Controls.Add(channelPanel);

            cursorPanel = new CursorSummaryPanel
            {
                Location = new System.Drawing.Point(200, 100),
                Anchor = AnchorStyles.None,
                Dock = DockStyle.None,
                Capture = true
            };
            this.Controls.Add(cursorPanel);

            InitializeDevice();
            InitializeChannels();
            SetDefaults();

            this.MouseDown += WaveformViewer_MouseDown;
            this.MouseMove += WaveformViewer_MouseMove;
            this.MouseUp += WaveformViewer_MouseUp;
            this.Resize += WaveformViewer_Resize;
        }

        private void SetDefaults()
        {
            currentTool = ToolMode.Zoom;
            modeToggleButton.Text = "\uE8A4 Zoom Mode"; // U+E8A4 is a magnifying glass icon
            Cursor = Cursors.Cross;

            currentZoom = new ZoomRegion();

            foreach (var ch in channels)
            {
                ch.ZoomedVoltageMin = null;
                ch.ZoomedVoltageMax = null;
                ch.Offset = 0f;
                ch.Scale = 1.0f;
                ch.Visible = true;
                ch.Rulers = new List<Ruler>
        {
            new Ruler { Voltage = 2.0f },
            new Ruler { Voltage = 2.0f }
        };
            }

            timeRulers = new List<TimeRuler>
    {
        new TimeRuler { Time = 0.3f },
        new TimeRuler { Time = 0.7f }
    };

            phaseRuler1 = new TimeRuler { Time = 0.2f };
            phaseRuler2 = new TimeRuler { Time = 0.8f };

            showPhaseRulers = false;
        }


        private RectangleF? drawingArea = null;

        private RectangleF DrawingArea
        {
            get
            {
                if (drawingArea.HasValue) return drawingArea.Value;

                float rightSpace = RIGHT_MARGIN * channels.Count(c => c.Visible);
                return new RectangleF(
                    LEFT_MARGIN,
                    TOP_MARGIN,
                    Width - (LEFT_MARGIN + rightSpace),
                    Height - (TOP_MARGIN + BOTTOM_MARGIN)
                );
            }
        }

        private void UpdateChannelCount()
        {
            CHANNEL_COUNT = channels.Count(c => c.Visible);
        }

        private int HitTestYAxis(SharpDX.Point mousePosition)
        {
            float currentX = Width - RIGHT_MARGIN;
            float yStart = 30; // Top margin
            float yEnd = Height - 40; // Bottom margin accounting for time axis
            float hitTestWidth = 60f; // Width of the hit test area, includes labels

            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Visible)
                {
                    // Create a wider hit test region that includes the axis line and labels
                    if (mousePosition.X >= currentX - 5 &&                 // Left of axis line
                        mousePosition.X <= currentX + hitTestWidth &&      // Right edge including labels
                        mousePosition.Y >= yStart &&                       // Top boundary
                        mousePosition.Y <= yEnd)                           // Bottom boundary
                    {
                        return i;
                    }
                    currentX -= RIGHT_MARGIN;
                }
            }
            return -1;
        }

        // Modify your Resize handler
        private void WaveformViewer_Resize(object sender, EventArgs e)
        {
            if (d2dRenderTarget == null || swapChain == null) return;

            d2dRenderTarget.Dispose();
            d2dRenderTarget = null;

            swapChain.ResizeBuffers(
                1,
                Width,
                Height,
                Format.B8G8R8A8_UNorm,
                SwapChainFlags.None);

            using (var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0))
            using (var surface = backBuffer.QueryInterface<Surface>())
            {
                var properties = new RenderTargetProperties(
                    RenderTargetType.Hardware,
                    new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    96.0f,
                    96.0f,
                    RenderTargetUsage.None,
                    FeatureLevel.Level_DEFAULT);

                d2dRenderTarget = new RenderTarget(d2dFactory, surface, properties);
                d2dRenderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
                d2dRenderTarget.TextAntialiasMode= SharpDX.Direct2D1.TextAntialiasMode.Cleartype;


            }

            Invalidate();
        }

        private void WaveformViewer_MouseDown(object sender, MouseEventArgs e)
        {
            var drawingArea = DrawingArea;
            Point mousePos = new SharpDX.Point(e.Location.X, e.Location.Y);


            if (e.Button == MouseButtons.Left)
            {
                // Check for ruler handle drag (on the Y-axis, right margin)
                float currentX = Width - RIGHT_MARGIN;

                for (int i = 0; i < channels.Count; i++)
                {
                    if (!channels[i].Visible)
                        continue;

                    var rulers = channels[i].Rulers;
                    var world = GetWorldBounds(channels[i]);
                    var transform3 = new ViewportTransform(world, DrawingArea);

                    for (int r = 0; r < rulers.Count; r++)
                    {
                        float screenY = transform3.WorldToScreenY(rulers[r].Voltage);
                        screenY += channels[i].Offset;
                        float clampedY = Math.Max(DrawingArea.Top + 1, screenY);

                        var handleBox = new RectangleF(currentX + 6, clampedY - 10, 50, 20);
                        if (handleBox.Contains(new Vector2(e.Location.X, e.Location.Y)))
                        {
                            draggingRuler = (i, r);
                            rulerDragStartY = e.Y;
                            rulerStartVoltage = rulers[r].Voltage;
                            Cursor = Cursors.SizeNS;
                            return;
                        }
                    }

                    currentX -= RIGHT_MARGIN;
                }

                if (currentTool == ToolMode.Zoom)
                {
                    // First check if we're in the drawing area
                    if (mousePos.X >= drawingArea.Left && mousePos.X <= drawingArea.Right &&
                        mousePos.Y >= drawingArea.Top && mousePos.Y <= drawingArea.Bottom)
                    {
                        // Start zoom selection
                        isZoomSelecting = true;
                        zoomStartPoint = mousePos;
                        zoomEndPoint = mousePos;
                        Cursor = Cursors.Cross;
                        return;
                    }
                }
                else if (currentTool == ToolMode.Pan)
                {
                    // Start panning if inside drawing area and not dragging anything else
                    var mp = new RawVector2(e.Location.X, e.Location.Y);
                    if (!isDragging && !isZoomSelecting &&
                        mp.X >= DrawingArea.Left && mp.X <= DrawingArea.Right &&
                        mp.Y >= DrawingArea.Top && mp.Y <= DrawingArea.Bottom)
                    {
                        isPanning = true;
                        panStartMousePos = new SharpDX.Point(e.Location.X, e.Location.Y);
                        panStartZoom = currentZoom?.Clone() ?? GetEffectiveZoom();
                        Cursor = Cursors.Hand;
                        return;
                    }
                }

                // Check for axis drag
                int axisIndex = HitTestYAxis(new SharpDX.Point(mousePos.X, mousePos.Y));
                if (axisIndex != -1)
                {
                    isDraggingAxis = true;
                    draggedAxisIndex = axisIndex;
                    dragStartY = e.Y;
                    dragStartOffset = channels[axisIndex].Offset;
                    Cursor = Cursors.SizeNS;
                }
                else
                {
                    lastMousePosition = new SharpDX.Point(mousePos.X, mousePos.Y);
                    dragChannelIndex = GetChannelAtPoint(new SharpDX.Point(mousePos.X, mousePos.Y));
                    isDragging = dragChannelIndex != -1;
                }
                var zoom = GetEffectiveZoom();
                var timeWorld = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1);
                var transform = new ViewportTransform(timeWorld, DrawingArea);
                

                // Phase Ruler hit test
                if (showPhaseRulers)
                {
                    float[] times = { phaseRuler1.Time, phaseRuler2.Time };
                    for (int i = 0; i < 2; i++)
                    {
                        float x = transform.WorldToScreenX(times[i] * DEFAULT_TIME_SPAN);
                        var rect = new RectangleF(x - 30, DrawingArea.Bottom + 30, 60, 20);
                        if (rect.Contains(new Vector2(e.Location.X, e.Location.Y)))
                        {
                            draggingPhaseRuler = i;
                            phaseRulerDragStartX = e.X;
                            phaseRulerStartTime = transform.ScreenToWorldX(e.X);
                            Cursor = Cursors.SizeWE;
                            return;
                        }
                    }
                }

                float timeStart = zoom.Left * DEFAULT_TIME_SPAN;
                float timeEnd = (zoom.Left + zoom.Width) * DEFAULT_TIME_SPAN;
                float timeSpan = timeEnd - timeStart;

                var timeWorld2 = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1);
                var transform2 = new ViewportTransform(timeWorld2, DrawingArea);
                for (int i = 0; i < timeRulers.Count; i++)
                {
                    float absoluteTime = timeRulers[i].Time * DEFAULT_TIME_SPAN;

                    if (absoluteTime < zoom.TimeStart || absoluteTime > zoom.TimeEnd)
                        continue;

                    float x = transform2.WorldToScreenX(absoluteTime);

                    var handleBox = new RectangleF(x - 30, DrawingArea.Bottom + 5, 60, 20);
                    if (handleBox.Contains(new SharpDX.Point(e.Location.X, e.Location.Y)))
                    {
                        draggingTimeRuler = (i);
                        timeRulerDragStartX = e.X;
                        timeRulerStartTime = timeRulers[i].Time;
                        Cursor = Cursors.SizeWE;
                        return;
                    }
                }


            }
            else if (e.Button == MouseButtons.Right)
            {
                // Reset zoom
                currentZoom = new ZoomRegion();
                ResetVerticalZooms();

                Invalidate();
            }
        }

        private void DragRuler(MouseEventArgs e)
        {
            if (draggingRuler == null)
                return;

            int i = draggingRuler.Value.channelIndex;
            int r = draggingRuler.Value.rulerIndex;

            var channel = channels[i];
            var ruler = channel.Rulers[r];

            var zoom = GetEffectiveZoom();
            var area = DrawingArea;
            var world = GetWorldBounds(channel);
            var transform = new ViewportTransform(world, area);

            // Use voltage range directly from current zoom state
            float voltsMin, voltsMax;
            if (channel.ZoomedVoltageMin.HasValue && channel.ZoomedVoltageMax.HasValue)
            {
                voltsMin = channel.ZoomedVoltageMin.Value;
                voltsMax = channel.ZoomedVoltageMax.Value;
            }
            else
            {
                float totalRange = channel.EffectiveVoltsPerDivision * DIVISIONS_Y;
                float mid = 0f;
                voltsMin = mid - totalRange / 2f;
                voltsMax = mid + totalRange / 2f;
            }

            float pixelsPerVolt = area.Height / (voltsMax - voltsMin);

            float deltaY = e.Y - rulerDragStartY;

            float adjustedY = e.Y - channel.Offset;
            float newVoltage = transform.ScreenToWorldY(adjustedY);
            ruler.Voltage = newVoltage;

            float screenY = transform.WorldToScreenY(newVoltage);
            ruler.IsAtTop = Math.Abs(screenY - (area.Top + 1)) < 0.01f;

            if ((DateTime.Now - lastCursorUpdate) > cursorUpdateInterval)
            {
                UpdateCursorPanel();
                lastCursorUpdate = DateTime.Now;
            }

            Invalidate();
        }

        private void DragTimeRuler(MouseEventArgs e)
        {
            if (draggingTimeRuler == null) return;

            int index = draggingTimeRuler.Value;
            var zoom = GetEffectiveZoom();

            var timeWorld = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1);
            var transform = new ViewportTransform(timeWorld, DrawingArea);

            float zoomStartSeconds = zoom.Left * DEFAULT_TIME_SPAN;
            float zoomEndSeconds = (zoom.Left + zoom.Width) * DEFAULT_TIME_SPAN;
            float zoomTimeSpan = zoomEndSeconds - zoomStartSeconds;

            float startX = timeRulerDragStartX;
            float currentX = e.X;

            float startTime = transform.ScreenToWorldX(startX);
            float currentTime = transform.ScreenToWorldX(currentX);
            float deltaTime = currentTime - startTime;

            float newAbsoluteTime = (timeRulerStartTime * DEFAULT_TIME_SPAN) + deltaTime;

            // Convert back to normalized time over full 10s range
            timeRulers[index].Time = ClampTime(newAbsoluteTime / DEFAULT_TIME_SPAN);


            if ((DateTime.Now - lastCursorUpdate) > cursorUpdateInterval)
            {
                UpdateCursorPanel();
                lastCursorUpdate = DateTime.Now;
            }

            Invalidate();
        }

        private void DragPhaseRuler(MouseEventArgs e)
        {
            if (draggingPhaseRuler == null) return;

            var zoom = GetEffectiveZoom();
            var timeWorld = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1);
            var transform = new ViewportTransform(timeWorld, DrawingArea);

            // Get current time under mouse
            float currentTime = transform.ScreenToWorldX(e.X);

            // Get delta time in world units
            float deltaTime = currentTime - phaseRulerStartTime;

            // Add that to whichever ruler was selected
            if (draggingPhaseRuler == 0)
                phaseRuler1.Time = ClampTime(phaseRuler1.Time + deltaTime / DEFAULT_TIME_SPAN);
            else
                phaseRuler2.Time = ClampTime(phaseRuler2.Time + deltaTime / DEFAULT_TIME_SPAN);

            // Update reference point for next move
            phaseRulerStartTime = currentTime;

            Invalidate();
        }

        private void PanFromMouse(Point currentMousePos)
        {
            var deltaX = currentMousePos.X - panStartMousePos.X;
            var deltaY = currentMousePos.Y - panStartMousePos.Y;

            var area = DrawingArea;
            var zoom = panStartZoom;

            float timeDelta = -(deltaX / area.Width) * zoom.TimeSpan;
            float viewportTopDelta = deltaY / area.Height;

            // Clamp horizontal pan to bounds
            float newTimeStart = Math.Max(0, Math.Min(DEFAULT_TIME_SPAN - zoom.TimeSpan, zoom.TimeStart + timeDelta));
            float newTimeEnd = newTimeStart + zoom.TimeSpan;

            currentZoom = new ZoomRegion
            {
                TimeStart = newTimeStart,
                TimeEnd = newTimeEnd,
                ViewportTopNorm = zoom.ViewportTopNorm + viewportTopDelta,
                ViewportHeightNorm = zoom.ViewportHeightNorm
            };

            Invalidate();
        }


        private void WaveformViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isZoomSelecting)
            {
                zoomEndPoint = new SharpDX.Point(e.Location.X, e.Location.Y);
                Invalidate(); // Redraw to show selection rectangle
                return;
            }

            if (isPanning)
            {
                PanFromMouse(new SharpDX.Point(e.Location.X, e.Location.Y));
                return;
            }

            if (draggingRuler != null)
            {
                DragRuler(e);
                return;
            }

            if (draggingPhaseRuler != null)
            {
                DragPhaseRuler(e);
                return;
            }

            if (draggingTimeRuler != null)
            {
                DragTimeRuler(e);
                return;
            }

            if (isDraggingAxis)
            {
                var channel = channels[draggedAxisIndex];
                float deltaY = e.Y - dragStartY;

                var world = GetWorldBounds(channel);
                var transform = new ViewportTransform(world, DrawingArea);

                float voltsStart = transform.ScreenToWorldY(dragStartY);
                float voltsNow = transform.ScreenToWorldY(e.Y);
                float deltaVolts = voltsNow - voltsStart;

                float baseRange = channel.EffectiveVoltsPerDivision * DIVISIONS_Y;
                float pixelsPerVolt = DrawingArea.Height / baseRange;

                // Flip the direction to match user expectation
                float newOffset = dragStartOffset - (deltaVolts * pixelsPerVolt);

                channel.Offset = newOffset;
                Invalidate();
            }
            else if (isDragging)
            {
                float deltaY = e.Y - lastMousePosition.Y;
                channels[dragChannelIndex].Offset += deltaY;
                lastMousePosition = new SharpDX.Point(e.Location.X, e.Location.Y);
                Invalidate();
            }
            else
            {
                if (HitTestYAxis(new SharpDX.Point(e.Location.X, e.Location.Y)) != -1)
                {
                    Cursor = Cursors.SizeNS;
                }
                else
                {
                    var drawingArea = DrawingArea;
                    if (e.Location.X >= drawingArea.Left && e.Location.X <= drawingArea.Right &&
                        e.Location.Y >= drawingArea.Top && e.Location.Y <= drawingArea.Bottom)
                    {
                        Cursor = (currentTool == ToolMode.Zoom) ? Cursors.Cross : Cursors.Hand;
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }
        private float ClampTime(float t) => Math.Max(0f, Math.Min(1f, t));

        private ZoomRegion GetZoomRegionFromScreenPoints(Point start, Point end)
        {
            Debug.WriteLine("[GetZoomRegionFromScreenPoints]");
            var area = DrawingArea;

            // Clamp to drawing area
            int x1 = Math.Max((int)area.Left, Math.Min((int)area.Right, start.X));
            int x2 = Math.Max((int)area.Left, Math.Min((int)area.Right, end.X));
            int y1 = Math.Max((int)area.Top, Math.Min((int)area.Bottom, start.Y));
            int y2 = Math.Max((int)area.Top, Math.Min((int)area.Bottom, end.Y));

            if (x2 - x1 < 10 || y2 - y1 < 10)
                return null;

            float sx = Math.Min(x1, x2);
            float ex = Math.Max(x1, x2);
            float sy = Math.Min(y1, y2);
            float ey = Math.Max(y1, y2);

            var zoom = GetEffectiveZoom();
            var timeWorld = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1);
            var transform = new ViewportTransform(timeWorld, DrawingArea);

            float tStart = transform.ScreenToWorldX(sx);
            float tEnd = transform.ScreenToWorldX(ex);
            float vTopNorm = (sy - area.Top) / area.Height;
            float vHeightNorm = (ey - sy) / area.Height;

            foreach (var channel in channels.Where(c => c.Visible))
            {
                float screenTop = sy;
                float screenBottom = ey;

                ApplyZoomToChannel(channel, screenTop, screenBottom);
            }

            return new ZoomRegion
            {
                TimeStart = tStart,
                TimeEnd = tEnd,
                ViewportTopNorm = vTopNorm,
                ViewportHeightNorm = vHeightNorm
            };
        }

        private void ApplyZoomToChannel(WaveformChannel channel, float screenTop, float screenBottom)
        {
            RectangleF world;

            if (!channel.ZoomedVoltageMin.HasValue || !channel.ZoomedVoltageMax.HasValue)
            {
                // ✅ Unzoomed, but apply Offset here!
                float fullRange = channel.EffectiveVoltsPerDivision * DIVISIONS_Y;
                float mid = 0f;
                float vMin = mid - fullRange / 2f;
                float vMax = mid + fullRange / 2f;
                world = new RectangleF(0, vMin, DEFAULT_TIME_SPAN, vMax - vMin);
            }
            else
            {
                // ✅ Already zoomed — offset was baked into original zoom selection
                float vMin = channel.ZoomedVoltageMin.Value;
                float vMax = channel.ZoomedVoltageMax.Value;
                world = new RectangleF(0, vMin, DEFAULT_TIME_SPAN, vMax - vMin);
            }

            // Create transform
            var transform = new ViewportTransform(world, DrawingArea);

            // ✅ Offset affects what the user *sees* on screen in unzoomed mode
            bool isZoomed = channel.ZoomedVoltageMin.HasValue && channel.ZoomedVoltageMax.HasValue;
            float screenTopAdjusted = isZoomed ? screenTop : screenTop - channel.Offset;
            float screenBottomAdjusted = isZoomed ? screenBottom : screenBottom - channel.Offset;

            float vTop = transform.ScreenToWorldY(screenTopAdjusted);
            float vBottom = transform.ScreenToWorldY(screenBottomAdjusted);

            float newMin = Math.Min(vTop, vBottom);
            float newMax = Math.Max(vTop, vBottom);

            bool isFirstZoom = !channel.ZoomedVoltageMin.HasValue || !channel.ZoomedVoltageMax.HasValue;
            bool isNestedZoomWithinCurrent = false;

            if (!isFirstZoom)
            {
                isNestedZoomWithinCurrent =
                    newMin >= channel.ZoomedVoltageMin.Value &&
                    newMax <= channel.ZoomedVoltageMax.Value;
            }

            if (isFirstZoom || isNestedZoomWithinCurrent)
            {
                channel.ZoomedVoltageMin = newMin;
                channel.ZoomedVoltageMax = newMax;
                Debug.WriteLine($"[ZOOM SET] Channel Index: {channel.Index}, Channel Name: CH{channel.Name}: {newMin:F3}V to {newMax:F3}V");
            }
            else
            {
                channel.ZoomedVoltageMin = null;
                channel.ZoomedVoltageMax = null;
                Debug.WriteLine($"[ZOOM SKIP] Channel Index: {channel.Index}, Channel Name: CH{channel.Name}: outside zoom box, set channel.ZoomedVoltageMin = null, channel.ZoomedVoltageMax = null");
            }
        }

        private bool IsZoomedIn()
        {
            return currentZoom != null && !currentZoom.IsEmpty &&
                   channels.Any(c => c.ZoomedVoltageMin.HasValue && c.ZoomedVoltageMax.HasValue);
        }

        private void ResetVerticalZooms()
        {
            foreach (var channel in channels)
            {
                channel.ZoomedVoltageMin = null;
                channel.ZoomedVoltageMax = null;
            }
        }

        private void WaveformViewer_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (isZoomSelecting)
                {
                    Debug.WriteLine($"[WaveformViewer_MouseUp] isZoomSelecting: {isZoomSelecting}, zoomStartPoint: {zoomStartPoint}, zoomEndPoint: {zoomEndPoint}");

                    var selection = GetZoomRegionFromScreenPoints(zoomStartPoint, zoomEndPoint);

                    if (selection != null && !selection.IsEmpty)
                    {
                        currentZoom = selection;
                    }

                    isZoomSelecting = false;
                    Invalidate();
                }

                if (isPanning)
                {
                    isPanning = false;
                    Cursor = Cursors.Default;
                    Invalidate();
                    return;
                }
                ClearDragState();
            }
            catch(Exception e2)
            {
                Debug.WriteLine(e2);
            }
        }

        private void ClearDragState()
        {
            isDraggingAxis = false;
            isDragging = false;
            draggedAxisIndex = -1;
            dragChannelIndex = -1;
            draggingRuler = null;
            draggingTimeRuler = null;
            draggingPhaseRuler = null;
            Cursor = Cursors.Default;
        }

        private int GetChannelAtPoint(SharpDX.Point location)
        {
            for (int i = 0; i < channels.Count; i++)
            {
                float yCenter = Height / 2f + channels[i].Offset;
                if (Math.Abs(location.Y - yCenter) < 20)
                    return i;
            }
            return -1;
        }


        private void InitializeChannels()
        {
            var defaultColors = new[]
            {
                new RawColor4(0.8f, 0.8f, 0.0f, 1.0f),  // Yellow (R=1, G=1, B=0)
                new RawColor4(0.0f, 1.0f, 0.0f, 1.0f),  // Green  (R=0, G=1, B=0)
                new RawColor4(0.0f, 0.0f, 1.0f, 1.0f),  // Blue   (R=0, G=0, B=1)
                new RawColor4(1.0f, 0.0f, 0.0f, 1.0f)   // Red    (R=1, G=0, B=0)
            };



            for (int i = 0; i < CHANNEL_COUNT; i++)
            {
                var channel = new WaveformChannel(i)
                {
                    Color = defaultColors[i],
                    VoltsPerDivision = 1.0f,
                    Visible = true,
                    Data = new Vector2[0],
                    Brush = new SolidColorBrush(d2dRenderTarget, defaultColors[i]),
                    Rulers = new List<Ruler>
                    {
                        new Ruler { Voltage = 2.0f },
                        new Ruler { Voltage = 2.0f }
                    }
                };
                channels.Add(channel);
                channels[i].Offset = i * 100; // Offset each channel vertically


                var ctrl = CreateChannelControl(i);
                channelPanel.Controls.Add(ctrl);

            }
            UpdateChannelCount();
        }

        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.DeviceContext d3dContext;



        

        public void ImportSnaponScopeFile(string filePath)
        {
            SetDefaults();
            var scopedata = new Snapon.Scope.Data.ScopeData();
            var data = scopedata.ReadAllFramesData(filePath);

            channelPanel.Controls.Clear();
            for (int i = 0; i < data.Traces.Count; i++)
            {
                var trace = data.Traces[i];
                int traceId = trace.Id?.Value ?? 0;

                // Map to your viewer channel index (traceId 1-4 → index 0-3)
                int channelIndex = traceId - 1;

                float traceScale = (float)trace.Scale.FullScaleValue; // This is ±20V
                float fullScale = traceScale * 2;                     // ✅ So we go from -20V to +20V
                channels[channelIndex].FullScale = fullScale;
                channels[channelIndex].MaxExpectedVoltage = traceScale;
                channels[channelIndex].Scale = 1.0f;
                channels[channelIndex].Visible = trace.Enabled.Value;
                string unit = trace.Scale.Unit.ToString() ?? "V"; // fallback to "V" if null

                // Apply vertical scale (viewer assumes ±fullScale over n divisions → fullScale / 4 per div)
                float voltsPerDiv = (float)(fullScale / DIVISIONS_Y);

                var color = GetChannelColor(i); // add this if needed

                var ctrl = CreateChannelControl(i);
                channelPanel.Controls.Add(ctrl);

            }

            UpdateChannelCount();

            double secondsPerFrame = data.Sweep.Seconds;

            uint frameSize = scopedata.FrameSize?.Value ?? 0;
            uint bufferSize = scopedata.BufferSize?.Value ?? 0;
            int totalFrames = (int)((bufferSize - frameSize) / frameSize) + 1;

            var waveformsByTrace = new Dictionary<int, List<Waveform>>();

            for (uint i = 0; i <= bufferSize - frameSize; i += frameSize)
            {
                scopedata.BufferPosition = new BufferPosition(i);
                var waveforms = scopedata.GetWaveforms(Snapon.Scope.Data.Interfaces.GetWaveformReadType.GetWaveformReadType_Default);

                foreach (var wf in waveforms)
                {
                    int traceId = wf.TraceId?.Value ?? 0;
                    int channelIndex = traceId - 1;

                    float maxExpected = (channelIndex >= 0 && channelIndex < channels.Count)
                                        ? channels[channelIndex].MaxExpectedVoltage
                                        : 10.0f;

                    for (int j = 0; j < wf.Points.Length; j++)
                    {
                        wf.Points[j] = Sanitize(wf.Points[j], maxExpected); // 💡 Fix NaN / Inf
                    }

                    if (!waveformsByTrace.ContainsKey(traceId))
                        waveformsByTrace[traceId] = new List<Waveform>();

                    waveformsByTrace[traceId].Add(wf);
                }
            }

            foreach (var kvp in waveformsByTrace)
            {
                int traceId = kvp.Key;
                if (traceId < 1 || traceId > CHANNEL_COUNT)
                    continue; // Ignore unknown traces

                var allPoints = kvp.Value.SelectMany(wf => wf.Points).ToArray();
                int totalPoints = allPoints.Length;

                float timeScale = (float)(totalPoints - 1) / (totalPoints / DEFAULT_TIME_SPAN);

                var vectorPoints = allPoints.Select((v, i) =>
                    new Vector2(i * DEFAULT_TIME_SPAN / (totalPoints - 1), v)).ToArray();

                UpdateChannelData(traceId - 1, vectorPoints);
            }
        }
        public RawColor4 GetChannelColor(int index)
        {
            if (index < 0 || index >= channels.Count)
                return Color.White;

            var c = channels[index].Color;
            return c;
        }
        public static Color ToAbgr(RawColor4 c)
        {
            var i = (int)(
                ((byte)(c.A * 255) << 24) |
                ((byte)(c.B * 255) << 16) |
                ((byte)(c.G * 255) << 8) |
                ((byte)(c.R * 255))
            );

            return Color.FromAbgr(i);
        }
        private float Sanitize(float v, float maxAbs)
        {
            if (float.IsNaN(v)) return 0f;
            if (float.IsNegativeInfinity(v)) return -maxAbs;
            if (float.IsPositiveInfinity(v)) return maxAbs;
            return Math.Max(-maxAbs, Math.Min(maxAbs, v)); // Clamp to ±maxAbs
        }

        private void InitializeDevice()
        {
            try
            {
                writeFactory = new SharpDX.DirectWrite.Factory();
                textFormat = new TextFormat(writeFactory, "Consolas", 14);

                // Create Direct3D Device
                SharpDX.Direct3D11.Device.CreateWithSwapChain(
                    SharpDX.Direct3D.DriverType.Hardware,
                    SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport,
                    new[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 },
                    new SwapChainDescription
                    {
                        BufferCount = 1,
                        Flags = SwapChainFlags.None,
                        IsWindowed = true,
                        ModeDescription = new ModeDescription(
                            Width,
                            Height,
                            new Rational(60, 1),
                            Format.B8G8R8A8_UNorm),
                        OutputHandle = Handle,
                        SampleDescription = new SampleDescription(1, 0),
                        SwapEffect = SwapEffect.Discard,
                        Usage = Usage.RenderTargetOutput
                    },
                    out d3dDevice,
                    out swapChain);

                d3dContext = d3dDevice.ImmediateContext;

                // Create Direct2D Factory
                d2dFactory = new Factory(SharpDX.Direct2D1.FactoryType.SingleThreaded);

                // Get the backbuffer
                using (var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0))
                using (var surface = backBuffer.QueryInterface<Surface>())
                {
                    // Create Direct2D Render Target
                    var properties = new RenderTargetProperties(
                        RenderTargetType.Hardware,
                        new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                        96.0f,
                        96.0f,
                        RenderTargetUsage.None,
                        FeatureLevel.Level_DEFAULT);

                    d2dRenderTarget = new RenderTarget(d2dFactory, surface, properties);
                }

                // Configure render target
                d2dRenderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
            }
            catch (SharpDXException ex)
            {
                MessageBox.Show($"DirectX Initialization Error:\n{ex.Message}\nHRESULT: {ex.HResult:X8}");
                throw;
            }
        }


        private void DrawAxes()
        {
            if (d2dRenderTarget == null) return;

            // Create brushes
            using (var axisBrush = new SolidColorBrush(d2dRenderTarget, new Color4(1.0f, 1.0f, 1.0f, 1.0f)))
            {
                // Draw time axis (X-axis)
                DrawTimeAxis(axisBrush);
                DrawTimeRulers();

                // Draw voltage axes (Y-axes)
                float currentX = Width - RIGHT_MARGIN;

                for (int i = 0; i < channels.Count; i++)
                {
                    if (channels[i].Visible)
                    {
                        DrawVoltageAxis(i, currentX, channels[i].Color, axisBrush);
                        currentX -= RIGHT_MARGIN;
                    }
                }
            }
        }

        private void DrawTimeRulers()
        {
            var zoom = GetEffectiveZoom();
            var area = DrawingArea;

            // ⚡ Create world bounds ONLY for time domain — no vertical concern
            var worldBounds = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1); // height = 1 for dummy Y
            var transform = new ViewportTransform(worldBounds, area); // ⚡ one transform handles all 3 conversions

            using (var brush = new SolidColorBrush(d2dRenderTarget, new RawColor4(1, 1, 1, 1)))
            using (var dashed = new StrokeStyle(d2dFactory, new StrokeStyleProperties { DashStyle = DashStyle.Dash }))
            {
                foreach (var ruler in timeRulers)
                {
                    if (!ruler.Active) continue;

                    float time = ruler.Time * DEFAULT_TIME_SPAN;
                    float screenX = transform.WorldToScreenX(time); // ✅ replacement

                    if (screenX < area.Left || screenX > area.Right)
                        continue;

                    d2dRenderTarget.DrawLine(
                        new RawVector2(screenX, area.Top),
                        new RawVector2(screenX, area.Bottom),
                        brush, 1.0f, dashed);

                    // Degree phase check — also use same transform
                    bool useDegrees = false;
                    float px1 = transform.WorldToScreenX(phaseRuler1.Time * DEFAULT_TIME_SPAN);
                    float px2 = transform.WorldToScreenX(phaseRuler2.Time * DEFAULT_TIME_SPAN);
                    if (px1 > px2) (px1, px2) = (px2, px1);
                    useDegrees = showPhaseRulers && (screenX >= px1 && screenX <= px2);

                    string label = useDegrees
                        ? $"{((ruler.Time - phaseRuler1.Time) / (phaseRuler2.Time - phaseRuler1.Time) * 720):F0}°"
                        : $"{time:F2}s";

                    var layout = new TextLayout(writeFactory, label, textFormat, 60, 20);
                    var labelBox = new RawRectangleF(screenX - 30, area.Bottom + 5, screenX + 30, area.Bottom + 25);

                    using (var bgBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0f, 0f, 0f, 1f)))
                    {
                        d2dRenderTarget.FillRectangle(labelBox, bgBrush);
                    }

                    d2dRenderTarget.DrawRectangle(labelBox, brush, 1);
                    d2dRenderTarget.DrawTextLayout(new RawVector2(labelBox.Left + 5, labelBox.Top + 2), layout, brush);
                    layout.Dispose();
                }
            }
        }


        private void DrawTimeAxis(SolidColorBrush brush)
        {
            var area = DrawingArea;
            float yPosition = Height - BOTTOM_MARGIN;

            var zoom = GetEffectiveZoom();

            // 🧠 World bounds = time domain only
            var worldBounds = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1); // 1 for dummy height
            var transform = new ViewportTransform(worldBounds, area); // Use this to convert time to X

            // Draw main axis line
            d2dRenderTarget.DrawLine(
                new Vector2(area.Left, yPosition),
                new Vector2(area.Right, yPosition),
                brush, 1.0f);

            // Draw divisions and labels
            for (int i = 0; i <= DIVISIONS_X; i++)
            {
                float time = zoom.TimeStart + (i / (float)DIVISIONS_X) * zoom.TimeSpan;
                float x = transform.WorldToScreenX(time); // ✅ updated

                // Tick mark
                d2dRenderTarget.DrawLine(
                    new Vector2(x, yPosition - 5),
                    new Vector2(x, yPosition + 5),
                    brush, 1.0f);

                // Label
                string label = $"{time:F2}s";
                var layout = new TextLayout(writeFactory, label, textFormat, 60, 20);

                d2dRenderTarget.DrawText(
                    label,
                    textFormat,
                    new SharpDX.RectangleF(x - 25, yPosition + 5, 60, 20),
                    brush);

                layout.Dispose();
            }
        }

        private void DrawVoltageAxis(int channelIndex, float xPosition, RawColor4 channelColor, SolidColorBrush axisBrush)
        {
            var channel = channels[channelIndex];
            var area = DrawingArea;

            float lineWidth = 1.0f;

            if (IsZoomedIn() &&
            (!channel.ZoomedVoltageMin.HasValue || !channel.ZoomedVoltageMax.HasValue))
            {
                // This channel is excluded from the current zoom → don't draw its axis
                return;
            }

            // Get per-channel vertical zoom if available
            float vMin, vMax;
            if (channel.ZoomedVoltageMin.HasValue && channel.ZoomedVoltageMax.HasValue)
            {
                vMin = channel.ZoomedVoltageMin.Value;
                vMax = channel.ZoomedVoltageMax.Value;
            }
            else
            {
                float baseRange = channel.EffectiveVoltsPerDivision * DIVISIONS_Y;
                float mid = 0f;
                vMin = mid - baseRange / 2f;
                vMax = mid + baseRange / 2f;
            }

            float tickStep = (vMax - vMin) / DIVISIONS_Y;

            using (var channelBrush = new SolidColorBrush(d2dRenderTarget, channelColor))
            {
                // Draw vertical axis line
                d2dRenderTarget.DrawLine(
                    new Vector2(xPosition, area.Top),
                    new Vector2(xPosition, area.Bottom),
                    channelBrush, lineWidth);

                Debug.WriteLine($"[AXIS] CH{channel.Name}:");
                Debug.WriteLine($"   ZoomedVMin={vMin:F3}, ZoomedVMax={vMax:F3}");

                var world = GetWorldBounds(channel);
                var transform = new ViewportTransform(world, DrawingArea);

                for (int i = 0; i <= DIVISIONS_Y; i++)
                {
                    float voltage = vMax - i * tickStep;
                    float screenY = transform.WorldToScreenY(voltage);
                    screenY += channel.Offset;

                    //Debug.WriteLine($"   Tick {i}: Voltage={voltage:F3}V → Y={screenY:F1}px");

                    // Tick mark
                    d2dRenderTarget.DrawLine(
                        new Vector2(xPosition - 5, screenY),
                        new Vector2(xPosition + 5, screenY),
                        channelBrush, lineWidth);

                    // Label
                    var label = $"{voltage:F2}V";
                    var layout = new TextLayout(writeFactory, label, textFormat, 50, 20);
                    d2dRenderTarget.DrawText(
                        label,
                        textFormat,
                        new RectangleF(xPosition + 5, screenY - 10, 50, 20),
                        channelBrush);
                    layout.Dispose();
                }

                // Channel label (e.g. CH1)
                string channelLabel = $"CH{channelIndex + 1}";
                var labelLayout = new TextLayout(writeFactory, channelLabel, textFormat, 50, 20);
                d2dRenderTarget.DrawText(
                    channelLabel,
                    textFormat,
                    new RectangleF(xPosition + 5, area.Top - 25, 50, 20),
                    channelBrush);
                labelLayout.Dispose();

                Debug.WriteLine($"   Channel Index: {channel.Index}");
                Debug.WriteLine($"   Scale: {channel.Scale}");
                Debug.WriteLine($"   Volts/Div: {channel.VoltsPerDivision}");
                Debug.WriteLine($"   EffectiveVoltsPerDivision: {channel.EffectiveVoltsPerDivision}");
                Debug.WriteLine($"   Range: {channel.EffectiveVoltsPerDivision * DIVISIONS_Y}");
            }
        }

        // Add a method to reset positions
        public void ResetPositions()
        {
            for (int i = 0; i < channels.Count; i++)
            {
                channels[i].Offset = 0;
            }
            Invalidate();
        }
        // Modify your Render method to include axes

        private void Render()
        {
            if (d2dRenderTarget == null) return;

            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.Clear(new RawColor4(0, 0, 0, 1));

            // Draw grid in UI coordinates (unzoomed)
            DrawGrid();

            // Draw waveform data under zoom transform
            if (currentZoom != null && !currentZoom.IsEmpty)
            {
                d2dRenderTarget.PushAxisAlignedClip(DrawingArea, AntialiasMode.Aliased);
                
            }

            DrawWaveforms();

            if (currentZoom != null && !currentZoom.IsEmpty)
            {
               
                d2dRenderTarget.PopAxisAlignedClip();
            }

            // These are drawn in screen coordinates (not zoomed)
            DrawPhaseRulers();
            DrawAxes();
            DrawVoltageRulers();
            DrawZoomSelection();

            d2dRenderTarget.EndDraw();
            swapChain.Present(1, PresentFlags.None);
        }

        private ZoomRegion GetEffectiveZoom()
        {
            return currentZoom != null && !currentZoom.IsEmpty
                ? currentZoom
                : new ZoomRegion
                {
                    TimeStart = 0,
                    TimeEnd = DEFAULT_TIME_SPAN,
                    ViewportTopNorm = 0,
                    ViewportHeightNorm = 1
                };
        }

        private void DrawZoomSelection()
        {
            if (!isZoomSelecting) return;

            using (var selectionBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(1.0f, 1.0f, 1.0f, 0.3f)))
            {
                float x1 = Math.Min(zoomStartPoint.X, zoomEndPoint.X);
                float x2 = Math.Max(zoomStartPoint.X, zoomEndPoint.X);
                float y1 = Math.Min(zoomStartPoint.Y, zoomEndPoint.Y);
                float y2 = Math.Max(zoomStartPoint.Y, zoomEndPoint.Y);

                var rect = new RawRectangleF(x1, y1, x2, y2);

                d2dRenderTarget.FillRectangle(rect, selectionBrush);
                d2dRenderTarget.DrawRectangle(rect, selectionBrush, 1.0f);
            }
        }

        private float GetCurrentZoomScale()
        {
            if (currentZoom == null || currentZoom.IsEmpty) return 1.0f;
            return 1.0f / currentZoom.Width; // Use X scale since we want uniform scaling
        }

        public void SetChannelScale(int channelIndex, float uiScale)
        {
            if (channelIndex >= 0 && channelIndex < channels.Count)
            {
                channels[channelIndex].Scale = uiScale;
                Invalidate();
            }
        }

        private void DrawWaveforms()
        {
            var zoom = GetEffectiveZoom();
            var area = DrawingArea;
            float zoomScale = zoom.TimeSpan > 0 ? DEFAULT_TIME_SPAN / zoom.TimeSpan : 1.0f;
            float lineWidth = 1.0f;

            for (int i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];
                if (!channel.Visible || channel.Data == null || channel.Data.Length < 2)
                    continue;

                // Use per-channel zoomed voltage bounds if present
                float vMin, vMax;
                if (channel.ZoomedVoltageMin.HasValue && channel.ZoomedVoltageMax.HasValue)
                {
                    vMin = channel.ZoomedVoltageMin.Value;
                    vMax = channel.ZoomedVoltageMax.Value;
                }
                else
                {
                    float range = channel.EffectiveVoltsPerDivision * DIVISIONS_Y;
                    vMin = -range / 2f;
                    vMax = +range / 2f;
                }

                // Optional skip if nothing visible
                float tMin = zoom.TimeStart;
                float tMax = zoom.TimeEnd;

                bool hasVisiblePoints = channel.Data.Any(p =>
                    p.X >= tMin && p.X <= tMax &&
                    p.Y >= vMin && p.Y <= vMax);

                if (!hasVisiblePoints)
                    continue;

                // Create transform from world (time + voltage) to screen
                var worldBounds = new RectangleF(tMin, vMin, zoom.TimeSpan, vMax - vMin);
                var transform = new ViewportTransform(worldBounds, area);

                using (var geometry = new PathGeometry(d2dFactory))
                using (var sink = geometry.Open())
                {
                    var first = channel.Data[0];
                    bool isZoomed = channel.ZoomedVoltageMin.HasValue && channel.ZoomedVoltageMax.HasValue;
                    Vector2 screenFirst = transform.WorldToScreen(first);
                    screenFirst.Y += channel.Offset;
                    sink.BeginFigure(new RawVector2(screenFirst.X, screenFirst.Y), FigureBegin.Hollow);

                    for (int j = 1; j < channel.Data.Length; j++)
                    {
                        Vector2 screenPoint = transform.WorldToScreen(channel.Data[j]);
                        screenPoint.Y += channel.Offset;
                        sink.AddLine(new RawVector2(screenPoint.X, screenPoint.Y));
                    }

                    sink.EndFigure(FigureEnd.Open);
                    sink.Close();

                    d2dRenderTarget.DrawGeometry(geometry, channel.Brush, lineWidth);
                }
            }
        }


        private void DrawPhaseRulers()
        {
            if (!showPhaseRulers) return;

            var zoom = GetEffectiveZoom();
            var area = DrawingArea;

            // 🧠 Time domain only (Y is dummy)
            var worldBounds = new RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1);
            var transform = new ViewportTransform(worldBounds, area);

            float x1 = transform.WorldToScreenX(phaseRuler1.Time * DEFAULT_TIME_SPAN);
            float x2 = transform.WorldToScreenX(phaseRuler2.Time * DEFAULT_TIME_SPAN);

            if (x1 > x2) (x1, x2) = (x2, x1);
            float segmentWidth = (x2 - x1) / 4f;

            using (var phaseBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0.4f, 0.8f, 1f, 0.4f)))
            {
                d2dRenderTarget.FillRectangle(new RawRectangleF(x1, area.Top, x1 + segmentWidth, area.Bottom), phaseBrush);
                d2dRenderTarget.FillRectangle(new RawRectangleF(x1 + 2 * segmentWidth, area.Top, x1 + 3 * segmentWidth, area.Bottom), phaseBrush);
            }

            using (var edgeBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0.4f, 0.8f, 1f, 0.8f)))
            {
                d2dRenderTarget.DrawLine(new RawVector2(x1, area.Top), new RawVector2(x1, area.Bottom), edgeBrush, 1.0f);
                d2dRenderTarget.DrawLine(new RawVector2(x2, area.Top), new RawVector2(x2, area.Bottom), edgeBrush, 1.0f);
            }

            using (var handleBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0.4f, 0.8f, 1f, 0.5f)))
            {
                float[] positions = { x1, x2 };
                string[] labels = { "0°", "720°" };

                for (int j = 0; j < 2; j++)
                {
                    float x = positions[j];
                    var rect = new RawRectangleF(x - 30, area.Bottom + 30, x + 30, area.Bottom + 50);
                    d2dRenderTarget.FillRectangle(rect, handleBrush);

                    using (var layout = new TextLayout(writeFactory, labels[j], textFormat, 60, 20))
                    {
                        d2dRenderTarget.DrawTextLayout(new RawVector2(x - 15, area.Bottom + 30), layout, handleBrush);
                    }
                }
            }
        }

        private void DrawVoltageRulers()
        {
            float axisX = Width - RIGHT_MARGIN;

            foreach (var channel in channels)
            {
                if (!channel.Visible) continue;

                var rulers = channel.Rulers;
                var zoom = GetEffectiveZoom();
                var area = DrawingArea;

                using (var brush = new SolidColorBrush(d2dRenderTarget, channel.Color))
                using (var dashed = new StrokeStyle(d2dFactory, new StrokeStyleProperties { DashStyle = DashStyle.Dash }))
                {
                    var world = GetWorldBounds(channel);
                    var transform = new ViewportTransform(world, DrawingArea);

                    foreach (var ruler in rulers)
                    {
                        if (!ruler.Active) continue;

                        float rawY = transform.WorldToScreenY(ruler.Voltage);
                        rawY += channel.Offset;

                        bool isDocked = rawY <= area.Top + 1;
                        if (isDocked)
                            rawY = area.Top + 1;

                        // Draw dashed horizontal line
                        if (!isDocked)
                        {
                            d2dRenderTarget.DrawLine(
                                new RawVector2(area.Left, rawY),
                                new RawVector2(area.Right, rawY),
                                brush, 1.0f, dashed);
                        }

                        // Label box
                        string label = $"{ruler.Voltage:F2}V";
                        var layout = new TextLayout(writeFactory, label, textFormat, 50, 20);

                        var labelBox = new RawRectangleF(axisX + 6, rawY - 10, axisX + 56, rawY + 10);
                        using (var backgroundBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0f, 0f, 0f, 1f)))
                        {
                            d2dRenderTarget.FillRectangle(labelBox, backgroundBrush);
                        }

                        d2dRenderTarget.DrawRectangle(labelBox, brush, 1);
                        d2dRenderTarget.DrawTextLayout(new RawVector2(labelBox.Left + 2, labelBox.Top + 2), layout, brush);

                        layout.Dispose();
                    }
                }

                axisX -= RIGHT_MARGIN;
            }
        }

        private void DrawGrid()
        {
            var drawingArea = DrawingArea;
            float zoomScale = GetCurrentZoomScale();
            float lineWidth = 0.5f / zoomScale; // Thinner lines for grid

            using (var gridBrush = new SolidColorBrush(d2dRenderTarget,
                   new RawColor4(0.2f, 0.2f, 0.2f, 1)))
            using (var gridStyle = new StrokeStyle(d2dFactory,
                   new StrokeStyleProperties { DashStyle = DashStyle.Solid }))
            {
                float xSpacing = drawingArea.Width / DIVISIONS_X;
                float ySpacing = drawingArea.Height / DIVISIONS_Y;

                // Draw vertical lines
                for (float x = 0; x <= DIVISIONS_X; x++)
                {
                    float xPos = drawingArea.Left + (x * xSpacing);
                    d2dRenderTarget.DrawLine(
                        new RawVector2(xPos, drawingArea.Top),
                        new RawVector2(xPos, drawingArea.Bottom),
                        gridBrush,
                        lineWidth,
                        gridStyle);
                }

                // Draw horizontal lines
                for (float y = 0; y <= DIVISIONS_Y; y++)
                {
                    float yPos = drawingArea.Top + (y * ySpacing);
                    d2dRenderTarget.DrawLine(
                        new RawVector2(drawingArea.Left, yPos),
                        new RawVector2(drawingArea.Right, yPos),
                        gridBrush,
                        lineWidth,
                        gridStyle);
                }
            }
        }

        private void UpdateCursorPanel()
        {
            // Voltage Rulers (per channel)
            for (int i = 0; i < channels.Count; i++)
            {
                var rulers = channels[i].Rulers;
                if (rulers.Count >= 2)
                {
                    float? v1 = rulers[0].Active ? rulers[0].Voltage : (float?)null;
                    float? v2 = rulers[1].Active ? rulers[1].Voltage : (float?)null;

                    string colorName = GetColorName(channels[i].Color);
                    cursorPanel.SetCursorValues($"{colorName}", v1, v2, "V");
                }
            }

            // Time Rulers
            // Time Rulers with conditional degree conversion
            if (timeRulers.Count >= 2)
            {
                var t1Norm = timeRulers[0].Time;
                var t2Norm = timeRulers[1].Time;

                bool t1Inside = IsBetween(t1Norm, phaseRuler1.Time, phaseRuler2.Time);
                bool t2Inside = IsBetween(t2Norm, phaseRuler1.Time, phaseRuler2.Time);

                if (showPhaseRulers && t1Inside && t2Inside)
                {
                    float x1 = (t1Norm - phaseRuler1.Time) / (phaseRuler2.Time - phaseRuler1.Time) * 720f;
                    float x2 = (t2Norm - phaseRuler1.Time) / (phaseRuler2.Time - phaseRuler1.Time) * 720f;
                    cursorPanel.SetCursorValues("Time", x1, x2, "°");
                }
                else
                {
                    float? t1 = timeRulers[0].Active ? t1Norm * DEFAULT_TIME_SPAN : (float?)null;
                    float? t2 = timeRulers[1].Active ? t2Norm * DEFAULT_TIME_SPAN : (float?)null;
                    cursorPanel.SetCursorValues("Time", t1, t2, "s");
                }
            }

        }

        // Optional helper to match color with name
        private string GetColorName(RawColor4 color)
        {
            if (color.R > 0.7f && color.G > 0.7f && color.B < 0.2f) return "Yellow";
            if (color.G > 0.7f && color.R < 0.2f && color.B < 0.2f) return "Green";
            if (color.B > 0.7f && color.R < 0.2f && color.G < 0.2f) return "Blue";
            if (color.R > 0.7f && color.G < 0.2f && color.B < 0.2f) return "Red";
            return "Channel";
        }

        private bool IsBetween(float value, float a, float b)
        {
            if (a > b) (a, b) = (b, a);
            return value >= a && value <= b;
        }

        public void UpdateChannelData(int channelIndex, Vector2[] data)
        {
            if (channelIndex >= 0 && channelIndex < channels.Count)
            {
                var dataManager = new WaveformDataManager();
                channels[channelIndex].Data = dataManager.DecimateData(data, Width);
                Invalidate();
            }
        }

        private void RecalculateChannelOffsets()
        {
            float spacing = 100f;
            int visibleIndex = 0;

            for (int i = 0; i < channels.Count; i++)
            {
                if (!channels[i].Visible) continue;
                channels[i].Offset = visibleIndex * spacing;
                visibleIndex++;
            }
        }

        private ChannelControl CreateChannelControl(int index)
        {
            var ch = channels[index];
            var ctrl = new ChannelControl(ch);

            ctrl.SetEnabledState(ch.Visible); // reflects initial visibility

            ctrl.OnScaleChanged = (channelIndex2, newScale) =>
            {
                SetChannelScale(channelIndex2, newScale);
            };

            ctrl.OnToggleVisibility = (channelIndex2, visible) =>
            {
                if (channelIndex2 >= 0 && channelIndex2 < channels.Count)
                {
                    channels[channelIndex2].Visible = visible;
                    ctrl.SetEnabledState(visible);
                    UpdateChannelCount();
                    RecalculateChannelOffsets();
                    Invalidate();
                }
            };

            return ctrl;
        }

        private RectangleF GetWorldBounds(WaveformChannel channel)
        {
            var zoom = GetEffectiveZoom();

            float timeStart = zoom.TimeStart;
            float timeWidth = zoom.TimeSpan;

            float vMin, vMax;
            if (channel.ZoomedVoltageMin.HasValue && channel.ZoomedVoltageMax.HasValue)
            {
                vMin = channel.ZoomedVoltageMin.Value;
                vMax = channel.ZoomedVoltageMax.Value;
            }
            else
            {
                float baseRange = channel.EffectiveVoltsPerDivision * DIVISIONS_Y;
                vMin = -baseRange / 2f;
                vMax = +baseRange / 2f;
            }

            return new RectangleF(timeStart, vMin, timeWidth, vMax - vMin);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Render();
        }
    }
}