using Xamarin.Forms;
using CocosSharp;
using System;

namespace CocosSharpMathGame
{
	public class App : Application
	{
		public HangarLayer CurrentHangarLayer;
		public bool FinishedLoading = false;
		public App ()
		{
			// The root page of your application
			MainPage = new CocosSharpMathGame.MainPage(this);
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected async override void OnSleep ()
		{
			// Handle when your app sleeps
			if (CurrentHangarLayer != null && FinishedLoading)
				await CurrentHangarLayer.SaveToFile();
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
			// DEBUG: textures are lost when app is closed with back button
			//		  thought this could fix it, but it doesn't
			//UIElement.spriteSheet = new CCSpriteSheet("ui.plist");
			//Part.spriteSheet = new CCSpriteSheet("parts.plist");
		}

		public void OnBackPressed()
		{
			// currently there is only one case where the back button can be used:
			// to return from the MODIFY_AIRCRAFT state (which would also be possible by double tapping)
			if (CurrentHangarLayer != null && CurrentHangarLayer.Parent != null && CurrentHangarLayer.State == HangarLayer.HangarState.MODIFY_AIRCRAFT)
				CurrentHangarLayer.StartTransition(HangarLayer.HangarState.WORKSHOP);
		}
	}
}

