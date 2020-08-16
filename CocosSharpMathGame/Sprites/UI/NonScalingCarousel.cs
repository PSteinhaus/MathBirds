using CocosSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    internal class NonScalingCarousel : Carousel
    {
        internal float SigmoidFactor { get; set; } = 1.0f / 40f;
        internal NonScalingCarousel(CCSize contentSize) : base(contentSize)
        {
            //SwallowTouch = false;
        }

        private protected override CCNode UpdateNodes()
        {
            // first reset the positions (necessary as the folowing algorithm is based on them)
            UpdatePositionsInCollection();
            if (CollectionNode.ChildrenCount == 0) return null; // just to make sure nothing bad happens if it's empty
            CCPoint middle = BoundingBoxTransformedToWorld.Center;
            int maxZOrder = int.MinValue;
            CCNode middleNode = null;
            var firstBox = ((CCNode)Collection.First()).BoundingBoxTransformedToParent;
            var lastBox  = ((CCNode)Collection.Last()).BoundingBoxTransformedToParent;
            foreach (var gameObject in CollectionNode.Children)
            {
                var node = (CCNode)gameObject;
                float Sigmoid(float val)
                {
                    return 1.0f / (1.0f + (float)Math.Exp(-val));
                }
                float sigmoid = (IsHorizontal ? Sigmoid((node.PositionWorldspace.X - middle.X) * SigmoidFactor) : Sigmoid((node.PositionWorldspace.Y - middle.Y) * SigmoidFactor));
                
                if (IsHorizontal)
                    node.PositionX = - CollectionNode.Position.X + firstBox.Size.Width / 2 + sigmoid * (ContentSize.Width - lastBox.Size.Width / 2);
                else
                    node.PositionY = - CollectionNode.Position.Y + lastBox.Size.Height / 2 + sigmoid * (ContentSize.Height - firstBox.Size.Height);

                node.ZOrder = (int)((1-Math.Abs(sigmoid - 0.5))*50000);
                //node.VertexZ = - (float)Math.Abs(sigmoid - 0.5)*200;
                if (node.ZOrder > maxZOrder)
                {
                    maxZOrder = node.ZOrder;
                    middleNode = node;
                }
            }
            return middleNode;
        }

        private protected override void OnTouchesBeganUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesBeganUI(touches, touchEvent); // stops the scroller
            // bug-fix: let your middle node check for being pressed
            if (MiddleNode is PartCarouselNode pcn)
                pcn.PartCollectionNode.OnTouchesBegan(touches, touchEvent);

        }
    }
}
