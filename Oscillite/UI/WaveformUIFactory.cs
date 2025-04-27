using System.Drawing;
using System.Windows.Forms;

namespace Oscillite.UI
{
    public static class WaveformUIFactory
    {
        public static FlowLayoutPanel CreateLeftPanel(WaveformViewer owner)
        {
            var leftPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10),
                WrapContents = false,
                BackColor = Color.Black
            };

            leftPanel.Controls.Add(CreateLogo());
            leftPanel.Controls.Add(owner.CreateStyledButton("\uE8B7 Open File", (s, e) => owner.OpenFile(), new Padding(10)));

            var zoomButton = owner.CreateStyledButton("\uE8A4 Zoom Mode", owner.ToggleToolMode);
            owner.SetModeToggleButton(zoomButton);
            leftPanel.Controls.Add(zoomButton);

            leftPanel.Controls.Add(owner.CreateStyledButton("\uE713 Phase Rulers", owner.TogglePhaseRulers));


            var channelPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Left,
                Width = 70,
                BackColor = Color.Black,
                Padding = new Padding(5),
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            owner.SetChannelPanel(channelPanel); // ✅ Set internal reference
            leftPanel.Controls.Add(channelPanel);

            return leftPanel;
        }


        private static PictureBox CreateLogo()
        {
            return new PictureBox
            {
                Size = new Size(125, 125),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Helpers.LoadEmbeddedImage("Oscillite.Oscillite.png"),
                Margin = new Padding(5),
                BackColor = Color.Transparent
            };
        }
    }
}
