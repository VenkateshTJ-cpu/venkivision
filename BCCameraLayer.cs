using System;
using UIKit;
using CoreGraphics;
namespace DaveNet.IOS
{
	public class BCCameraLayer : UIView
	{
		UILabel topLabel, lineLabel;
		UIButton cancelButton, flashButton;
		UIInterfaceOrientation orientation;

		public BCCameraLayer()
		{

			topLabel = new UILabel()
			{
				TextColor = CoreSettings.LennoxRedColor,
				BackgroundColor = CoreSettings.BackButtonColor,
				Font = UIFont.FromName("Helvetica-Bold", FindFontSize(14f)),
				Text = "Hold the camera up to the barcode\nAbout 6 inches away",
				TextAlignment = UITextAlignment.Center,
				LineBreakMode = UILineBreakMode.WordWrap,
				Lines = -1
			};
			this.BringSubviewToFront(topLabel);
			lineLabel = new UILabel() { BackgroundColor = CoreSettings.LennoxRedColor };

			cancelButton = new UIButton() { Font = UIFont.FromName("Helvetica-Bold", FindFontSize(15f)), BackgroundColor = CoreSettings.BackButtonColor };
			cancelButton.SetTitle("Cancel", UIControlState.Normal);
			cancelButton.SetTitleColor(CoreSettings.LennoxRedColor, UIControlState.Normal);


			flashButton = new UIButton() { Font = UIFont.FromName("Helvetica-Bold", FindFontSize(15f)), BackgroundColor = CoreSettings.BackButtonColor };
			flashButton.SetTitle("Flash", UIControlState.Normal);
			flashButton.SetTitleColor(CoreSettings.LennoxRedColor, UIControlState.Normal);

			this.AddSubviews(topLabel, lineLabel, cancelButton, flashButton);


		}

		public UIButton CancelButton
		{
			get
			{
				return cancelButton;
			}
		}
		public UIButton FlashButton
		{
			get
			{
				return flashButton;
			}
		}
		public UILabel LineLabel
		{
			get
			{
				return lineLabel;
			}
		}

		private nfloat FindFontSize(nfloat currentFontSize)
		{
			orientation = UIApplication.SharedApplication.StatusBarOrientation;
			if (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown)
				currentFontSize = (currentFontSize / 375) * AppDelegate.MainWindow.Frame.Width;
			else
				currentFontSize = (currentFontSize / 375) * AppDelegate.MainWindow.Frame.Height;

			Console.WriteLine("Font Size " + currentFontSize);
			return currentFontSize;
		}
		private nfloat FindHeight(nfloat height)
		{
			height = (height / 667) * AppDelegate.MainWindow.Frame.Height;

			Console.WriteLine("Height Size " + height);
			return height;
		}
		private nfloat FindWidth(nfloat width)
		{
			width = (width / 375) * AppDelegate.MainWindow.Frame.Width;

			Console.WriteLine("width Size " + width);
			return width;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			orientation = UIApplication.SharedApplication.StatusBarOrientation;
			topLabel.Frame = new CGRect(0, 0, AppDelegate.MainWindow.Frame.Width, FindHeight(90));
			lineLabel.Frame = new CGRect(0, AppDelegate.MainWindow.Frame.Height / 2 - FindHeight(3) / 2, AppDelegate.MainWindow.Frame.Width, FindHeight(3));


			if (orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight)
				cancelButton.Frame = new CGRect(FindWidth(15), AppDelegate.MainWindow.Frame.Height - FindWidth(40), FindHeight(150), FindWidth(25));
			else
				cancelButton.Frame = new CGRect(FindWidth(15), AppDelegate.MainWindow.Frame.Height - FindHeight(80), FindWidth(100), FindHeight(40));



			flashButton.Frame = new CGRect(AppDelegate.MainWindow.Frame.Width - (cancelButton.Frame.Width + FindWidth(15)), cancelButton.Frame.Y, cancelButton.Frame.Width, cancelButton.Frame.Height);

		}
	}
}
