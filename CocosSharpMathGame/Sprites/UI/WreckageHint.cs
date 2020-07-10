using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WreckageHint : GameObjectNode
    {
        internal WreckageHint(string hint, bool arrowUp)
        {
            CCLabel label = new CCLabel(hint, "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
            GameObjectSprite arrow = new GameObjectSprite(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals("arrow.png")));
            label.AnchorPoint = CCPoint.AnchorLowerLeft;
            label.IsAntialiased = false;
            label.Scale = 1f;
            arrow.AnchorPoint = CCPoint.AnchorMiddle;
            arrow.Scale = 2f;
            AddChild(label);
            AddChild(arrow);
            const float BORDER = 16f;
            float width = Math.Max(label.ScaledContentSize.Width, arrow.ScaledContentSize.Width);
            if (arrowUp) // up
            {
                arrow.MyRotation = -90f;
                arrow.Position = new CCPoint(width / 2, arrow.ScaledContentSize.Height / 2);
                label.Position = new CCPoint(0, arrow.ScaledContentSize.Height + BORDER);
                ContentSize = new CCSize(width, label.BoundingBoxTransformedToParent.MaxY);
                /*
                label.Position = CCPoint.Zero;
                arrow.Position = new CCPoint(label.ScaledContentSize.Width / 2, arrow.ScaledContentSize.Height / 2 + label.ScaledContentSize.Height + BORDER);
                ContentSize = new CCSize(width, arrow.BoundingBoxTransformedToParent.MaxY);
                */
            }
            else // down
            {
                arrow.MyRotation = 90f;
                arrow.Position = new CCPoint(width / 2, arrow.ScaledContentSize.Height / 2 + label.ScaledContentSize.Height + BORDER);
                label.Position = CCPoint.Zero;
                ContentSize = new CCSize(width, arrow.BoundingBoxTransformedToParent.MaxY);
                /*
                label.Position = new CCPoint(0, arrow.ScaledContentSize.Height + BORDER);
                arrow.Position = new CCPoint(label.ScaledContentSize.Width / 2, arrow.ScaledContentSize.Height / 2);
                ContentSize = new CCSize(width, label.BoundingBoxTransformedToParent.MaxY);
                */
            }
        }
    }
}
