using SharpDX;

namespace Oscillite
{
    public struct ViewportTransform
    {
        public RectangleF WorldBounds;
        public RectangleF ViewportBounds;

        public ViewportTransform(RectangleF world, RectangleF view)
        {
            WorldBounds = world;
            ViewportBounds = view;
        }

        public float WorldToScreenX(float worldX)
        {
            float normX = (worldX - WorldBounds.Left) / WorldBounds.Width;
            return ViewportBounds.Left + normX * ViewportBounds.Width;
        }

        public float WorldToScreenY(float worldY)
        {
            float normY = 1f - ((worldY - WorldBounds.Top) / WorldBounds.Height);
            return ViewportBounds.Top + normY * ViewportBounds.Height;
        }

        public float ScreenToWorldX(float screenX)
        {
            float normX = (screenX - ViewportBounds.Left) / ViewportBounds.Width;
            return WorldBounds.Left + normX * WorldBounds.Width;
        }

        public float ScreenToWorldY(float screenY)
        {
            float normY = (screenY - ViewportBounds.Top) / ViewportBounds.Height;
            return WorldBounds.Top + (1f - normY) * WorldBounds.Height;
        }

        public Vector2 WorldToScreen(Vector2 worldPoint)
        {
            return new Vector2(WorldToScreenX(worldPoint.X), WorldToScreenY(worldPoint.Y));
        }

        public Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            return new Vector2(ScreenToWorldX(screenPoint.X), ScreenToWorldY(screenPoint.Y));
        }
    }

}
