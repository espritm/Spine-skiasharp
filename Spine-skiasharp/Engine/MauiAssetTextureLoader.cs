using SkiaSharp;
using Spine;

namespace Spine_skiasharp.Engine
{
    public class MauiAssetTextureLoader : TextureLoader
    {
        private readonly Dictionary<string, SKBitmap> textures = new Dictionary<string, SKBitmap>();

        public MauiAssetTextureLoader(Stream stream, string name)
        {
            textures[name] = SKBitmap.Decode(stream);
        }

        List<string> lsFiles;

        public MauiAssetTextureLoader(List<string> lsFiles) 
        {
            this.lsFiles = lsFiles != null ? new List<string>(lsFiles) : new List<string>();
        }

        public async Task LoadAssets()
        {
            foreach (string file in lsFiles)
            {
                Stream stream = await FileSystem.OpenAppPackageFileAsync(file);
                textures[file] = SKBitmap.Decode(stream);
            }
        }

        public SKBitmap GetTexture(string textureName)
        {
            return textures.TryGetValue(textureName, out var bitmap) ? bitmap : null;
        }

        public void Load(AtlasPage page, string name)
        {
            // Vérifie si la texture existe
            if (textures.TryGetValue(name, out var bitmap))
            {
                page.rendererObject = bitmap;
                page.width = bitmap.Width;
                page.height = bitmap.Height;
            }
            else
            {
                throw new System.Exception($"Texture not found: {name}");
            }
        }

        public void Unload(object texture)
        {
            if (texture is SKBitmap bitmap)
            {
                bitmap.Dispose();
            }
        }
    }
}