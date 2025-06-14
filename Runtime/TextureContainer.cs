using UnityEngine;

namespace Pixelsmao.UnityCommonSolution.TextureAsyncLoader
{
    public class TextureContainer
    {
        public Texture2D texture { get; private set; }

        public void SetTexture(Texture2D texture2D)
        {
            texture = texture2D;
        }
    }
}