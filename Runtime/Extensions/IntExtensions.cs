using System;

namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public static class IntExtensions
    {


        /// <summary>
        /// 是否是4的幂
        /// </summary>
        public static bool IsPow4(this int value)
        {
            if (value <= 0) return false;
            return IsPow2(value) && (value & 0x55555555) != 0;
        }

        /// <summary>
        /// 是否是2的幂
        /// </summary>
        public static bool IsPow2(this int value)
        {
            if (value <= 0) return false;
            return (value & (value - 1)) == 0;
        }

        /// <summary>
        /// 取最接近4的倍数
        /// </summary>
        public static int GetMultipleNearest4(this int value)
        {
            return (int)(Math.Ceiling(value / 4.0f) * 4);
        }

        /// <summary>
        /// 是否是4的倍数
        /// </summary>
        public static bool IsMultiple4(this int value)
        {
            return value % 4 == 0;
        }

        
    }
}