using System;
using System.Collections.Generic;
using CocosSharp;
using Microsoft.Xna.Framework;

namespace CocosSharpMathGame
{
    public class PlayLayer : CCLayerColor
    {
        //private MathSprite mathSprite1 = new MathSprite("(a+b)*((x))");
        private CCDrawNode drawNode = new CCDrawNode();
        public PlayLayer() : base(CCColor4B.Black)
        {
            /*
            // for now place some MathSprites as a test
            AddChild(mathSprite1);
            AddChild(mathSprite2);
            AddChild(mathSprite3);
            */
            // a DrawNode is always useful for debugging
            AddChild(drawNode);
            drawNode.ZOrder = -400;
            
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();    // MAGIC
            var bounds = VisibleBoundsWorldspace;

            /*
            var center = bounds.Center;
            var point1 = new CCPoint(center.X, center.Y + bounds.Size.Height / 3);
            var point2 = center;
            var point3 = new CCPoint(center.X, center.Y - bounds.Size.Height / 3);

            mathSprite1.Position = point1;
            mathSprite2.Position = point2;
            mathSprite3.Position = point3;

            float desiredWidth = 800;
            mathSprite1.FitToWidth(desiredWidth);
            mathSprite2.FitToWidth(desiredWidth);
            mathSprite3.FitToWidth(desiredWidth);

            // create a DrawNode to check the boundaries
            //drawNode.DrawRect(mathSprite1.BoundingBoxTransformedToParent, CCColor4B.AliceBlue);
            //drawNode.DrawRect(mathSprite2.BoundingBoxTransformedToParent, CCColor4B.Green);
            //drawNode.DrawRect(mathSprite3.BoundingBoxTransformedToParent, CCColor4B.Red);

            //drawNode.DrawSolidCircle(mathSprite1.Position, mathSprite1.ContentSize.Width / 2, CCColor4B.Gray);
            //drawNode.DrawSolidCircle(mathSprite2.Position, mathSprite2.ContentSize.Width / 2, CCColor4B.LightGray);
            //drawNode.DrawSolidCircle(mathSprite3.Position, mathSprite3.ContentSize.Width / 2, CCColor4B.Black);
            */
        }
    }
}

