using System;
using SharpDX;

namespace Oscillite
{
    public static class DataUtilities
    {
        private const int MAX_POINTS_PER_SECOND = 10000;

        public static Vector2[] DecimateData(Vector2[] data, float currentFileTimespan)
        {
            int totalPoints = (int)(currentFileTimespan * MAX_POINTS_PER_SECOND);

            if (data == null || data.Length <= MAX_POINTS_PER_SECOND)
                return data;

            int reduction = data.Length / totalPoints;
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
