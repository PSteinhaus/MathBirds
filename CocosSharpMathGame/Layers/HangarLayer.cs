using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Microsoft.Xna.Framework;
using SkiaSharp;
using Symbolism;

namespace CocosSharpMathGame
{
    public class HangarLayer : MyLayer
    {
        const float TRANSITION_TIME = 0.5f;
        internal enum HangarState
        {
            TRANSITION, HANGAR, WORKSHOP, SCRAPYARD,
            MODIFY_AIRCRAFT
        }
        internal HangarState State = HangarState.HANGAR;
        internal CCDrawNode HighDrawNode { get; private protected set; } = new CCDrawNode();
        internal CCDrawNode LowDrawNode { get; private protected set; }  = new CCDrawNode();
        public HangarGUILayer GUILayer { get; set; }
        internal CCNode BGNode { get; private protected set; }
        private CCDrawNode BGDrawNode { get; set; }
        private CCColor4B BGColor { get; set; } = new CCColor4B(50, 50, 50);
        private IGameObject RotationSelectedNode;
        internal List<Aircraft> Aircrafts = new List<Aircraft>();
        internal List<Part> Parts = new List<Part>();
        public HangarLayer() : base(CCColor4B.Black)
        {
            NewAircraftButton = new NewAircraftButton(this);
            NewAircraftButton.Visible = false;
            AddChild(NewAircraftButton);
            AddChild(LowDrawNode,  int.MinValue);
            LowDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            AddChild(HighDrawNode, int.MaxValue);
            HighDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            // a double tap starts the transition back the workshop state (of course only coming from the modify-aircraft state)
            DoubleTapEvent += (sender, args) => { if(State == HangarState.MODIFY_AIRCRAFT) StartTransition(HangarState.WORKSHOP); }; 
            GUILayer = new HangarGUILayer(this);
            BGNode = new CCNode();
            AddChild(BGNode);
            BGDrawNode = new CCDrawNode();
            BGDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            BGNode.AddChild(BGDrawNode);
            DrawBG();
            BGNode.ZOrder = -20;
            //BGNode.Rotation = 45f;
            // add some aircrafts
            AddAircraft(new TestAircraft(), CCPoint.Zero);
            AddAircraft(new TestAircraft(false), CCPoint.Zero);
            AddAircraft(new TestAircraft(), CCPoint.Zero);
            AddAircraft(new TestAircraft(), CCPoint.Zero);
            AddAircraft(new TestAircraft(), CCPoint.Zero);
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            AddEventListener(touchListener, this);

            // add a mouse listener
            var mouseListener = new CCEventListenerMouse();
            mouseListener.OnMouseScroll = OnMouseScrollZoom;
            AddEventListener(mouseListener, this);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            if (Aircrafts.Any())
            {
                CalcBoundaries();
            }
            CameraSize = new CCSize(MaxCameraWidth, MaxCameraHeight) / 2;
            CameraPosition = new CCPoint(-CameraSize.Width / 2, -CameraSize.Height / 2);
            UpdateCamera();
            CreateActions();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (State == HangarState.TRANSITION)
            {
                TimeInTransition += dt;
                MoveCameraInTransition();
            }
        }

        internal void IncreaseBGAlpha()
        {
            BGColor = new CCColor4B(BGColor.R, BGColor.G, BGColor.B, (byte)(BGColor.A + 1 <= byte.MaxValue ? BGColor.A + 1 : byte.MaxValue));
            DrawBG();
        }

        internal void DecreaseBGAlpha()
        {
            BGColor = new CCColor4B(BGColor.R, BGColor.G, BGColor.B, (byte)(BGColor.A - 1 >= 0 ? BGColor.A - 1 : 0));
            DrawBG();
        }

        internal void DrawBG()
        {
            BGDrawNode.Clear();
            const float bgSize = 6000f;
            for (int i = -40; i < 40; i++)
            {
                BGDrawNode.DrawLine(new CCPoint(i * bgSize / 40, -bgSize), new CCPoint(i * bgSize / 40, bgSize), 4f, BGColor);
                BGDrawNode.DrawLine(new CCPoint(-bgSize, i * bgSize / 40), new CCPoint(bgSize, i * bgSize / 40), 4f, BGColor);
            }
        }

        private void CreateActions()
        {
            const float easeRate = 0.6f;
            TakeoffNodeToHangar = new CCSequence(new CCCallFunc(() => { GUILayer.TakeoffNode.Visible = true; }), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, 8f)), easeRate) );
            TakeoffNodeLeave    = new CCSequence(new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, -GUILayer.TakeoffNode.BoundingBoxTransformedToWorld.Size.Height)), easeRate), new CCCallFunc(() => { GUILayer.TakeoffNode.Visible = false; }));
            BGFadeOut           = new CCRepeat(new CCSequence(new CCCallFunc(DecreaseBGAlpha), new CCDelayTime(TRANSITION_TIME / 255)), 255);
            BGFadeIn            = new CCRepeat(new CCSequence(new CCCallFunc(IncreaseBGAlpha), new CCDelayTime(TRANSITION_TIME / 255)), 255);
            FadeOut             = new CCEaseIn(new CCFadeTo(TRANSITION_TIME, 0),   easeRate);
            FadeIn              = new CCEaseIn(new CCFadeTo(TRANSITION_TIME, 255), easeRate);
            RemoveCarousel      = new CCSequence(new CCEaseIn(new CCMoveBy(TRANSITION_TIME, new CCPoint(0, GUILayer.HangarOptionCarousel.BoundingBoxTransformedToWorld.Size.Height + 0f)), easeRate*0.7f), new CCCallFunc(() => { GUILayer.HangarOptionCarousel.Visible = false; }));
            AddCarousel         = new CCSequence(new CCCallFunc(() => { GUILayer.HangarOptionCarousel.Visible = true; }), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, GUILayer.VisibleBoundsWorldspace.MaxY)), easeRate*0.7f));
            RemovePartCarousel  = new CCSequence(new CCEaseIn(new CCMoveBy(TRANSITION_TIME, new CCPoint(0, GUILayer.PartCarousel.ContentSize.Height)), easeRate * 0.7f), new CCCallFunc(() => { GUILayer.PartCarousel.Visible = false; }));
            AddPartCarousel     = new CCSequence(new CCDelayTime(TRANSITION_TIME), new CCCallFunc(() => { GUILayer.PartCarousel.Visible = true; }), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, GUILayer.VisibleBoundsWorldspace.MaxY)), easeRate * 0.7f));
            AddNewAircraftButton = new CCSequence(new CCCallFunc(() => { NewAircraftButton.Visible = true; }), FadeIn, new CCCallFunc(() => { NewAircraftButton.Pressable = true; }));
            RemoveNewAircraftButton = new CCSequence(new CCCallFunc(() => { NewAircraftButton.Pressable = false; }), FadeOut, new CCCallFunc(() => { NewAircraftButton.Visible = false; }));
            AddGOButton         = new CCSequence(new CCCallFunc(() => { GUILayer.GOButton.Visible = true; }), new CCEaseOut(new CCMoveTo(TRANSITION_TIME, GUILayer.GOButtonInPosition), 20f));
            RemoveGOButton      = new CCSequence(new CCEaseIn(new CCMoveTo(TRANSITION_TIME, GUILayer.GOButtonOutPosition), 10f), new CCCallFunc(() => { GUILayer.GOButton.Visible = false; }));
        }

        internal float TimeInTransition = 0f;

        internal CCAction AddGOButton;
        internal CCAction RemoveGOButton;
        private CCAction AddNewAircraftButton;
        private CCAction RemoveNewAircraftButton;
        private CCAction AddPartCarousel;
        private CCAction RemovePartCarousel;
        private CCAction AddCarousel;
        private CCAction RemoveCarousel;
        private CCFiniteTimeAction FadeOut;
        private CCFiniteTimeAction FadeIn;
        private CCAction TransistionAction;
        private CCAction TakeoffNodeToHangar;
        private CCAction TakeoffNodeLeave;
        private CCAction BGFadeOut;
        private CCAction BGFadeIn;
        private const int MoveAircraftTag  = 73828192;
        private const int ScaleAircraftTag = 73828193;
        private const int RotateAircraftTag = 73828193;

        private CCPoint CameraPositionHangar;
        private CCSize  CameraSizeHangar;

        private CCPoint LastCameraPosition;
        private CCPoint NextCameraPosition;
        private CCSize  LastCameraSize;
        private CCSize  NextCameraSize;
        internal float TransitionTime { get; private protected set; }
        internal float CameraMoveTime { get; private protected set; }
        private void MoveCameraInTransition()
        {
            float ratio = Constants.Clamp(TimeInTransition / CameraMoveTime, 0, 1);
            CameraPosition = LastCameraPosition + (NextCameraPosition - LastCameraPosition) * ratio;
            CameraSize = new CCSize(LastCameraSize.Width  + (NextCameraSize.Width  - LastCameraSize.Width)  * ratio,
                                    LastCameraSize.Height + (NextCameraSize.Height - LastCameraSize.Height) * ratio);
            UpdateCamera();
        }
        internal void MiddleNodeChanged(object sender, EventArgs args)
        {
            var state = HangarState.HANGAR;
            // get the state to go to
            var carousel = (Carousel)sender;
            var middle = carousel.MiddleNode;
            if (middle == GUILayer.HangarOptionHangar)
            {
                state = HangarState.HANGAR;
            }
            else if (middle == GUILayer.HangarOptionWorkshop)
            {
                state = HangarState.WORKSHOP;
            }
            StartTransition(state);
        }

        private Dictionary<Aircraft, CCPoint> HangarPositions = new Dictionary<Aircraft, CCPoint>();
        private Dictionary<Aircraft, float>   HangarRotations = new Dictionary<Aircraft, float>();
        private const float WorkshopBoxBorderY = 120f;
        internal NewAircraftButton NewAircraftButton { get; private set; }
        private CCPoint WorkshopPosition(Aircraft aircraft)
        {
            CCPoint pos = CCPoint.Zero;
            // the first position is taken by the button for creating a new aircraft
            pos -= new CCPoint(0, NewAircraftButton.BoundingBoxTransformedToWorld.Size.Height + WorkshopBoxBorderY);
            Aircraft lastAircraft = null;
            foreach (var aircr in Aircrafts)
            {
                if (lastAircraft != null)
                {
                    float scaleLast = WorkshopScale(lastAircraft);
                    pos -= new CCPoint(0, lastAircraft.ContentSize.Height * scaleLast / 2);
                    float scale = WorkshopScale(aircr);
                    pos -= new CCPoint(0, aircr.ContentSize.Height * scale / 2 + WorkshopBoxBorderY);
                }
                if (aircr == aircraft)
                    return pos;
                lastAircraft = aircr;
            }
            return CCPoint.Zero;
        }
        private readonly CCPoint CameraPositionWorkshop = new CCPoint(-Constants.COCOS_WORLD_WIDTH / 2, -Constants.COCOS_WORLD_HEIGHT * 0.75f);
        internal void StartTransition(HangarState state)
        {
            var oldState = State;
            TransitionTime = TRANSITION_TIME; // usually the transition takes TRANSITION_TIME seconds
            CameraMoveTime = TRANSITION_TIME; // the camera move (if there is one) as well
            LastCameraPosition = CameraPosition;
            LastCameraSize = CameraSize;
            if (State == HangarState.HANGAR)
            {
                CameraPositionHangar = CameraPosition;
                CameraSizeHangar = CameraSize;
            }
            State = HangarState.TRANSITION;
            CalcBoundaries(); // allow the camera to move freely
            // stop all current transition actions
            StopAllTransitionActions();
            // disable the touchBegan listeners for all gui elements (exept the carousel usually)
            GUILayer.DisableTouchBegan(state == HangarState.MODIFY_AIRCRAFT || oldState == HangarState.MODIFY_AIRCRAFT);
            // start transition actions
            TransistionAction = new CCSequence(new CCDelayTime(TRANSITION_TIME), new CCCallFunc(() => FinalizeTransition(state)));
            if (state != HangarState.HANGAR)
            {
                GUILayer.GOButton.AddAction(RemoveGOButton);
                GUILayer.TakeoffNode.AddAction(TakeoffNodeLeave);
                BGNode.AddAction(BGFadeOut);
            }
            if (state != HangarState.WORKSHOP)
            {
                NewAircraftButton.AddAction(RemoveNewAircraftButton);
            }
            if (state == HangarState.HANGAR)
            {
                GUILayer.TakeoffNode.AddAction(TakeoffNodeToHangar);
                // if the takeoffnode holds at least one aircraft add the GO-button
                if (GUILayer.TakeoffCollectionNode.Collection.Any())
                    GUILayer.GOButton.AddAction(AddGOButton);
                BGNode.AddAction(BGFadeIn);
                foreach (var aircraft in Aircrafts)
                {
                    if (aircraft.Parent != this) continue;
                    var moveAction = new CCMoveTo(TRANSITION_TIME, HangarPositions[aircraft]);
                    moveAction.Tag = MoveAircraftTag;
                    aircraft.AddAction(moveAction);
                    var scaleAction = new CCScaleTo(TRANSITION_TIME, Constants.STANDARD_SCALE);
                    scaleAction.Tag = ScaleAircraftTag;
                    aircraft.AddAction(scaleAction);
                    var rotateAction = new CCRotateTo(TRANSITION_TIME, HangarRotations[aircraft]);
                    rotateAction.Tag = RotateAircraftTag;
                    aircraft.AddAction(rotateAction);
                }
                NextCameraPosition = CameraPositionHangar;
                NextCameraSize = CameraSizeHangar;
            }
            else if (state == HangarState.WORKSHOP)
            {
                NextCameraSize = new CCSize(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT);
                if (oldState != HangarState.MODIFY_AIRCRAFT)
                {
                    NextCameraPosition = CameraPositionWorkshop;
                }
                else
                {
                    // the transition starts off at MODIFY_AIRCRAFT, so (of course) do a different kind of transition
                    // the carousel was removed so add it again
                    GUILayer.HangarOptionCarousel.AddAction(AddCarousel);
                    // remove the part carousel
                    GUILayer.PartCarousel.AddAction(RemovePartCarousel);
                    // stop drawing the connections
                    HighDrawNode.Clear();
                    LowDrawNode.Clear();
                    // if the ModifiedAircraft has no Body it needs to be removed
                    if (ModifiedAircraft.Body == null)
                    {
                        RemoveAircraft(ModifiedAircraft);
                    }
                    // the aircraft has (probably) been modified, so it should run through the placement algorithm once more
                    var currentPos = ModifiedAircraft.Position;
                    PlaceAircraft(ModifiedAircraft, HangarPositions[ModifiedAircraft]);
                    ModifiedAircraft.Position = currentPos;
                    // get the standard configuration positions
                    ModifiedAircraft.InWorkshopConfiguration = false;
                    var totalParts = ModifiedAircraft.TotalParts;
                    CCPoint[] standardConfigurationsPositions = new CCPoint[totalParts.Count()];
                    for (int i = 0; i < totalParts.Count(); i++)
                        standardConfigurationsPositions[i] = totalParts.ElementAt(i).Position;
                    ModifiedAircraft.InWorkshopConfiguration = true;
                    // now move the parts slowly to these positions
                    for (int i = 0; i < totalParts.Count(); i++)
                        totalParts.ElementAt(i).AddAction(new CCEaseIn(new CCMoveTo(TRANSITION_TIME, standardConfigurationsPositions[i]), 2.6f));
                    ModifiedAircraft.AddAction(new CCSequence(new CCDelayTime(TRANSITION_TIME), new CCCallFunc(() => { ModifiedAircraft.InWorkshopConfiguration = false; ModifiedAircraft = null; })));
                    // focus on the selected aircraft
                    NextCameraPosition = new CCPoint(CameraPositionWorkshop.X, ModifiedAircraft.Position.Y - NextCameraSize.Height / 2);
                }
                foreach (var aircraft in Aircrafts)
                {
                    if (aircraft.Parent != this) continue;
                    aircraft.AddAction(FadeIn);
                    var moveAction = new CCMoveTo(TRANSITION_TIME, WorkshopPosition(aircraft));
                    moveAction.Tag = MoveAircraftTag;
                    aircraft.AddAction(moveAction);
                    var scaleAction = new CCScaleTo(TRANSITION_TIME, WorkshopScale(aircraft));
                    scaleAction.Tag = ScaleAircraftTag;
                    aircraft.AddAction(scaleAction);
                    var rotateAction = new CCRotateTo(TRANSITION_TIME, 0f);
                    rotateAction.Tag = RotateAircraftTag;
                    aircraft.AddAction(rotateAction);
                }
                NewAircraftButton.AddAction(AddNewAircraftButton);
            }
            else if (state == HangarState.MODIFY_AIRCRAFT)
            {
                TransitionTime = TRANSITION_TIME * 2 + 0.0001f;
                // disable and remove the carousel
                GUILayer.HangarOptionCarousel.AddAction(RemoveCarousel);
                // fade out all aircrafts but the selected one
                foreach (var aircraft in Aircrafts)
                {
                    if (aircraft.Parent != this || aircraft == ModifiedAircraft) continue;
                    aircraft.AddAction(FadeOut);
                }
                // add the part carousel
                GUILayer.PartCarousel.AddAction(AddPartCarousel);
                // get the workshop configuration positions
                ModifiedAircraft.InWorkshopConfiguration = true;
                var totalParts = ModifiedAircraft.TotalParts;
                // and the workshop configutation size
                float newCamWidth = ModifyAircraftWidth();
                if (totalParts != null)
                {
                    CCPoint[] workshopConfigurationsPositions = new CCPoint[totalParts.Count()];
                    for (int i = 0; i < totalParts.Count(); i++)
                        workshopConfigurationsPositions[i] = totalParts.ElementAt(i).Position;
                    ModifiedAircraft.InWorkshopConfiguration = false;
                    // now move the parts slowly to these positions
                    for (int i = 0; i < totalParts.Count(); i++)
                        totalParts.ElementAt(i).AddAction(new CCSequence(new CCDelayTime(TRANSITION_TIME), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, workshopConfigurationsPositions[i]), 2.6f)));
                }
                ModifiedAircraft.InWorkshopConfiguration = false;
                ModifiedAircraft.AddAction(new CCSequence(new CCDelayTime(TRANSITION_TIME*2), new CCCallFunc(() => { ModifiedAircraft.InWorkshopConfiguration = true; })));
                NextCameraSize = new CCSize(newCamWidth, CameraSize.Height * newCamWidth / CameraSize.Width);
                // focus on the selected aircraft
                NextCameraPosition = ModifiedAircraft.Position - ((CCPoint)NextCameraSize / 2);
                // this special transition takes twice the time
            }
            TransistionAction = new CCSequence(new CCDelayTime(TransitionTime), new CCCallFunc(() => FinalizeTransition(state)));
            AddAction(TransistionAction);
        }

        /// <summary>
        /// Add a part to the collection of parts owned by the player
        /// </summary>
        /// <param name="part"></param>
        internal void AddPart(Part part)
        {
            // find the correct collection
            foreach (var node in GUILayer.PartCarousel.CollectionNode.Children)
            {
                var pNode = (PartCarouselNode)node;
                if (part.Types.Contains(pNode.PartType))
                {
                    pNode.AddPart(part);
                    break;
                }
            }
        }

        internal float ModifyAircraftWidth()
        {
            var totalParts = ModifiedAircraft.TotalParts;
            float xMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity;
            if (totalParts.Any())
            {
                foreach (var part in totalParts)
                {
                    CCRect box = part.BoundingBoxTransformedToWorld;
                    if (box.MinX < xMin) xMin = box.MinX;
                    if (box.MaxX > xMax) xMax = box.MaxX;
                }
                return Math.Max(Math.Abs(xMin), Math.Abs(xMax)) * 2 + 200f; // the last value is an additional border to the edge of the screen
            }
            else
                return 300f;
        }

        internal Aircraft ModifiedAircraft { get; set; }

        private float WorkshopScale(Aircraft aircraft)
        {
            float scale = Constants.STANDARD_SCALE;
            float maxWidth = Constants.COCOS_WORLD_WIDTH * 0.8f;
            if (aircraft.ContentSize.Width * scale > maxWidth)
                scale = maxWidth / aircraft.ContentSize.Width;
            return scale;
        }

        private float WorkshopHeight()
        {
            if (!Aircrafts.Any())
                return 0f;
            return -WorkshopPosition(Aircrafts.Last()).Y;
        }

        private void StopAllTransitionActions()
        {
            // stop all actions that could be happening right now
            if (TransistionAction != null) StopAction(TransistionAction.Tag);
            GUILayer.HangarOptionCarousel.StopAction(AddCarousel.Tag);
            GUILayer.HangarOptionCarousel.StopAction(RemoveCarousel.Tag);
            GUILayer.TakeoffNode.StopAction(TakeoffNodeToHangar.Tag);
            GUILayer.TakeoffNode.StopAction(TakeoffNodeLeave.Tag);
            GUILayer.PartCarousel.StopAction(AddPartCarousel.Tag);
            GUILayer.PartCarousel.StopAction(RemovePartCarousel.Tag);
            GUILayer.GOButton.StopAction(AddGOButton.Tag);
            GUILayer.GOButton.StopAction(RemoveGOButton.Tag);
            BGNode.StopAction(BGFadeOut.Tag);
            BGNode.StopAction(BGFadeIn.Tag);
            NewAircraftButton.StopAction(AddNewAircraftButton.Tag);
            NewAircraftButton.StopAction(RemoveNewAircraftButton.Tag);
            foreach (var aircraft in Aircrafts)
            {
                aircraft.StopAction(MoveAircraftTag);
                aircraft.StopAction(ScaleAircraftTag);
                aircraft.StopAction(RotateAircraftTag);
                aircraft.StopAction(FadeOut.Tag);
                aircraft.StopAction(FadeIn.Tag);
            }
            TimeInTransition = 0; // reset transition time
        }

        internal void DrawInModifyAircraftState()
        {
            HighDrawNode.Clear();
            LowDrawNode.Clear();
            // draw the connections between mount points and mounted parts
            const float LINE_WIDTH = 4f;
            const float RADIUS = 5f;
            CCColor4B LINE_COLOR = new CCColor4B(70, 70, 70);
            CCColor4B EMPTY_MOUNT_COLOR = new CCColor4B(150, 150, 150);
            //CCColor4B EMPTY_MOUNT_COLOR = CCColor4B.White;
            void DrawMountCircle(CCPoint pos, CCColor4B color, CCDrawNode drawNode)
            {
                drawNode.DrawSolidCircle(pos, RADIUS + LINE_WIDTH, color);
                drawNode.DrawSolidCircle(pos, RADIUS, CCColor4B.Black);
            }
            void DrawMountLine(CCPoint start, CCPoint end, CCColor4B color, CCDrawNode drawNode)
            {
                // first draw the diagonal segment
                CCPoint diff = end - start;
                CCPoint middle = CCPoint.Zero;
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    middle = new CCPoint(start.X + Math.Sign(diff.X) * Math.Abs(diff.Y), end.Y);
                else
                    middle = new CCPoint(end.X, start.Y + Math.Sign(diff.Y) * Math.Abs(diff.X));
                drawNode.DrawLine(start, middle, LINE_WIDTH, color, CCLineCap.Round);
                drawNode.DrawLine(middle, end, LINE_WIDTH, color, CCLineCap.Round);
            }
            foreach (var part in ModifiedAircraft.TotalParts)
            {
                foreach (var mountPoint in part.PartMounts)
                {
                    var mountedPart = mountPoint.MountedPart;
                    if (mountedPart != null)
                    {
                        DrawMountLine(mountPoint.PositionWorldspace, mountedPart.PositionWorldspace, LINE_COLOR, LowDrawNode);
                        //DrawMountCircle(mountedPart.PositionWorldspace, CCColor4B.White, LowDrawNode);
                    }
                    else if (mountPoint.Available)
                    {
                        // use different colors, depending on whether the mount point is fitting for the part type of the part carousel current middle node
                        // and also depending on whether there is a drag-and-drop part that would be mounted here if dropped now
                        CCColor4B color = LINE_COLOR;
                        if ((GUILayer.DragAndDropObject == null && mountPoint.AllowedTypes.Contains(((PartCarouselNode)GUILayer.PartCarousel.MiddleNode).PartType)) ||
                            (GUILayer.DragAndDropObject != null && mountPoint.CanMount((Part)GUILayer.DragAndDropObject)))
                            color = EMPTY_MOUNT_COLOR;
                        if (GUILayer.DragAndDropObject != null && mountPoint.CanMount((Part)GUILayer.DragAndDropObject) && CCPoint.IsNear(mountPoint.PositionModifyAircraft, GUICoordinatesToHangar(((Part)GUILayer.DragAndDropObject).PositionWorldspace), HangarGUILayer.MOUNT_DISTANCE))
                            color = CCColor4B.White;
                        // draw a line to the where the mount point is visualized
                        DrawMountLine(mountPoint.PositionWorldspace, mountPoint.PositionModifyAircraft, color, LowDrawNode);
                        DrawMountCircle(mountPoint.PositionModifyAircraft, color, HighDrawNode);
                    }
                }
            }
            if (!ModifiedAircraft.TotalParts.Any())
            {
                CCColor4B color = LINE_COLOR;
                if ((GUILayer.DragAndDropObject == null && ((PartCarouselNode)GUILayer.PartCarousel.MiddleNode).PartType == Part.Type.BODY) ||
                    (GUILayer.DragAndDropObject != null && ((Part)GUILayer.DragAndDropObject).Types.Contains(Part.Type.BODY)))
                    color = EMPTY_MOUNT_COLOR;
                if (GUILayer.DragAndDropObject != null && ((Part)GUILayer.DragAndDropObject).Types.Contains(Part.Type.BODY) && CCPoint.IsNear(ModifiedAircraft.PositionWorldspace, GUICoordinatesToHangar(((Part)GUILayer.DragAndDropObject).PositionWorldspace), HangarGUILayer.MOUNT_DISTANCE))
                    color = CCColor4B.White;
                DrawMountCircle(ModifiedAircraft.PositionWorldspace, color, HighDrawNode);
            }
        }
        private void FinalizeTransition(HangarState state)
        {
            State = state;
            switch (State)
            {
                case HangarState.MODIFY_AIRCRAFT:
                    {
                        DrawInModifyAircraftState();
                    }
                    break;
            }
            CameraPosition = NextCameraPosition;
            CameraSize = NextCameraSize;
            UpdateCamera();
            CalcBoundaries();
            GUILayer.EnableTouchBegan(state);
        }
        internal CCPoint GUICoordinatesToHangar(CCPoint pointInGUICoord)
        {
            return CameraPosition + pointInGUICoord * GUIScaleToHangar();
        }

        internal float GUIScaleToHangar()
        {
            return VisibleBoundsWorldspace.Size.Width / GUILayer.VisibleBoundsWorldspace.Size.Width;
        }

        internal void ReceiveAircraftFromCollection(object sender, ScrollableCollectionNode.CollectionRemovalEventArgs e)
        {
            Aircraft aircraft = (Aircraft)e.RemovedNode;
            aircraft.ResetAnchorPoint();
            aircraft.Position = e.TouchOnRemove.Location;
            GUILayer.DragAndDropObject = aircraft;
            // check if the takeoffnode still holds any aircrafts
            if (!GUILayer.TakeoffCollectionNode.Collection.Any())
                GUILayer.GOButton.AddAction(RemoveGOButton);
        }

        internal void ReceivePartFromCollection(object sender, ScrollableCollectionNode.CollectionRemovalEventArgs e)
        {
            var node = e.RemovedNode;
            var gameObject = (IGameObject)e.RemovedNode;
            node.Position = e.TouchOnRemove.Location;
            node.Scale = GUILayer.HangarScaleToGUI() * Constants.STANDARD_SCALE;
            GUILayer.DragAndDropObject = gameObject;
        }
        internal void AddAircraft(Aircraft aircraft, CCPoint hangarPos)
        {
            Aircrafts.Add(aircraft);
            AddAircraftChild(aircraft);
            HangarRotations[aircraft] = 0f;
            PlaceAircraft(aircraft, hangarPos);
        }

        /// <summary>
        /// WARNING: the following function assumes the aircraft is a child of the HangarLayer and that IT IS NOT CONTAINED ANYWHERE ELESE
        /// </summary>
        /// <param name="aircraft"></param>
        internal void RemoveAircraft(Aircraft aircraft)
        {
            Aircrafts.Remove(aircraft);
            aircraft.PrepareForRemoval();
            RemoveChild(aircraft);
        }

        internal void AddAircraftChild(Aircraft aircraft)
        {
            AddChild(aircraft, (int)aircraft.Area);
        }

        internal void ModifyNewAircraft()
        {
            Aircraft newAircraft = new Aircraft();
            AddAircraft(newAircraft, CCPoint.Zero);
            newAircraft.Position = CCPoint.Zero;
            ModifiedAircraft = newAircraft;
            StartTransition(HangarState.MODIFY_AIRCRAFT);
        }

        internal void PlaceAircraft(Aircraft aircraft, CCPoint hangarPos)
        {
            const float SAFETY = 0.001f; // numeric safety
            aircraft.Position = hangarPos;
            HangarPositions[aircraft] = aircraft.Position;
            var placementRect = PlacementRect(aircraft);
            if (RectAvailable(placementRect, out CCRect blockingRect, aircraft))
            {
                // all is well, the position is available
            }
            else
            {
                // the position is not available
                // move against the (4-way) direction in which the center of the blocking rect lies
                CCPoint movement;
                CCPoint myCenter = placementRect.Center;
                CCPoint bCenter  = blockingRect.Center;
                float dx = bCenter.X - myCenter.X;
                float dy = bCenter.Y - myCenter.Y;
                if (Math.Abs(dx) > Math.Abs(dy))
                    if (dx > 0)
                        movement = new CCPoint(blockingRect.MinX - placementRect.MaxX - SAFETY, 0);
                    else
                        movement = new CCPoint(blockingRect.MaxX - placementRect.MinX + SAFETY, 0);
                else
                    if (dy > 0)
                        movement = new CCPoint(0, blockingRect.MinY - placementRect.MaxY - SAFETY);
                    else
                        movement = new CCPoint(0, blockingRect.MaxY - placementRect.MinY + SAFETY);
                aircraft.Position += movement;
                HangarPositions[aircraft] = aircraft.Position;
                placementRect = PlacementRect(aircraft);
                // check whether the new position is available
                while (!RectAvailable(placementRect, out blockingRect, aircraft))
                {
                    // it's not, so move further (into the same direction as before)
                    if (movement.Y == 0)
                        movement = movement.X < 0 ?
                                    new CCPoint(blockingRect.MinX - placementRect.MaxX - SAFETY, 0) :
                                    new CCPoint(blockingRect.MaxX - placementRect.MinX + SAFETY, 0);
                    else
                        movement = movement.Y < 0 ?
                                    new CCPoint(0, blockingRect.MinY - placementRect.MaxY - SAFETY) :
                                    new CCPoint(0, blockingRect.MaxY - placementRect.MinY + SAFETY);
                    aircraft.Position += movement;
                    HangarPositions[aircraft] = aircraft.Position;
                    placementRect = PlacementRect(aircraft);
                }
            }
            // update the saved hangar position
            HangarPositions[aircraft] = aircraft.Position;
            // update the camera boundary variables
            CalcBoundaries();
            // and also the camera itself
            CameraPosition = CameraPosition;
            CameraSize = CameraSize;
            UpdateCamera();
        }

        internal void CalcBoundaries()
        {
            switch(State)
            {
                case HangarState.TRANSITION:
                    {
                        // during a transition everything goes (because the camera is moved by the program, not the player)
                        CameraSpace = new CCRect(float.MinValue, float.MinValue, float.PositiveInfinity, float.PositiveInfinity);
                        MaxCameraWidth = float.PositiveInfinity;
                        MaxCameraHeight = float.PositiveInfinity;
                    }
                    break;
                case HangarState.HANGAR:
                    {
                        const float BORDER = 300f;
                        float minX = float.PositiveInfinity;
                        float minY = float.PositiveInfinity;
                        float maxX = float.NegativeInfinity;
                        float maxY = float.NegativeInfinity;
                        foreach (var aircraft in Aircrafts)
                        {
                            var rect = aircraft.BoundingBoxTransformedToWorld;
                            if (rect.MinX < minX) minX = rect.MinX;
                            if (rect.MinY < minY) minY = rect.MinY;
                            if (rect.MaxX > maxX) maxX = rect.MaxX;
                            if (rect.MaxY > maxY) maxY = rect.MaxY;
                        }
                        float takeoffNodeHeight = GUILayer.TakeoffNode.ContentSize.Height * VisibleBoundsWorldspace.Size.Width / GUILayer.VisibleBoundsWorldspace.Size.Width;
                        CameraSpace = new CCRect(minX - BORDER, minY - BORDER - takeoffNodeHeight, maxX - minX + BORDER * 2, maxY - minY + BORDER * 2 + takeoffNodeHeight);
                        var size = LargestAircraftSize();
                        float widthRel = size.Width / Constants.COCOS_WORLD_WIDTH;
                        float heightRel = size.Height / Constants.COCOS_WORLD_HEIGHT;
                        float max = widthRel > heightRel ? widthRel : heightRel;
                        MaxCameraWidth = Constants.COCOS_WORLD_WIDTH * max * 8;
                        MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT * max * 8;
                    }
                    break;
                case HangarState.WORKSHOP:
                    {
                        float cameraMinY = CameraPositionWorkshop.Y - WorkshopHeight() + Constants.COCOS_WORLD_HEIGHT * 0.25f;
                        if (cameraMinY > CameraPositionWorkshop.Y) cameraMinY = CameraPositionWorkshop.Y;
                        CameraSpace = new CCRect(CameraPositionWorkshop.X, cameraMinY, Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT + Math.Abs(CameraPositionWorkshop.Y - cameraMinY) + Constants.COCOS_WORLD_HEIGHT * 0.25f);
                        MaxCameraWidth = Constants.COCOS_WORLD_WIDTH;
                        MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT;
                    }
                    break;
                case HangarState.MODIFY_AIRCRAFT:
                    {
                        float width = ModifyAircraftWidth();
                        var  bounds = VisibleBoundsWorldspace;
                        float ratio = bounds.Size.Height / bounds.Size.Width;
                        CameraSpace = new CCRect(-width * 2, ModifiedAircraft.Position.Y - width * 2 * ratio, width * 4, width * 4 * ratio);
                    }
                    break;
            }
            
        }

        internal CCRect PlacementRect(Aircraft aircraft)
        {
            // the aircraft has to be temporarily moved to its hangar position for this to work as intended
            // it would be ideal to rotate it as well, but MyRotation sadly cannot account for rotations made with CCActions
            CCPoint currentPos = aircraft.Position;
            aircraft.Position = HangarPositions[aircraft];
            const float BORDER = 10f;
            var rect = aircraft.BoundingBoxTransformedToParent;
            aircraft.Position = currentPos;
            return new CCRect(rect.MinX - BORDER, rect.MinY - BORDER, rect.Size.Width + BORDER * 2, rect.Size.Height + BORDER * 2);
        }

        internal bool RectAvailable(CCRect rect, out CCRect blockingRect, Aircraft exceptedAircraft=null)
        {
            blockingRect = CCRect.Zero;
            foreach (var aircraft in Aircrafts)
            {
                if ((exceptedAircraft != null && aircraft == exceptedAircraft) || aircraft.Parent != this) continue;
                var placementRect = PlacementRect(aircraft);
                if (placementRect.IntersectsRect(rect))
                {
                    blockingRect = placementRect;
                    return false;
                }
            }
            return true;
        }

        internal CCSize LargestAircraftSize()
        {
            var size = new CCSize(0,0);
            var sizeArea = 0f;
            foreach (var aircraft in Aircrafts)
            {
                var aircraftSize = aircraft.ScaledContentSize;
                var aircraftSizeArea = aircraftSize.Width * aircraftSize.Height;
                if (aircraftSizeArea > sizeArea)
                {
                    size = aircraftSize;
                    sizeArea = aircraftSizeArea;
                }
            }
            return size;
        }
        internal Aircraft SelectedAircraft { get; set; } = null;
        new private protected void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesBegan(touches, touchEvent);
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        switch(State)
                        {
                            case HangarState.HANGAR:
                                // if the touch is upon an aircraft, select it
                                foreach (var aircraft in Aircrafts)
                                    if (aircraft.Parent == this && aircraft.BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation))
                                    {
                                        CCRect box = aircraft.BoundingBoxTransformedToWorld;
                                        CCRect reducedBoundingBox = new CCRect(box.MinX + box.Size.Width / 4,
                                                                               box.MinY + box.Size.Height / 4,
                                                                               box.Size.Width / 2, box.Size.Height / 2);
                                        if (reducedBoundingBox.ContainsPoint(touch.StartLocation))
                                        {
                                            GUILayer.DragAndDropObject = aircraft;
                                        }
                                        else
                                        {
                                            // rotate the aircraft!
                                            RotationSelectedNode = aircraft;
                                        }
                                        break;
                                    }
                                break;
                            case HangarState.WORKSHOP:
                                // if the touch is upon an aircraft, select it
                                foreach (var aircraft in Aircrafts)
                                    if (aircraft.Parent == this && aircraft.BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation))
                                    {
                                        ModifiedAircraft = aircraft;
                                        StartTransition(HangarState.MODIFY_AIRCRAFT);
                                        break;
                                    }
                                break;
                            case HangarState.MODIFY_AIRCRAFT:
                                {
                                    // if the touch is upon a part unmount it and drag it
                                    foreach (Part part in ModifiedAircraft.TotalParts)
                                        if (part.BoundingBoxTransformedToWorld.ContainsPoint(touch.Location))
                                        {
                                            // change into standard state for unmounting first (god knows what would happen else)
                                            CCPoint realPos = part.PositionWorldspace;
                                            ModifiedAircraft.InWorkshopConfiguration = false;
                                            var mountParent = part.MountParent;
                                            bool flipped = part.Flipped;
                                            if (mountParent != null)
                                            {
                                                mountParent.Unmount(part);
                                            }
                                            else
                                            {
                                                // the part is the body (as only the body has no MountParent)
                                                part.Aircraft.Body = null;
                                            }
                                            if (flipped) part.Flip();
                                            part.Position = GUILayer.HangarCoordinatesToGUI(realPos);// + (flipped ? new CCPoint(0, (part.AnchorPoint.Y - 0.5f) * 8 * part.BoundingBoxTransformedToWorld.Size.Height) : CCPoint.Zero));
                                            part.Scale = GUILayer.HangarScaleToGUI() * Constants.STANDARD_SCALE;
                                            GUILayer.DragAndDropObject = part;
                                            ModifiedAircraft.InWorkshopConfiguration = true;
                                            DrawInModifyAircraftState();
                                            break;
                                        }
                                }
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private protected void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (State != HangarState.TRANSITION)
                switch (touches.Count)
                {
                    case 1:
                        {
                            var touch = touches[0];
                            bool moveCam = true;
                            // if you are currently rotating a node rotate it
                            if (RotationSelectedNode != null)
                            {
                                moveCam = false;
                                CCPoint pos = ((CCNode)RotationSelectedNode).BoundingBoxTransformedToWorld.Center;
                                CCPoint vecPosToPrevTouch = pos - touch.PreviousLocation;
                                CCPoint vecPosToTouch = pos - touch.Location;
                                float previousAngle = Constants.DxDyToCCDegrees(vecPosToPrevTouch.X, vecPosToPrevTouch.Y);
                                float currentAngle  = Constants.DxDyToCCDegrees(vecPosToTouch.X, vecPosToTouch.Y);
                                float dAngle = Constants.AngleFromToDeg(previousAngle, currentAngle);
                                RotationSelectedNode.MyRotation += dAngle;
                            }
                            if (GUILayer.DragAndDropObject != null)
                                moveCam = false;
                            // move the camera
                            if (moveCam)
                                OnTouchesMovedMoveAndZoom(touches, touchEvent);
                        }
                        break;
                    case 2:
                        {
                            if (State == HangarState.HANGAR || State == HangarState.MODIFY_AIRCRAFT)
                                OnTouchesMovedMoveAndZoom(touches, touchEvent);
                        }
                        break;
                    default:
                        break;
                }
        }

        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (State != HangarState.TRANSITION)
                switch (touches.Count)
                {
                    case 1:
                        {
                            var touch = touches[0];
                            // if a node is selected for rotation deselect it
                            if (RotationSelectedNode != null)
                            {
                                if (State == HangarState.HANGAR)
                                    HangarRotations[(Aircraft)RotationSelectedNode] = RotationSelectedNode.MyRotation;
                                RotationSelectedNode = null;
                            }
                            else // else scroll with inertia
                                base.OnTouchesEnded(touches, touchEvent);
                        }
                        break;
                    default:
                        break;
                }
        }
    }
}
