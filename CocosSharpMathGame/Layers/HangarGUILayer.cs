using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CocosSharpMathGame.Sprites.Parts;

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
        internal NonScalingCarousel PartCarousel { get; private protected set; }
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
                        if (((CCNode)dragAndDropObject).Layer == HangarLayer)
                        {
                            ((CCNode)dragAndDropObject).Scale = HangarScaleToGUI() * dragAndDropObject.GetScale();
                            ((CCNode)dragAndDropObject).Position = HangarCoordinatesToGUI(((CCNode)dragAndDropObject).PositionWorldspace);
                        }
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
            PartCarousel = new NonScalingCarousel(new CCSize(bounds.Size.Width, bounds.Size.Height / 4));
            PartCarousel.SpacingFactor = 0.3f;
            PartCarousel.MiddleChangedEvent += (sender, args) =>
            {
                foreach(var node in PartCarousel.CollectionNode.Children)
                {
                    PartCarouselNode pNode = (PartCarouselNode)node;
                    pNode.PartCollectionNode.Pressable = false;
                }
                ((PartCarouselNode)PartCarousel.MiddleNode).PartCollectionNode.Pressable = true;
            };
            PartCarousel.IsHorizontal = false;
            PartCarousel.AnchorPoint = CCPoint.AnchorUpperLeft;
            PartCarousel.Position = new CCPoint(0, bounds.MaxY);
            foreach (Part.Type type in Enum.GetValues(typeof(Part.Type)))
            {
                PartCarousel.AddToCollection(new PartCarouselNode(type));
            }
            // TEST
            for (int i=0; i<3; i++)
            {
                HangarLayer.AddPart(new TestBody());
                HangarLayer.AddPart(new TestDoubleWing());
                HangarLayer.AddPart(new TestRotor());
                HangarLayer.AddPart(new TestWeapon());
                HangarLayer.AddPart(new TestRudder());
            }
            AddChild(PartCarousel);
            // move the part carousel away as the hangar does not start there
            PartCarousel.PositionY += PartCarousel.ContentSize.Height * 1.5f;
            HangarOptionHangar = new HangarOptionNode();
            HangarOptionWorkshop = new HangarOptionNode();
            HangarOptionCarousel = new Carousel(new CCSize(bounds.Size.Width, HangarOptionHangar.BoundingBoxTransformedToWorld.Size.Height));
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
            // listen to the part carousel for a change of the middle node
            PartCarousel.MiddleChangedEvent += (sender, args) =>
            { 
                if (HangarLayer.State == HangarLayer.HangarState.MODIFY_AIRCRAFT)
                    HangarLayer.DrawInModifyAircraftState();
            };
            // also listen to each part carousel node's collectionNode
            foreach (var node in PartCarousel.CollectionNode.Children)
            {
                PartCarouselNode pNode = (PartCarouselNode)node;
                pNode.PartCollectionNode.CollectionRemovalEvent += HangarLayer.ReceivePartFromCollection;
            }
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
                            switch (HangarLayer.State)
                            {
                                case HangarLayer.HangarState.MODIFY_AIRCRAFT:
                                    {
                                        // update the mount lines (to correctly visualize proximity to a fitting mount point)
                                        HangarLayer.DrawInModifyAircraftState();
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        internal const float MOUNT_DISTANCE = 64f;
        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        if (DragAndDropObject != null)
                        {
                            touchEvent.StopPropogation();
                            switch (HangarLayer.State)
                            {
                                case HangarLayer.HangarState.HANGAR:
                                    var selectedAircraft = DragAndDropObject as Aircraft;
                                    DragAndDropObject = null;
                                    if (selectedAircraft != null)
                                    {
                                        // if an aircraft is dragged upon the takeoff node add it to the collection
                                        if (!(TakeoffNode.BoundingBoxTransformedToParent.ContainsPoint(touch.Location) && TakeoffCollectionNode.AddToCollection(selectedAircraft)))
                                        {
                                            // if not, then place it back into the hangar
                                            HangarLayer.AddAircraftChild(selectedAircraft);
                                            selectedAircraft.Scale = Constants.STANDARD_SCALE;
                                            HangarLayer.PlaceAircraft(selectedAircraft, HangarLayer.GUICoordinatesToHangar(selectedAircraft.Position));
                                        }
                                    }
                                    break;
                                case HangarLayer.HangarState.MODIFY_AIRCRAFT:
                                    {
                                        bool mounted = false;
                                        // the object is a part
                                        var part = (Part)DragAndDropObject;
                                        DragAndDropObject = null;
                                        // if it is a body and the aircraft currently has none (which means no parts at all)
                                        if (HangarLayer.ModifiedAircraft.Body == null && part.Types.Contains(Part.Type.BODY) && CCPoint.IsNear(HangarLayer.GUICoordinatesToHangar(part.PositionWorldspace), HangarLayer.ModifiedAircraft.PositionWorldspace, MOUNT_DISTANCE))
                                        {
                                            // set it as the aircraft body
                                            HangarLayer.ModifiedAircraft.InWorkshopConfiguration = false;
                                            part.Scale = 1;
                                            HangarLayer.ModifiedAircraft.Body = part;
                                            HangarLayer.ModifiedAircraft.InWorkshopConfiguration = true;
                                            HangarLayer.CalcBoundaries(); // the aircraft has changed size, so update the boundaries
                                            HangarLayer.DrawInModifyAircraftState();
                                            mounted = true;
                                        }
                                        // check if the part is currently at a mount point where it can be mounted
                                        else
                                            foreach (var modPart in HangarLayer.ModifiedAircraft.TotalParts)
                                            {
                                                if (mounted) break;
                                                foreach (var mountPoint in modPart.PartMounts)
                                                {
                                                    if (mountPoint.Available && mountPoint.CanMount(part))
                                                        if (CCPoint.IsNear(HangarLayer.GUICoordinatesToHangar(part.PositionWorldspace), mountPoint.PositionModifyAircraft, MOUNT_DISTANCE))
                                                        {
                                                            // better mount in non-workshop configuration
                                                            HangarLayer.ModifiedAircraft.InWorkshopConfiguration = false;
                                                            part.Scale = 1;
                                                            modPart.MountPart(mountPoint, part);
                                                            HangarLayer.ModifiedAircraft.InWorkshopConfiguration = true;
                                                            HangarLayer.CalcBoundaries(); // the aircraft has probably changed size, so update the boundaries
                                                            HangarLayer.DrawInModifyAircraftState();
                                                            mounted = true;
                                                            break;
                                                        }
                                                }
                                            }
                                        // if the part has not been mounted the part is just dropped and added to the collection
                                        if (!mounted)
                                        {
                                            // first disassemble it though
                                            var totalParts = part.TotalParts;
                                            part.Disassemble();
                                            foreach (var singlePart in totalParts)
                                                HangarLayer.AddPart(singlePart);
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

        internal void DisableTouchBegan(bool disableCarousel = false)
        {
            TakeoffCollectionNode.Pressable = false;
            // the carousel on top is usually exempted from this, as it is not dangerous to click on it
            if (disableCarousel) HangarOptionCarousel.Pressable = false;
        }

        internal void EnableTouchBegan(HangarLayer.HangarState state)
        {
            if (state == HangarLayer.HangarState.HANGAR)
                TakeoffCollectionNode.Pressable = true;
            if (state != HangarLayer.HangarState.MODIFY_AIRCRAFT)
                HangarOptionCarousel.Pressable = true;
        }
    }
}
