using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using SkiaSharp;

namespace CocosSharpMathGame
{
    public class HangarLayer : MyLayer
    {
        const float TRANSITION_TIME = 0.5f;
        internal enum HangarState
        {
            TRANSITION, HANGAR, WORKSHOP, SCRAPYARD
        }
        internal HangarState State = HangarState.HANGAR;
        public HangarGUILayer GUILayer { get; set; }
        internal CCNode BGNode { get; private protected set; }
        private CCDrawNode BGDrawNode { get; set; }
        private CCColor4B BGColor { get; set; } = new CCColor4B(50, 50, 50);
        internal List<Aircraft> Aircrafts = new List<Aircraft>();
        internal List<Part> Parts = new List<Part>();
        public HangarLayer() : base(CCColor4B.Black)
        {
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
            TakeoffNodeToHangar = new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, 8f)), easeRate);
            TakeoffNodeLeave    = new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, -GUILayer.TakeoffNode.BoundingBoxTransformedToWorld.Size.Height)), easeRate);
            BGFadeOut           = new CCRepeat(new CCSequence(new CCCallFunc(DecreaseBGAlpha), new CCDelayTime(TRANSITION_TIME / 255)), 255);
            BGFadeIn            = new CCRepeat(new CCSequence(new CCCallFunc(IncreaseBGAlpha), new CCDelayTime(TRANSITION_TIME / 255)), 255);
            // TODO: MORE ACTIONS ARE NEEDED
        }

        private CCAction TakeoffNodeToHangar;
        private CCAction TakeoffNodeLeave;
        private CCAction BGFadeOut;
        private CCAction BGFadeIn;

        internal void StartTransition(object sender, EventArgs args)
        {
            StopAllTransitionActions();
            State = HangarState.TRANSITION;
            // stop all current transition actions
            // get the state to go to
            var carousel = (Carousel)sender;
            var middle = carousel.MiddleNode;
            if (middle == GUILayer.HangarOptionHangar)
            {
                // TODO: start a transition to the hangar state
                GUILayer.TakeoffNode.AddAction(TakeoffNodeToHangar);
                BGNode.AddAction(BGFadeIn);
            }
            else if (middle == GUILayer.HangarOptionWorkshop)
            {
                // TODO: start a transition to the workshop state
                GUILayer.TakeoffNode.AddAction(TakeoffNodeLeave);
                BGNode.AddAction(BGFadeOut);
            }
        }
        private void StopAllTransitionActions()
        {
            GUILayer.TakeoffNode.StopAction(TakeoffNodeToHangar.Tag);
            GUILayer.TakeoffNode.StopAction(TakeoffNodeLeave.Tag);
            BGNode.StopAction(BGFadeOut.Tag);
            BGNode.StopAction(BGFadeIn.Tag);
            // TODO: Stop the rest of the actions
        }

        internal CCPoint GUICoordinatesToHangar(CCPoint pointInGUICoord)
        {
            return CameraPosition + pointInGUICoord * VisibleBoundsWorldspace.Size.Width / GUILayer.VisibleBoundsWorldspace.Size.Width;
        }

        internal void ReceiveAircraftFromCollection(object sender, ScrollableCollectionNode.CollectionRemovalEventArgs e)
        {
            Aircraft aircraft = (Aircraft)e.RemovedNode;
            AddChild(aircraft);
            SelectedAircraft = aircraft;
            aircraft.ResetAnchorPoint();
            aircraft.Position = GUICoordinatesToHangar(e.TouchOnRemove.Location);
            Console.WriteLine("Touch: " + GUICoordinatesToHangar(e.TouchOnRemove.Location));
        }

        internal void AddAircraft(Aircraft aircraft, CCPoint hangarPos)
        {
            Aircrafts.Add(aircraft);
            AddChild(aircraft);
            SelectedAircraft = aircraft;
            PlaceSelectedAircraft(hangarPos);
        }

        internal void PlaceSelectedAircraft(CCPoint hangarPos)
        {
            const float SAFETY = 0.001f; // numeric safety
            SelectedAircraft.Position = hangarPos;
            var placementRect = PlacementRect(SelectedAircraft);
            if (RectAvailable(placementRect, out CCRect blockingRect))
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
                SelectedAircraft.Position += movement;
                placementRect = PlacementRect(SelectedAircraft);
                // check whether the new position is available
                while (!RectAvailable(placementRect, out blockingRect))
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
                    SelectedAircraft.Position += movement;
                    placementRect = PlacementRect(SelectedAircraft);
                }
            }
            SelectedAircraft = null;
            // update the camera boundary variables
            CalcBoundaries();
            // and also the camera itself
            CameraPosition = CameraPosition;
            CameraSize = CameraSize;
            UpdateCamera();
        }

        internal void CalcBoundaries()
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
            CameraSpace = new CCRect(minX - BORDER, minY - BORDER, maxX - minX + BORDER * 2, maxY - minY + BORDER * 2);
            var size = LargestAircraftSize();
            float widthRel  = size.Width  / Constants.COCOS_WORLD_WIDTH;
            float heightRel = size.Height / Constants.COCOS_WORLD_HEIGHT;
            float max = widthRel > heightRel ? widthRel : heightRel;
            MaxCameraWidth  = Constants.COCOS_WORLD_WIDTH  * max * 8;
            MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT * max * 8;
        }

        internal CCRect PlacementRect(Aircraft aircraft)
        {
            const float BORDER = 10f;
            var rect = aircraft.BoundingBoxTransformedToParent;
            return new CCRect(rect.MinX - BORDER, rect.MinY - BORDER, rect.Size.Width + BORDER * 2, rect.Size.Height + BORDER * 2);
        }

        internal bool RectAvailable(CCRect rect, out CCRect blockingRect)
        {
            blockingRect = CCRect.Zero;
            foreach (var aircraft in Aircrafts)
            {
                if (aircraft == SelectedAircraft) continue;
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
                        // if the touch is upon an aircraft, select it
                        foreach (var aircraft in Aircrafts)
                            if (aircraft.Parent == this && aircraft.BoundingBoxTransformedToWorld.ContainsPoint(touches[0].StartLocation))
                            {
                                SelectedAircraft = aircraft;
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
            switch (touches.Count)
            {
                case 1:
                    {
                        // if you are currently moving an aircraft move the aircraft
                        if (SelectedAircraft != null)
                            SelectedAircraft.Position += touches[0].Delta;
                        // move the camera
                        else
                            OnTouchesMovedMoveAndZoom(touches, touchEvent);
                    }
                    break;
                case 2:
                    {
                        OnTouchesMovedMoveAndZoom(touches, touchEvent);
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
                        // if an aircraft is selected deselect it and place it
                        if (SelectedAircraft != null)
                        {
                            PlaceSelectedAircraft(SelectedAircraft.Position);
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
