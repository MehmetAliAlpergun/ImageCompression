using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using System.IO;
using Android.Database;
using Android.Provider;
using Android.Media;

namespace ImageCompression_Xamarin
{
    public class ImageFactory
    {
        protected Context Context;
        public List<Bitmap> Bitmaps = new List<Bitmap>();
        public ImageFactory (Context context)
        {
            Context = context;
        }

        public void Process(Intent data)
        {
            if(data.Data != null)
            {
                ProcessUri(data.Data);
            }
            else if(data.ClipData != null)
            {
                Process(data.ClipData);
            }
        }

        private void Process(ClipData clipData)
        {
            for (int i = 0; i < clipData.ItemCount; i++)
            {
                var uri = clipData.GetItemAt(i).Uri;

                ProcessUri(uri);
            }
        }

        private void ProcessUri(Android.Net.Uri uri)
        {
            var imageAbsolutePath = GetImagePath(uri);

            Matrix matrix = GetMatrix(imageAbsolutePath);

            var SourceBitmap = BitmapFactory.DecodeFile(imageAbsolutePath);

            var ReducedBitmap = ResizeAndReduceQuality(SourceBitmap);

            var FinalBitmap = Bitmap.CreateBitmap(ReducedBitmap, 0, 0, ReducedBitmap.Width, ReducedBitmap.Height, matrix, false);

            Bitmaps.Add(FinalBitmap);
        }

        private string GetImagePath(Android.Net.Uri uri)
        {
            ICursor cursor = Context.ContentResolver.Query(uri, null, null, null, null);
            cursor.MoveToFirst();
            string document_id = cursor.GetString(0);
            document_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            cursor.Close();

            cursor = Context.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, null, MediaStore.Images.Media.InterfaceConsts.Id + " = ? ", new string[] { document_id }, null);
            cursor.MoveToFirst();
            string path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
            cursor.Close();
            GC.Collect();
            return path;
        }

        public byte[] BitmapToArray(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            System.IO.Stream str = stream;
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
            GC.Collect();
            return stream.ToArray();
        }

        private Bitmap ResizeAndReduceQuality(Bitmap bitmap)
        {
            var ResizedBitmap = ResizeImage(bitmap);
            var data = BitmapToArray(ResizedBitmap);
            ResizedBitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
            GC.Collect();
            return ResizedBitmap;
        }

        private Bitmap ResizeImage(Bitmap sourceImage)
        {
            int SourceWidth = sourceImage.Width;
            int SourceHeight = sourceImage.Height;
            int ScaledWidth = 1280;
            int ScaledHeight = 720;

            if (SourceWidth < SourceHeight)
            {
                ScaledHeight = 1280;
                ScaledWidth = 720;
            }
            float Percent = 0;
            float PercentWidth = 0;
            float PercentHeight = 0;

            PercentWidth = ((float)ScaledWidth / (float)SourceWidth);
            PercentHeight = ((float)ScaledHeight / (float)SourceHeight);

            if (PercentHeight < PercentWidth)
                Percent = PercentHeight;
            else
                Percent = PercentWidth;

            int destWidth = (int)(SourceWidth * Percent);
            int destHeight = (int)(SourceHeight * Percent);

            var scaledBitmap = Bitmap.CreateScaledBitmap(sourceImage, destWidth, destHeight, false);

            GC.Collect();

            return scaledBitmap;
        }

        private Matrix GetMatrix(string imageAbsoluePath)
        {
            ExifInterface ei = new ExifInterface(imageAbsoluePath);
            int orientation = ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Normal);
            int rotate = 0;
            switch (orientation)
            {
                case (int)Android.Media.Orientation.Rotate270:
                    rotate = 270;
                    break;
                case (int)Android.Media.Orientation.Rotate180:
                    rotate = 180;
                    break;
                case (int)Android.Media.Orientation.Rotate90:
                    rotate = 90;
                    break;
            }

            Matrix matrix = new Matrix();
            matrix.PreRotate(rotate);

            GC.Collect();

            return matrix;
        }
    }
}