using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oscillite
{
    public partial class SplashForm : Form
    {
        public SplashForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black; // Or any background you want
            this.ShowInTaskbar = false;

            var picture = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = Helpers.LoadEmbeddedImage("Oscillite.Oscillite.png"),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            this.Controls.Add(picture);
            this.ClientSize = new Size(600, 600); // Adjust to your logo
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

    }

}
