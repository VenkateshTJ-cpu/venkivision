// ***********************************************************************
// Assembly         : DaveNetIOS
// Author           : Lennox Industries
// Created          : 08-07-2014
//
// Last Modified By : Lennox Industries
// Last Modified On : 09-15-2014
// ***********************************************************************
// <copyright file="DaveNetHomeController.cs" company="Lennox">
//     Copyright (c) Lennox. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.IO;
using System.Threading.Tasks;
using BigTed;
using DaveNet.IOS.Controls;
using DaveNet.IOS;
using LennoxPros.Shared;
using LennoxPros.Shared.Processor;
using LennoxPros.Shared.Processor.Logger;
using Foundation;
using UIKit;
using CoreGraphics;
using System.Diagnostics;
using DaveNet.Shared;
using CoreLocation;
using System.Net;
using System.Threading;
using UserNotifications;
using AVFoundation;
using xamarinBindingIOS;
using System.Collections.Generic;
using CoreFoundation;
using ScanditBarcodeScanner.iOS;
using CoreMedia;
using LennoxPros;
using System.Net.Http;
using System.Linq;
using System.Globalization;
using System.Linq;
/// <summary>
/// The Views namespace.
/// </summary>
namespace DaveNet.IOS.Views
{
    /// <summary>
    /// Class DaveNetHomeController.
    /// </summary>
    public partial class DaveNetHomeController : UIViewController
    {
        #region Fields


        //NSObject observer;

        /// <summary>
        /// The _error code
        /// </summary>
        private static int _errorCode = 0;
        /// <summary>
        /// The _main web view
        /// </summary>
        private static UIWebView _mainWebView;

        private static string _downloadPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        /// <summary>
        /// The _file to download
        /// </summary>
        private static string _fileToDownload;
        /// <summary>
        /// The _file downloaded name and extension
        /// </summary>
        private static string _fileDownloadedNameAndExtension;

        /// <summary>
        /// WarrantyLookupURL url
        /// </summary>
        public static string WarrantyLookupURL;
        /// <summary>
        /// RepairPartURL url
        /// </summary>
        public static string RepairPartURL;
        public static string BarcodeDiffValue;

        public static bool test = false;

        public static string linkToTry;
        public static string linkToTryActual;
        /// <summary>
        /// The lennox network available
        /// </summary>
        public static bool LennoxNetworkAvailable;

        public static bool isErrorPageLoaded = false;

        public LPLocationManager lpLocationManager = new LPLocationManager();


        //NSObject observer;

        static readonly object _object = new object();

        bool isLoadedFirst;
        bool isLoginSuccess = false;

        UILabel labelToast;


        CLLocationManager mgr;
        private bool isLoginFailure = false;
        bool LastVistedURLUpdate = false;
        static UIImageView viewLoggingIn;
        UILabel lableLoggingIn;
        bool isTimeout = false, isSignInRequest = false;
        NSTimer timer;

        UIImage portLaunchImage, landLaunchImage;

        string loggingInString = "Logging into your account";
        string networkSlowError = "It's taking us longer than usual\nto login, please wait...";

        PickerScanDelegate scanDelegate;
        OverlayCancelDelegate cancelDelegate;

        PickerProcessFrameDelegate processFrameDelegate;
      
        UILabel topLabel;
        UIButton cancelButton, flashButton;
        UIInterfaceOrientation orientation;

        BCCameraLayer cameraView;

        AVCaptureVideoPreviewLayer videoPreviewLayer;

        AVCaptureSession session;
        string SDKSwitchValue;
        public static string barcodeServiceSwitchVaue;
        string quickOrderValue = string.Empty;
        string ocsScanFieldId = string.Empty;

        BarcodePicker picker = null;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// A constructor used when creating managed representations of unmanaged objects;  Called by the runtime.
        /// </summary>
        /// <param name="handle">Pointer (handle) to the unmanaged object.</param>
        /// <remarks>This constructor is invoked by the runtime infrastructure (<see cref="M:MonoTouch.ObjCRuntime.GetNSObject (System.IntPtr)" />) to create a new managed representation for a pointer to an unmanaged Objective-C object.    You should not invoke this method directly, instead you should call the GetNSObject method as it will prevent two instances of a managed object to point to the same native object.</remarks>
        public DaveNetHomeController(IntPtr handle)
            : base(handle)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaveNetHomeController" /> class.
        /// </summary>
        public DaveNetHomeController()
            : base()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether [user interface idiom is phone].
        /// </summary>
        /// <value><c>true</c> if [user interface idiom is phone]; otherwise, <c>false</c>.</value>
        private static bool UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        /// <summary>
        /// Gets a value indicating whether [user interface orientation is portrait].
        /// </summary>
        /// <value><c>true</c> if [user interface orientation is portrait]; otherwise, <c>false</c>.</value>
        private static bool UserInterfaceOrientationIsPortrait
        {
            get
            {
                var orientation = UIApplication.SharedApplication.StatusBarOrientation;
                return orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;
            }
        }

        #endregion Properties

        #region Methods


        /// <summary>
        /// In order to listen to events, we need to handle the ShouldStartLoad event. If
        /// it's an event that we want to handle ourselves, rather than having the web view
        /// do it, we need to return false, so that the navigation doesn't happen. in this
        /// particular case we are checking for links that have //LOCAL/Action='whateverAction'
        /// </summary>
        /// <param name="webView">The web view.</param>
        /// <param name="request">The request.</param>
        /// <param name="navigationType">Type of the navigation.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool HandleStartLoad(UIWebView webView, NSUrlRequest request,
            UIWebViewNavigationType navigationType)
        {

            bool returnVal = true;

            Helper.Instance.ConsoleWrite1("Handle Started Started B"+ request.Url.ToString());

            //if(webView.Request !=null)
            //Helper.Instance.ConsoleWrite("Document URL  " + request.MainDocumentURL);

            var currentUserAgent = webView.EvaluateJavascript("navigator.userAgent");
            //Helper.Instance.ConsoleWrite(navigationType.ToString());
            Logger.LogInformation(LennoxProsConstants.SystemType.Ios, currentUserAgent);
            var showActivityIndicatorIcon = true;
            var isReachable = true;
            linkToTryActual = request.Url.ToString();
            linkToTry = request.Url.ToString().ToLower();


            if (linkToTry.Contains("youtube"))
            {
                var isYoutubePopup = webView.EvaluateJavascript(@"document.getElementById('oculusembedvideo').getAttribute('data-video')");//keepmesignedin//remember_id

                if (!String.IsNullOrEmpty(isYoutubePopup) && linkToTry.Contains(isYoutubePopup.ToLower()))
                    return true;

            }

            Helper.Instance.ConsoleWrite("Handle URL " + linkToTry);

            if (linkToTry.Contains("/timeout") && !isTimeout)
            {
                //        [[NSURLCache sharedURLCache]
                //removeAllCachedResponses];

                //NSUrlCache.SharedCache.RemoveAllCachedResponses();
                isTimeout = true;
                AppDelegate.lastVisitedURL = UserPreferences.GetValue("LastVisitedURL");

                return true;
            }
            if (linkToTry.Contains("/content/error/error.html") && LennoxNetworkAvailable)
            {
                webView.GoBack();
                webView.GoBack();
                return true;
            }
            if(linkToTry.Contains("/lead/download/agreement/"))
            {
                var pdfViewer = new PdfViewer(null,_mainWebView);
                pdfViewer.DocumentUrl = linkToTryActual;
                NavigationController.PushViewController(pdfViewer,true);
                return false;
            }

            if (UrlDecisionHelper.IsLogoutTrue(linkToTry, AppDelegate.DaveNetHomeUrl))
            {
                UserPreferences.SetValue(string.Empty, "keepmesignedin");
                UserPreferences.SetValue(string.Empty, "username");
                UserPreferences.SetValue(string.Empty, "password");

            }

            string getkeepmesignedin = UserPreferences.GetValue("keepmesignedin");
            string getUsername = UserPreferences.GetValue("username");

            if ((UrlDecisionHelper.IsLoginPage(linkToTry, AppDelegate.DaveNetHomeUrl) || UrlDecisionHelper.IsSigninPage(linkToTry, AppDelegate.DaveNetHomeUrl)) && getUsername != null && getUsername != "")
            {
                viewLoggingIn.Hidden = false;


            }

            //Helper.Instance.ConsoleWrite("Test PDF URL" + linkToTry);
            //First check to see if we still are connected
            if (isReachable)
            {

                // first, we check to see if it's a link
                //if (navigationType == UIWebViewNavigationType.Other)
                //{
                   

                //    if (_errorCode > 0)
                //    {
                //        //Show Error Routine or retry getting connection
                //        //TryMobileSite();
                //        ShowActivityIndicator(false);
                //        returnVal = true;
                //    }
                //    else
                //    {
                //        CheckUserClickAction(webView, ref returnVal, ref showActivityIndicatorIcon, linkToTry);
                //    }


                //}

                if (navigationType == UIWebViewNavigationType.LinkClicked || navigationType == UIWebViewNavigationType.Other)
                {
                    if (linkToTry.ToLower().Contains(LennoxProsConstants.PDPLandingResponseKey))
                    {
                        if (!string.IsNullOrEmpty(linkToTryActual) && linkToTryActual.Contains(':'))
                        {
                            var qaArray = linkToTryActual.Split(':');
                            if (qaArray.Count() > 2)
                                quickOrderValue = qaArray[2];
                            else
                                quickOrderValue = string.Empty;
                        }
                        else
                            quickOrderValue = string.Empty;
                        
                        BarcodeDiffValue = LennoxProsConstants.PDPLandingResponseKey;
                        
                    }
                    else if (linkToTry.ToLower().Contains(LennoxProsConstants.BarcodeScanResponseKey))
                    {
                        BarcodeDiffValue = LennoxProsConstants.BarcodeScanResponseKey;
                    }
                    else if (linkToTry.ToLower().Contains(LennoxProsConstants.OcculusBarcodeResponseKey))
                    {
                        BarcodeDiffValue = LennoxProsConstants.OcculusBarcodeResponseKey;

                        ocsScanFieldId = linkToTryActual.Split('/').Last();

                    }
                    else
                    {
                        BarcodeDiffValue = string.Empty;
                        ocsScanFieldId = string.Empty;
                    }

                    if (_errorCode > 0)
                    {
                        ShowActivityIndicator(false);
                        //Show Error Routine or retry getting connection
                        TryMobileSite();
                        returnVal = true;
                    }
                    else
                    {
                        CheckUserClickAction(webView, ref returnVal, ref showActivityIndicatorIcon, linkToTry);
                    }
                }


                if (navigationType == UIWebViewNavigationType.FormSubmitted)
                {
                    ShowActivityIndicator(false);

                   

                    //var fullHTML = _mainWebView.EvaluateJavascript(@"document.documentElement.outerHTML");
                    //Get Users KeepmesignedIn value
                    if (UrlDecisionHelper.IsLoginPage(webView.Request.Url.ToString(), AppDelegate.DaveNetHomeUrl))
                    {
                        getkeepmesignedin = webView.EvaluateJavascript(@"document.getElementById('keepmesignedin').checked");//keepmesignedin//remember_id

                        if (getkeepmesignedin.Length > 0)
                        {
                            UserPreferences.SetValue(getkeepmesignedin, "keepmesignedin");
                            GAService.GetGASInstance().Track_App_Event("AutoLogin", "User enabled auto login " + getkeepmesignedin);
                        }

                    }

                    if (_errorCode > 0)
                    {

                        //Show Error Routine or retry getting connection
                        TryMobileSite();
                        returnVal = true;
                    }
                    else
                    {
                        CheckUserClickAction(webView, ref returnVal, ref showActivityIndicatorIcon, linkToTry);
                    }

                }
            }

            // if we got here, it's not a link we want to handle
            return returnVal;

        }

        private void GetBarcodeSDKSwitch()
        {
            try
            {
                var switchValue = _mainWebView.EvaluateJavascript(@"document.getElementById('barcodeSDKSwitchControl').value");//keepmesignedin//remember_id
                if (!string.IsNullOrEmpty(switchValue))
                    SDKSwitchValue = switchValue.ToLower();

                var serviceSwitchValue = _mainWebView.EvaluateJavascript(@"document.getElementById('barcodeServiceSwitchControl').value");//keepmesignedin//remember_id
                if (!string.IsNullOrEmpty(serviceSwitchValue))
                    barcodeServiceSwitchVaue = serviceSwitchValue.ToLower();  
                
            }
            catch(Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception("Barcode Switch From Hbris :" + ex.Message, false);
            }
            Helper.Instance.ConsoleWrite1("Handle Started Started E" + linkToTry.ToString());

        }

        private void AutoLogin()
        {
            try
            {
                var loadingURL = _mainWebView.Request.Url.ToString();
                if (isLoginSuccess && !string.IsNullOrEmpty(AppDelegate.lastVisitedURL))
                {
                    LoadRequest(AppDelegate.lastVisitedURL);

                }

                if (isLoginSuccess)
                {
                    isTimeout = false;
                    isSignInRequest = false;
                    isLoginSuccess = false;

                }

                if (!isLoginSuccess && loadingURL.ToLower() == AppDelegate.lastVisitedURL.ToLower())
                {
                    AppDelegate.lastVisitedURL = string.Empty;
                    isTimeout = false;
                    isSignInRequest = false;
                    viewLoggingIn.Hidden = true;
                }
                string getkeepmesignedin = UserPreferences.GetValue("keepmesignedin");

                if (getkeepmesignedin == "false" || getkeepmesignedin == "")
                {
                    UserPreferences.SetValue(string.Empty, "username");
                    UserPreferences.SetValue(string.Empty, "password");
                }


                string getUserName = UserPreferences.GetValue("username");


                if (getUserName == null || getUserName == "")
                {
                    viewLoggingIn.Hidden = true;
                }
                else
                {
                    lableLoggingIn.Hidden = false;
                    lableLoggingIn.Text = loggingInString;
                }
				//if(!string.IsNullOrEmpty(AppDelegate.URLFromNotification))
				//{
				//	viewLoggingIn.Hidden = true;
				//	AppDelegate.URLFromNotification = string.Empty;
				//}

                if (isTimeout && !isSignInRequest)// (UrlDecisionHelper.IsRestartSession(loadingURL,AppDelegate.DaveNetHomeUrl))
                {
                    //isLoginFailure = true;

                    GAService.GetGASInstance().Track_App_Event("AutoLogin", "Automatically login if user opted keep me signed In After session time out");

                    LoadRequest(UrlDecisionHelper.GetSignInURL(AppDelegate.DaveNetHomeUrl));

                    isSignInRequest = true;
                    //isTimeout = false;
                    //return;
                }

                if (UrlDecisionHelper.IsLoginPage(loadingURL, AppDelegate.DaveNetHomeUrl))
                {
                    if (!isLoginFailure)
                    {
                        //var rememberMeCheck = _mainWebView.EvaluateJavascript("document.getElementById('remember_id').checked");
                        //var fullHTML = _mainWebView.EvaluateJavascript(@"document.documentElement.outerHTML");

                        //Helper.Instance.ConsoleWrite(fullHTML);

                        string iskeepmesignedin = _mainWebView.EvaluateJavascript(@"document.getElementById('keepmesignedin').checked");//keepmesignedin//remember_id

                        if (getkeepmesignedin.ToLower() == "true" && getUserName != "")
                        {

                            if (iskeepmesignedin.ToLower() == "")
                            {
                                UserPreferences.SetValue(string.Empty, "username");
                                UserPreferences.SetValue(string.Empty, "password");
                                UserPreferences.SetValue(string.Empty, "keepmesignedin");
                                UserPreferences.SetValue(string.Empty, "LastVisitedURL");

                                LoginSwitchAlert();
                                return;
                            }



                            ShowActivityIndicator(true);

                            injectLoginCredentials(_mainWebView);


                        }
                        else
                        {
                            viewLoggingIn.Hidden = true;
                            isTimeout = false;
                            isSignInRequest = false;
                        }


                    }
                    else
                    {
                        isLoginFailure = false;
                        viewLoggingIn.Hidden = true;
                        //ShowActivityIndicator(false);
                    }
                }
                else if (UrlDecisionHelper.IsLoginSuccess(loadingURL, AppDelegate.DaveNetHomeUrl))
                {
                    if (string.IsNullOrEmpty(AppDelegate.lastVisitedURL))

                        viewLoggingIn.Hidden = true;

                    else
                        isLoginSuccess = true;

                    ShowActivityIndicator(false, true);
                    GetLoginCredentials(_mainWebView);


                    GAService.GetGASInstance().Track_App_Event("Login", "Login success");

                }
                else if (UrlDecisionHelper.IsLoginFailure(loadingURL, AppDelegate.DaveNetHomeUrl))
                {
                    isLoginFailure = true;
                    viewLoggingIn.Hidden = true;
                    GAService.GetGASInstance().Track_App_Event("Login", "Login success");

                }
                else if (UrlDecisionHelper.IsLoginFailure(loadingURL, AppDelegate.DaveNetHomeUrl))
                {
                    isLoginFailure = true;
                    viewLoggingIn.Hidden = true;
                    GAService.GetGASInstance().Track_App_Event("Login", "Login failure");
                }
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);
            }

            //viewLoggingIn.Hidden = true;
        }

        private static void LoadRequest(string requestURL)
        {
            try
            {
                if (requestURL.ToLower().Contains("?scansn") || requestURL.ToLower().Contains(LennoxProsConstants.PDPLandingBarcodeURL.ToLower()))
                {
                    NSMutableCharacterSet _urlAllowedCharacterSet;
                    _urlAllowedCharacterSet = new NSMutableCharacterSet();
                    _urlAllowedCharacterSet.AddCharacters(new NSString("#"));
                    _urlAllowedCharacterSet.UnionWith(NSUrlUtilities_NSCharacterSet.UrlHostAllowedCharacterSet);
                    _urlAllowedCharacterSet.UnionWith(NSUrlUtilities_NSCharacterSet.UrlPathAllowedCharacterSet);
                    _urlAllowedCharacterSet.UnionWith(NSUrlUtilities_NSCharacterSet.UrlQueryAllowedCharacterSet);
                    _urlAllowedCharacterSet.UnionWith(NSUrlUtilities_NSCharacterSet.UrlFragmentAllowedCharacterSet);

                    var nsReqURL = (NSString)requestURL;
                    requestURL = nsReqURL.CreateStringByAddingPercentEncoding(_urlAllowedCharacterSet);
                }

                if (requestURL.ToLower() == UrlDecisionHelper.GetSignInURL(AppDelegate.DaveNetHomeUrl).ToLower())
                {
                    var weburl = new NSUrl(UrlDecisionHelper.GetSignInURL(AppDelegate.DaveNetHomeUrl));

                    var request = new NSMutableUrlRequest(weburl);

                    NSMutableDictionary refDic = new NSMutableDictionary();
                    refDic.Add(new NSString("Referer"), new NSString(AppDelegate.DaveNetHomeUrl + "/"));
                    request.Headers = refDic;

                    _mainWebView.LoadRequest(request);
                }
                else
                {

                    var nsURL = new NSUrl(requestURL);
                    var nsURLReq = new NSUrlRequest(nsURL, NSUrlRequestCachePolicy.ReturnCacheDataElseLoad, 60);


                    _mainWebView.LoadRequest(nsURLReq);
                }
            }
            catch (Exception ex)
            {
                Helper.Instance.ConsoleWrite(ex.Message);
            }
        }

        #region Barcode

        private void BCCameraLayer()
        {

            topLabel = new UILabel()
            {
                TextColor = CoreSettings.LennoxRedColor,
                BackgroundColor = CoreSettings.BackButtonColor,
                Font = UIFont.FromName("Helvetica-Bold", FindFontSizeBB(14f)),
                Text = "Hold the camera up to the barcode\nAbout 6 inches away",
                TextAlignment = UITextAlignment.Center,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = -1
            };
            View.BringSubviewToFront(topLabel);

            cancelButton = new UIButton() { Font = UIFont.FromName("Helvetica-Bold", FindFontSizeBB(15f)), BackgroundColor = CoreSettings.BackButtonColor };
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(CoreSettings.LennoxRedColor, UIControlState.Normal);


            flashButton = new UIButton() { Font = UIFont.FromName("Helvetica-Bold", FindFontSizeBB(15f)), BackgroundColor = CoreSettings.BackButtonColor };
            flashButton.SetTitle("Flash", UIControlState.Normal);
            flashButton.SetTitleColor(CoreSettings.LennoxRedColor, UIControlState.Normal);

        }
        private nfloat FindFontSizeBB(nfloat currentFontSize)
        {
            orientation = UIApplication.SharedApplication.StatusBarOrientation;
            if (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown)
                currentFontSize = (currentFontSize / 375) * AppDelegate.MainWindow.Frame.Width;
            else
                currentFontSize = (currentFontSize / 375) * AppDelegate.MainWindow.Frame.Height;

            return currentFontSize;
        }
        private nfloat FindHeight(nfloat height)
        {
            height = (height / 667) * AppDelegate.MainWindow.Frame.Height;

            return height;
        }
        private nfloat FindWidth(nfloat width)
        {
            width = (width / 375) * AppDelegate.MainWindow.Frame.Width;

            return width;
        }

        private void AVFbarcode()
        {
            try
            {
                
                if (SDKSwitchValue=="on")
                {
                    ScanditBarcode();
                    GAService.GetGASInstance().Track_App_Event("Barcode Switch ", SDKSwitchValue +" :Scandit");
                }
                else
                {
                    NativeBarcode();
                    GAService.GetGASInstance().Track_App_Event("Barcode Switch ", SDKSwitchValue + " :Native");

                }
            }
            catch(Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception("Barcode Scanner Switch Exception : " + ex.Message,false );

            }
            
        }

        private void NativeBarcode()

        {

            try

            {
                //if(cameraView!=null)
                //{                   
                 
                //    session.StopRunning();

                //    videoPreviewLayer.RemoveFromSuperLayer();

                //    cameraView.RemoveFromSuperview();
                //    cameraView.Dispose();
                //    cameraView = null;
                //    UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                //}

                cameraView = new BCCameraLayer() { };



                session = new AVCaptureSession();

                var audio = new AudioManager();

                var camera = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);

                var input = AVCaptureDeviceInput.FromDevice(camera);

                session.AddInput(input);


                //Add the metadata output channel

                var metadataOutput = new AVCaptureMetadataOutput();

                var metadataDelegate = new MyMetadataOutputDelegate();

                metadataOutput.SetDelegate(metadataDelegate, DispatchQueue.MainQueue);

                session.AddOutput(metadataOutput);

                //Confusing! *After* adding to session, tell output what to recognize...

                metadataOutput.MetadataObjectTypes = metadataOutput.AvailableMetadataObjectTypes;


                videoPreviewLayer = new AVCaptureVideoPreviewLayer(session);

                videoPreviewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;

                cameraView.Layer.AddSublayer(videoPreviewLayer);

                cameraView.Layer.InsertSublayer(videoPreviewLayer, 0);

                View.AddSubview(cameraView);



                UIApplication.SharedApplication.SetStatusBarHidden(true, false);

                View.SetNeedsLayout();

                session.StartRunning();

                if (camera.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                {
                    NSError err = null;
                    if (camera.LockForConfiguration(out err))
                    {
                        if (camera.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                            camera.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                        else if (camera.IsFocusModeSupported(AVCaptureFocusMode.AutoFocus))
                            camera.FocusMode = AVCaptureFocusMode.AutoFocus;

                        if (camera.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
                            camera.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                        else if (camera.IsExposureModeSupported(AVCaptureExposureMode.AutoExpose))
                            camera.ExposureMode = AVCaptureExposureMode.AutoExpose;

                        if (camera.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
                            camera.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                        else if (camera.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.AutoWhiteBalance))
                            camera.WhiteBalanceMode = AVCaptureWhiteBalanceMode.AutoWhiteBalance;

                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && camera.AutoFocusRangeRestrictionSupported)
                            camera.AutoFocusRangeRestriction = AVCaptureAutoFocusRangeRestriction.Near;

                        if (camera.FocusPointOfInterestSupported)
                            camera.FocusPointOfInterest = new CGPoint(0.5f, 0.5f);

                        if (camera.ExposurePointOfInterestSupported)
                            camera.ExposurePointOfInterest = new CGPoint(0.5f, 0.5f);

                        camera.UnlockForConfiguration();
                    }
                    else
                        Helper.Instance.ConsoleWrite("Failed to Lock for Config: " + err.Description);
                }







                cameraView.FlashButton.TouchUpInside += (sender, e) =>

                {

                    try

                    {

                        var error = new NSError();

                        if (camera.HasTorch)

                        {


                            if (camera.TorchMode == AVCaptureTorchMode.Off)

                            {



                                camera.LockForConfiguration(out error);

                                camera.TorchMode = AVCaptureTorchMode.On;

                                camera.UnlockForConfiguration();

                            }

                            else

                            {

                                camera.LockForConfiguration(out error);

                                camera.TorchMode = AVCaptureTorchMode.Off;

                                camera.UnlockForConfiguration();

                            }

                        }



                    }

                    catch (Exception ex)

                    {

                        GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

                    }





                };



                cameraView.CancelButton.TouchUpInside += (sender, e) =>

                {

                    try

                    {

                        session.StopRunning();

                        videoPreviewLayer.RemoveFromSuperLayer();

                        cameraView.RemoveFromSuperview();

                        UIApplication.SharedApplication.SetStatusBarHidden(false, false);

                    }

                    catch (Exception ex)

                    {

                        GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

                    }

                };



                metadataDelegate.MetadataFound += async (object sender, AVMetadataMachineReadableCodeObject e) =>

                {

                    try

                    {

                        Helper.Instance.ConsoleWrite("Barcode Detected " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));


                        Helper.Instance.ConsoleWrite(string .Format( "barcode scanned: {0}, '{1}'", e.Type.ToString(), e.StringValue));
                        // Stop the scanner directly on the session.
                        await Task.Run(() =>
                        {
                            UIApplication.SharedApplication.InvokeOnMainThread(() =>
                            {
                                session.StopRunning();

                                cameraView.LineLabel.BackgroundColor = UIColor.Green;

                                audio.PlaySound("barcode.mp3");

                            });
                        });

                        await Task.Run(() =>
                        {
                            UIApplication.SharedApplication.InvokeOnMainThread(() =>
                            {

                                videoPreviewLayer.RemoveFromSuperLayer();
                                cameraView.RemoveFromSuperview();
                                UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                                ShowActivityIndicator(true);
                            });
                        });

                        await Task.Run(() =>
                        {
                            // If you want to edit something in the view hierarchy make sure to run it on the UI thread.
                            UIApplication.SharedApplication.InvokeOnMainThread(() =>
                            {

                                var barcodeType = e.Type;

                                var scannedData = BarcodeHelper.GetInstance().GetFieldsFromScannedData(e.StringValue.ToString());
                                Helper.Instance.ConsoleWrite("Barcode Logic done " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                               

                                var formSNo = String.IsNullOrEmpty(scannedData.SerialNumber) ? string.Empty : scannedData.SerialNumber;

                                GAService.GetGASInstance().Track_App_Event("Barcode", "Native :" + e.Type.ToString() + " | RAW DATA " + e.StringValue.ToString() + "   | Encoded Value " + formSNo);
                                Helper.Instance.ConsoleWrite("Barcode " + e.Type.ToString() + " | RAW DATA " + e.StringValue.ToString() + "   | Encoded Value " + formSNo);

                                var resultScanner = String.IsNullOrEmpty(scannedData.SerialNumber) ? e.StringValue : scannedData.SerialNumber;


                                if (!string.IsNullOrEmpty(quickOrderValue))
                                {
                                    _mainWebView.EvaluateJavascript(@"scanQuickOrderBarcode ('" + resultScanner + "','" + quickOrderValue + "')");
                                    ShowActivityIndicator(false);
                                    UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                                    GAService.GetGASInstance().Track_App_Event("Barcode", "Calling Quick order js");
                                }
                                else if (BarcodeDiffValue == LennoxProsConstants.BarcodeScanResponseKey)
                                {
                                    if (!string.IsNullOrEmpty(WarrantyLookupURL))
                                    {


                                        LoadRequest(WarrantyLookupURL + resultScanner);
                                    }
                                    else if (!string.IsNullOrEmpty(RepairPartURL))

                                    {

                                        LoadRequest(RepairPartURL + resultScanner);
                                    }
                                }
                                else if (BarcodeDiffValue == LennoxProsConstants.PDPLandingResponseKey)
                                {
                                    if (barcodeServiceSwitchVaue.ToLower()== "on")
                                    {
                                        var pdpURL = AppDelegate.DaveNetHomeUrl + LennoxProsConstants.PDPLandingBarcodeURL;


                                        LoadRequest(pdpURL + resultScanner);
                                        GAService.GetGASInstance().Track_App_Event("Barcode", "Calling barcode_service URL");

                                    }
                                    else
                                    {
                                        _mainWebView.EvaluateJavascript(@"barcode_search ('" + resultScanner + "')");
                                        ShowActivityIndicator(false);
                                        UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                                        GAService.GetGASInstance().Track_App_Event("Barcode","Calling barcode_search js");

                                    }
                                   
                                }
                                else if(BarcodeDiffValue == LennoxProsConstants.OcculusBarcodeResponseKey)
                                {
                                    //setScannedBarcode(value, elementId).

                                    _mainWebView.EvaluateJavascript(@"setScannedBarcode ('" + resultScanner + "','"+ocsScanFieldId.Replace(LennoxProsConstants.OcculusBarcodeResponseKey,"")+"')");
                                    ShowActivityIndicator(false);
                                    UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                                    GAService.GetGASInstance().Track_App_Event("Barcode", "Calling occulus barcode js");

                                }
                                Helper.Instance.ConsoleWrite("Barcode Load Req " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));



                            });

                        });

                    }

                    catch (Exception ex)

                    {

                        GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

                    }

                    finally

                    {

                    }

                };



            }

            catch (Exception ex)

            {

                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }

        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);
            if (videoPreviewLayer != null)
                BarcodeOrientation();
        }

        private void BarcodeOrientation()

        {


            var thisOrientation = UIApplication.SharedApplication.StatusBarOrientation;

            switch (thisOrientation)

            {

                case UIInterfaceOrientation.LandscapeLeft:

                    videoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.LandscapeLeft;

                    break;



                case UIInterfaceOrientation.LandscapeRight:

                    videoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.LandscapeRight;

                    break;

                case UIInterfaceOrientation.Portrait:

                    videoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;

                    break;



                case UIInterfaceOrientation.PortraitUpsideDown:

                    videoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.PortraitUpsideDown;

                    break;



                default:

                    videoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;

                    break;



            }



        }



        public class MyMetadataOutputDelegate : AVCaptureMetadataOutputObjectsDelegate

        {



            public event EventHandler<AVMetadataMachineReadableCodeObject> MetadataFound = delegate { };

            public override void DidOutputMetadataObjects(AVCaptureMetadataOutput captureOutput, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)

            {

                foreach (var m in metadataObjects)

                {



                    if (m is AVMetadataMachineReadableCodeObject)

                    {

                        MetadataFound(this, m as AVMetadataMachineReadableCodeObject);



                    }

                }

            }

        }






        private void ScanditBarcode()
        {
            //if (picker != null )
            //{               
            //    return;
            //}

            bool isTorchOn = false;
            // The scanning behavior of the barcode picker is configured through scan
            // settings. We start with empty scan settings and enable a very generous
            // set of symbologies. In your own apps, only enable the symbologies you
            // actually need.
            ScanSettings settings = ScanSettings.DefaultSettings();
            NSSet symbologiesToEnable = new NSSet(
                Symbology.EAN13,
                Symbology.EAN8,
                Symbology.UPC12,
                Symbology.UPCE,
                Symbology.Datamatrix,
                Symbology.QR,
                Symbology.Code39,
                Symbology.Code128,
                Symbology.ITF
            );
            settings.EnableSymbologies(symbologiesToEnable);

            //if (BarcodeDiffValue == LennoxProsConstants.PDPLandingResponseKey)
            //{
            //    settings.MatrixScanEnabled = true;
            //    settings.MaxNumberOfCodesPerFrame = 6;
            //}


            // Some 1d barcode symbologies allow you to encode variable-length data. By default, the
            // Scandit BarcodeScanner SDK only scans barcodes in a certain length range. If your
            // application requires scanning of one of these symbologies, and the length is falling
            // outside the default range, you may need to adjust the "active symbol counts" for this
            // symbology. This is shown in the following 3 lines of code.

            NSMutableSet codeLengths = new NSMutableSet();
            int i = 0;
            for (i = 7; i <= 20; i++)
            {
                codeLengths.Add(new NSNumber(i));
            }
            settings.SettingsForSymbology(Symbology.Code128).ActiveSymbolCounts = codeLengths;
            // For details on defaults and how to calculate the symbol counts for each symbology, take
            // a look at http://docs.scandit.com/stable/c_api/symbologies.html.

          
            // Setup the barcode scanner
             picker = new BarcodePicker(settings);

            picker.OverlayView.ShowToolBar(false);
            picker.OverlayView.SetBeepEnabled(true);
            picker.OverlayView.SetTorchEnabled(false);


            picker.OverlayView.View.AddSubviews(topLabel, flashButton, cancelButton);

            cancelButton.TouchUpInside += (sender, e) =>
                {
                    try
                    {
                        picker.SwitchTorchOn(false);
                        picker.StopScanning();
                        picker.PresentingViewController.DismissViewController(true, null);
                        picker.Dispose();
                        picker = null;

                    }
                    catch (Exception ex)
                    {
                        GAService.GetGASInstance().Track_App_Exception(ex.Message, false);
                    }
                };

            flashButton.TouchUpInside += (sender, e) =>
                {
                    try
                    {
                        if (!isTorchOn)
                        {
                            picker.SwitchTorchOn(true);
                            isTorchOn = true;
                        }
                        else
                        {
                            picker.SwitchTorchOn(false);
                            isTorchOn = false;
                        }



                    }
                    catch (Exception ex)
                    {
                        GAService.GetGASInstance().Track_App_Exception(ex.Message, false);
                    }

                };
            // Add delegates for the scan and cancel event. We keep references to the
            // delegates until the picker is no longer used as the delegates are softly
            // referenced and can be removed because of low memory.
            scanDelegate = new PickerScanDelegate(this);
            picker.ScanDelegate = scanDelegate;

            cancelDelegate = new OverlayCancelDelegate(this);
            picker.OverlayView.CancelDelegate = cancelDelegate;

            //if (BarcodeDiffValue == LennoxProsConstants.PDPLandingResponseKey)
            //{
            //        picker.OverlayView.GuiStyle = GuiStyle.MatrixScan;// SBSGuiStyleMatrixScan;
          
            //// Register a SBSProcessFrameDelegate delegate to be able to reject tracked codes.
            //        processFrameDelegate = new PickerProcessFrameDelegate();
            //        picker.ProcessFrameDelegate = processFrameDelegate;
            //}

            PresentViewController(picker, true, null);

            picker.StartScanning();



        }

        public class PickerProcessFrameDelegate : ProcessFrameDelegate
        {
            public override void DidCaptureImage(BarcodePicker picker, CMSampleBuffer frame, IScanSession session)
            {
                if (session.TrackedCodes != null && session.TrackedCodes.Values.Length>0 )
                {
                    // For each tracked codes in the last processed frame.
                    foreach (TrackedBarcode code in session.TrackedCodes.Values)
                    {
                        if (code.SymbologyString.Equals("@UNKNOWN"))
                        {
                            var location = code.Location;
                            var predLocation = code.PredictedLocation;
                            // As an example, let's visually reject all EAN8 codes.
                            if (code.Symbology == Symbology.Unknown)
                            {
                                session.RejectTrackedCode(code);
                            }

                            //if (code.Symbology == Symbology.QR)
                            //picker.OverlayView.SetViewfinderColor (216,34,165);
                        }

                    }

                    // If you want to implement your own visualization of the code matrix scan, 
                    // you should update it in this callback.
                }

                frame.Dispose();
            }



        }

        public class PickerScanDelegate : ScanDelegate
        {

            UIViewController presentingViewController;

            List<String> rawDataList = new List<string>();
            public PickerScanDelegate(UIViewController controller)
            {
                presentingViewController = controller;
            }

            public override void DidScan(BarcodePicker picker, IScanSession session)
            {

                //for multiple scan


                //if (BarcodeDiffValue == LennoxProsConstants.PDPLandingResponseKey)
                //{
                //    picker.PauseScanning();


                //    string foundSerialNumber = "";
                //    Console.WriteLine("NewlyRecognizedCodes Count " + session.NewlyRecognizedCodes.Count.ToString());

                //    Console.WriteLine("NewlyLocalizedCodes Count " + session.NewlyLocalizedCodes.Count.ToString());

                //    Console.WriteLine("AllRecognizedCodes Count " + session.AllRecognizedCodes.Count.ToString());



                //    session.NewlyRecognizedCodes.ToList().ForEach((Barcode obj) =>
                //    {
                //        if (!rawDataList.Exists((string item) => (item.Equals(obj.RawData.ToString()))))
                //        {
                //            rawDataList.Add(obj.RawData.ToString());
                //            //Task.Factory.StartNew(() =>
                //            //{
                //            Console.WriteLine(DateTime.UtcNow.ToString("Entry " + "yyyy-MM-dd HH:mm:ss.fff",
                //                                    CultureInfo.InvariantCulture));
                //            var checkSerialNumber = BarcodeHelper.GetInstance().GetFieldsFromScannedData(obj.RawData.ToString());

                //            Console.WriteLine(DateTime.UtcNow.ToString("Exit " + "yyyy-MM-dd HH:mm:ss.fff",
                //                              CultureInfo.InvariantCulture));
                //            if (!String.IsNullOrEmpty(checkSerialNumber.SerialNumber) && foundSerialNumber == "")
                //            {
                //                foundSerialNumber = checkSerialNumber.SerialNumber;
                //                session.StopScanning();
                //                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                //                {

                //                    presentingViewController.DismissViewController(true, null);
                //                    ShowActivityIndicator(true);
                //                });

                //            }
                //        }

                //        //});
                //    });

                //    if (String.IsNullOrEmpty(foundSerialNumber))
                //    {


                //        var allRegCode = session.AllRecognizedCodes.ToList();

                //        var selectedAllRegCode = allRegCode.GroupBy((Barcode arg) => arg.RawData).Select(g => g.First()).ToList();

                //        if (selectedAllRegCode.Count == 1)
                //        {
                //            if (allRegCode.Count() >= 3)
                //            {
                //                foundSerialNumber = allRegCode.First().RawData.ToString();
                //                session.StopScanning();
                //                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                //                {
                //                    presentingViewController.DismissViewController(true, null);
                //                    ShowActivityIndicator(true);
                //                });
                //            }
                //            else
                //            {
                //                picker.ResumeScanning();
                //                return;

                //            }
                //        }
                //        else if (selectedAllRegCode.Count >= 1)
                //        {
                //            if (allRegCode.Count() >= 15 || selectedAllRegCode.Count >= 6)
                //            {
                //                //foundSerialNumber = allRegCode.First().RawData.ToString();
                //                session.StopScanning();
                //                picker.StopScanning();

                //                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                //                {
                //                    presentingViewController.DismissViewController(true, null);
                //                    ShowActivityIndicator(false);
                //                });
                //                return;
                //            }
                //            else
                //            {
                //                picker.ResumeScanning();
                //                return;

                //            }
                //        }
                //        else
                //        {
                //            picker.ResumeScanning();
                //            return;

                //        }

                //    }
                //    else
                //    {
                //        Console.WriteLine("Found Serial Number " + foundSerialNumber);
                //    }

                //    //var resultScanner = String.IsNullOrEmpty(scannedData.SerialNumber) ? code.Data : scannedData.SerialNumber;
                //    Task.Factory.StartNew(() =>
                //    {
                //        // If you want to edit something in the view hierarchy make sure to run it on the UI thread.
                //        UIApplication.SharedApplication.InvokeOnMainThread(() =>
                //        {
                //            if (barcodeServiceSwitchVaue.ToLower() == "on")
                //            {
                //                var pdpURL = AppDelegate.DaveNetHomeUrl + LennoxProsConstants.PDPLandingBarcodeURL;

                //                LoadRequest(pdpURL + foundSerialNumber);
                //                GAService.GetGASInstance().Track_App_Event("Barcode", "Calling barcode_service URL");

                //            }
                //            else
                //            {
                //                _mainWebView.EvaluateJavascript(@"barcode_search ('" + foundSerialNumber + "')");
                //                ShowActivityIndicator(false);
                //                UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                //                GAService.GetGASInstance().Track_App_Event("Barcode", "Calling barcode_search js");

                //            }
                //        });
                //    });
                //    return;

                //}
                //else
                //{

                    if (session.NewlyRecognizedCodes.Count > 0)
                    {
                        Helper.Instance.ConsoleWrite("Barcode Detected " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

                        Barcode code = session.NewlyRecognizedCodes.GetItem<Barcode>(0);
                        Helper.Instance.ConsoleWrite(string.Format("barcode scanned: {0}, '{1}'", code.SymbologyString, code.Data));
                        // Stop the scanner directly on the session.
                        session.StopScanning();
                        UIApplication.SharedApplication.InvokeOnMainThread(() =>
                        {
                            presentingViewController.DismissViewController(true, null);
                            ShowActivityIndicator(true);
                        });

                        Task.Factory.StartNew(() =>
                                        {
                                        // If you want to edit something in the view hierarchy make sure to run it on the UI thread.
                                        UIApplication.SharedApplication.InvokeOnMainThread(() =>
                                            {

                                                var barcodeType = code.SymbologyName;

                                                var scannedData = BarcodeHelper.GetInstance().GetFieldsFromScannedData(code.RawData.ToString());
                                                Helper.Instance.ConsoleWrite("Barcode Logic done " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

                                                var formSNo = String.IsNullOrEmpty(scannedData.SerialNumber) ? string.Empty : scannedData.SerialNumber;

                                                GAService.GetGASInstance().Track_App_Event("Barcode", "Scandit : " + code.SymbologyName + " | RAW DATA " + code.RawData.ToString() + "   | Encoded Value " + formSNo);
                                                Helper.Instance.ConsoleWrite("Barcode" + code.SymbologyName + " | RAW DATA " + code.RawData.ToString() + "   | Encoded Value " + formSNo);

                                                var resultScanner = String.IsNullOrEmpty(scannedData.SerialNumber) ? code.Data : scannedData.SerialNumber;

                                                var daveNetController = presentingViewController as DaveNetHomeController;

                                                if (!string.IsNullOrEmpty(daveNetController.quickOrderValue))
                                                {
                                                    _mainWebView.EvaluateJavascript(@"scanQuickOrderBarcode ('" + resultScanner + "','" + daveNetController.quickOrderValue + "')");
                                                    ShowActivityIndicator(false);
                                                    UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                                                    GAService.GetGASInstance().Track_App_Event("Barcode", "Calling Quick order js");
                                                }
                                                else if (BarcodeDiffValue == LennoxProsConstants.BarcodeScanResponseKey)
                                                {
                                                    if (!string.IsNullOrEmpty(WarrantyLookupURL))
                                                    {

                                                        LoadRequest(WarrantyLookupURL + resultScanner);
                                                    }
                                                    else if (!string.IsNullOrEmpty(RepairPartURL))

                                                    {
                                                        LoadRequest(RepairPartURL + resultScanner);
                                                    }
                                                }
                                                else if (BarcodeDiffValue == LennoxProsConstants.PDPLandingResponseKey)
                                                {
                                                    if (barcodeServiceSwitchVaue.ToLower() == "on")
                                                    {
                                                        var pdpURL = AppDelegate.DaveNetHomeUrl + LennoxProsConstants.PDPLandingBarcodeURL;

                                                        LoadRequest(pdpURL + resultScanner);
                                                        GAService.GetGASInstance().Track_App_Event("Barcode", "Calling barcode_service URL");

                                                    }
                                                    else
                                                    {
                                                        _mainWebView.EvaluateJavascript(@"barcode_search ('" + resultScanner + "')");
                                                        ShowActivityIndicator(false);
                                                        UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                                                        GAService.GetGASInstance().Track_App_Event("Barcode", "Calling barcode_search js");

                                                    }
                                                }
                                                else if (BarcodeDiffValue == LennoxProsConstants.OcculusBarcodeResponseKey)
                                                {
                                                //setScannedBarcode(value, elementId).

                                                _mainWebView.EvaluateJavascript(@"setScannedBarcode ('" + resultScanner + "','" + daveNetController.ocsScanFieldId.Replace(LennoxProsConstants.OcculusBarcodeResponseKey, "") + "')");
                                                    ShowActivityIndicator(false);
                                                    UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                                                    GAService.GetGASInstance().Track_App_Event("Barcode", "Calling occulus barcode js");

                                                }
                                                Helper.Instance.ConsoleWrite("Barcode Load Req " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

                                            });

                                        }).ContinueWith((arg) =>
                                        {

                                        });
                    }
                }
            //}
        }

        public class OverlayCancelDelegate : CancelDelegate
        {

            UIViewController presentingViewController;

            public OverlayCancelDelegate(UIViewController controller)
            {
                presentingViewController = controller;
            }

            public override void DidCancel(ScanOverlay overlay, NSDictionary status)
            {
                Helper.Instance.ConsoleWrite("Cancel was pressed.");
                presentingViewController.DismissViewController(true, null);
                
            }
        }

        #endregion

        /// <summary>
        /// Tells the webview what to do with Clicks from the user
        /// </summary>
        /// <param name="webView"></param>
        /// <param name="returnVal"></param>
        /// <param name="showActivityIndicatorIcon"></param>
        /// <param name="linkToTry"></param>
        private void CheckUserClickAction(UIWebView webView, ref bool returnVal, ref bool showActivityIndicatorIcon, string linkToTry)
        {

            try
            {

                LennoxProsConstants.UrlRedirectTo whatToDo = UrlDecisionHelper.WhatToDoWithUrl(linkToTry, out showActivityIndicatorIcon);

                switch (whatToDo)
                {
                    ////Commented out for new LennoxPROs, No validation for Scan Image, Passson through will take care


                    case LennoxProsConstants.UrlRedirectTo.RedirectURL:
                        {

                            if (linkToTry.EndsWith("warranty") || linkToTry.EndsWith("/warranty/search"))
                            {

                                WarrantyLookupURL = AppDelegate.DaveNetHomeUrl + "/warranty?ScanSN=";
                                RepairPartURL = null;

                                returnVal = true;
                                return;
                            }

                            if (linkToTry.EndsWith("oem-repair-parts") || linkToTry.EndsWith("/repairpartssearch/search"))
                            {
                                RepairPartURL = AppDelegate.DaveNetHomeUrl + "/oem-repair-parts?ScanSN=";
                                WarrantyLookupURL = null;
                                returnVal = true;
                                return;
                            }


                            break;
                        }
                    case LennoxProsConstants.UrlRedirectTo.scan:
                        {
                            GAService.GetGASInstance().Track_App_Event("BarcodeScan", "User using barcode scanner");

                            switch (AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video))
                            {
                                case AVAuthorizationStatus.Authorized:
                                    // User granted camera access.
                                    AVFbarcode();
                                    break;
                                case AVAuthorizationStatus.Denied:
                                    // User denied camera access.
                                    BarcodeCameraSettingAlert();
                                    break;
                                case AVAuthorizationStatus.Restricted:
                                    // Camera access is restricted. Should not happen for AVMediaType.Video.
                                    // Check AVCaptureDevice.Devices.Length > 0 to determine if there is a video device
                                    // before using this code.
                                    break;
                                case AVAuthorizationStatus.NotDetermined:
                                    // Ask user for camera access.

                                    if (AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video).Result)
                                    {
                                        // User granted camera access.
                                        AVFbarcode();
                                    }

                                    else
                                    {
                                        BarcodeCameraSettingAlert();
                                    }
                                    break;

                                default:
                                    break;
                            }


                            returnVal = false;
                            break;
                        }
                    case LennoxProsConstants.UrlRedirectTo.DeviceBrowser:
                        {

                            //var currentRequestURL = CoreSettings.SystemEnvironment == SystemEnvironments.Qa ? "https://apps.liidaveqa.com/ssocode/trafficcop.aspx" : CoreSettings.SystemEnvironment == SystemEnvironments.Production ? "https://apps.lennoxpros.com/ssocode/trafficcop.aspx" : "https://apps.m.lpint1.com/ssocode/trafficcop.aspx";
                            //var passRequestURL = CoreSettings.SystemEnvironment == SystemEnvironments.Qa ? "https://www2.liidaveqa.com/PartnerResources/Pages/Learning.aspx" : CoreSettings.SystemEnvironment == SystemEnvironments.Production ? "https://www2.lennoxpros.com/PartnerResources/Pages/Learning.aspx" : "https://www2.m.lpint1.com/PartnerResources/Pages/Learning.aspx";

                            //Open in Safari s
                            //new UIAlertView("Url","Open New Window", null, "OK", null).Show();
                            ShowActivityIndicator(showActivityIndicatorIcon);
                            linkToTry = linkToTryActual;



                            //linkToTry = "https://integrationic3brokered.blob.core.windows.net/reports/005e148a-2a44-4b25-99c0-cf647536487d%2F2016_7_005e148a-2a44-4b25-99c0-cf647536487d.pdf?sr=b&sv=2015-02-21&st=2016-08-18T09%3A20%3A28Z&se=2016-08-18T10%3A20%3A28Z&sp=rwd&sig=iEw5MfPWCUP9AoXQharSPJsOA8jCp1K2OumqGMEWy7Q%3D";


                            //if (linkToTry.Contains(currentRequestURL))
                            //{
                            //    UIApplication.SharedApplication.OpenUrl(new NSUrl(passRequestURL));
                            //}
                            //else
                            //if (linkToTry.Contains("http://lennox.com"))
                            //{
                            UIApplication.SharedApplication.OpenUrl(new NSUrl(linkToTry));
                            //}
                            returnVal = false;
                            break;
                        }
                    case LennoxProsConstants.UrlRedirectTo.PassOnThrough:
                        {
                        
                            //Load like normal
                            // new UIAlertView("Url", "Pass On Through", null, "OK", null).Show();
                            //ShowActivityIndicator(showActivityIndicatorIcon);


                            returnVal = true;
                            break;
                        }
                    case LennoxProsConstants.UrlRedirectTo.DocViewer:
                        {
                            DownloadHelper.RemoveAllDocuments(_downloadPath);

                            var uri = new Uri(linkToTry);
                            var fi = new FileInfo(uri.AbsolutePath);
                            var ext = fi.Extension;

                            _fileDownloadedNameAndExtension=System.IO.Path.GetFileName(uri.LocalPath);

                            if (Path.HasExtension(_fileDownloadedNameAndExtension))
                                ext = Path.GetExtension(_fileDownloadedNameAndExtension);
                            
                            _fileToDownload = linkToTryActual;

                            //if extension failes, then manual method will work.
                            var docExt = UrlDecisionHelper.DocumentExtensionLists.Find(stringToCheck => linkToTry.ToLower().Contains(stringToCheck.ToLower())).ToString();

                            if (string.IsNullOrEmpty(ext))
                            {
                                
                                _fileDownloadedNameAndExtension = linkToTry.Substring(linkToTry.LastIndexOf("/",
                                                                 System.StringComparison.Ordinal) + 1);

                                if (!linkToTry.EndsWith(docExt, true, null))
                                {

                                    string[] results = linkToTry.Split(new string[] { docExt + "?" }, StringSplitOptions.None);

                                    if (results.Length > 0)
                                    {
                                        _fileDownloadedNameAndExtension =
                                            results[0].Substring(results[0].LastIndexOf("/",
                                                    System.StringComparison.Ordinal) + 1);

                                    }
                                }

                            }


                            if (!_fileDownloadedNameAndExtension.Contains("."))
                            {
                                if (linkToTry.ToLower().Contains("genpdf"))
                                {
                                    _fileDownloadedNameAndExtension += ".pdf";
                                }
                                else
                                {
                                    _fileDownloadedNameAndExtension += docExt;
                                }
                            }


                            Logger.LogInformation(LennoxProsConstants.SystemType.Ios, "Downloading and Viewing "+ ext +":" + _fileDownloadedNameAndExtension);

                            //Use shared down loader to save file to folder on device
                            StartDownloadHandler(webView, new EventArgs());
                            returnVal = false;


                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
        }

        private async void BarcodeCameraSettingAlert()
        {
            GAService.GetGASInstance().Track_App_Event("Barcode", "Camera access prohibited message in popup");
            string title, message;
            title = "Access to the camera had been prohibited";
            message = "Please enable it in the settings to continue.";

            try
            {
                //Delete the current user
                CommonControls confirmAlert;
                int isAlert;
                //Alert function
                confirmAlert = new CommonControls();
                isAlert = await confirmAlert.ShowModalAletViewAsync(title, message, "cancel", "settings");
                //if alert 1 mean, user has confirmed

                if (isAlert == 1)
                {

                    UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));

                }


            }
            catch (Exception ex)
            {
                Helper.Instance.ConsoleWrite(ex.Message);
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }

        }

        private async void LocationSettingAlert()
        {

            string title, message;
            title = "Access to the location had been prohibited";
            message = "Please enable it in the settings to continue to provide accurate product availability.";
            GAService.GetGASInstance().Track_App_Event("Location", "Location settings prohibited message");
            try
            {
                //Delete the current user
                CommonControls confirmAlert;
                int isAlert;
                //Alert function
                confirmAlert = new CommonControls();
                isAlert = await confirmAlert.ShowModalAletViewAsync(title, message, "cancel", "settings");
                //if alert 1 mean, user has confirmed

                if (isAlert == 1)
                {

                    UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));

                }


            }
            catch (Exception ex)
            {
                Helper.Instance.ConsoleWrite(ex.Message);
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }

        }


        private async void LoginSwitchAlert()
        {
            string title, message;
            title = "Message";
            message = "Auto login has been turned off due to heavy traffic.";
            GAService.GetGASInstance().Track_App_Event("AutoLogin", "Auto login has been turned off due to heavy traffic.");
            try
            {
                UIAlertView alert = new UIAlertView(title, message, null, "OK");
                alert.Clicked += (sender, args) =>
                {
                    // check if the user NOT pressed the cancel button
                    if (args.ButtonIndex == 0)
                    {
                        viewLoggingIn.Hidden = true;
                        TryMobileSite();
                    }
                };
                alert.Show();

            }
            catch (Exception ex)
            {
                Helper.Instance.ConsoleWrite(ex.Message);

                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);
            }

        }

        /// <summary>
        /// Shows the PDF.
        /// </summary>
        private void ShowPdf(string url="")
        {
            try
            {
                ShowActivityIndicator(false);
                string localFileDownload = string.Empty;
                //Now show PDF File after download
                if (string.IsNullOrEmpty(url))
                {
                    localFileDownload = Path.Combine(_downloadPath, _fileDownloadedNameAndExtension);                   
                   var pdfView = new PdfViewer(localFileDownload, _mainWebView);
                    this.NavigationController.PushViewController(pdfView, false);
                }
                else
                {
                    
                    NSHttpCookieStorage.SharedStorage.AcceptPolicy = NSHttpCookieAcceptPolicy.Always;
                    var pdfView = new DocumentLoader();
                    pdfView.documentURL = url;
                    this.NavigationController.PushViewController(pdfView, false); 
                }
                ShowActivityIndicator(false);
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
        }


        /// <summary>
        /// Starts the download handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        async void StartDownloadHandler(object sender, EventArgs e)
        {
            try
            {
                ProgressHUD.Shared.BackgroundColor = UIColor.Clear;
                ProgressHUD.Shared.HudBackgroundColour = UIColor.Black;
                ProgressHUD.Shared.HudForegroundColor = UIColor.Red;

                ProgressHUD.Shared.Ring.Color = UIColor.Red;
                //ProgressHUD.Shared.ShowWith("Continuous progress...", ProgressHUD.MaskType.None);
                BTProgressHUD.Show("Downloading File", 0f, ProgressHUD.MaskType.None);

                var progressReporter = new Progress<DownloadBytesProgress>();

                progressReporter.ProgressChanged += (s, args) => BTProgressHUD.Show("Downloading File", args.PercentComplete, ProgressHUD.MaskType.None);
                progressReporter.ProgressChanged += (s, args) => ProgressHUD.Shared.Show("Downloading File", args.PercentComplete, ProgressHUD.MaskType.None);

                //NSHttpCookie cookie = new NSHttpCookie ();
                String cookieValue = "";
                NSHttpCookieStorage cookieJar = NSHttpCookieStorage.SharedStorage;// [NSHTTPCookieStorage sharedHTTPCookieStorage];

                foreach (var cookie in cookieJar.Cookies)
                {

                    cookieValue += cookie.Name.ToString() + "=" + cookie.Value.ToString() + "; ";

                }

                Task<int> downloadTask = DownloadHelper.CreateDownloadTask(_fileToDownload, _fileDownloadedNameAndExtension, progressReporter, _downloadPath, cookieValue);

                await downloadTask;

                BTProgressHUD.ShowSuccessWithStatus("Download Complete of File : " + _fileDownloadedNameAndExtension, 1000D);

                var filename = Path.Combine(_downloadPath, _fileDownloadedNameAndExtension);
                Helper.Instance.ConsoleWrite(filename);
                GAService.GetGASInstance().Track_App_Event("PDF Download category", "Download File name:" + filename);

#if DEBUG
               
                if (System.IO.File.Exists(filename))
                {
                    ShowPdf();
                    GAService.GetGASInstance().Track_App_Event("PDF", "PDF Downloaded and shown");
                }
                else
                {
                    UIAlertView _error = new UIAlertView("Message", "File not found!", null, "Ok", null);
                    GAService.GetGASInstance().Track_App_Event("PDF", "PDF file not exist, might be download issue");
                    _error.Show();
                }
#else

                if (System.IO.File.Exists(filename))
                {
                    ShowPdf();
                    GAService.GetGASInstance().Track_App_Event("PDF", "PDF Downloaded and shown");
                }
                else
                {
                    UIAlertView _error = new UIAlertView("Message", "File not found!", null, "Ok", null);
                    GAService.GetGASInstance().Track_App_Event("PDF", "PDF file not exist, might be download issue");
                    _error.Show();
                }


#endif

            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);
            }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            _mainWebView.Reload();
        }

        /// <summary>
        /// Called after the controller闂傚倷鑳�1�7?�1�7�1�7��銊╁磿閺屻儲錄1�7?? <see cref="P:MonoTouch.UIKit.UIViewController.View" /> is loaded into memory.
        /// </summary>
        /// <remarks>This method is called after <c>this</c><see cref="T:MonoTouch.UIKit.UIViewController" />'s <see cref="P:MonoTouch.UIKit.UIViewController.View" /> and its entire view hierarchy have been loaded into memory. This method is called whether the <see cref="T:MonoTouch.UIKit.UIView" /> was loaded from a .xib file or programmatically.</remarks>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                Helper.Instance.ConsoleWrite1("DidLoad Started B");

                License.SetAppKey(AppDelegate.ScanditAppKey);
                // Perform any additional setup after loading the view, typically from a nib.

                // observer = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", UpdateSettings);

                this.View.BackgroundColor = UIColor.White;
                //lpLocationManager = new LPLocationManager();

                new Thread(GetGeoLocation).Start();
                updateNetworkStatus();
                GimBalConfig();

                TryMobileSite();
                //Since Need clarification from Born Team Whether we need to publish it or not from mobile app- Mani
                //UNUserNotificationCenter.Current.Delegate = new UserRemoteNotificationService(_mainWebView);

                ToastLabel();
                BCCameraLayer();

                NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), (NSNotification obj) =>
                {
                    DeviceRotated();

                });

                UIApplication.Notifications.ObserveWillEnterForeground((sender, args) =>
                {

                    Task.Factory.StartNew(GetGeoLocation);
                    GAService.GetGASInstance().Track_App_Event("GeoLocation", "App from Background and calling Location Manager");
                });


                GAService.GetGASInstance().Track_App_Page("Davenet Home Page");
                Helper.Instance.ConsoleWrite1("DidLoad Started E");

            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
        }


        public void DeviceRotated()
        {

            #region Barcode
            orientation = UIApplication.SharedApplication.StatusBarOrientation;


            if (orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight)
            {
                topLabel.Frame = new CGRect(0, 0, AppDelegate.MainWindow.Frame.Width, FindHeight(100));
                cancelButton.Frame = new CGRect(FindWidth(15), AppDelegate.MainWindow.Frame.Height - FindWidth(40), FindHeight(150), FindWidth(25));
            }
            else
            {
                topLabel.Frame = new CGRect(0, 0, AppDelegate.MainWindow.Frame.Width, FindHeight(90));

                cancelButton.Frame = new CGRect(FindWidth(15), AppDelegate.MainWindow.Frame.Height - FindHeight(80), FindWidth(100), FindHeight(40));
            }


            flashButton.Frame = new CGRect(AppDelegate.MainWindow.Frame.Width - (cancelButton.Frame.Width + FindWidth(15)), cancelButton.Frame.Y, cancelButton.Frame.Width, cancelButton.Frame.Height);

            #endregion
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            ExtendLaunchImage();
            _mainWebView.Frame = new CGRect(0, UIApplication.SharedApplication.StatusBarFrame.Bottom, View.Frame.Width, View.Frame.Height - UIApplication.SharedApplication.StatusBarFrame.Height);

            viewLoggingIn.Frame = AppDelegate.MainWindow.Frame;
            lableLoggingIn.Frame = new CGRect(0, (View.Frame.Height / 5) * 4, View.Frame.Width, (View.Frame.Height / 5));
            //viewLoggingIn.Hidden = true;

            #region Barcode
            orientation = UIApplication.SharedApplication.StatusBarOrientation;


            if (orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight)
            {
                topLabel.Frame = new CGRect(0, 0, AppDelegate.MainWindow.Frame.Width, FindHeight(100));

                cancelButton.Frame = new CGRect(FindWidth(15), AppDelegate.MainWindow.Frame.Height - FindWidth(40), FindHeight(150), FindWidth(25));
            }
            else
            {
                topLabel.Frame = new CGRect(0, 0, AppDelegate.MainWindow.Frame.Width, FindHeight(90));

                cancelButton.Frame = new CGRect(FindWidth(15), AppDelegate.MainWindow.Frame.Height - FindHeight(80), FindWidth(100), FindHeight(40));
            }


            flashButton.Frame = new CGRect(AppDelegate.MainWindow.Frame.Width - (cancelButton.Frame.Width + FindWidth(15)), cancelButton.Frame.Y, cancelButton.Frame.Width, cancelButton.Frame.Height);

            if (cameraView != null)

                cameraView.Frame = View.Bounds;

            if (videoPreviewLayer != null)

            {

                videoPreviewLayer.Frame = cameraView.Layer.Bounds;

                BarcodeOrientation();

            }
            #endregion

        }


        private void ExtendLaunchImage()
        {
            var this_orientation = UIApplication.SharedApplication.StatusBarOrientation;

            if (this_orientation == UIInterfaceOrientation.Portrait || this_orientation == UIInterfaceOrientation.PortraitUpsideDown)
            {
                if (portLaunchImage == null)
                {
                    portLaunchImage = new UIImage();

                    GetLaunchImage();
                }
                else
                {
                    viewLoggingIn.Image = portLaunchImage;
                }
            }
            else
            {
                if (landLaunchImage == null)
                {
                    landLaunchImage = new UIImage();

                    GetLaunchImage();
                }
                else
                {
                    viewLoggingIn.Image = landLaunchImage;
                }
            }


        }
        private void ToastLabel()
        {
            labelToast = new UILabel() { BackgroundColor = UIColor.LightGray, Alpha = 0.5f };
            labelToast.Frame = new CGRect(10, View.Frame.Height - 170, View.Frame.Width - 20, 150);

            labelToast.Hidden = true;
            labelToast.LineBreakMode = UILineBreakMode.WordWrap;
            labelToast.Lines = -1;
            View.AddSubview(labelToast);
            View.BringSubviewToFront(labelToast);
        }
        private void GetGeoLocation()
        {
       
            if (CLLocationManager.Status == CLAuthorizationStatus.NotDetermined)
            {

                lpLocationManager.LocationManager.AuthorizationChanged += LocationManagerOnAuthorizationChanged;
            }
            else
            {
                SendLocation();
            }

        }


        private void GimBalConfig()
        {
            try
            {
                var dict = NSDictionary.FromObjectAndKey((NSString)"dada", (NSString)"dida");
                //GimbalSdkBinding.Gimbal.SetAPIKey("9c53da19a25758d4c1458e97b2603e90", objDict);
                Gimbal.SetAPIKey(AppDelegate.gimbalAPIKey, dict);

                if (CLLocationManager.Status == CLAuthorizationStatus.AuthorizedAlways)
                {
                    lpLocationManager.Start();
                }
                var establishedLocs = GMBLEstablishedLocationManager.EstablishedLocations;

                // mgr = new CLLocationManager();
                //mgr.Delegate = gmblMgr;
                //mgr.RequestAlwaysAuthorization();
            }
            catch (Exception ex)
            {
                Helper.Instance.ConsoleWrite("Gimbal Exception :" + ex.Message);

                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
        }

        private void LocationManagerOnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs args)
        {
            try
            {
                //if (CLLocationManager.Status == CLAuthorizationStatus.Denied || CLLocationManager.Status == CLAuthorizationStatus.Restricted)
                //{
                //    LocationSettingAlert();
                //    return;
                //}

                if (CLLocationManager.Status == CLAuthorizationStatus.AuthorizedAlways || CLLocationManager.Status == CLAuthorizationStatus.AuthorizedWhenInUse)
                {
                    lpLocationManager.Start();
                }
                SendLocation(true);
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
        }

        private void SendLocation(bool isFromAuthorization = false)
        {


            Monitor.Enter(_object);


            try
            {
                var keepmesignedIn = UserPreferences.GetValue("keepmesignedin");

                if (!isLoadedFirst || keepmesignedIn == "true")
                    return;


                if (CLLocationManager.Status != CLAuthorizationStatus.NotDetermined)
                {

                    if (!CLLocationManager.LocationServicesEnabled || CLLocationManager.Status == CLAuthorizationStatus.Denied || CLLocationManager.Status == CLAuthorizationStatus.Restricted)
                    {
                        PostRequest(false, "", "");
                        //PostRequest("https://m.liidaveqa.com/mobileapp/location?longitude=&latitude=");
                    }

                    if (CLLocationManager.Status == CLAuthorizationStatus.AuthorizedWhenInUse || CLLocationManager.Status == CLAuthorizationStatus.AuthorizedAlways)
                    {
                        if (lpLocationManager == null)
                            lpLocationManager = new LPLocationManager();

                          var location = lpLocationManager.LocationManager.Location;
                         PostRequest(true, location.Coordinate.Latitude.ToString(), location.Coordinate.Longitude.ToString());                           //https://m.lennoxpro.local:9002/mobileapp/location?longitude=-73.961452&latitude=40.714224



                        if (isFromAuthorization && isLoginSuccess)
                        {
                            Task.Run(async () =>
                            {
                                string decryptUsername = LennoxSaveCredential.Decrypt(UserPreferences.GetValue("username"), UserPreferences.encryptKey);
                                await WebConnector.NearestLead(decryptUsername, location.Coordinate.Latitude.ToString(), location.Coordinate.Longitude.ToString(), AppDelegate.DaveNetHomeUrl);
                            });
                        }
                        //PostRequest("https://m.liidaveqa.com/mobileapp/location?longitude="+location.Coordinate.Longitude.ToString()+"&latitude="+location.Coordinate.Latitude.ToString());
                    }
                }

            }
            catch (Exception ex)
            {

                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

                Helper.Instance.ConsoleWrite("Exception at SendLocation Method: " + ex.Message);
            }

            finally
            {
                Monitor.Exit(_object);
            }


        }

        private void PostRequest(bool isLocationEnabled, string locaLatitude, string locaLongitude)
        {
            BeginInvokeOnMainThread(() =>
            {
                labelToast.Hidden = true;

                try
                {

                    if (isLocationEnabled)
                    {
                        _mainWebView.EvaluateJavascript(@"reverseGeoLocationMobileApp('" + locaLatitude + "','" + locaLongitude + "')");
                        labelToast.Text = "JSFunction reverseGeoLocationMobileApp('" + locaLatitude + "','" + locaLongitude + "')" + " Called";
                        GAService.GetGASInstance().Track_App_Event("GeoLocation", "JSFunction reverseGeoLocationMobileApp('" + locaLatitude + "','" + locaLongitude + "')" + " Called");
                    }
                    else
                    {
                        _mainWebView.EvaluateJavascript(@"geo_block_app()");
                        labelToast.Text = "JSFunction geo_block_app()" + "  Called";
                        GAService.GetGASInstance().Track_App_Event("GeoLocation", "JSFunction geo_block_app()" + "  Called");

                    }

                }
                catch (Exception ex)
                {
                    if (ex != null)
                    {
                        labelToast.Text = "Caught Exception " + ex.Message;
                        GAService.GetGASInstance().Track_App_Exception("GeoLocation: " + ex.Message, false);

                    }
                    else
                    {
                        labelToast.Text = "Caught Exception ex is null";
                        GAService.GetGASInstance().Track_App_Exception("GeoLocation: " + ex.Message, false);

                    }
                }
                finally
                {

                    //lpLocationManager = null;

                }
            });

        }

        //[Export("hide:")]
        //private void HideToast(NSString arg)
        //{
        //    labelToast.Hidden = true;        
        //}
        /// <summary>
        /// Shows the no connection warning.
        /// </summary>
        private static void ShowNoConnectionWarning()
        {
            viewLoggingIn.Hidden = true;
            GAService.GetGASInstance().Track_App_Event("Network", "Network disconnected showing error html page");

            string fileName = "Content/error/error.html"; // remember case-sensitive
            string localHtmlUrl = Path.Combine(NSBundle.MainBundle.BundlePath, fileName);
            _mainWebView.LoadRequest(new NSUrlRequest(new NSUrl(localHtmlUrl, false)));
            _mainWebView.ScalesPageToFit = true;


            return;

        }



        /// <summary>
        /// Initializes the web view.
        /// </summary>
        private void InitWebView()
        {
            try
            {

                if (_mainWebView == null)
                {
                    _mainWebView = new UIWebView();
                    _mainWebView.ScalesPageToFit = true;
                    _mainWebView.ScrollView.DecelerationRate = UIScrollView.DecelerationRateNormal;
                    _mainWebView.BackgroundColor = UIColor.White;
                    _mainWebView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                    _mainWebView.ShouldStartLoad += this.HandleStartLoad;
                    _mainWebView.LoadStarted += WebViewOnLoadStarted;
                    _mainWebView.LoadFinished += WebViewOnLoadFinished;
                    _mainWebView.LoadError += WebViewOnLoadError;
                    _mainWebView.AllowsInlineMediaPlayback = true;
                    _mainWebView.MediaPlaybackRequiresUserAction = false;
                    View.AddSubview(_mainWebView);

                    ////To avoid the distnace between status bar and
                    //_mainWebView.ScrollView.ContentInset = new UIEdgeInsets(-20, 0, 0, 0);

                    viewLoggingIn = new UIImageView() { BackgroundColor = UIColor.Clear, Hidden = false };

                    ExtendLaunchImage();

                    lableLoggingIn = new UILabel()
                    {

                        TextColor = UIColor.White,
                        Font = UIFont.FromName("Helvetica-Bold", FindFontSize(25f)),
                        Text = "",
                        TextAlignment = UITextAlignment.Center,
                        Hidden = true,
                        LineBreakMode = UILineBreakMode.WordWrap,
                        Lines = -1
                    };




                    viewLoggingIn.AddSubview(lableLoggingIn);
                    //_mainWebView.AddSubview(viewLoggingIn);
                    AppDelegate.MainWindow.AddSubview(viewLoggingIn);
                }
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
        }


        private void GetLaunchImage()
        {
            try
            {
                var imagesDict = (NSArray)NSBundle.MainBundle.InfoDictionary.ValueForKey((Foundation.NSString)@"UILaunchImages");

                bool isFirstImage = false;
                foreach (var nsdic in NSArray.FromArray<NSDictionary>(imagesDict))
                {
                    var imageName = nsdic[@"UILaunchImageName"];
                    var image = UIImage.FromFile(imageName.ToString());
                    var imageSize = image.Size;
                    //portLaunchImage = image;
                    if (UserInterfaceOrientationIsPortrait)
                    {
                        if (CGSize.Equals(imageSize, AppDelegate.MainWindow.Bounds.Size) && UIDevice.CurrentDevice.Orientation.ToString().ToLower().Contains(nsdic[@"UILaunchImageOrientation"].ToString().ToLower()))
                        {

                            portLaunchImage = image;

                        }
                    }
                    else
                    {
                        if (!isFirstImage)
                        {
                            if (imageSize.Width >= View.Bounds.Size.Width)
                            {
                                landLaunchImage = image;
                                isFirstImage = true;
                            }
                        }
                        else
                        {
                            if (imageSize.Width >= View.Bounds.Size.Width && imageSize.Width <= landLaunchImage.Size.Width)
                            {
                                landLaunchImage = image;
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception("Launch Image " + ex.Message, false);
            }

        }

        private nfloat FindFontSize(nfloat currentFontSize)
        {
            currentFontSize = (currentFontSize / 375) * AppDelegate.MainWindow.Frame.Width;

            return currentFontSize;

        }
        /// <summary>
        /// Shows the activity indicator.
        /// </summary>
        /// <param name="show">if set to <c>true</c> [show].</param>
        private static void ShowActivityIndicator(bool show, bool isLastProcess = false)
        {
            if (show)
            {

                ActivityIndicator.Show();
            }
            else
            {
                if (viewLoggingIn.Hidden)
                    ActivityIndicator.Hide(isLastProcess);
            }
        }
        /// <summary>
        /// Views the will appear.
        /// </summary>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        /// <remarks><para>This method is called prior to the <see cref="T:MonoTouch.UIKit.UIView" /> that is this <see cref="T:MonoTouch.UIKit.UIViewController" />闂傚倷鑳�1�7?�1�7�1�7��銊╁磿閺屻儲錄1�7?? <see cref="P:MonoTouch.UIKit.UIViewController.View" /> property being added to the display <see cref="T:MonoTouch.UIKit.UIView" /> hierarchy. </para>
        /// <para>Application developers who override this method must call <c>base.ViewWillAppear()</c> in their overridden method.</para></remarks>
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            Reachability.ReachabilityChanged += NetworkStatusCheck;

        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            //if (observer != null)
            //{
            //    NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
            //    observer = null;
            //}
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            Reachability.ReachabilityChanged -= NetworkStatusCheck;

        }


        public void NetworkStatusCheck(object sender, EventArgs e)
        {
            updateNetworkStatus();
        }

        private void WebViewOnLoadError(object sender, EventArgs eventArgs)
        {
            try
            {
                Helper.Instance.ConsoleWrite("Webview Load Error");
            }
            catch
            {


            }
        }
        /// <summary>
        /// Webs the view on load finished.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void WebViewOnLoadFinished(object sender, EventArgs eventArgs)
        {
            try
            {
                Helper.Instance.ConsoleWrite1("webview Finished B "+_mainWebView.Request.Url.ToString());

                DisposeTimer();
                var loadedURL = _mainWebView.Request.Url.ToString();

                UIApplication.SharedApplication.SetStatusBarHidden(false, false);
                Helper.Instance.ConsoleWrite(" URL Finished " + loadedURL);

                LastURLVisited(loadedURL);
              
                Task.Factory.StartNew(() => {
                    BeginInvokeOnMainThread(GetBarcodeSDKSwitch);
                });
              
                AutoLogin();

                if (!isLoadedFirst)
                {
                    new Thread(GetGeoLocation).Start();
                    isLoadedFirst = true;
                    GAService.GetGASInstance().Track_App_Event("GeoLocation", "Page load finished first time and called Location Manager");
                }
                else if (UrlDecisionHelper.isLoginSuccess(loadedURL))
                {
                   
                    Helper.Instance.SaveRealCredentials(GetLoginCredentials(_mainWebView));
                    Helper.Instance.PublishDeviceDetailsToHyrbis();
                }

 

                if (UrlDecisionHelper.isLoginSuccess(loadedURL))
                {
                    Task.Run(async () => {
                        //BeginInvokeOnMainThread(async() =>
                        //{

                        if (CLLocationManager.Status != CLAuthorizationStatus.NotDetermined)
                        {


                            if (CLLocationManager.Status == CLAuthorizationStatus.AuthorizedWhenInUse || CLLocationManager.Status == CLAuthorizationStatus.AuthorizedAlways)
                            {
                                if (lpLocationManager == null)
                                    lpLocationManager = new LPLocationManager();

                                var location = lpLocationManager.LocationManager.Location;
                                string decryptUsername = LennoxSaveCredential.Decrypt(UserPreferences.GetValue("username"), UserPreferences.encryptKey);

                                await WebConnector.NearestLead(decryptUsername, location.Coordinate.Latitude.ToString(), location.Coordinate.Longitude.ToString(), AppDelegate.DaveNetHomeUrl);

                            }
                        }


                    });
                    //});

                }

                ShowActivityIndicator(false, true);



            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
            finally
            {
                Helper.Instance.ConsoleWrite1("Webview Finished E " + _mainWebView.Request.Url.ToString());

            }
        }



        private Account GetLoginCredentials(UIWebView view)
        {
            Account acc = new Account();
            try
            {
                string getUsername = _mainWebView.EvaluateJavascript(@"document.getElementById('username').value");
                string getPassword = _mainWebView.EvaluateJavascript(@"document.getElementById('password').value");
                UserPreferences.SetValue(LennoxSaveCredential.Encrypt(getUsername, UserPreferences.encryptKey), "username");
                UserPreferences.SetValue(LennoxSaveCredential.Encrypt(getPassword, UserPreferences.encryptKey), "password");
                //UserPreferences.SetValue(getkeepmesignedin, "keepmesignedin");

                //Helper.Instance.ConsoleWrite(" Username Pwd from JS " + LennoxSaveCredential.Encrypt(getUsername, UserPreferences.encryptKey) + LennoxSaveCredential.Encrypt(getPassword, UserPreferences.encryptKey));
                acc.userName = getUsername;
                acc.passWord = getPassword;
                GAService.GetGASInstance().Track_App_Event("AutoLogin", "Persisting Login Credential");

            }
            catch (Exception e)
            {
            }
            return acc;
        }
        /// <summary>
        /// Webs the view on load started.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void WebViewOnLoadStarted(object sender, EventArgs eventArgs)
        {
            try
            {
                Helper.Instance.ConsoleWrite1("webview started B " + _mainWebView.Request.Url.ToString());

                ShowActivityIndicator(true);

                SlowNetworkCheck();

                var currentLoadingURL = _mainWebView.Request.Url.ToString();
                Helper.Instance.ConsoleWrite(" URL Started " + currentLoadingURL);
                LastURLVisited(currentLoadingURL);
                GAService.GetGASInstance().Track_App_Event("Webview", "Start loading URL : " + currentLoadingURL);
                
                if (!LennoxNetworkAvailable && !isErrorPageLoaded)
                {
                    //If site is not reachable log and set error to 1 for Not Reachable
                    Logger.LogInformation(LennoxProsConstants.SystemType.Ios, "Site : " + AppDelegate.DaveNetHomeUrl + " is not reachable showing try again page");
                    _errorCode = 1;
                    //Show internal Error page
                    ShowNoConnectionWarning();
                    isErrorPageLoaded = true;

                    if (!_mainWebView.Request.Url.ToString().ToLower().Contains("file:") && _mainWebView.Request.Url.ToString().Length > 8 && _mainWebView.Request.Url.ToString() != "")
                    {
                        if (!viewLoggingIn.Hidden)
                        {
                            AppDelegate.currentLoadURL = UrlDecisionHelper.GetSignInURL(AppDelegate.DaveNetHomeUrl);
                            viewLoggingIn.Hidden = true;
                        }
                        else
                            AppDelegate.currentLoadURL = _mainWebView.Request.Url.ToString();
                    }
                    return;
                }


            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
            finally
            {
                Helper.Instance.ConsoleWrite1("webview started E " + _mainWebView.Request.Url.ToString());

            }


        }


        private void SlowNetworkCheck()
        {
            if (!isLoadedFirst)
            {
                if (timer == null)
                {
                    int count = 0;
                    timer = NSTimer.CreateRepeatingScheduledTimer(new TimeSpan(0, 0, 60), delegate
                      {

                          lableLoggingIn.Text = networkSlowError;
                          lableLoggingIn.Hidden = false;

                          if (count == 1)
                          {

                              DisposeTimer();
                              ShowNoConnectionWarning();
                          }
                          count++;
                      });
                }
            }
        }

        private void DisposeTimer()
        {
            if (timer != null)
            {
                timer.Invalidate();
                timer.Dispose();
                lableLoggingIn.Hidden = true;
                lableLoggingIn.Text = "";
            }
            if(picker!=null)
            {
                picker.Dispose();
                picker = null;
            }
        }

        private void LastURLVisited(string currentLoadingURL)
        {
            try
            {
                Helper.Instance.ConsoleWrite("LastVisitedURL Persistence " + currentLoadingURL);

                //Add Last URL Visited
                if (currentLoadingURL.ToLower().Contains("/cart") || currentLoadingURL.ToLower().Contains("/p/") || currentLoadingURL.ToLower().Contains("/search/"))
                {

                    UserPreferences.SetValue(currentLoadingURL, "LastVisitedURL");

                    Helper.Instance.ConsoleWrite(currentLoadingURL);

                    GAService.GetGASInstance().Track_App_Event("Webview", "Capturing URL which need to be persist if it is Last Visit : " + currentLoadingURL);

                }

                else if (!isTimeout)
                    UserPreferences.SetValue(string.Empty, "LastVisitedURL");
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }

        }



        private void injectLoginCredentials(UIWebView view)
        {
            try
            {
                string getUserName = UserPreferences.GetValue("username");
                string getPassword = UserPreferences.GetValue("password");

                if (getUserName != "" && getPassword != "")
                {
                    string decryptUsername = LennoxSaveCredential.Decrypt(getUserName, UserPreferences.encryptKey);
                    string decryptPassword = LennoxSaveCredential.Decrypt(getPassword, UserPreferences.encryptKey);
                    //isLoadedFirst = false;
                    view.EvaluateJavascript("javascript:" +
                               "try{document.getElementById ('j_username').value ='" + decryptUsername + "';" +
                               "document.getElementById ('j_password').value = '" + decryptPassword + "';" +
                               "document.getElementById ('loginForm').submit ();}" +
                               "catch (err) {  console.log ('iOS Webview Exception - injectLoginCredentials: '+ err.message);}");

                    GAService.GetGASInstance().Track_App_Event("AutoLogin", "Injecting credential to Auto Login for user");

                }
                else
                {
                    viewLoggingIn.Hidden = true;
                    ShowActivityIndicator(false, true);

                }
            }
            catch (Exception e)
            {
                GAService.GetGASInstance().Track_App_Exception("AutoLogin - " + e.Message, false);
            }
        }

        //private void GetLoginCredentials(UIWebView view)
        //{
        //    try
        //    {


        //        string getUsername = _mainWebView.EvaluateJavascript(@"document.getElementById('username').value");
        //        string getPassword = _mainWebView.EvaluateJavascript(@"document.getElementById('password').value");

        //        UserPreferences.SetValue(LennoxSaveCredential.Encrypt(getUsername, UserPreferences.encryptKey), "username");
        //        UserPreferences.SetValue(LennoxSaveCredential.Encrypt(getPassword, UserPreferences.encryptKey), "password");
        //        //UserPreferences.SetValue(getkeepmesignedin, "keepmesignedin");

        //        Helper.Instance.ConsoleWrite("Test Username Pwd from JS " + LennoxSaveCredential.Encrypt(getUsername, UserPreferences.encryptKey) + LennoxSaveCredential.Encrypt(getPassword, UserPreferences.encryptKey));

        //    }
        //    catch (Exception e)
        //    {
        //    }
        //}
        /// <summary>
        /// Loads the start page.
        /// </summary>
		public void TryMobileSite(bool isFromNetworkConnect = false)
        {
            //updateNetworkStatus();

            try
            {
                InitWebView();


                if (LennoxNetworkAvailable)
                {
					
                    
					//if(!string.IsNullOrEmpty(AppDelegate.URLFromNotification))
					//{
					//	if (AppDelegate.URLFromNotification.ToLower().Contains("/cart"))
					//	{
					//		AppDelegate.lastVisitedURL = AppDelegate.URLFromNotification;
					//		AppDelegate.URLFromNotification = string.Empty;
					//	}
					//	else
					//	{
					//		LoadRequest(AppDelegate.URLFromNotification);
					//		return;
					//	}

					//}
                    //It will show the current page after netwrok connected and it not show Login page
					if (isFromNetworkConnect )
                    {
                        LoadRequest(AppDelegate.currentLoadURL);
                        return;
                    }

                    _errorCode = 0;
                    //new UIAlertView ("Url", AppDelegate.DaveNetHomeUrl, null, "OK", null).Show ();

                    var keepmesignedIn = UserPreferences.GetValue("keepmesignedin");
                    if (keepmesignedIn == "true")
                    {

                        LoadRequest(UrlDecisionHelper.GetSignInURL(AppDelegate.DaveNetHomeUrl));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(AppDelegate.lastVisitedURL))
                        {
                            LoadRequest(AppDelegate.currentLoadURL);
                        }
                        else
                        {
                            LoadRequest(AppDelegate.lastVisitedURL);
                            AppDelegate.lastVisitedURL = string.Empty;
                        }
                    }

                }
                else
                {
                    //If site is not reachable log and set error to 1 for Not Reachable
                    Logger.LogInformation(LennoxProsConstants.SystemType.Ios, "Site : " + AppDelegate.DaveNetHomeUrl + " is not reachable showing try again page");
                    _errorCode = 1;
                    //Show internal Error page
                    ShowNoConnectionWarning();



                }
            }
            catch (Exception ex)
            {
                GAService.GetGASInstance().Track_App_Exception(ex.Message, false);

            }
        }




        private void updateNetworkStatus()
        {
            try
            {

                LennoxNetworkAvailable = Reachability.InternetConnectionStatus() != NetworkStatus.NotReachable && Reachability.IsHostReachable("google.com");

                if (LennoxNetworkAvailable)
                {
                    Helper.Instance.ConsoleWrite("Network Connected");
                    GAService.GetGASInstance().Track_App_Event("Network", "Network connected");

                    isErrorPageLoaded = true;


                    if (_mainWebView != null && _mainWebView.Request.Url.ToString().ToLower().Contains("/content/error/error.html"))
                        TryMobileSite(true);
                }
                else
                {
                    Helper.Instance.ConsoleWrite("Network disConnected");
                    GAService.GetGASInstance().Track_App_Event("Network", "Network disconnected");

                    isErrorPageLoaded = false;

                    //When first time loading and netwrok got disconnect
                    if (!isLoadedFirst)
                        TryMobileSite();

                    if (!viewLoggingIn.Hidden)
                    {
                        TryMobileSite();

                    }



                }

            }

            catch (Exception ex)
            {
                Helper.Instance.ConsoleWrite(ex.Message);
                GAService.GetGASInstance().Track_App_Exception("Network - " + ex.Message, false);

            }
        }
        #endregion Methods


    }


}
