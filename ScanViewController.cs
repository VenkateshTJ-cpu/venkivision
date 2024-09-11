using AVFoundation;
using CoreFoundation;
using System.Collections.Generic;
public class ScanViewController : UIViewController
{
AVCaptureSession session;
AVCaptureMetadataOutput metadataOutput;

public ScanViewController() 
{
}

public override void DidReceiveMemoryWarning()
{
// Releases the view if it doesn't have a superview.
base.DidReceiveMemoryWarning();
}

public override void ViewDidLoad()
{
base.ViewDidLoad();

AVCaptureDevice.RequestAccessForMediaType (AVAuthorizationMediaType.Video, (bool isAccessGranted) => {                    
   //if has access              
   if(isAccessGranted)
   {
      //do something
   }
   //if has no access
   else
   {
    return;
      //show an alert
   }
});


session = new AVCaptureSession();
var camera = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video) ;
var input = AVCaptureDeviceInput.FromDevice(camera);
session.AddInput(input);

//Add the metadata output channel
metadataOutput = new AVCaptureMetadataOutput();
var metadataDelegate = new MyMetadataOutputDelegate();
metadataOutput.SetDelegate(metadataDelegate, DispatchQueue.MainQueue);
session.AddOutput(metadataOutput);
//Confusing! *After* adding to session, tell output what to recognize...
// foreach(var t in metadataOutput.AvailableMetadataObjectTypes)
// {
// Console.WriteLine(t);
// }
metadataOutput.MetadataObjectTypes = AVMetadataObjectType.QRCode | AVMetadataObjectType.EAN13Code;


var previewLayer = new AVCaptureVideoPreviewLayer(session);
var view = new ContentView(UIColor.Blue, previewLayer, metadataDelegate);

session.StartRunning();

this.View = view;
}
}

public class MyMetadataOutputDelegate : AVCaptureMetadataOutputObjectsDelegate
{
public override void DidOutputMetadataObjects(AVCaptureMetadataOutput captureOutput, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
{
foreach(var m in metadataObjects)
{
if(m is AVMetadataMachineReadableCodeObject)
{
MetadataFound(this, m as AVMetadataMachineReadableCodeObject);
}
}
}

public event EventHandler<AVMetadataMachineReadableCodeObject> MetadataFound = delegate {};
}


public class ContentView : UIView
{
AVCaptureVideoPreviewLayer layer;

public ContentView(UIColor fillColor, AVCaptureVideoPreviewLayer layer, MyMetadataOutputDelegate metadataSource)
{
BackgroundColor = fillColor;

this.layer = layer;
layer.MasksToBounds = true;
layer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;

Frame = UIScreen.MainScreen.Bounds;
layer.Frame = Frame;
Layer.AddSublayer(layer);

var label = new UILabel(new CGRect(40, 80, UIScreen.MainScreen.Bounds.Width - 80, 80));
AddSubview(label);

metadataSource.MetadataFound += (s, e) => label.Text = e.StringValue;

}

public override void LayoutSubviews()
{
base.LayoutSubviews();
layer.Frame = Bounds;
}
}