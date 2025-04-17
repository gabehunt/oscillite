using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oscillite;

namespace Oscillite
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"This program must be run from shopsctream connect folder. {ex.Message}");
            }
        }

    }
}
