using Spine;
using SkiaSharp;

namespace Spine_skiasharp.Engine
{
    public class SpineSkiaSharpRenderer
    {
        private readonly Atlas atlas;
        private readonly MauiAssetTextureLoader textureLoader;

        public SpineSkiaSharpRenderer(Atlas atlas, MauiAssetTextureLoader textureLoader)
        {
            this.textureLoader = textureLoader;
        }

        public void DrawSkeleton(SKCanvas canvas, Skeleton skeleton)
        {
            foreach (Slot slot in skeleton.DrawOrder)
            {
                Attachment attachment = slot.Attachment;
                if (attachment is RegionAttachment regionAttachment)
                {
                    DrawRegionAttachment(canvas, regionAttachment, slot);
                }
                else if (attachment is MeshAttachment meshAttachment)
                {
                    DrawMeshAttachment(canvas, meshAttachment, slot);
                }
            }
        }

        private void DrawRegionAttachment(SKCanvas canvas, RegionAttachment regionAttachment, Slot slot)
        {
            Bone bone = slot.Bone;

            // Calculate position and cumulated rotation
            float worldX = bone.WorldX;
            float worldY = bone.WorldY;
            float rotation = bone.WorldRotationX + regionAttachment.Rotation;
            float scaleX = bone.WorldScaleX * regionAttachment.ScaleX;
            float scaleY = bone.WorldScaleY * regionAttachment.ScaleY;

            // Convert rotation to rad
            float radians = rotation * MathF.PI / 180f;
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            // Calculate world verticies
            float offsetX = regionAttachment.X;
            float offsetY = regionAttachment.Y;
            float renderX = worldX + (offsetX * cos - offsetY * sin);
            float renderY = worldY - (offsetX * sin - offsetY * cos); // Should inverse Y axis for Skiasharp

            // Retrieve the texture from the texture loader
            string textureName = "spineboy-ess.png";
            var bitmap = textureLoader.GetTexture(textureName);
            if (bitmap == null)
            {
                Console.WriteLine($"Texture missing for attachment: {regionAttachment.Name}");
                return;
            }

            // Get the region attachment texture coordinates
            float textureWidth = bitmap.Width;
            float textureHeight = bitmap.Height;
            var sourceRect = new SKRect(
                regionAttachment.Region.u * textureWidth,
                regionAttachment.Region.v * textureHeight,
                regionAttachment.Region.u2 * textureWidth,
                regionAttachment.Region.v2 * textureHeight
            );

            // Calculate the attachment width and height
            float attachmentWidth = regionAttachment.Width * scaleX;
            float attachmentHeight = regionAttachment.Height * scaleY;

            // Define the destination rectangle
            var destRect = new SKRect(
                renderX - attachmentWidth / 2,
                renderY - attachmentHeight / 2,
                renderX + attachmentWidth / 2,
                renderY + attachmentHeight / 2
            );

            // Render texture on SKiasharp canvas
            using var paint = new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };
            canvas.DrawBitmap(bitmap, sourceRect, destRect, paint);
        }


        private void DrawMeshAttachment(SKCanvas canvas, MeshAttachment mesh, Slot slot)
        {
            // Todo
        }
    }
}
