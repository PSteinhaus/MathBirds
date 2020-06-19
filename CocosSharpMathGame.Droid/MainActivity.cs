using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;


namespace CocosSharpMathGame.Droid
{
    [Activity(Label = "CocosSharpMathGame.Droid", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		private App app;
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Constants.oS = Constants.OS.ANDROID;
			global::Xamarin.Forms.Forms.Init(this, bundle);
			app = new App();
			LoadApplication(app);
		}

		public override void OnBackPressed()
		{
			app.OnBackPressed();
		}
	}
}