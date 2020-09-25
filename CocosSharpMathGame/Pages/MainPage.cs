using System;

using Xamarin.Forms;
using CocosSharp;
using CSharpMath.Forms;
using CSharpMath.Rendering;
using CSharpMath.SkiaSharp;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

using Symbolism;
using MathNet.Symbolics;
using MonoGame;
using System.IO;


namespace CocosSharpMathGame
{
	public class MainPage : ContentPage
	{
		// MainPage will be a singleton, so keep a reference to yourself at global scope
		public static MainPage Instance;
		CocosSharpView gameView;
		private App myApp;

		public MainPage(App myApp)
		{
			this.myApp = myApp;
			// set the global variable
			Instance = this;
			gameView = new CocosSharpView()
			{
				// Notice it has the same properties as other XamarinForms Views
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				// This gets called after CocosSharp starts up:
				ViewCreated = HandleViewCreated
			};
			this.Content = gameView;
		}

		// LoadGame is called when CocosSharp is initialized. We can begin creating
		// our CocosSharp objects here:
		void HandleViewCreated(object sender, EventArgs e)
		{
			var gameView = sender as CCGameView;
			if (gameView != null)
			{
				// show the stats
				//gameView.Stats.Enabled = true;

				// This sets the game "world" resolution:
				gameView.DesignResolution = new CCSizeI(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT);
				gameView.ResolutionPolicy = CCViewResolutionPolicy.ShowAll;

				//gameView.ContentManager.RootDirectory = "Content";
				gameView.ContentManager.SearchPaths = new List<string>()
				{
					"sounds",
					"hd/graphics",
					"hd/fonts"
				};

				// tell CCSprite to scale all textures by the factor 8, as standard behaviour
				//CCSprite.DefaultTexelToContentSizeRatio = 0.125f;

				// GameScene is the root of the CocosSharp rendering hierarchy:
				//gameScene = new GameScene(gameView);
				var myScene = new CCScene(gameView);
				//var playLayer = new PlayLayer();
				var hangarLayer = new HangarLayer();
				myApp.FinishedLoading = true;

				myScene.AddLayer(hangarLayer.GUILayer);
				myScene.AddLayer(hangarLayer, zOrder: int.MinValue);
				//myScene.AddChild(playLayer);
				//myScene.AddChild(playLayer.GUILayer);

				// Starts CocosSharp:
				gameView.RunWithScene(myScene);
			}
		}
	}
}


