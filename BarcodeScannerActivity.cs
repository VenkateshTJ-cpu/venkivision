
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Vision;
using Android.Gms.Vision.Barcodes;
using GMVisionAndroid;
using Android.Graphics;
using Android.Content.PM;
using Java.Lang.Reflect;

namespace VisionSample
{
    [Activity (Label = "BarcodeScannerActivity")]			
    public class BarcodeScannerActivity : Activity
    {
        const string TAG = "FaceTracker";

        CameraSource mCameraSource;
        CameraSourcePreview mPreview;
        GraphicOverlay mGraphicOverlay;
         RuntimePermissionUtil requestpermission;


        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.RequestFeature(WindowFeatures.NoTitle);

            requestpermission = new RuntimePermissionUtil(this);
            SetContentView (GMVisionAndroid.Resource.Layout.scan_vision);
            mPreview = FindViewById<CameraSourcePreview> (Resource.Id.preview);
            mPreview.SetBackgroundColor(Color.Red);
            mGraphicOverlay = FindViewById<GraphicOverlay> (Resource.Id.faceOverlay);
          

        }
   public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            for (int i = 0, len = permissions.Length; i < len; i++)
            {
                string permission = permissions[i];
                //string permissionString = "";
                TypeOfPermission typeOfPermission = TypeOfPermission.camera;
                if (grantResults[i] != (int)Permission.Granted)
                {
                    if (requestCode == RuntimePermissionUtil.CameraRequestCode)
                    {
                        typeOfPermission = TypeOfPermission.camera;
                       // permissionString = "Location";
                    }
                    bool showRationale = ShouldShowRequestPermissionRationale(permission);
                    if (!showRationale) //Denied with never ask again
                    {
                        //Toast.MakeText (this, "Camera Permission Denied ToastLength.Short).Show ();
                       // Log.Error("RequestPermission", "showRationale False");
                     //   RuntimePermissionUtil saiMobileAppsRuntimePermission = new RuntimePermissionUtil(this);
                        string Msg = "Allow MyWork to access Camera to scan the barcode";
                        requestpermission.ShowRequestPermissionDialog(typeOfPermission, PermissionPopupType.goToSettings, permissions, "MyWork", Msg);
                    }
                    else
                    {
                        requestpermission.RequestCameraPermissions(requestCode);
                       // Log.Error("RequestPermission", "showRationale True");
                    }
                }
                else
                {
                    doWork();
                }

            }
        }
         void doWork()
        {
            var detector = new BarcodeDetector.Builder(Application.Context).Build();
            detector.SetProcessor(new MultiProcessor.Builder(new GraphicBarcodeTrackerFactory(mGraphicOverlay, this)).Build());
            if (!detector.IsOperational)
            {
                // Note: The first time that an app using barcode API is installed on a device, GMS will
                // download a native library to the device in order to do detection.  Usually this
                // completes before the app is run for the first time.  But if that download has not yet
                // completed, then the above call will not detect any barcodes.
                //
                // IsOperational can be used to check if the required native library is currently
                // available.  The detector will automatically become operational once the library
                // download completes on device.
              //  Logger.LogInfo(this, "Barcode detector dependencies are not yet available.");
            }
            mCameraSource = new CameraSource.Builder(Application.Context, detector)
                              .SetRequestedPreviewSize(640, 480)
                              .SetFacing((int)Android.Gms.Vision.CameraFacing.Back)
                              .SetRequestedFps(30.0f)
                              .SetAutoFocusEnabled(true)
                              .Build();

        }
      protected override void OnResume()
        {
            base.OnResume();
            if (requestpermission.PermissionRequestDialog != null && requestpermission.PermissionRequestDialog.IsShowing)
            {

            }
            else
            {
                if (requestpermission.RequestCameraPermissions(213) != AskRuntimePermission.permissionAsking)
                {
                    doWork();
                }
            }
            StartCameraSource();
        }


        protected override void OnPause ()
        {
            base.OnPause ();

            mPreview.Stop ();
        }

      protected override void OnDestroy()
        {
            if (mCameraSource != null)
                mCameraSource.Release();

            base.OnDestroy();
        }


        //==============================================================================================
        // Camera Source Preview
        //==============================================================================================

        /**
     * Starts or restarts the camera source, if it exists.  If the camera source doesn't exist yet
     * (e.g., because onResume was called before the camera source was created), this will be called
     * again when the camera source is created.
     */
        void StartCameraSource ()
        {
            try {
                mPreview.Start (mCameraSource, mGraphicOverlay);
            } catch (Exception e) {
                Android.Util.Log.Error (TAG, "Unable to start camera source.", e);
                mCameraSource.Release ();
                mCameraSource = null;
            }
        }
        //  public void setAutoFocus()
        // {
        //     try
        //     {
        //         var _cam = getCameraObject();
        //         if (_cam == null) return;
        //         var _pareMeters = _cam.GetParameters();
        //         var _listOfSuppo = _cam.GetParameters().SupportedFocusModes;
        //         _pareMeters.FocusMode = _listOfSuppo[4];
        //         _cam.SetParameters(_pareMeters);
        //     }
        //     catch (Exception e)
        //     {
        //         Logger.LogException(this, e);
        //     }

        // }
     Camera getCameraObject()
        {
            if (mCameraSource == null) return null;
            Field[] cFields = mCameraSource.Class.GetDeclaredFields();
            Camera _cam = null;
            try
            {
                foreach (Field item in cFields)
                {
                    if (item.Name.Equals("zzbNN"))
                    {
                        Console.WriteLine("Camera");
                        item.Accessible = true;
                        try
                        {
                            _cam = (Camera)item.Get(mCameraSource);
                        }
                        catch (Exception e)
                        {
                         //   Logger.LogException(this, e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
               // Logger.LogException(this, e);
            }
            return _cam;
        }

        public void mStopCamera(){
            mPreview.Stop();
            mCameraSource.Release ();
            mCameraSource = null;
                Finish();
        }

        

        //==============================================================================================
        // Graphic Face Tracker
        //==============================================================================================

        /**
     * Factory for creating a face tracker to be associated with a new face.  The multiprocessor
     * uses this factory to create face trackers as needed -- one for each individual.
     */
        class GraphicBarcodeTrackerFactory : Java.Lang.Object, MultiProcessor.IFactory
        {    
             BarcodeScannerActivity barcodeScannerActivity;
            public GraphicBarcodeTrackerFactory (GraphicOverlay overlay,BarcodeScannerActivity barcodeScannerActivity) : base ()
            {
                Overlay = overlay;
                this.barcodeScannerActivity = barcodeScannerActivity;
            }



            
            public GraphicOverlay Overlay { get; private set; }

            public Android.Gms.Vision.Tracker Create (Java.Lang.Object item)
            {
           
              // this.barcodeScannerActivity.mStopCamera();
            
                return new GraphicBarcodeTracker (Overlay,this.barcodeScannerActivity);
            }
        }

        /**
     * Face tracker for each detected individual. This maintains a face graphic within the app's
     * associated face overlay.
     */
        class GraphicBarcodeTracker : Tracker
        {
            GraphicOverlay mOverlay;
            BarcodeGraphic mBarcodeGraphic;
            Activity activity;
            public GraphicBarcodeTracker (GraphicOverlay overlay,Activity activity) 
            {
                mOverlay = overlay;
                mBarcodeGraphic = new BarcodeGraphic (overlay);
                this.activity = activity;
            }

            /**
            * Start tracking the detected face instance within the face overlay.
            */
            public override void OnNewItem (int idValue, Java.Lang.Object item)
            {
                mBarcodeGraphic.Id = idValue;
            }

            /**
            * Update the position/characteristics of the face within the overlay.
            */
            public override void OnUpdate (Detector.Detections detections, Java.Lang.Object item)
            {
              //  mOverlay.Add (mBarcodeGraphic);
                var barcode = item.JavaCast<Barcode>();
                var raw_value = barcode.RawValue;
                Intent barcoderesult = new Intent();
                barcoderesult.PutExtra(this.activity.GetString(Resource.String.scan_result), raw_value);
                activity.SetResult(Result.Ok, barcoderesult);
                // Helper _helper = new Helper(this.activity);
                // _helper.playBarcodeSuccessSound();
                activity.Finish();
                // Logger.LogInfo(_activity, "Scanned barcode:" + raw_value);
                mBarcodeGraphic.UpdateBarcode(item.JavaCast<Barcode>());
            }

            /**
            * Hide the graphic when the corresponding face was not detected.  This can happen for
            * intermediate frames temporarily (e.g., if the face was momentarily blocked from
            * view).
            */
            public override void OnMissing (Detector.Detections detections)
            {
                mOverlay.Remove (mBarcodeGraphic);
            }

            /**
            * Called when the face is assumed to be gone for good. Remove the graphic annotation from
            * the overlay.
            */
            public override void OnDone ()
            {
                mOverlay.Remove (mBarcodeGraphic);
            }
        }
    }
}

