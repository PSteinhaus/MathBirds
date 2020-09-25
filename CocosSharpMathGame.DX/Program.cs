# define DX

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using CocosSharp;

namespace CocosSharpMathGame.DX
{

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[STAThread]
        static void Main(string[] args)
        {
            Constants.oS = Constants.OS.WINDOWS;
            CCApplication application = new CCApplication(false, new CCSize(Constants.COCOS_WORLD_WIDTH/3, Constants.COCOS_WORLD_HEIGHT/3));
            application.ApplicationDelegate = new AppDelegate();

            application.StartGame();
        }
    }


}

