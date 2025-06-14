using UnityEngine;

namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public static class Vector2IntExtensions
    {
        public static bool IsMultiple4(this Vector2Int vector2)
        {
            return vector2.x.IsMultiple4() && vector2.y.IsMultiple4();
        }

        public static Vector2Int ToMultipleNearest4(this Vector2Int vector2)
        {
            return new Vector2Int(vector2.x.GetMultipleNearest4(), vector2.y.GetMultipleNearest4());
        }

        public static Vector2Int ToMultipleNearest4(this Vector2Int vector2, int adaptiveMaxSize)
        {
            var ceilMultipleOf4Vector2 = vector2.ToMultipleNearest4();
            var ceilMultipleOf4MaxValue = adaptiveMaxSize.GetMultipleNearest4();
            var ratio = (float)ceilMultipleOf4Vector2.x / ceilMultipleOf4Vector2.y;
            // 判断原始尺寸是否超过最大尺寸
            if (ceilMultipleOf4Vector2.x > ceilMultipleOf4MaxValue ||
                ceilMultipleOf4Vector2.y > ceilMultipleOf4MaxValue)
            {
                if (ratio > 1)
                {
                    return new Vector2Int(ceilMultipleOf4MaxValue,
                        ((int)(ceilMultipleOf4MaxValue / ratio)).GetMultipleNearest4());
                }

                if (ratio < 1)
                {
                    return new Vector2Int(((int)(ceilMultipleOf4MaxValue * ratio)).GetMultipleNearest4(),
                        ceilMultipleOf4MaxValue);
                }

                return new Vector2Int(ceilMultipleOf4MaxValue, ceilMultipleOf4MaxValue);
            }

            return ceilMultipleOf4Vector2;
        }
    }
}