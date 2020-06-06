using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class Carousel : UIElementNode
    {
        private CCNode middleNode; 
        internal CCNode MiddleNode
        {
            get { return middleNode; }
            set
            {
                if (value != middleNode)
                {
                    middleNode = value;
                    // fire the "middle changed" event
                    MiddleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        internal CCPoint NodeAnchor { get; set; } = CCPoint.AnchorMiddle;
        internal float ScaleFactor = Constants.STANDARD_SCALE;
        internal float Border { get; set; } = 0f;
        internal float SpacingFactor { get; set; } = 1.3f;
        internal bool IsHorizontal { get; set; } = true;
        private protected Scroller Scroller { get; set; } = new Scroller();
        private protected GameObjectNode CollectionNode { get; set; } = new GameObjectNode();
        internal Carousel(CCSize contentSize)
        {
            Schedule();
            ContentSize = contentSize;
            CollectionNode.Scale = 1f;
            CollectionNode.AnchorPoint = CCPoint.AnchorLowerLeft;
            CollectionNode.PositionY = contentSize.Height;
            CollectionNode.PositionX = MaxX;
            AddChild(CollectionNode);
            AnchorPoint = CCPoint.AnchorLowerLeft;
            Scale = 1f;
            Scroller.MoveFunction = MoveCollectionNode;
            MakeClickable(OnTouchesBegan, OnTouchesMoved, OnTouchesEnded, null, touchMustEndOnIt: false);
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            if (!Pressed)
            {
                // scroll on using inertia
                Scroller.Update(dt);
                // slowly snap to the nearest node in the collection
                SnapToMiddleNode();
            }
        }
        private bool Snapped { get; set; } = false;
        internal void SnapToMiddleNode()
        {
            if (Snapped || MiddleNode == null) return;
            // move so that the distance decreases
            var center = BoundingBoxTransformedToWorld.Center;
            CCPoint vector = center - MiddleNode.BoundingBoxTransformedToWorld.Center;
            if (vector.Length > 1f)
                MoveCollectionNode(vector / 40);
            else
            {
                MoveCollectionNode(vector);
                Snapped = true;
            }
        }
        internal void MoveCollectionNode(CCPoint movement)
        {
            CollectionNode.Position += movement;
            CollectionNode.PositionX = Constants.Clamp(CollectionNode.PositionX + movement.X, MinX, MaxX);
            CollectionNode.PositionY = Constants.Clamp(CollectionNode.PositionY + movement.Y, MinY, MaxY);
            if (IsHorizontal ? CollectionNode.PositionX == MaxX || CollectionNode.PositionX == MinX : CollectionNode.PositionY == MaxY || CollectionNode.PositionY == MinY)
                Scroller.ScrollVelocity = CCPoint.Zero;
            // update all individual nodes (for scale, zOrder, ...)
            CCPoint middle = BoundingBoxTransformedToWorld.Center;
            int maxZOrder = int.MinValue;
            CCNode middleNode = null;
            foreach (var node in CollectionNode.Children)
            {
                float value = (float)Math.Sin( (IsHorizontal ? 1 - Math.Abs(node.PositionWorldspace.X - middle.X) / ContentSize.Width * 1.0f : 1 - Math.Abs(node.PositionWorldspace.Y - middle.Y) / ContentSize.Height * 1.3f) * Math.PI/2 );
                node.Scale = value * ScaleFactor;
                node.ZOrder = (int)(value * 1000);
                if (node.ZOrder > maxZOrder)
                {
                    maxZOrder = node.ZOrder;
                    middleNode = node;
                }
            }
            MiddleNode = middleNode;
            Snapped = false;
        }
        internal float MinX
        {
            get
            {
                return IsHorizontal ? - CollectionNode.ContentSize.Width + ContentSize.Width / 2 : 0f;
            }
        }
        internal float MaxX
        {
            get
            {
                return IsHorizontal ? ContentSize.Width / 2 : 0f;
            }
        }

        internal float MinY
        {
            get
            {
                return IsHorizontal ? ContentSize.Height : ContentSize.Height / 2;
            }
        }
        internal float MaxY
        {
            get
            {
                return IsHorizontal ? MinY : MinY + CollectionNode.ContentSize.Height;
            }
        }

        internal void AddToCollection(IGameObject gameObject)
        {
            var ccNode = (CCNode)gameObject;
            CollectionNode.AddChild(ccNode);
            // place the node correctly
            ccNode.AnchorPoint = NodeAnchor;
            UpdatePositionsInCollection();
            if (MiddleNode == null)
                MiddleNode = ccNode;
        }

        protected void RemoveFromCollection(CCNode node)
        {
            CollectionNode.RemoveChild(node);
            UpdatePositionsInCollection();
        }

        internal event EventHandler MiddleChangedEvent;

        internal void UpdatePositionsInCollection()
        {
            var children = CollectionNode.Children;
            if (!children.Any()) return;
            CCPoint position = CCPoint.Zero;
            foreach (var node in children)
            {
                position += IsHorizontal ? new CCPoint(node.ContentSize.Width * SpacingFactor, 0) : new CCPoint(0, -node.ContentSize.Height * SpacingFactor);
                node.Position = position;
                position += IsHorizontal ? new CCPoint(node.ContentSize.Width * SpacingFactor + Border, 0) : new CCPoint(0, -node.ContentSize.Height * SpacingFactor - Border);
            }
            var firstRect = children.First().BoundingBoxTransformedToParent;
            var lastRect = children.Last().BoundingBoxTransformedToParent;
            CCRect boundingRect = new CCRect(firstRect.MinX, firstRect.MaxY, lastRect.MaxX-firstRect.MinX, Math.Abs(firstRect.MaxY - lastRect.MinY));
            foreach (var node in children)
            {
                node.Position -= boundingRect.LowerLeft;
            }
            CollectionNode.ContentSize = boundingRect.Size;
        }

        private protected void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // stop all scrolling
                        Scroller.OnTouchesBegan(touches, touchEvent);
                    }
                    break;
                default:
                    break;
            }
        }
        private protected void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // move the collectionNode via scroller
                        Scroller.OnTouchesMoved(touches, touchEvent);
                    }
                    break;
                default:
                    break;
            }
        }

        private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // start inert scrolling
                        Scroller.OnTouchesEnded(touches, touchEvent);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
