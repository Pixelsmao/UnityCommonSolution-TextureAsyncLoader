using System;
using System.IO;
using UnityEngine;

namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public class Texture2DInfo : IDisposable
    {
        private FileInfo _fileInfo;
        private Uri _uri;
        private byte[] _texturePNGData;
        private byte[] _textureEXRData;
        private byte[] _textureJPGData;
        private byte[] _textureTGAData;

        #region public Properties

        public bool isUsable => mainTexture != null;

        /// <summary>
        /// 主要纹理
        /// </summary>
        public Texture2D mainTexture { get; private set; }

        /// <summary>
        /// 纹理文件信息
        /// </summary>
        public FileInfo fileInfo
        {
            get
            {
                if (_fileInfo != null) return _fileInfo;
                Debug.LogWarning("纹理来源不是本地计算机无法提供文件系统信息!");
                return null;
            }
            set => _fileInfo = value;
        }

        /// <summary>
        /// 纹理uri信息
        /// </summary>
        public Uri uri
        {
            get
            {
                if (_uri != null) return _uri;
                Debug.LogWarning("纹理来源不是通过远程下载无法提供Uri信息!");
                return null;
            }
            set => _uri = value;
        }

        /// <summary>
        /// 是否为四元对齐纹理：符合此标准的纹理支持GPU纹理压缩
        /// </summary>
        public bool isQuadAlignedTexture => isUsable && mainTexture.IsQuadAlignedTexture();

        /// <summary>
        /// 是否为二元对齐纹理
        /// </summary>
        public bool isDyadicAlignedTexture => isUsable && mainTexture.IsDyadicAlignedTexture();

        /// <summary>
        /// 是否为压缩纹理
        /// </summary>
        public bool isCompressedTexture => isUsable && mainTexture.IsCompressedTexture();

        /// <summary>
        /// 原始纹理数据
        /// </summary>
        public byte[] rawTextureData => isUsable ? mainTexture.GetRawTextureData() : null;

        /// <summary>
        /// 纹理纵横比
        /// </summary>
        public float aspectRatio => mainTexture.GetAspectRatio();

        /// <summary>
        /// 是否为横图
        /// </summary>
        public bool isHorizontal => aspectRatio > 1;

        /// <summary>
        /// 是否为竖图
        /// </summary>
        public bool isVertical => aspectRatio < 1;

        /// <summary>
        /// 纹理文件的名称(含扩展名)
        /// </summary>
        public string textureName => _fileInfo == null ? string.Empty : _fileInfo.Name;

        /// <summary>
        /// 纹理文件的名称(不含扩展名)
        /// </summary>
        public string textureNameWithoutExtension =>
            _fileInfo == null ? string.Empty : Path.GetFileNameWithoutExtension(_fileInfo.FullName);

        /// <summary>
        /// 纹理扩展名
        /// </summary>
        public string extensionName => _fileInfo == null ? string.Empty : _fileInfo.Extension;

        /// <summary>
        /// 纹理所在目录
        /// </summary>
        public string textureDirectory =>
            _fileInfo == null ? string.Empty : Path.GetDirectoryName(_fileInfo.FullName);

        /// <summary>
        /// PNG纹理数据
        /// </summary>
        public byte[] pngTextureData
        {
            get
            {
                if (mainTexture == null) return null;
                _texturePNGData ??= mainTexture.EncodeToPNG();
                return _texturePNGData;
            }
        }

        /// <summary>
        /// EXR纹理数据
        /// </summary>
        public byte[] exrTextureData
        {
            get
            {
                if (mainTexture == null) return null;
                _textureEXRData ??= mainTexture.EncodeToEXR();
                return _textureEXRData;
            }
        }

        /// <summary>
        /// JPG纹理数据
        /// </summary>
        public byte[] jpgTextureData
        {
            get
            {
                if (mainTexture == null) return null;
                _textureJPGData ??= mainTexture.EncodeToJPG();
                return _textureJPGData;
            }
        }

        /// <summary>
        /// TGA纹理数据
        /// </summary>
        public byte[] tgaTextureData
        {
            get
            {
                if (mainTexture == null) return null;
                _textureTGAData ??= mainTexture.EncodeToTGA();
                return _textureTGAData;
            }
        }

        #endregion


        public Texture2DInfo(Texture2D texture)
        {
            mainTexture = texture;
        }


        /// <summary>
        /// 从路径加载纹理：当使用异步加载时，使用前请检查isUsable属性。
        /// </summary>
        /// <param name="texturePath">纹理路径</param>
        /// <param name="async">是否异步加载</param>
        public Texture2DInfo(string texturePath, bool async)
        {
            if (texturePath.IsNetworkPath())
            {
                uri = new Uri(texturePath);
                DownloadTexture();
            }
            else
            {
                fileInfo = new FileInfo(texturePath);
                LoadTexture(async);
            }
        }

        /// <summary>
        /// 从文件信息加载纹理：当使用异步加载时，使用前请检查isUsable属性。
        /// </summary>
        /// <param name="textureFileInfo">纹理文件信息</param>
        /// <param name="async">是否异步加载</param>
        public Texture2DInfo(FileInfo textureFileInfo, bool async)
        {
            this.fileInfo = textureFileInfo;
            LoadTexture(async);
        }

        /// <summary>
        /// 从Uri加载纹理：此方法将异步下载纹理，使用前请检查isUsable属性。
        /// </summary>
        /// <param name="textureUri"></param>
        public Texture2DInfo(Uri textureUri)
        {
            this.uri = textureUri;
            DownloadTexture();
        }

        /// <summary>
        /// 压缩纹理：使用此方法压缩纹理将覆盖mainTexture，若要保留原始纹理请使用mainTexture的CompressTexture扩展方法。
        /// 当使用异步方式创建Texture2DInfo对象时，使用前请检查isUsable属性。
        /// </summary>
        public void CompressTexture()
        {
            if (!isUsable) return;
            if (!mainTexture.CompressTexture(out var compressedTextures)) return;
            mainTexture = compressedTextures;
        }

        /// <summary>
        /// 重采样并压缩纹理：使用此方法压缩纹理将覆盖mainTexture，若要保留原始纹理请使用mainTexture的ResampleAlsoCompressTexture扩展方法。
        /// 当使用异步方式创建Texture2DInfo对象时，使用前请检查isUsable属性。
        /// </summary>
        /// <param name="adaptiveMaxSize">自适应最大尺寸</param>
        public void ResampleAlsoCompressTexture(int adaptiveMaxSize)
        {
            if (!isUsable) return;
            if (!mainTexture.ResampleAlsoCompressTexture(adaptiveMaxSize, out var compressedTextures)) return;
            mainTexture = compressedTextures;
        }

        #region Save As

        public void SaveAsPNG(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".png", pngTextureData, false);
        }

        public void SaveAsPNGAsync(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".png", pngTextureData, true);
        }


        public void SaveAsEXR(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".exr", exrTextureData, false);
        }

        public void SaveAsEXRAsync(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".exr", exrTextureData, true);
        }

        public void SaveAsJPG(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".jpg", jpgTextureData, false);
        }

        public void SaveAsJPGAsync(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".jpg", jpgTextureData, true);
        }

        public void SaveAsTGA(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".tga", tgaTextureData, false);
        }

        public void SaveAsTGAAsync(string saveDirectory, string saveName)
        {
            SaveAs(saveDirectory, saveName, ".tga", tgaTextureData, true);
        }

        private static void SaveAs(string directory, string fileName, string extension, byte[] data, bool async)
        {
            var savePath = Path.Combine(directory, fileName + extension);
            if (async) File.WriteAllBytesAsync(savePath, data);
            else File.WriteAllBytes(savePath, data);
        }

        #endregion

        private async void LoadTexture(bool async)
        {
            mainTexture = async
                ? await TextureAsyncLoader.LoadTexture2DAsync(fileInfo)
                : TextureAsyncLoader.LoadTexture2D(fileInfo);
        }

        private async void DownloadTexture()
        {
            mainTexture = await TextureAsyncLoader.DownloadTexture2DAsync(uri);
        }

        /// <summary>
        /// 销毁纹理对象：销毁纹理对象并释放引用
        /// </summary>
        public void Destroy()
        {
            if (isUsable) UnityEngine.Object.Destroy(mainTexture);
            Dispose();
        }

        /// <summary>
        /// 释放纹理对象的引用，此方法不会销毁纹理对象
        /// </summary>
        public void Dispose()
        {
            mainTexture = null;
            _texturePNGData = null;
            _textureEXRData = null;
            _textureJPGData = null;
            _textureTGAData = null;
            Resources.UnloadUnusedAssets();
        }
    }
}