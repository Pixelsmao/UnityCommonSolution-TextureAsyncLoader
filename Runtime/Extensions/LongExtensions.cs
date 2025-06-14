namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public static class LongExtensions
    {
        private const float scale = 1024.0f;

        public static float ToKB(this long value)
        {
            return value / scale;
        }

        public static float ToMB(this long value)
        {
            return value / (scale * scale);
        }

        public static float ToTB(this long value)
        {
            return value / (scale * scale * scale);
        }
    }
}