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
            CCApplication application = new CCApplication(false, new CCSize(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT/2));
            application.ApplicationDelegate = new AppDelegate();

            application.StartGame();
        }
    }


}

