using System.Reflection;
using Microsoft.Xna.Framework;
using CocosSharp;
using CocosDenshion;
using System.Collections.Generic;

namespace CocosSharpMathGame.DX
{
    public class AppDelegate : CCApplicationDelegate
    {

        public override void ApplicationDidFinishLaunching(CCApplication application, CCWindow mainWindow)
        {
            application.ContentRootDirectory = "Content";
            var windowSize = mainWindow.WindowSizeInPixels;
            mainWindow.DisplayStats = true;

            // This will set the world bounds to be (0,0, w, h)
            // CCSceneResolutionPolicy.ShowAll will ensure that the aspect ratio is preserved
            CCScene.SetDefaultDesignResolution(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT, CCSceneResolutionPolicy.ShowAll);

            // Determine whether to use the high or low def versions of our images
            // Make sure the default texel to content size ratio is set correctly
            // Of course you're free to have a finer set of image resolutions e.g (ld, hd, super-hd)
            /*
            if (desiredWidth < windowSize.Width)
            {
                application.ContentSearchPaths.Add("hd");
                CCSprite.DefaultTexelToContentSizeRatio = 2.0f;
            }
            else
            {
                application.ContentSearchPaths.Add("ld");
                CCSprite.DefaultTexelToContentSizeRatio = 1.0f;
            }
            */
            application.ContentSearchPaths = new List<string>()
            {
                "sounds",
                "hd/graphics"
            };
            //CCSprite.DefaultTexelToContentSizeRatio = 1.0f;
            //CCSprite.DefaultTexelToContentSizeRatio = 0.125f;
            //CCSprite.DefaultTexelToContentSizeRatio = 0.0625f;
            var scene = new CCScene(mainWindow);
            var playLayer = new PlayLayer();

            scene.AddChild(playLayer);
            scene.AddChild(playLayer.GUILayer);
            
            mainWindow.RunWithScene(scene);
        }

        public override void ApplicationDidEnterBackground(CCApplication application)
        {
            application.Paused = true;
        }

        public override void ApplicationWillEnterForeground(CCApplication application)
        {
            application.Paused = false;
        }
    }
}