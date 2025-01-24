using SkiaSharp;
using SkiaSharp.Views.Maui;
using Spine;
using Spine_skiasharp.Engine;

namespace Spine_skiasharp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Preload();
        }   

        private SKCanvas canvas;
        private Skeleton skeleton;
        private AnimationState animationState;
        private SpineSkiaSharpRenderer renderer;
        private MauiAssetTextureLoader textureLoader;

        async Task Preload()
        {
            try
            {
                //Load player png and store it in textureLoader.
                textureLoader = new MauiAssetTextureLoader(new List<string> { "spineboy-ess.png" });
                await textureLoader.LoadAssets();

                //Load atlas file as a streamReader and instanciate an Spine Atlas object.
                Stream streamAtlas = await FileSystem.OpenAppPackageFileAsync("spineboy-ess.atlas");
                StreamReader reader = new StreamReader(streamAtlas);

                Atlas atlas = new Atlas(reader, "", textureLoader);

                SkeletonJson skeletonJson = new SkeletonJson(new AtlasAttachmentLoader(atlas));

                Stream streamSkel = await FileSystem.OpenAppPackageFileAsync("spineboy-ess.json");
                var skeletonData = skeletonJson.ReadSkeletonData(new StreamReader(streamSkel));

                skeleton = new Skeleton(skeletonData);

                AnimationStateData stateData = new AnimationStateData(skeletonData);
                stateData.DefaultMix = 0.1f;
                animationState = new AnimationState(stateData);

                renderer = new SpineSkiaSharpRenderer(atlas, textureLoader);

                // Define an animation
                TrackEntry entry = animationState.SetAnimation(0, "run", true);


                //Start the game, 60fps
                ActivityIndicator.IsRunning = false;
                Application.Current.Windows.First().Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
                {
                    OnGameUpdate();

                    return true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Label.Text = ex.Message;
                Label.IsVisible = true;
                ActivityIndicator.IsRunning = false;
            }
        }

        void DrawSpineboy()
        {
            animationState.Update(1 / 60f);
            animationState.Apply(skeleton);
            skeleton.UpdateWorldTransform(Skeleton.Physics.Update);
            skeleton.X = 400;
            skeleton.Y = 400;
            renderer.DrawSkeleton(canvas, skeleton);
        }

        private void OnGameUpdate()
        {
            //Invalidate glview's surface to redraw : this will trigger OnGLPaintSurface.
            GameGLView.InvalidateSurface();
        }

        private void OnGLPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
        {
            canvas = e.Surface.Canvas;

            // Clean surface with transparent background
            canvas.Clear(SKColors.Transparent);

            DrawSpineboy();
        }
    }
}
