namespace GMVisionAndroid;
using Android.Content;
using AndroidX.Activity;
using VisionSample;

[Activity (Label = "VisionSample", MainLauncher = true)]
    public class MainActivity : Activity
    {         
        Button buttonFaceTracker;
        Button buttonPhotoFace;
        Button buttonBarcode;

        int barcodeResultCode = 211;
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);

            buttonFaceTracker = FindViewById<Button> (Resource.Id.buttonFaceTracker);
            buttonPhotoFace = FindViewById<Button> (Resource.Id.buttonPhotoFace);
            buttonBarcode = FindViewById<Button> (Resource.Id.buttonBarcode);

            buttonFaceTracker.Click += delegate {
                StartActivity (typeof(FaceTrackerActivity));    
            };

            buttonPhotoFace.Click += delegate {
                
            };

            buttonBarcode.Click += delegate {
        
                // StartActivity (typeof(BarcodeScannerActivity));
                 Intent intent = new Intent(this, typeof(BarcodeScannerActivity));
                 this.StartActivityForResult(intent, barcodeResultCode);

            };
        }   

         protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
             base.OnActivityResult(requestCode, resultCode, data);

            try
            {
             if(requestCode==barcodeResultCode)
             {
                if(resultCode== Result.Ok)
                {
                 Bundle extras = data.Extras;
                 var valu = extras.GetString(GetString(Resource.String.scan_result), string.Empty);

                 Console.WriteLine(valu);
                }
             }
            }
            catch (Exception e)
            {
              //  Logger.LogException(this, e);
            }
        }


    }
  