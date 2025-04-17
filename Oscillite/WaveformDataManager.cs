using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Oscillite
{
    public class WaveformDataManager
    {
        private const int MAX_POINTS_PER_SCREEN = 2000;

        public Vector2[] DecimateData(Vector2[] data, float viewportWidth)
        {
            if (data == null || data.Length <= MAX_POINTS_PER_SCREEN)
                return data;

            int reduction = data.Length / MAX_POINTS_PER_SCREEN;
            var decimated = new Vector2[data.Length / reduction];

            for (int i = 0; i < decimated.Length; i++)
            {
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                int startIdx = i * reduction;
                int endIdx = Math.Min(startIdx + reduction, data.Length);

                for (int j = startIdx; j < endIdx; j++)
                {
                    minY = Math.Min(minY, data[j].Y);
                    maxY = Math.Max(maxY, data[j].Y);
                }

                decimated[i] = new Vector2(
                    data[startIdx].X,
                    (minY + maxY) / 2
                );
            }

            return decimated;
        }
    }

}
