using CocosSharp;
using MathNet.Symbolics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    internal class ScrollableCollectionNode : UIElementNode
    {
        internal float XBorder { get; set; } = 10f;
        internal float YBorder { get; set; } = 10f;
        internal List<CCNode> Collection { get; private protected set; } = new List<CCNode>();
        // standard behavior is a horizontal collection with one row
        private int columns;
        internal int Columns
        {
            get
            {
                return columns;
            }
            set
            {
                columns = value;
                CollectionNode.ContentSize = new CCSize(columns * BoxSize.Width, CollectionNode.ContentSize.Height);
            }
        } 
        private int rows;
        internal int Rows
        {
            get
            {
                return rows;
            }
            set
            {
                rows = value;
                CollectionNode.ContentSize = new CCSize(CollectionNode.ContentSize.Width, rows * BoxSize.Height);
            }
        }
        private CCSize boxSize;
        /// <summary>
        /// the bounding box of elements in the collection
        /// </summary>
        internal CCSize BoxSize
        {
            get
            {
                return boxSize;
            }
            set
            {
                boxSize = value;
                CollectionNode.ContentSize = new CCSize(columns * boxSize.Width, rows * boxSize.Height);
            }
        }
        internal bool Clicked { get; private protected set; } = false;
        internal GameObjectNode CollectionNode { get; private protected set; } = new GameObjectNode();
        internal ScrollableCollectionNode(CCSize contentSize)
        {
            Schedule();
            ContentSize = contentSize;
            CollectionNode.Scale = 1f;
            CollectionNode.AnchorPoint = CCPoint.AnchorUpperLeft;
            CollectionNode.PositionY = contentSize.Height;
            AddChild(CollectionNode);
            AnchorPoint = CCPoint.AnchorLowerLeft;
            Scale = 1f;
            MakeClickable(OnTouchesBegan, OnTouchesMoved, OnTouchesEnded, null, touchMustEndOnIt: false);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            // scroll on using inertia
            ScrollUsingInertia(dt);
        }

        private CCPoint ScrollVelocity { get; set; }
        private float ScrollTime { get; set; }
        private float TotalScrollTime { get; set; }

        internal void ScrollUsingInertia(float dt)
        {
            if (ScrollVelocity != CCPoint.Zero)
            {
                //Console.WriteLine("REALLY SCROLLING: " + ScrollVelocity);
                MoveCollectionNode(ScrollVelocity * (1 - (float)Math.Pow(ScrollTime / TotalScrollTime, 1)) * dt);
                ScrollTime += dt;
                if (ScrollTime > TotalScrollTime)
                    ScrollVelocity = CCPoint.Zero;
            }
        }

        internal bool AddToCollection(IGameObject gameObject)
        {
            if(Collection.Count() < Columns * Rows) // if there is space left
            {
                var ccNode = (CCNode)gameObject;
                Collection.Add(ccNode);
                CollectionNode.AddChild(ccNode);
                // place the node correctly
                ccNode.AnchorPoint = CCPoint.AnchorUpperLeft;
                ccNode.Position = PositionInCollection(Collection.Count() - 1);
                float ratioWidth  = ccNode.ContentSize.Width  / BoxSize.Width;
                float ratioHeight = ccNode.ContentSize.Height / BoxSize.Height;
                if (ratioWidth > ratioHeight)
                    gameObject.FitToWidth(BoxSize.Width);
                else
                    gameObject.FitToHeight(BoxSize.Height);
                return true;
            }
            return false;
        }

        internal CCPoint PositionInCollection(int index)
        {
            return new CCPoint((index % Columns) * (BoxSize.Width + XBorder), CollectionNode.ContentSize.Height - (index / Columns) * (BoxSize.Height + YBorder));
        }
        internal float MinX
        {
            get
            {
                float minX = ContentSize.Width - BoxSize.Width * MaxBoxesPerRow;
                if (minX > 0) minX = 0;
                return minX;
            }
        }

        internal float MinY
        {
            get
            {
                return ContentSize.Height;
            }
        }
        internal float MaxY
        {
            get
            {
                float minY = MinY;
                float maxY = minY + BoxSize.Height * MaxBoxesPerColumn - ContentSize.Height;
                if (maxY < minY) maxY = MinY;
                return maxY;
            }
        }
        internal int MaxBoxesPerRow
        {
            get
            {
                int maxBoxesPerRow = Collection.Count();
                if (maxBoxesPerRow > Columns) maxBoxesPerRow = Columns;
                return maxBoxesPerRow;
            }
        }
        internal int MaxBoxesPerColumn
        {
            get
            {
                int count = Collection.Count();
                if (count == 0) return 0;
                var maxBoxesPerColumn = (count - 1) / Columns + 1;
                if (maxBoxesPerColumn > Rows) maxBoxesPerColumn = Rows;
                return maxBoxesPerColumn;
            }
        }

        private void MoveCollectionNode(CCPoint movement)
        {
            CollectionNode.Position += movement;
            CollectionNode.PositionX = Constants.Clamp(CollectionNode.PositionX + movement.X, MinX, 0);
            CollectionNode.PositionY = Constants.Clamp(CollectionNode.PositionY + movement.Y, MinY, MaxY);
        }

        private protected void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // stop all scrolling
                        ScrollVelocity = CCPoint.Zero;
                    }
                    break;
                default:
                    break;
            }
        }

        protected void RemoveFromCollection(CCNode node, CCTouch touchOnRemove=null)
        {
            Collection.Remove(node);
            CollectionNode.RemoveChild(node);
            UpdateCollectionPositions();
            node.Scale = Constants.STANDARD_SCALE;
            // raise an event
            var handler = CollectionRemovalEvent;
            if (handler != null)
            {
                var args = new CollectionRemovalEventArgs();
                args.RemovedNode = node;
                args.TouchOnRemove = touchOnRemove;
                handler(this, args);
            }
        }

        private void UpdateCollectionPositions()
        {
            var count = Collection.Count;
            for (int i=0; i<count; i++)
            {
                Collection[i].Position = PositionInCollection(i);
            }
        }

        internal class CollectionRemovalEventArgs : EventArgs
        {
            internal CCNode RemovedNode { get; set; }
            internal CCTouch TouchOnRemove { get; set; }
        }

        internal event EventHandler<CollectionRemovalEventArgs> CollectionRemovalEvent;

        private protected void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        if (Rows == 1 && Math.Abs(touch.Delta.Y) > Math.Abs(touch.Delta.X) && Math.Abs(touch.Delta.Y) > 16.0)
                        {
                            foreach (var node in Collection)
                                if (node.BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation))
                                {
                                    RemoveFromCollection(node, touch);
                                    Pressed = false;
                                    return;
                                }
                        }
                        // move the collectionNode
                        MoveCollectionNode(touch.Delta);
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
                        var touch = touches[0];
                        var xDif = touch.Location.X - touch.PreviousLocation.X;
                        var yDif = touch.Location.Y - touch.PreviousLocation.Y;
                        CCPoint DiffOnScreen = touch.LocationOnScreen - touch.PreviousLocationOnScreen;
                        ScrollVelocity = new CCPoint(xDif, yDif);
                        ScrollVelocity *= (float)Math.Pow(DiffOnScreen.Length, 0.2);
                        ScrollVelocity *= 16;
                        ScrollTime = 0;
                        TotalScrollTime = (float)Math.Pow(DiffOnScreen.Length, 0.4) / 8;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
