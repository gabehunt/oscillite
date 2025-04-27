using System;
using System.Drawing;
using System.Windows.Forms;
using Oscillite;

namespace Oscillite
{
    public partial class ChannelControl : UserControl
    {
        private WaveformChannel Channel;

        public Action<int, float> OnScaleChanged;
        private const float MinScale = 0.1f;
        private const float MaxScale = 5.0f;
        private const float ScaleStep = 0.1f;
        public Action<int, bool> OnToggleVisibility;
        private Label topRow;

        public ChannelControl(WaveformChannel channel)
        {
            InitializeComponent();

            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.Black;
            ForeColor = Helpers.ToWinFormsColor(channel.Color);
            Width = 120;    
            Height = 50;

            topRow = new Label
            {
                Text = $"CH{channel.Name}  {channel.MaxExpectedVoltage}{channel.Unit}",
                ForeColor = Helpers.ToWinFormsColor(channel.Color),
                BackColor = Color.FromArgb(30, 30, 30),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 20,
                Font = new Font("Consolas", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            topRow.Click += (s, e) =>
            {
                channel.Visible = !channel.Visible;
                OnToggleVisibility?.Invoke(channel.Index, channel.Visible);
                topRow.ForeColor = channel.Visible ? Helpers.ToWinFormsColor(channel.Color) : Color.Gray;
                topRow.Text = channel.Visible ? $"{channel.Name}  {channel.MaxExpectedVoltage}{channel.Unit}" : $"{channel.Name} (Off)";
            };

            var minus = new Button
            {
                Text = "-",
                ForeColor = Helpers.ToWinFormsColor(channel.Color),
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Left,
                Cursor = Cursors.Hand
            };
            minus.FlatAppearance.BorderColor = Helpers.ToWinFormsColor(channel.Color);
            minus.FlatAppearance.BorderSize = 1;

            var scaleLabel = new Label
            {
                Text = channel.Scale.ToString("0.0"),
                ForeColor = Helpers.ToWinFormsColor(channel.Color),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var plus = new Button
            {
                Text = "+",
                ForeColor = Helpers.ToWinFormsColor(channel.Color),
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand
            };
            plus.FlatAppearance.BorderColor = Helpers.ToWinFormsColor(channel.Color);
            plus.FlatAppearance.BorderSize = 1;

            var rowPanel = new TableLayoutPanel
            {
                Height = 30,
                Dock = DockStyle.Bottom,
                ColumnCount = 3,
                RowCount = 1,
            };
            rowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
            rowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

            rowPanel.Controls.Add(minus, 0, 0);
            rowPanel.Controls.Add(scaleLabel, 1, 0);
            rowPanel.Controls.Add(plus, 2, 0);

            Controls.Add(topRow);
            Controls.Add(rowPanel);

            minus.Click += (s, e) =>
            {
                channel.Scale = Math.Max(MinScale, channel.Scale - ScaleStep);
                scaleLabel.Text = channel.Scale.ToString("0.0");
                OnScaleChanged?.Invoke(channel.Index, channel.Scale);
            };

            plus.Click += (s, e) =>
            {
                channel.Scale = Math.Min(MaxScale, channel.Scale + ScaleStep);
                scaleLabel.Text = channel.Scale.ToString("0.0");
                OnScaleChanged?.Invoke(channel.Index, channel.Scale);
            };
            this.Channel = channel;
        }

        public void SetEnabledState(bool enabled)
        {
            topRow.Text = Channel.Visible ? $"{Channel.Name}  {Channel.MaxExpectedVoltage}{Channel.Unit}" : $"{Channel.Name} (Off)";
            topRow.ForeColor = Channel.Visible ? Helpers.ToWinFormsColor(Channel.Color) : Color.Gray;
        }
    }
}
