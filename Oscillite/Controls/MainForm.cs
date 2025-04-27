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
    public partial class MainForm : Form
    {
        private WaveformViewer waveformViewer;

        public MainForm()
        {
            InitializeComponent();

            this.ControlAdded += (s, e) =>
            {
                if (e.Control is Button btn)
                {
                    btn.Enter += (s2, e2) =>
                    {
                        // Prevent that button from becoming AcceptButton when it gains focus
                        this.AcceptButton = null;
                    };
                }
            };

            this.KeyPreview = true;
            waveformViewer = new WaveformViewer();
            waveformViewer.Dock = DockStyle.Fill;
            this.BringToFront();
            using (var splash = new SplashForm())
            {
                splash.Show();
                splash.Refresh();
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                this.Controls.Add(waveformViewer);
                splash.Hide();
                waveformViewer.OpenFile();
            }

            DisableAcceptButtonBehavior(this);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // Suppress AcceptButton default behavior when editing
            if (keyData == Keys.Enter)
            {
                var args = new KeyEventArgs(keyData);
                waveformViewer.ForwardKeyDown(args);
                return true; // Suppress AcceptButton
            }

            return base.ProcessDialogKey(keyData);
        }

        private void DisableAcceptButtonBehavior(Control root)
        {
            foreach (Control ctrl in root.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.Enter += (s, e) => this.AcceptButton = null;
                }

                // Recurse into child containers
                if (ctrl.HasChildren)
                {
                    DisableAcceptButtonBehavior(ctrl);
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Force focus
            this.TopMost = true;
            this.Activate();
            this.BringToFront();
            this.Focus();
            this.TopMost = false;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            waveformViewer.ForwardKeyDown(e);
        }
    }
}