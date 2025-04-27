using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Oscillite.Input;
using Oscillite.Models;
using Oscillite.UI;
using Oscillite.Utilities;
using Oscillite.Utilities.Oscillite;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Factory = SharpDX.Direct2D1.Factory;

namespace Oscillite
{
    public partial class WaveformViewer : UserControl
    {
        // Rendering
        private SharpDX.Direct3D11.Device d3dDevice;
        private Factory d2dFactory;
        private RenderTarget d2dRenderTarget;
        private SwapChain swapChain;
        private SharpDX.DirectWrite.Factory writeFactory;
        private TextFormat textFormat;
        private WaveformRenderer waveformRenderer = new WaveformRenderer();

        //Layout / UI Constants
        private const float LEFT_MARGIN = 190f;    // Space for left labels
        private const float RIGHT_MARGIN = 60f;    // Space for each Y-axis
        private const float TOP_MARGIN = 10f;      // Space for top labels
        private const float BOTTOM_MARGIN = 60f;   // Space for time axis
        internal const int DIVISIONS_Y = 8;
        private const int DIVISIONS_X = 10;
        private const float DEFAULT_TIME_SPAN = 10.0f;
        float? currentFileTimespan = null;
        private RectangleF? drawingArea;

        //Channel Management
        private int DEFAULT_CHANNEL_COUNT = 4;
        private List<WaveformChannel> channels;
        internal List<WaveformChannel> Channels => channels;
        private FlowLayoutPanel channelPanel = new FlowLayoutPanel();
        internal FlowLayoutPanel ChannelPanel => channelPanel;

        private RawColor4[] defaultColors = new[]
{
                new RawColor4(0.8f, 0.8f, 0.0f, 1.0f),  // Yellow
                new RawColor4(0.0f, 1.0f, 0.0f, 1.0f),  // Green
                new RawColor4(0.12f, 0.56f, 1.0f, 1.0f),  // Blue
                new RawColor4(1.0f, 0.0f, 0.0f, 1.0f)   // Red
            };

        //Zoom / Pan State
        private WaveformToolController toolController;
        internal WaveformToolController ToolController => toolController;

        private ZoomRegion currentZoom = null;
        private Button modeToggleButton;
        internal Button ModeToggleButton => modeToggleButton;

        //Dragging State
        private DragState dragState = new DragState();
        internal DragState DragState => dragState;

        //Ruler / Cursor UI
        private List<TimeRuler> timeRulers = new List<TimeRuler>
        {
            new TimeRuler { Time = 0.0f },
            new TimeRuler { Time = 1.0f }
        };
        internal List<TimeRuler> TimeRulers => timeRulers;
        private bool showPhaseRulers = false;
        internal bool ShowPhaseRulers => showPhaseRulers;
        private TimeRuler phaseRuler1 = new TimeRuler { Time = 0.0f };
        private TimeRuler phaseRuler2 = new TimeRuler { Time = 1.0f };
        internal TimeRuler PhaseRuler1 => phaseRuler1;
        internal TimeRuler PhaseRuler2 => phaseRuler2;
        private CursorSummaryOverlay cursorPanel;
        internal CursorSummaryOverlay CursorPanel => cursorPanel;        

        //Debug / Misc
        private WaveformInputHandler inputHandler;
        internal bool IsDraggingVoltageRuler => dragState.DraggingVoltageRuler != null;
        internal bool IsDraggingPhaseRuler => dragState.DraggingPhaseRuler.HasValue;
        internal bool IsDraggingTimeRuler => dragState.DraggingTimeRuler.HasValue;
        internal bool IsDraggingVoltageAxis => dragState.IsDraggingAxis;
        internal bool IsDraggingChannel => dragState.IsDraggingChannel;

        public WaveformViewer()
        {
            InitializeComponent();
            ConfigureControlStyles();
            InitializeUI();
            InitializeDevice();
            SetDefaults();
            RegisterMouseEvents();
        }

        private void ConfigureControlStyles()
        {
            SetStyle(
                ControlStyles.Opaque |          // Don't draw background
                ControlStyles.UserPaint |       // We'll handle all painting
                ControlStyles.AllPaintingInWmPaint, // No background painting
                true);

            // Explicitly disable double buffering
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
        }

        private void RegisterMouseEvents()
        {
            this.MouseDown += (s, e) => inputHandler.HandleMouseDown(e);
            this.MouseMove += (s, e) => inputHandler.HandleMouseMove(e);
            this.MouseUp += (s, e) => inputHandler.HandleMouseUp(e);
            this.Resize += WaveformViewer_Resize;
        }

        internal void SetChannelPanel(FlowLayoutPanel panel)
        {
            channelPanel = panel;
        }

        internal Button CreateStyledButton(string text, EventHandler onClick, Padding? margin = null)
        {
            var button = new Button
            {
                Size = new System.Drawing.Size(120, 40),
                Text = text,
                Font = new System.Drawing.Font("Segoe MDL2 Assets", 12),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                ForeColor = System.Drawing.Color.White,
                Cursor = Cursors.Hand,
                Margin = margin ?? new Padding(10, 0, 10, 10)
            };

            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            button.Click += onClick;
            return button;
        }

        private void InitializeUI()
        {
            var leftPanel = WaveformUIFactory.CreateLeftPanel(this);
            this.Controls.Add(leftPanel);
            channels = new List<WaveformChannel>();
        }

        internal void TogglePhaseRulers(object sender, EventArgs e)
        {
            showPhaseRulers = !showPhaseRulers;
            if (IsZoomedIn())
            {
                var zoom = GetEffectiveZoom();
                if (zoom == null) return;

                // zoom.TimeStart and zoom.TimeSpan are in seconds!
                float zoomStartNorm = zoom.TimeStart / CurrentFileTimespan;
                float zoomSpanNorm = zoom.TimeSpan / CurrentFileTimespan;

                phaseRuler1.Time = ClampTime(zoomStartNorm + zoomSpanNorm * 0.30f);
                phaseRuler2.Time = ClampTime(zoomStartNorm + zoomSpanNorm * 0.70f);
            }
            else
            {
                // If no zoom, just use global 30%-70% of file
                phaseRuler1.Time = 0.30f;
                phaseRuler2.Time = 0.70f;
            }

            UpdateCursorPanel();
            Invalidate();
        }



        internal void ToggleToolMode(object sender, EventArgs e)
        {
            toolController?.ToggleToolMode();
        }

        public void OpenFile()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Files|*.vsm;*.vss;*.ocsv";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var result = WaveformFileLoader.Load(ofd.FileName);
                        ApplyLoadedWaveformData(result);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load waveform:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        internal void SetModeToggleButton(Button button)
        {
            modeToggleButton = button;
        }

        private void SetDefaults()
        {
            cursorPanel = new CursorSummaryOverlay(this);
            inputHandler = new WaveformInputHandler(this);
            toolController = new WaveformToolController(this);
            dragState = new DragState();
            Cursor = Cursors.Cross;
            ResetZoom();
            
            InitializeChannels();

            timeRulers = new List<TimeRuler>
            {
                new TimeRuler { Time = 0.0f },
                new TimeRuler { Time = 1.0f }
            };

            phaseRuler1 = new TimeRuler { Time = 0.0f };
            phaseRuler2 = new TimeRuler { Time = 1.0f };

            showPhaseRulers = false;

            UpdateCursorPanel();
        }

        internal RectangleF DrawingArea
        {
            get
            {
                if (drawingArea == null)
                    drawingArea = CalculateDrawingArea();

                return drawingArea.Value;
            }
        }

        private RectangleF CalculateDrawingArea()
        {
            float rightSpace = RIGHT_MARGIN * channels.Count(c => c.Visible);
            return new RectangleF(
                LEFT_MARGIN,
                TOP_MARGIN,
                Width - (LEFT_MARGIN + rightSpace),
                Height - (TOP_MARGIN + BOTTOM_MARGIN)
            );
        }

        public float CurrentFileTimespan
        {
            get
            {
                if(currentFileTimespan.HasValue)
                    return currentFileTimespan.Value;

                return DEFAULT_TIME_SPAN;
            }
            set
            {
                currentFileTimespan = value;
            }
        }
        private void InvalidateDrawingArea()
        {
            drawingArea = null;
        }

        internal int HitTestYAxis(SharpDX.Point mousePosition)
        {
            float currentX = Width - RIGHT_MARGIN;
            float yStart = TOP_MARGIN; // Top margin
            float yEnd = Height - BOTTOM_MARGIN; // Bottom margin accounting for time axis
            float hitTestWidth = RIGHT_MARGIN; // Width of the hit test area, includes labels

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
            InvalidateDrawingArea();
            Invalidate();
        }

        internal void ResetZoom()
        {
            toolController.ResetZoom();
            toolController.ResetVerticalZooms();
            Invalidate();
        }

        internal float ClampTime(float t) => Math.Max(0f, Math.Min(1f, t));

        internal bool IsZoomedIn()
        {
            if (currentZoom == null || currentZoom.IsEmpty)
                return false;

            bool anyChannelZoomed = channels.Any(c => c.ZoomedVoltageMin.HasValue && c.ZoomedVoltageMax.HasValue);
            return anyChannelZoomed;
        }

        private void ResetVerticalZooms()
        {
            foreach (var channel in channels)
            {
                channel.ZoomedVoltageMin = null;
                channel.ZoomedVoltageMax = null;
            }
        }

        private void InitializeChannels()
        {
            channels.Clear();
            channelPanel.Controls.Clear();

            for (int i = 0; i < DEFAULT_CHANNEL_COUNT; i++)
            {
                var channel = new WaveformChannel(i)
                {
                    Color = defaultColors[i],
                    VoltsPerDivision = 1.0f,
                    Visible = true,
                    Data = new Vector2[0],
                    Brush = new SolidColorBrush(d2dRenderTarget, defaultColors[i]),
                    Rulers = new List<VoltageRuler>
            {
                new VoltageRuler { Voltage = 2.0f },
                new VoltageRuler { Voltage = 2.0f }
            }
                };
                channels.Add(channel);
                channels[i].Offset = i * 100;

                var ctrl = CreateChannelControl(i); // 💡 This is what creates the +/- buttons
                channelPanel.Controls.Add(ctrl);   // ✅ Make sure this is targeting the correct panel
            }
        }

        private void ApplyLoadedWaveformData(WaveformFileResult result)
        {
            CurrentFileTimespan = result.Duration;
            TimeRulers[0].Time = 0.0f;
            TimeRulers[1].Time = 1.0f;
            channels.Clear();
            channelPanel.Controls.Clear();
            showPhaseRulers = false;
            ResetZoom();
            toolController.ResetToZoomMode();

            foreach (var data in result.Channels)
            {
                var index = data.ChannelIndex;
                var color = defaultColors[index % defaultColors.Length];

                var ch = new WaveformChannel(index)
                {
                    Data = DataUtilities.DecimateData(data.Data, CurrentFileTimespan),
                    FullScale = data.FullScale,
                    MaxExpectedVoltage = data.FullScale / 2f,
                    Scale = 1.0f,
                    Visible = data.Visible,
                    Color = color,
                    Brush = new SolidColorBrush(d2dRenderTarget, color),
                    Offset = index * 100f,
                    Unit = data.UnitString,
                    Rulers = new List<VoltageRuler>
                    {
                        new VoltageRuler { Voltage = 2.0f },
                        new VoltageRuler { Voltage = 2.0f }
                    },
                    ZoomedVoltageMin = null,
                    ZoomedVoltageMax = null
                };

                channels.Add(ch);

                var ctrl = CreateChannelControl(index);
                channelPanel.Controls.Add(ctrl);
            }
            InvalidateDrawingArea();
            Invalidate();
        }

        private void InitializeDevice()
        {
            try
            {
                writeFactory = new SharpDX.DirectWrite.Factory();
                textFormat = new TextFormat(writeFactory, "Consolas", 11);

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

        internal ZoomRegion GetEffectiveZoom()
        {
            return toolController.GetEffectiveZoom();

        }

        public void SetChannelScale(int channelIndex, float uiScale)
        {
            if (channelIndex >= 0 && channelIndex < channels.Count)
            {
                channels[channelIndex].Scale = uiScale;
                ResetZoom();
                Invalidate();
            }
        }

        internal void UpdateCursorPanel()
        {
            // Voltage Rulers (per channel)
            for (int i = 0; i < channels.Count; i++)
            {
                var rulers = channels[i].Rulers;
                if (rulers.Count >= 2)
                {
                    float? v1 = rulers[0].Active ? rulers[0].Voltage : (float?)null;
                    float? v2 = rulers[1].Active ? rulers[1].Voltage : (float?)null;

                    cursorPanel.SetCursorValues($"{channels[i].Name}", v1, v2, channels[i].Unit, channels[i].Color);
                }
            }

            // Time Rulers
            if (timeRulers.Count >= 2)
            {
                var t1Norm = timeRulers[0].Time; // these are already normalized 0-1
                var t2Norm = timeRulers[1].Time;

                bool t1Inside = Helpers.IsBetween(t1Norm, phaseRuler1.Time, phaseRuler2.Time);
                bool t2Inside = Helpers.IsBetween(t2Norm, phaseRuler1.Time, phaseRuler2.Time);

                if (showPhaseRulers && t1Inside && t2Inside)
                {
                    float x1 = (t1Norm - phaseRuler1.Time) / (phaseRuler2.Time - phaseRuler1.Time) * 720f;
                    float x2 = (t2Norm - phaseRuler1.Time) / (phaseRuler2.Time - phaseRuler1.Time) * 720f;
                    cursorPanel.SetCursorValues("Time", x1, x2, "°", Color4.White);
                }
                else
                {
                    float? t1 = timeRulers[0].Active ? t1Norm * CurrentFileTimespan : (float?)null;
                    float? t2 = timeRulers[1].Active ? t2Norm * CurrentFileTimespan : (float?)null;
                    cursorPanel.SetCursorValues("Time", t1, t2, "s", Color4.White);
                }
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
                    RecalculateChannelOffsets();
                    InvalidateDrawingArea();
                    Invalidate();
                }
            };

            return ctrl;
        }

        internal RectangleF GetWorldBounds(WaveformChannel channel)
        {
            var zoom = GetEffectiveZoom();
            float timeStart = zoom.TimeStart;
            float timeWidth = zoom.TimeSpan;

            (float vMin, float vMax) = channel.GetVisibleVoltageRange();

            return new RectangleF(timeStart, vMin, timeWidth, vMax - vMin);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (d2dRenderTarget == null || swapChain == null)
                return;

            var context = new WaveformRenderContext
            {
                RenderTarget = d2dRenderTarget,
                D2DFactory = d2dFactory,
                WriteFactory = writeFactory,
                TextFormat = textFormat,
                DrawingArea = DrawingArea,
                Zoom = GetEffectiveZoom(),
                FileTimespan = CurrentFileTimespan,
                Channels = channels,
                TimeRulers = timeRulers,
                ShowPhaseRulers = showPhaseRulers,
                PhaseRuler1 = phaseRuler1,
                PhaseRuler2 = phaseRuler2,
                CursorOverlay = cursorPanel,
                IsZoomedIn = channels.Any(c => c.ZoomedVoltageMin.HasValue && c.ZoomedVoltageMax.HasValue),
                IsZoomingSelectionActive = ToolController?.IsZoomingSelectionActive ?? false,
                ZoomStart = ToolController?.ZoomSelectionStartPoint ?? new SharpDX.Point(),
                ZoomEnd = ToolController?.ZoomSelectionEndPoint ?? new SharpDX.Point()
            };

            waveformRenderer.Render(context);
            swapChain.Present(1, PresentFlags.None);
        }

        internal void SetZoomRegion(ZoomRegion zoom)
        {
            currentZoom = zoom;
        }

        protected override bool IsInputKey(Keys keyData) => true;

        public void ForwardKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            cursorPanel?.OnKeyPressed(e.KeyCode, e.Shift, e.Control);
        }

        private void WaveformViewer_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            cursorPanel?.OnKeyPressed(e.KeyCode, e.Shift, e.Control);
        }
    }
}