using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public static class TextureAsyncLoader
    {
        private static AsyncImageLoader.LoaderSettings loaderSettings = AsyncImageLoader.LoaderSettings.Default;

        public static Texture2D LoadTexture2D(string texturePath)
        {
            var textureData = File.ReadAllBytes(texturePath);
            return AsyncImageLoader.CreateFromImage(textureData);
        }

        public static async Task<Texture2D> LoadTexture2DAsync(string texturePath)
        {
            var textureData = await File.ReadAllBytesAsync(texturePath);
            return await AsyncImageLoader.CreateFromImageAsync(textureData);
        }

        public static Texture2D LoadTexture2D(FileInfo textureFileInfo)
        {
            return LoadTexture2D(textureFileInfo.FullName);
        }

        public static async Task<Texture2D> LoadTexture2DAsync(FileInfo textureFileInfo)
        {
            return await LoadTexture2DAsync(textureFileInfo.FullName);
        }


        public static async Task<Texture2D> DownloadTexture2DAsync(Uri textureUri)
        {
            var container = new TextureContainer();
            await DownloadTexture2D(textureUri, container).ToUniTask();
            return container.texture;
        }


        public static IEnumerator DownloadTexture2D(Uri textureUri, TextureContainer container)
        {
            var request = UnityWebRequestTexture.GetTexture(textureUri.AbsoluteUri);
            yield return request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }


            // Todo 此方法在转换为纹理时依然会阻塞主线程
            var texture = DownloadHandlerTexture.GetContent(request);
            container.SetTexture(texture);

            //  Todo 此方法因为无法识别图片格式，所以无法转换
            // var createdTextureTask = AsyncImageLoader.CreateFromImageAsync(request.downloadHandler.data, loaderSettings);
            // while (!createdTextureTask.IsCompleted)
            // {
            //     yield return null;
            // }
            //
            // container.SetTexture(createdTextureTask.Result);
        }
    }
}