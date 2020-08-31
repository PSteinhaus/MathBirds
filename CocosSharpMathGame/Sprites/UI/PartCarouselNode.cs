using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class PartCarouselNode : GameObjectNode
    {
        internal Part.Type PartType { get; private set; }
        internal ScrollableCollectionNode PartCollectionNode { get; private protected set; }
        internal HorizontalScalingButton BGNode { get; private protected set; }
        internal PartCarouselNode(Part.Type partType)
        {
            PartType = partType;
            Scale = 1f;
            BGNode = new PartCollectionBG();
            BGNode.ScaleY = 6f;
            AddChild(BGNode, -1);
            var box = BGNode.BoundingBoxTransformedToParent;
            ContentSize = box.Size;
            BGNode.Position = (CCPoint)ContentSize / 2;
            PartCollectionNode = new ScrollableCollectionNode(new CCSize(box.Size.Width, box.Size.Height * 0.7f));
            PartCollectionNode.MaxScale = Constants.STANDARD_SCALE * 2;
            PartCollectionNode.Rows = 1;
            PartCollectionNode.Columns = 4000;
            PartCollectionNode.BoxSize = new CCSize(PartCollectionNode.ContentSize.Height, PartCollectionNode.ContentSize.Height);
            PartCollectionNode.AnchorPoint = CCPoint.AnchorMiddle;
            PartCollectionNode.Position = (CCPoint)ContentSize / 2;
            AddChild(PartCollectionNode, 1);
        }
        internal void AddPart(Part part)
        {
            PartCollectionNode.AddToCollection(part);
        }

        internal IEnumerable<Part> GetParts()
        {
            return PartCollectionNode.Collection.Cast<Part>();
        }
    }
}
