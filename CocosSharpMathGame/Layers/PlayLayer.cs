using System;
using System.Collections.Generic;
using CocosSharp;
using Microsoft.Xna.Framework;

namespace CocosSharpMathGame
{
    public class PlayLayer : CCLayerColor
    {
        public PlayLayer() : base(CCColor4B.Black)
        {
            
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();    // MAGIC
        }
    }
}

