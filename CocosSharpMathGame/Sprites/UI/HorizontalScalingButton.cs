using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A button made from tree sprites placed horizontally. The button can scale by scaling the middle part and moving the end.
    /// The middle sprite has to have a width of 1.
    /// </summary>
    internal abstract class HorizontalScalingButton : UIElementNode
    {
        private protected GameObjectSprite StartSprite { get; set; }
        private protected GameObjectSprite MiddleSprite { get; set; }
        private protected GameObjectSprite EndSprite { get; set; }
        internal HorizontalScalingButton(string texName1, string texName2, string texName3)
        {
            StartSprite  = new GameObjectSprite(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(texName1)));
            MiddleSprite = new GameObjectSprite(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(texName2)));
            EndSprite    = new GameObjectSprite(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(texName3)));
            StartSprite.AnchorPoint  = CCPoint.AnchorLowerLeft;
            MiddleSprite.AnchorPoint = CCPoint.AnchorLowerLeft;
            EndSprite.AnchorPoint    = CCPoint.AnchorLowerLeft;
            StartSprite.Scale = 1f;
            MiddleSprite.Scale = 1f;
            EndSprite.Scale = 1f;
            UpdateSpritePlacement();
            AddChild(StartSprite);
            AddChild(MiddleSprite);
            AddChild(EndSprite);
            AnchorPoint = CCPoint.AnchorMiddle;
            Scale = Constants.STANDARD_SCALE;
        }

        private protected void UpdateSpritePlacement()
        {
            StartSprite.Position = CCPoint.Zero;
            MiddleSprite.Position = new CCPoint(StartSprite.ContentSize.Width, 0);
            EndSprite.Position = new CCPoint(MiddleSprite.Position.X + MiddleSprite.ScaleX -1f, 0);
            ContentSize = new CCSize(EndSprite.PositionX + EndSprite.ContentSize.Width, StartSprite.ContentSize.Height);
        }

        new internal void FitToWidth(float width)
        {
            MiddleSprite.ScaleX = width - StartSprite.ContentSize.Width - EndSprite.ContentSize.Width + 1f;
            UpdateSpritePlacement();
        }
    }
}
