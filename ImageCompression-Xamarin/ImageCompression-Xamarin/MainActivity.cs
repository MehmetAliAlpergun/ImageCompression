using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Runtime;
using System.Collections.Generic;

namespace ImageCompression_Xamarin
{
    [Activity(Label = "ImageCompression_Xamarin", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private readonly int ImageRequestCode = 1234;
        private Button ButtonPickImages;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            ButtonPickImages = FindViewById<Button>(Resource.Id.button);

            ButtonPickImages.Click += ButtonPickImages_Click;
        }

        private void ButtonPickImages_Click(object sender, System.EventArgs e)
        {
            var imageIntent = new Intent();
            imageIntent.SetType("image/*");
            imageIntent.PutExtra(Intent.ExtraAllowMultiple, true);
            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(imageIntent, Resources.GetString(Resource.String.ChoosePhoto)), ImageRequestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
           if(resultCode == Result.Ok && requestCode == ImageRequestCode)
           {
                ImageFactory ImageFactory = new ImageFactory(this);

                ImageFactory.Process(data);

                var bitmaps = ImageFactory.Bitmaps;
            }
        }
    }
}

