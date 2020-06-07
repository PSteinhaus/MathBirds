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
        private protected Scroller Scroller { get; set; } = new Scroller();
        internal float MaxScale { get; set; } = Constants.STANDARD_SCALE;
        internal float XBorder { get; set; } = 20f;
        internal float YBorder { get; set; } = 20f;
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
                CollectionNode.ContentSize = new CCSize(columns * (BoxSize.Width+XBorder), CollectionNode.ContentSize.Height);
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
                CollectionNode.ContentSize = new CCSize(CollectionNode.ContentSize.Width, rows * (BoxSize.Height+YBorder));
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
                CollectionNode.ContentSize = new CCSize(columns * (boxSize.Width+XBorder), rows * (boxSize.Height+YBorder));
            }
        }
        internal GameObjectNode CollectionNode { get; private protected set; } = new GameObjectNode();
        internal ScrollableCollectionNode(CCSize contentSize)
        {
            Schedule();
            ContentSize = contentSize;
            CollectionNode.Scale = 1f;
            CollectionNode.AnchorPoint = CCPoint.AnchorUpperLeft;
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
            // scroll on using inertia
            Scroller.Update(dt);
        }
        internal bool AddToCollection(IGameObject gameObject)
        {
            if(Collection.Count() < Columns * Rows) // if there is space left
            {

                var ccNode = (CCNode)gameObject;
                Collection.Add(ccNode);
                CollectionNode.AddChild(ccNode);
                // place the node correctly
                ccNode.AnchorPoint = CCPoint.AnchorMiddle;
                ccNode.Position = PositionInCollection(Collection.Count() - 1);
                float ratioWidth  = ccNode.ContentSize.Width  / BoxSize.Width;
                float ratioHeight = ccNode.ContentSize.Height / BoxSize.Height;
                if (ratioWidth > ratioHeight)
                    gameObject.FitToWidth(BoxSize.Width);
                else
                    gameObject.FitToHeight(BoxSize.Height);
                if (gameObject.GetScale() > MaxScale)
                    ccNode.Scale = MaxScale;
                return true;
            }
            return false;
        }

        internal CCPoint PositionInCollection(int index)
        {
            return new CCPoint((index % Columns) * (BoxSize.Width + XBorder) + BoxSize.Width / 2, CollectionNode.ContentSize.Height - (index / Columns) * (BoxSize.Height + YBorder) - BoxSize.Height / 2);
        }
        internal float MinX
        {
            get
            {
                float minX = ContentSize.Width - BoxSize.Width * MaxBoxesPerRow - XBorder;
                if (minX > MaxX) minX = MaxX;
                return minX;
            }
        }
        internal float MaxX
        {
            get
            {
                return XBorder;
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
                float maxY = minY + (BoxSize.Height + YBorder) * MaxBoxesPerColumn - ContentSize.Height - YBorder;
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
            CollectionNode.PositionX = Constants.Clamp(CollectionNode.PositionX + movement.X, MinX, MaxX);
            CollectionNode.PositionY = Constants.Clamp(CollectionNode.PositionY + movement.Y, MinY, MaxY);
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
                        var touch = touches[0];
                        if (Rows == 1 && Math.Abs(touch.Delta.Y) > Math.Abs(touch.Delta.X) && Math.Abs(touch.Delta.Y) > 16.0)
                        {
                            foreach (var node in Collection)
                                if (node.BoundingBoxTransformedToWorld.ContainsPoint(new CCPoint(touch.Location.X, touch.StartLocation.Y)))
                                {
                                    RemoveFromCollection(node, touch);
                                    Pressed = false;
                                    return;
                                }
                        }
                        else if (Columns == 1 && Math.Abs(touch.Delta.X) > Math.Abs(touch.Delta.Y) && Math.Abs(touch.Delta.X) > 16.0)
                        {
                            foreach (var node in Collection)
                                if (node.BoundingBoxTransformedToWorld.ContainsPoint(new CCPoint(touch.StartLocation.X, touch.Location.Y)))
                                {
                                    RemoveFromCollection(node, touch);
                                    Pressed = false;
                                    return;
                                }
                        }
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
