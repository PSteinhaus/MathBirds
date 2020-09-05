using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath.Atom.Atoms;

namespace CocosSharpMathGame
{
    /// <summary>
    /// has no moveable camera; is drawn last (on top of everything else);
    /// holds the GUI for the PlayScene
    /// </summary>
    [Serializable]
    public class HangarGUILayer : MyLayer
    {
        internal Carousel HangarOptionCarousel { get; set; }
        internal ScrollableCollectionNode TakeoffCollectionNode { get; set; }
        internal NonScalingCarousel PartCarousel { get; set; }
        internal CCNode TakeoffNode { get; set; } = new CCNode();
        private MathChallengeNode challengeNode;
        internal MathChallengeNode ChallengeNode
        {
            get { return challengeNode; }
            set
            {
                //if (value != challengeNode) // only do something if the value is actually changed
                {
                    // if there is an old node remove it first
                    if (challengeNode != null && challengeNode.Parent == this)
                        RemoveChild(challengeNode);
                    challengeNode = value;
                    if (challengeNode != null && challengeNode.Parent != this)
                    {
                        AddChild(challengeNode);
                    }
                }
            }
        }
        internal GOButton GOButton { get; set; }
        internal readonly CCPoint GOButtonOutPosition = new CCPoint(Constants.COCOS_WORLD_WIDTH / 2, Constants.COCOS_WORLD_HEIGHT * 1.5f);
        internal readonly CCPoint GOButtonInPosition  = new CCPoint(Constants.COCOS_WORLD_WIDTH / 2, Constants.COCOS_WORLD_HEIGHT * 0.87f);
        private HangarLayer HangarLayer { get; set; }
        internal GameObjectNode HangarOptionHangar { get; set; }
        internal GameObjectNode HangarOptionWorkshop { get; set; }
        internal GameObjectNode HangarOptionScrapyard { get; set; }
        private IGameObject dragAndDropObject;
        private CCPoint dragAndDropRelativePos;
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
                    // disable everything else while the drag and drop is in process
                    DisableTouchBegan(disableCarousel:true);
                    if (((CCNode)dragAndDropObject).Parent != null)
                    {
                        if (((CCNode)dragAndDropObject).Layer == HangarLayer)
                        {
                            ((CCNode)dragAndDropObject).Scale = HangarScaleToGUI() * dragAndDropObject.GetTotalScale();
                            ((CCNode)dragAndDropObject).Position = HangarCoordinatesToGUI(((CCNode)dragAndDropObject).PositionWorldspace);
                        }
                        ((CCNode)dragAndDropObject).Parent.RemoveChild((CCNode)dragAndDropObject);
                    }
                    AddChild((CCNode)dragAndDropObject, 100);
                }
                else
                {
                    // reenable everything
                    EnableTouchBegan(HangarLayer.State);
                    // reset the relative position
                    dragAndDropRelativePos = CCPoint.Zero;
                }
            }
        }
        internal void StartGame() { HangarLayer.StartGame(); }
        internal void SetDragAndDropObjectWithRelativeTouchPos(IGameObject g, CCTouch touch, bool changeTouchCoordToGUI=true)
        {
            DragAndDropObject = g;
            dragAndDropRelativePos = ((CCNode)g).Position - (changeTouchCoordToGUI ? HangarCoordinatesToGUI(touch.Location) : touch.Location);
        }
        internal CCPoint HangarCoordinatesToGUI(CCPoint pointInHangarCoord)
        {
            return (pointInHangarCoord - HangarLayer.CameraPosition) * HangarScaleToGUI();
        }

        internal float HangarScaleToGUI()
        {
            return VisibleBoundsWorldspace.Size.Width / HangarLayer.VisibleBoundsWorldspace.Size.Width;
        }
        public HangarGUILayer(HangarLayer hangarLayer) : base(CCColor4B.Transparent, countTouches:true)
        {
            HangarLayer = hangarLayer;
            Scroller = null;
            PartCarousel = new NonScalingCarousel(new CCSize(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT / 3));
            PartCarousel.SpacingFactor = 0.2f;
            PartCarousel.MiddleChangedEvent += (sender, args) =>
            {
                foreach (var node in PartCarousel.CollectionNode.Children)
                {
                    PartCarouselNode pNode = (PartCarouselNode)node;
                    pNode.PartCollectionNode.Pressable = false;
                }
                ((PartCarouselNode)PartCarousel.MiddleNode).PartCollectionNode.Pressable = true;
            };
            PartCarousel.IsHorizontal = false;
            PartCarousel.AnchorPoint = CCPoint.AnchorUpperLeft;
            PartCarousel.Position = new CCPoint(0, Constants.COCOS_WORLD_HEIGHT);
            foreach (Part.Type type in Enum.GetValues(typeof(Part.Type)))
            {
                PartCarousel.AddToCollection(new PartCarouselNode(type));
            }
            PartCarousel.Visible = false;
            AddChild(PartCarousel);
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = base.OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesCancelled = OnTouchesEnded;
            AddEventListener(touchListener, this);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            // DEBUG: test out the PopUp
            //PopUp.ShowPopUp(this, "This is a test.");

            var bounds = VisibleBoundsWorldspace;
            // move the part carousel away as the hangar does not start there
            PartCarousel.PositionY += PartCarousel.ContentSize.Height * 1.5f;
            HangarOptionHangar = new HangarOptionHangar();
            HangarOptionWorkshop = new HangarOptionWorkshop();
            HangarOptionScrapyard = new HangarOptionScrapyard();
            HangarOptionCarousel = new Carousel(new CCSize(bounds.Size.Width, HangarOptionHangar.BoundingBoxTransformedToWorld.Size.Height));
            HangarOptionCarousel.NodeAnchor = CCPoint.AnchorMiddleTop;
            AddChild(HangarOptionCarousel);
            HangarOptionCarousel.AnchorPoint = CCPoint.AnchorUpperLeft;
            HangarOptionCarousel.Position = new CCPoint(0, bounds.MaxY);
            HangarOptionCarousel.AddToCollection(HangarOptionHangar);
            HangarOptionCarousel.AddToCollection(HangarOptionWorkshop);
            HangarOptionCarousel.AddToCollection(HangarOptionScrapyard);
            TakeoffCollectionNode = new ScrollableCollectionNode(new CCSize(bounds.Size.Width, bounds.Size.Height / 7));
            float borderToCollection = 15f;
            TakeoffNode.Position = CCPoint.Zero;
            TakeoffNode.AnchorPoint = CCPoint.AnchorLowerLeft;
            TakeoffNode.AddChild(TakeoffCollectionNode);
            TakeoffCollectionNode.PositionY = borderToCollection;
            TakeoffCollectionNode.Columns = HangarLayer.UnlockedPlaneSlots;  // start off with only one plane slot unlocked
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
            GOButton = new GOButton();
            AddChild(GOButton); // place the go button a bit higher than the rest (in ZOrder)
            GOButton.Visible = false;
            GOButton.Position = GOButtonOutPosition;
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
                            ((CCNode)DragAndDropObject).Position = touch.Location + dragAndDropRelativePos;
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

        internal const float MOUNT_DISTANCE = 200f;
        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesEnded(touches, touchEvent);
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
                                        if (TakeoffCollectionNode.Collection.Any())
                                            GOButton.AddAction(HangarLayer.AddGOButton);
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
                                        else if (!mounted)
                                        {
                                            var possibleMounts = new List<PartMount>();
                                            foreach (var modPart in HangarLayer.ModifiedAircraft.TotalParts)
                                            {
                                                foreach (var mountPoint in modPart.PartMounts)
                                                {
                                                    if (mountPoint.Available && mountPoint.CanMount(part))
                                                        if (CCPoint.IsNear(HangarLayer.GUICoordinatesToHangar(part.PositionWorldspace), mountPoint.PositionModifyAircraft, MOUNT_DISTANCE))
                                                        {
                                                            possibleMounts.Add(mountPoint);
                                                        }
                                                }
                                            }
                                            // mount at the closest possible mount point
                                            float minDistance = float.PositiveInfinity;
                                            PartMount closestMount = null;
                                            foreach (var mountPoint in possibleMounts)
                                            {
                                                float distance = CCPoint.Distance(HangarLayer.GUICoordinatesToHangar(part.PositionWorldspace), mountPoint.PositionModifyAircraft);
                                                if (distance < minDistance)
                                                {
                                                    minDistance = distance;
                                                    closestMount = mountPoint;
                                                }
                                            }
                                            if (closestMount != null)
                                            {
                                                // better mount in non-workshop configuration
                                                HangarLayer.ModifiedAircraft.InWorkshopConfiguration = false;
                                                part.Scale = 1;
                                                closestMount.MyPart.MountPart(closestMount, part);
                                                HangarLayer.ModifiedAircraft.InWorkshopConfiguration = true;
                                                HangarLayer.CalcBoundaries(); // the aircraft has probably changed size, so update the boundaries
                                                HangarLayer.DrawInModifyAircraftState();
                                                mounted = true;
                                            }
                                        }
                                        // if the part has not been mounted the part is just dropped and added to the collection
                                        if (!mounted)
                                        {
                                            // first disassemble it though
                                            // and flip it if it is flipped
                                            var totalParts = part.TotalParts;
                                            part.Disassemble();
                                            foreach (var singlePart in totalParts)
                                            {
                                                if (singlePart.Flipped) singlePart.Flip();
                                                HangarLayer.AddPart(singlePart);
                                            }
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
            if (state != HangarLayer.HangarState.MODIFY_AIRCRAFT && state != HangarLayer.HangarState.SCRAPYARD_CHALLENGE)
                HangarOptionCarousel.Pressable = true;
        }

        internal override void Clear()
        {
            TouchCountSource = null;
            HangarLayer = null;
            
            this.PartCarousel = null;
            this.TakeoffCollectionNode = null;
            
            this.TakeoffNode = null;
            this.HangarOptionWorkshop = null;
            this.HangarOptionScrapyard = null;
            this.HangarOptionHangar = null;
            this.HangarOptionCarousel = null;
            
            this.GOButton = null;
            //this.DragAndDropObject = null;
            
            this.challengeNode = null;
            
            this.FirstTouchListener = null;
            //this.Scroller.MoveFunction = null;
            this.Scroller = null;
            
            
            this.StopAllActions();
            this.ResetCleanState();
        }
    }
}
