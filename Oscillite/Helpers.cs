using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;

namespace Oscillite
{
    public class Helpers
    {
        public static Image LoadEmbeddedImage(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                return Image.FromStream(stream);
            }
        }

        public static System.Drawing.Color ToWinFormsColor(RawColor4 color)
        {
            return System.Drawing.Color.FromArgb(
                ClampToByte(color.A * 255),
                ClampToByte(color.R * 255),
                ClampToByte(color.G * 255),
                ClampToByte(color.B * 255)
            );
        }

        public static byte ClampToByte(float value)
        {
            return (byte)Math.Max(0, Math.Min(255, (int)value));
        }
    }
}
