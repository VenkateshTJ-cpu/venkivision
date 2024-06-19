using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Util;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace GMVisionAndroid
{
    public enum AskRuntimePermission
    {
        notrequired,
        permissionAsking
    }

    public enum TypeOfPermission
    {

        camera
    }

    public enum PermissionPopupType
    {
        initial,
        goToSettings
    }





    public class RuntimePermissionUtil
    {
        string[] CameraPermission = { Manifest.Permission.Camera };
        public static int CameraRequestCode = 4;
        int permissionRequestCode;
        public AlertDialog PermissionRequestDialog;
        Activity currentActivity;

        public RuntimePermissionUtil(Activity activity)
        {
            currentActivity = activity;
        }

        void RedirectToAppSetting()
        {
            Intent intent = new Intent(Settings.ActionApplicationDetailsSettings);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", currentActivity.PackageName, null);
            intent.SetData(uri);
            currentActivity.StartActivityForResult(intent, 233);
        }


        //Location Permission Ask
       

        public AskRuntimePermission RequestCameraPermissions(int camreqcode)
        {
            // if (Build.VERSION.SdkInt < Build.VERSION_CODES.M)
            // {
            //     return AskRuntimePermission.notrequired;
            // }
            try
            {
                if (currentActivity == null)
                    throw new Exception("Activity is null");
                if (ContextCompat.CheckSelfPermission(currentActivity, Manifest.Permission.Camera) != (int)Permission.Granted)
                {
                    currentActivity.RunOnUiThread(() =>
                    {
                        {
                            //if (!ActivityCompat.ShouldShowRequestPermissionRationale (currentActivity, Manifest.Permission.AccessCoarseLocation)) {
                            //    string bodyMsg = "Allow" + " " + "Lennox VRF" + " " + "Location" + " " + " and " + "to access to your device\\s" + "try again" + ".";
                            //    ShowRequestPermissionDialog (TypeOfPermission.location, PermissionPopupType.initial, LocationPermission, "Lennox VRF", bodyMsg);
                            //} else {
                            currentActivity.RequestPermissions(CameraPermission, camreqcode);
                            //}
                        }
                    });
                    return AskRuntimePermission.permissionAsking;//Denied
                }
                else
                {
                    return AskRuntimePermission.notrequired; //Granted
                }
            }
            catch (Exception e)
            {
                Log.Error("RequestCameraPermissions", e.Message);
                return AskRuntimePermission.permissionAsking;
            }
        }

      
        public void ShowRequestPermissionDialog(TypeOfPermission typeOfPermission, PermissionPopupType permissionPopupType, string[] Permission, String headerMessage, String bodyMessage)
        {
            if (PermissionRequestDialog != null && PermissionRequestDialog.IsShowing)
                return;

            switch (typeOfPermission)
            {
                case TypeOfPermission.camera:
                    permissionRequestCode = CameraRequestCode;
                    break;
              
            }
            PermissionRequestDialog = (new AlertDialog.Builder(currentActivity)).Create();
            PermissionRequestDialog.SetMessage(bodyMessage);
            PermissionRequestDialog.SetTitle(headerMessage);

            switch (permissionPopupType)
            {
                case PermissionPopupType.initial:
                    PermissionRequestDialog.SetButton("Continue", new EventHandler<DialogClickEventArgs>(
                (s, args) =>
                {
                    currentActivity.RunOnUiThread(() =>
                    {
                        PermissionRequestDialog.Hide();
                        PermissionRequestDialog.Cancel();
                        currentActivity.RequestPermissions(Permission, permissionRequestCode);
                    });
                }));

                    break;
                case PermissionPopupType.goToSettings:
                    PermissionRequestDialog.SetButton("Continue", new EventHandler<DialogClickEventArgs>(
                    (s, args) =>
                    {
                        currentActivity.RunOnUiThread(() =>
                        {
                            PermissionRequestDialog.Hide();
                            PermissionRequestDialog.Cancel();
                            RedirectToAppSetting();
                        });
                    }));
                    break;
            }

            PermissionRequestDialog.SetButton2("Cancel", new EventHandler<DialogClickEventArgs>(
                    (s, args) =>
                    {
                        currentActivity.RunOnUiThread(() =>
                        {
                            PermissionRequestDialog.Hide();
                            PermissionRequestDialog.Cancel();
                        });
                    }));
            PermissionRequestDialog.Show();
        }

    }
}








