using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A button made from three sprites placed vertically. The button can scale by scaling the middle part and moving the end.
    /// The middle sprite has to have a height of 1.
    /// </summary>
    internal abstract class VerticalScalingButton : UIElementNode
    {
        private protected GameObjectSprite StartSprite { get; set; }
        private protected GameObjectSprite MiddleSprite { get; set; }
        private protected GameObjectSprite EndSprite { get; set; }
        internal VerticalScalingButton(string texName1, string texName2, string texName3)
        {
            StartSprite = new GameObjectSprite(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(texName1)));
            MiddleSprite = new GameObjectSprite(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(texName2)));
            EndSprite = new GameObjectSprite(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(texName3)));
            StartSprite.AnchorPoint = CCPoint.AnchorLowerLeft;
            MiddleSprite.AnchorPoint = CCPoint.AnchorLowerLeft;
            EndSprite.AnchorPoint = CCPoint.AnchorLowerLeft;
            UpdateSpritePlacement();
            AddChild(StartSprite);
            AddChild(MiddleSprite);
            AddChild(EndSprite);
            AnchorPoint = CCPoint.AnchorMiddle;
        }

        private protected void UpdateSpritePlacement()
        {
            StartSprite.Position = CCPoint.Zero;
            MiddleSprite.Position = new CCPoint(0, StartSprite.BoundingBoxTransformedToParent.MaxY);
            EndSprite.Position = new CCPoint(0, MiddleSprite.Position.Y + MiddleSprite.ScaleY);
            ContentSize = new CCSize(EndSprite.BoundingBoxTransformedToParent.MaxX, EndSprite.BoundingBoxTransformedToParent.MaxY);
        }

        new internal void FitToHeight(float height)
        {
            MiddleSprite.ScaleY = height - StartSprite.ContentSize.Height - EndSprite.ContentSize.Height;
            UpdateSpritePlacement();
        }
    }
}
