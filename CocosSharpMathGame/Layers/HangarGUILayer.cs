using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// has no moveable camera; is drawn last (on top of everything else);
    /// holds the GUI for the PlayScene
    /// </summary>
    public class HangarGUILayer : MyLayer
    {
        internal Carousel HangarOptionCarousel { get; private protected set; }
        internal ScrollableCollectionNode TakeoffCollectionNode { get; private protected set; }
        internal CCNode TakeoffNode { get; private protected set; } = new CCNode();
        private HangarLayer HangarLayer { get; set; }
        internal GameObjectNode HangarOptionHangar { get; private protected set; }
        internal GameObjectNode HangarOptionWorkshop { get; private protected set; }
        private IGameObject dragAndDropObject;
        internal IGameObject DragAndDropObject
        {
            get { return dragAndDropObject; }
            set
            {
                if (dragAndDropObject != null)
                    RemoveChild((CCNode)dragAndDropObject);
                dragAndDropObject = value;
                if (dragAndDropObject != null)
                {
                    if (((CCNode)dragAndDropObject).Parent != null)
                    {
                        ((CCNode)dragAndDropObject).Scale = HangarScaleToGUI() * dragAndDropObject.GetScale();
                        ((CCNode)dragAndDropObject).Position = HangarCoordinatesToGUI(((CCNode)dragAndDropObject).Position);
                        ((CCNode)dragAndDropObject).Parent.RemoveChild((CCNode)dragAndDropObject);
                    }
                    AddChild((CCNode)dragAndDropObject, 100);
                }
            }
        }
        internal CCPoint HangarCoordinatesToGUI(CCPoint pointInHangarCoord)
        {
            return (pointInHangarCoord - HangarLayer.CameraPosition) * HangarScaleToGUI();
        }

        internal float HangarScaleToGUI()
        {
            return VisibleBoundsWorldspace.Size.Width / HangarLayer.VisibleBoundsWorldspace.Size.Width;
        }
        public HangarGUILayer(HangarLayer hangarLayer) : base(CCColor4B.Transparent)
        {
            HangarLayer = hangarLayer;
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            AddEventListener(touchListener, this);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();

            var bounds = VisibleBoundsWorldspace;
            HangarOptionHangar = new HangarOptionNode();
            HangarOptionWorkshop = new HangarOptionNode();
            HangarOptionCarousel = new Carousel(new CCSize(bounds.Size.Width, HangarOptionHangar.ScaledContentSize.Height));
            HangarOptionCarousel.NodeAnchor = CCPoint.AnchorMiddleTop;
            AddChild(HangarOptionCarousel);
            HangarOptionCarousel.AnchorPoint = CCPoint.AnchorUpperLeft;
            HangarOptionCarousel.Position = new CCPoint(0, bounds.MaxY);
            HangarOptionCarousel.AddToCollection(HangarOptionHangar);
            HangarOptionCarousel.AddToCollection(HangarOptionWorkshop);
            TakeoffCollectionNode = new ScrollableCollectionNode(new CCSize(bounds.Size.Width, bounds.Size.Height / 7));
            float borderToCollection = 15f;
            TakeoffNode.Position = CCPoint.Zero;
            TakeoffNode.AnchorPoint = CCPoint.AnchorLowerLeft;
            TakeoffNode.AddChild(TakeoffCollectionNode);
            TakeoffCollectionNode.PositionY = borderToCollection;
            TakeoffCollectionNode.Columns = 4;
            TakeoffCollectionNode.Rows = 1;
            TakeoffCollectionNode.BoxSize = new CCSize(TakeoffCollectionNode.ContentSize.Height, TakeoffCollectionNode.ContentSize.Height);
            AddChild(TakeoffNode, zOrder: 1);
            TakeoffNode.ContentSize = new CCSize(TakeoffCollectionNode.ContentSize.Width, TakeoffCollectionNode.ContentSize.Height + 2 * borderToCollection);
            var drawNode = new CCDrawNode();
            TakeoffNode.AddChild(drawNode, zOrder: -1);
            drawNode.DrawRect(TakeoffNode.BoundingBoxTransformedToWorld, CCColor4B.Black);
            drawNode.DrawLine(new CCPoint(0, TakeoffNode.BoundingBoxTransformedToWorld.UpperRight.Y), TakeoffNode.BoundingBoxTransformedToWorld.UpperRight, 8f, CCColor4B.White);
            drawNode.DrawLine(CCPoint.Zero, new CCPoint (TakeoffNode.BoundingBoxTransformedToWorld.MaxX, 0), 8f, CCColor4B.White);
            TakeoffNode.ContentSize = new CCSize(TakeoffNode.ContentSize.Width, TakeoffNode.ContentSize.Height + 2 * 4f);
            TakeoffNode.PositionY += 8f;
            // let the hangar listen to the TakeoffCollectionNode
            TakeoffCollectionNode.CollectionRemovalEvent += HangarLayer.ReceiveAircraftFromCollection;
            // let the hangar listen to the Carousel for a change of the middle node
            HangarOptionCarousel.MiddleChangedEvent += HangarLayer.MiddleNodeChanged;
        }

        private protected void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        if (DragAndDropObject != null)
                        {
                            ((CCNode)DragAndDropObject).Position += touch.Delta;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        if (DragAndDropObject != null)
                        {
                            switch (HangarLayer.State)
                            {
                                case HangarLayer.HangarState.HANGAR:
                                    var selectedAircraft = DragAndDropObject as Aircraft;
                                    DragAndDropObject = null;
                                    if (selectedAircraft != null)
                                    {
                                        touchEvent.StopPropogation();
                                        // if an aircraft is dragged upon the takeoff node add it to the collection
                                        if (!(TakeoffNode.BoundingBoxTransformedToParent.ContainsPoint(touch.Location) && TakeoffCollectionNode.AddToCollection(selectedAircraft)))
                                        {
                                            // if not, then place it back into the hangar
                                            HangarLayer.AddChild(selectedAircraft);
                                            selectedAircraft.Scale = Constants.STANDARD_SCALE;
                                            HangarLayer.PlaceAircraft(selectedAircraft, HangarLayer.GUICoordinatesToHangar(selectedAircraft.Position));
                                        }
                                    }
                                    break;
                            }
                            DragAndDropObject = null;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        internal void DisableTouchBegan()
        {
            TakeoffCollectionNode.Pressable = false;
            // the carousel on top is usually exempted from this, as it is not dangerous to click on it
        }

        internal void EnableTouchBegan()
        {
            TakeoffCollectionNode.Pressable = true;
            HangarOptionCarousel.Pressable = true;
        }
    }
}
