using System;
using System.Drawing;
using System.IO;
using System.Reflection;
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

        public static string GetColorName(RawColor4 color)
        {
            if (color.R > 0.7f && color.G > 0.7f && color.B < 0.2f) return "Yellow";
            if (color.G > 0.7f && color.R < 0.2f && color.B < 0.2f) return "Green";
            if (color.B > 0.7f && color.R < 0.2f && color.G < 0.2f) return "Blue";
            if (color.R > 0.7f && color.G < 0.2f && color.B < 0.2f) return "Red";
            return "Channel";
        }


        public static byte ClampToByte(float value)
        {
            return (byte)Math.Max(0, Math.Min(255, (int)value));
        }

        public static int ClampToRange(int value, float min, float max)
        {
            return Math.Max((int)min, Math.Min((int)max, value));
        }

        public static ViewportTransform CreateTimeTransform(ZoomRegion zoom, SharpDX.RectangleF drawingArea)
        {
            var world = new SharpDX.RectangleF(zoom.TimeStart, 0, zoom.TimeSpan, 1);
            return new ViewportTransform(world, drawingArea);
        }

        public static ViewportTransform CreateTransformForChannel(SharpDX.RectangleF world, SharpDX.RectangleF drawingArea)
        {
            return new ViewportTransform(world, drawingArea);
        }

        public static bool IsBetween(float value, float a, float b)
        {
            if (a > b) (a, b) = (b, a);
            return value >= a && value <= b;
        }
    }
}
