using Spine;
using SkiaSharp;
using BlendMode = Spine.BlendMode;

namespace Spine_skiasharp.Engine
{
    public class SpineSkiaSharpRenderer
    {
        private float[] vertices = new float[1024];

        public SpineSkiaSharpRenderer(Atlas atlas, MauiAssetTextureLoader textureLoader)
        {
        }

        public void DrawSkeleton(SKCanvas canvas, Skeleton skeleton)
        {
            if (skeleton == null) return;

            foreach (Slot slot in skeleton.DrawOrder)
            {
                Attachment attachment = slot.Attachment;
                if (attachment == null) continue;

                SKBitmap bitmap = null;
                float[] uvs = null;
                int[] triangles = null;
                int verticesCount = 0;
                float[] worldVertices = null;

                if (attachment is RegionAttachment region)
                {
                    var atlasRegion = region.Region as AtlasRegion;
                    if (atlasRegion?.page?.rendererObject is SKBitmap b)
                        bitmap = b;

                    if (bitmap == null) continue;

                    verticesCount = 4;
                    if (vertices.Length < 8) Array.Resize(ref vertices, 8);
                    region.ComputeWorldVertices(slot, vertices, 0, 2);
                    worldVertices = vertices;

                    uvs = region.UVs;
                    triangles = new int[] { 0, 1, 2, 2, 3, 0 };
                }
                else if (attachment is MeshAttachment mesh)
                {
                    var atlasRegion = mesh.Region as AtlasRegion;
                    if (atlasRegion?.page?.rendererObject is SKBitmap b)
                        bitmap = b;

                    if (bitmap == null) continue;

                    verticesCount = mesh.WorldVerticesLength / 2;
                    if (vertices.Length < mesh.WorldVerticesLength) Array.Resize(ref vertices, mesh.WorldVerticesLength);
                    mesh.ComputeWorldVertices(slot, 0, mesh.WorldVerticesLength, vertices, 0, 2);
                    worldVertices = vertices;

                    uvs = mesh.UVs;
                    triangles = mesh.Triangles;
                }
                else
                {
                    continue;
                }

                if (bitmap != null)
                {
                    SKBlendMode blendMode = SKBlendMode.SrcOver;
                    switch (slot.Data.BlendMode)
                    {
                        case BlendMode.Additive: blendMode = SKBlendMode.Plus; break;
                        case BlendMode.Multiply: blendMode = SKBlendMode.Multiply; break;
                        case BlendMode.Screen: blendMode = SKBlendMode.Screen; break;
                        default: blendMode = SKBlendMode.SrcOver; break;
                    }

                    using (var paint = new SKPaint())
                    {
                        paint.IsAntialias = true;
                        paint.FilterQuality = SKFilterQuality.High;
                        paint.BlendMode = blendMode;

                        float a = skeleton.A * slot.A * 255;
                        float r = skeleton.R * slot.R * 255;
                        float g = skeleton.G * slot.G * 255;
                        float b = skeleton.B * slot.B * 255;

                        paint.ColorFilter = SKColorFilter.CreateBlendMode(
                            new SKColor((byte)r, (byte)g, (byte)b, (byte)a), 
                            SKBlendMode.Modulate);


                        SKPoint[] skVertices = new SKPoint[verticesCount];
                        SKPoint[] skTexs = new SKPoint[verticesCount];

                        for (int i = 0; i < verticesCount; i++)
                        {
                            skVertices[i] = new SKPoint(worldVertices[i * 2], worldVertices[i * 2 + 1]);
                            skTexs[i] = new SKPoint(uvs[i * 2] * bitmap.Width, uvs[i * 2 + 1] * bitmap.Height);
                        }

                        ushort[] indices = new ushort[triangles.Length];
                        for (int i = 0; i < triangles.Length; i++) indices[i] = (ushort)triangles[i];

                        using (var shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp))
                        {
                            paint.Shader = shader;
                            canvas.DrawVertices(SKVertexMode.Triangles, skVertices, skTexs, null, blendMode, indices, paint);
                        }
                    }
                }
            }
        }
    }
}
