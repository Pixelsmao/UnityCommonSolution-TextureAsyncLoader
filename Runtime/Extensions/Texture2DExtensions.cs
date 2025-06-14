using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public static class Texture2DExtensions
    {
        /// <summary>
        /// 是否为四元对齐纹理：符合此标准的纹理支持GPU纹理压缩
        /// </summary>
        public static bool IsQuadAlignedTexture(this Texture2D texture)
        {
            return texture.width.IsMultiple4() && texture.height.IsMultiple4();
        }

        /// <summary>
        /// 是否为二元对齐纹理：符合此标准的纹理支持GPU纹理压缩
        /// </summary>
        public static bool IsDyadicAlignedTexture(this Texture2D texture)
        {
            return texture.width.IsPow2() && texture.height.IsPow2();
        }

        /// <summary>
        /// 纹理纵横比
        /// </summary>
        /// <param name="texture">源纹理</param>
        /// <returns></returns>
        public static float GetAspectRatio(this Texture2D texture)
        {
            return (float)texture.width / texture.height;
        }

        public static int GetMaxLengthSide(this Texture2D texture)
        {
            return Mathf.Max(texture.width, texture.height);
        }

        /// <summary>
        /// 检查Texture2D是否为压缩纹理格式
        /// </summary>
        public static bool IsCompressedTexture(this Texture2D texture)
        {
            if (texture == null) return false;
            switch (texture.format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5:
                case TextureFormat.DXT5Crunched:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC_RGB4Crunched:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC2_RGBA8Crunched:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// 是否为支持纹理转换的格式
        /// </summary>
        public static bool IsSupportedForConvertTexture(this Texture2D texture)
        {
            // Step 1: 获取 TextureFormat 对应的 GraphicsFormat
            var graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(texture.format, texture.mipmapCount > 1);

            // Step 2: 检查该格式是否可用作 RenderTarget (FormatUsage.Render)
            return SystemInfo.IsFormatSupported(graphicsFormat, FormatUsage.Render);
        }

        /// <summary>
        /// 创建可读纹理：创建一个可读的 Texture2D 对象，该对象与原始纹理具有相同的大小和格式。
        /// </summary>
        public static bool GetReadable(this Texture2D texture, out Texture2D readableTexture)
        {
            readableTexture = null;
            if (texture.isReadable)
            {
                readableTexture = texture;
                return true;
            }

            var renderTexture =
                RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default);
            Graphics.Blit(texture, renderTexture);
            var newTexture = new Texture2D(texture.width, texture.height, texture.format, false);
            var previousRT = RenderTexture.active;
            try
            {
                RenderTexture.active = renderTexture;
                newTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                newTexture.Apply();
                readableTexture = newTexture;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to set texture readable: " + ex.Message);
                readableTexture = null;
                return false;
            }
            finally
            {
                RenderTexture.active = previousRT;
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            return true;
        }


        /// <summary>
        /// 纹理是否包含Alpha通道
        /// </summary>
        public static bool HasAlphaChannel(this Texture2D texture)
        {
            switch (texture.format)
            {
                case TextureFormat.RGBA32:
                case TextureFormat.RGBA4444:
                case TextureFormat.ARGB32:
                case TextureFormat.DXT5:
                case TextureFormat.ETC2_RGBA8:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 纹理转换重采样：使用纹理转换的方式创建和源纹理同样大小的重采样纹理，源格式如果不支持纹理转换则重采样纹理为RGBA32格式。
        /// </summary>
        /// <param name="texture2D">源纹理</param>
        /// <param name="resampleTexture">重采样纹理</param>
        /// <returns></returns>
        public static bool ResampleConvertTexture(this Texture2D texture2D, out Texture2D resampleTexture)
        {
            var format = texture2D.IsSupportedForConvertTexture() ? texture2D.format : TextureFormat.RGBA32;
            var tempTexture = new Texture2D(texture2D.width, texture2D.height, format, false);
            if (!Graphics.ConvertTexture(texture2D, tempTexture))
            {
                resampleTexture = texture2D;
                return false;
            }

            resampleTexture = tempTexture;
            Resources.UnloadUnusedAssets();
            return true;
        }

        /// <summary>
        /// 纹理转换重采样：使用纹理转换的方式创建指定大小的重采样纹理，源格式如果不支持纹理转换则重采样纹理为RGBA32格式。
        /// </summary>
        /// <param name="texture2D">源纹理</param>
        /// <param name="resampleSize">重采样大小</param>
        /// <param name="resampleTexture">重采样纹理</param>
        /// <returns></returns>
        public static bool ResampleConvertTexture(this Texture2D texture2D, Vector2Int resampleSize,
            out Texture2D resampleTexture)
        {
            var format = texture2D.IsSupportedForConvertTexture() ? texture2D.format : TextureFormat.RGBA32;
            var tempTexture = new Texture2D(resampleSize.x, resampleSize.y, format, false);
            if (!Graphics.ConvertTexture(texture2D, tempTexture))
            {
                resampleTexture = texture2D;
                return false;
            }

            resampleTexture = tempTexture;
            Resources.UnloadUnusedAssets();
            return true;
        }


        /// <summary>
        /// 纹理压缩：使用像素操作的方式创建可压缩纹理，纹理重采样到原始尺寸的4的倍数。
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="compressedTexture"></param>
        /// <returns></returns>
        public static bool CompressTexture(this Texture2D texture2D, out Texture2D compressedTexture)
        {
            if (texture2D.ResampleMultipleNearest4(out var resamplingTexture))
            {
                resamplingTexture.Compress(true);
                compressedTexture = resamplingTexture;
                Resources.UnloadUnusedAssets();
                return true;
            }

            compressedTexture = null;
            return false;
        }

        /// <summary>
        /// 重采样并压缩纹理：使用像素操作的方式创建可压缩重采样纹理，纹理保持纵横比，同时最大边不超过指定自适应尺寸。
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="adaptiveMaxSize">自适应最大尺寸</param>
        /// <param name="compressedTexture">压缩纹理</param>
        /// <returns></returns>
        public static bool ResampleAlsoCompressTexture(this Texture2D texture2D, int adaptiveMaxSize,
            out Texture2D compressedTexture)
        {
            if (texture2D.ResampleMultipleNearest4(adaptiveMaxSize, out var resamplingTexture))
            {
                resamplingTexture.Compress(true);
                compressedTexture = resamplingTexture;
                return true;
            }

            compressedTexture = null;
            return false;
        }

        /// <summary>
        /// 重采样纹理：使用像素操作的方式创建指定4的倍数大小重采样纹理。
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="resampleTexture"></param>
        public static bool ResampleMultipleNearest4(this Texture2D texture, out Texture2D resampleTexture)
        {
            var textureSize = new Vector2Int(texture.width, texture.height);
            var targetSize = textureSize.IsMultiple4() ? textureSize : textureSize.ToMultipleNearest4();
            if (texture.Resample(targetSize, out resampleTexture)) return true;
            resampleTexture = null;
            return false;
        }

        /// <summary>
        /// 重采样纹理：使用像素操作的方式创建指定4的倍数大小重采样纹理。
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="adaptiveMaxSize"></param>
        /// <param name="resampleTexture"></param>
        public static bool ResampleMultipleNearest4(this Texture2D texture, int adaptiveMaxSize,
            out Texture2D resampleTexture)
        {
            var textureSize = new Vector2Int(texture.width, texture.height);
            var targetSize = textureSize.IsMultiple4() ? textureSize : textureSize.ToMultipleNearest4(adaptiveMaxSize);
            if (texture.Resample(targetSize, out resampleTexture)) return true;
            resampleTexture = null;
            return false;
        }

        private static bool Resample(this Texture2D texture, Vector2Int resampleSize, out Texture2D resampleTexture)
        {
            try
            {
                // 创建一个临时的RenderTexture，用于缩放操作
                var tempRenderTexture =
                    RenderTexture.GetTemporary(resampleSize.x, resampleSize.y, 0, RenderTextureFormat.Default);

                // 将originalTexture的内容缩放到tempRenderTexture中
                Graphics.Blit(texture, tempRenderTexture);

                // 设置纹理格式
                var format = HasAlphaChannel(texture) ? TextureFormat.RGBA32 : TextureFormat.RGB24;

                // 创建一个新的Texture2D，用于存储缩放后的结果
                var resamplingTexture = new Texture2D(resampleSize.x, resampleSize.y, format, false);

                // 保存当前的RenderTexture
                var previousRT = RenderTexture.active;

                // 设置当前的RenderTexture为tempRenderTexture
                RenderTexture.active = tempRenderTexture;

                // 从tempRenderTexture读取像素到compressedTexture
                resamplingTexture.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);

                // 应用更改
                resamplingTexture.Apply();

                // 恢复之前的RenderTexture
                RenderTexture.active = previousRT;

                // 释放临时的RenderTexture
                RenderTexture.ReleaseTemporary(tempRenderTexture);
                resampleTexture = resamplingTexture;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                resampleTexture = null;
                return false;
            }
        }
    }
}