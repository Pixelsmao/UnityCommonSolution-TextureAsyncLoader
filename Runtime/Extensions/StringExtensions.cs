using System;
using System.IO;

namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public static class StringExtensions
    {
        public static bool IsNetworkPath(this string path)
        {
            var uri = new Uri(path);
            return uri.IsAbsoluteUri &&
                   (uri.Scheme == Uri.UriSchemeHttp ||
                    uri.Scheme == Uri.UriSchemeHttps ||
                    uri.Scheme == Uri.UriSchemeFtp ||
                    uri.Scheme == Uri.UriSchemeFile);
        }

        public static bool IsLocalPath(this string path)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}