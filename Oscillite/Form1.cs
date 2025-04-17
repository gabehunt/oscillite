using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oscillite;
using SharpDX;

namespace Oscillite
{
    public partial class Form1 : Form
    {
        private WaveformViewer waveformViewer;

        public Form1()
        {
            InitializeComponent();
            waveformViewer = new WaveformViewer();
            waveformViewer.Dock = DockStyle.Fill;

            using (var splash = new SplashForm())
            {
                splash.Show();
                splash.Refresh();
                System.Threading.Thread.Sleep(1000);
                this.Controls.Add(waveformViewer);
                splash.Hide();
            }
        }

    }
}