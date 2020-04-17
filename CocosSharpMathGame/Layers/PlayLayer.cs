using System;
using System.Collections.Generic;
using CocosSharp;
using Microsoft.Xna.Framework;

namespace CocosSharpMathGame
{
    public class PlayLayer : CCLayerColor
    {
        private MathSprite mathSprite1 = new MathSprite("(a+b)*((x))");
        private MathSprite mathSprite2 = new MathSprite("sqrt(a+b)/((x))");
        private MathSprite mathSprite3 = new MathSprite("z/(x/(3*f+(1/(x^2))))");
        public PlayLayer() : base(CCColor4B.White)
        {
            // for now place some MathSprites as a test
            AddChild(mathSprite1);
            AddChild(mathSprite2);
            AddChild(mathSprite3);
            // create a DrawNode to check the boundaries
            var drawNode = new CCDrawNode();
            drawNode.DrawRect(mathSprite1.BoundingBoxTransformedToParent, CCColor4B.AliceBlue);
            drawNode.DrawRect(mathSprite1.BoundingBoxTransformedToParent, CCColor4B.Green);
            drawNode.DrawRect(mathSprite1.BoundingBoxTransformedToParent, CCColor4B.Red);
            AddChild(drawNode);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();    // MAGIC
            var bounds = VisibleBoundsWorldspace;
            var center = bounds.Center;
            var point1 = new CCPoint(center.X, center.Y + bounds.Size.Height / 3);
            var point2 = center;
            var point3 = new CCPoint(center.X, center.Y - bounds.Size.Height / 3);

            mathSprite1.Position = point1;
            mathSprite1.Position = point2;
            mathSprite1.Position = point3;
        }
    }
}

