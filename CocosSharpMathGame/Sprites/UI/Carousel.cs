using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Microsoft.Xna.Framework;

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

        internal void ClearCollection()
        {
            CollectionNode.RemoveAllChildren();
            Collection.Clear();
            UpdatePositionsInCollection();
        }

        internal List<IGameObject> Collection { get; private protected set; } = new List<IGameObject>();
        internal CCPoint NodeAnchor { get; set; } = CCPoint.AnchorMiddle;
        internal float ScaleFactor = Constants.STANDARD_SCALE;
        internal float Border { get; set; } = 0f;
        internal float SpacingFactor { get; set; } = 1.3f;
        internal bool IsHorizontal { get; set; } = true;
        private protected Scroller Scroller { get; set; } = new Scroller();
        internal GameObjectNode CollectionNode { get; private protected set; } = new GameObjectNode();
        internal Carousel(CCSize contentSize, bool swallowTouches=true)
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
            MakeClickable(touchMustEndOnIt: false, swallowTouch: swallowTouches);
            // DEBUG:
            DrawNode = new CCDrawNode();
        }
        CCDrawNode DrawNode;
        protected override void AddedToScene()
        {
            base.AddedToScene();
            //Parent.AddChild(DrawNode, 99999999);
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
            //DrawNode.Clear();
            //DrawNode.DrawSolidCircle(MiddleNode.BoundingBoxTransformedToWorld.Center, 10f, CCColor4B.Green);
            if (vector.Length > 1f)
                MoveCollectionNode(vector / 10);
            else
            {
                MoveCollectionNode(vector);
                Snapped = true;
            }
        }
        internal void MoveCollectionNode(CCPoint movement)
        {
            CollectionNode.PositionX = Constants.Clamp(CollectionNode.PositionX + movement.X, MinX, MaxX);
            CollectionNode.PositionY = Constants.Clamp(CollectionNode.PositionY + movement.Y, MinY, MaxY);
            if (IsHorizontal ? CollectionNode.PositionX == MaxX || CollectionNode.PositionX == MinX : CollectionNode.PositionY == MaxY || CollectionNode.PositionY == MinY)
                Scroller.ScrollVelocity = CCPoint.Zero;
            // update all individual nodes (for scale, zOrder, ...)
            MiddleNode = UpdateNodes();
            Snapped = false;
        }
        private protected virtual CCNode UpdateNodes()
        {
            CCPoint middle = BoundingBoxTransformedToWorld.Center;
            int maxZOrder = int.MinValue;
            CCNode middleNode = null;
            foreach (var node in CollectionNode.Children)
            {
                float value = (float)Math.Sin((IsHorizontal ? 1 - Math.Abs(node.PositionWorldspace.X - middle.X) / ContentSize.Width * 1.0f : 1 - Math.Abs(node.PositionWorldspace.Y - middle.Y) / ContentSize.Height * 1.3f) * Math.PI / 2);
                node.Scale = value * ScaleFactor;
                node.ZOrder = (int)(value * 1000);
                if (node.ZOrder > maxZOrder)
                {
                    maxZOrder = node.ZOrder;
                    middleNode = node;
                }
            }
            return middleNode;
        }
        internal float MinX
        {
            get
            {
                return IsHorizontal ? - CollectionNode.ContentSize.Width + ContentSize.Width / 2 : (-CollectionNode.ContentSize.Width + ContentSize.Width) / 2;
            }
        }
        internal float MaxX
        {
            get
            {
                return IsHorizontal ? ContentSize.Width / 2 : MinX;
            }
        }

        internal float MinYMod = 0f;
        internal float MinY
        {
            get
            {
                return (IsHorizontal ? ContentSize.Height : ContentSize.Height / 2) + MinYMod;
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
            Collection.Add(gameObject);
            CollectionNode.AddChild(ccNode);
            // place the node correctly
            ccNode.AnchorPoint = NodeAnchor;
            UpdatePositionsInCollection();
            if (MiddleNode == null)
                MiddleNode = ccNode;
        }

        internal CCPoint RemoveFromCollection(IGameObject gameObject)
        {
            CollectionNode.RemoveChild((CCNode)gameObject);
            Collection.Remove(gameObject);
            return UpdatePositionsInCollection();
        }

        internal event EventHandler MiddleChangedEvent;

        internal CCPoint UpdatePositionsInCollection()
        {
            CCPoint referencePoint;
            if (!Collection.Any()) return CCPoint.Zero;
            else referencePoint = ((CCNode)Collection.First()).Position;
            CCPoint position = CCPoint.Zero;
            foreach (var gameObject in Collection)
            {
                CCNode node = (CCNode)gameObject;
                position += IsHorizontal ? new CCPoint(node.ContentSize.Width * SpacingFactor, 0) : new CCPoint(0, -node.ContentSize.Height * SpacingFactor);
                node.Position = position;
                position += IsHorizontal ? new CCPoint(node.ContentSize.Width * SpacingFactor + Border, 0) : new CCPoint(0, -node.ContentSize.Height * SpacingFactor - Border);
            }
            var firstRect = ((CCNode)Collection.First()).BoundingBoxTransformedToParent;
            var lastRect = ((CCNode)Collection.Last()).BoundingBoxTransformedToParent;
            CCRect boundingRect = new CCRect(firstRect.MinX, firstRect.MaxY, lastRect.MaxX-firstRect.MinX, Math.Abs(firstRect.MaxY - lastRect.MinY));
            foreach (var gameObject in Collection)
            {
                CCNode node = (CCNode)gameObject;
                node.Position -= boundingRect.LowerLeft;
            }
            CollectionNode.ContentSize = boundingRect.Size;
            // return the change
            return ((CCNode)Collection.First()).Position - referencePoint;
        }

        private protected override void OnTouchesBeganUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            // stop all scrolling
            Scroller.OnTouchesBegan(touches, touchEvent);
        }
        private protected override void OnTouchesMovedUI(List<CCTouch> touches, CCEvent touchEvent)
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

        private protected override void OnTouchesEndedUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            // start inert scrolling
            Scroller.OnTouchesEnded(touches, touchEvent);
        }
    }
}
