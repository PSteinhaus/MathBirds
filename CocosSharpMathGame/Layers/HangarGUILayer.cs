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
        internal ScrollableCollectionNode TakeoffCollectionNode { get; private protected set; }
        internal CCNode TakeoffNode { get; private protected set; } = new CCNode();
        private HangarLayer HangarLayer { get; set; }
        public HangarGUILayer(HangarLayer hangarLayer) : base(CCColor4B.Transparent)
        {
            HangarLayer = hangarLayer;
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            //touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            AddEventListener(touchListener, this);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();

            var bounds = VisibleBoundsWorldspace;
            TakeoffCollectionNode = new ScrollableCollectionNode(new CCSize(bounds.Size.Width, bounds.Size.Height / 6));
            float borderToCollection = 30f;
            TakeoffNode.Position = CCPoint.Zero;
            TakeoffNode.AnchorPoint = CCPoint.AnchorLowerLeft;
            TakeoffNode.AddChild(TakeoffCollectionNode);
            TakeoffCollectionNode.PositionY = borderToCollection;
            TakeoffCollectionNode.Columns = 100;
            TakeoffCollectionNode.Rows = 1;
            TakeoffCollectionNode.BoxSize = new CCSize(TakeoffCollectionNode.ContentSize.Height, TakeoffCollectionNode.ContentSize.Height);
            AddChild(TakeoffNode, zOrder: 1);
            TakeoffNode.ContentSize = new CCSize(TakeoffCollectionNode.ContentSize.Width, TakeoffCollectionNode.ContentSize.Height + 2 * borderToCollection);
            var drawNode = new CCDrawNode();
            AddChild(drawNode, zOrder: -1);
            drawNode.DrawRect(TakeoffNode.BoundingBoxTransformedToWorld, CCColor4B.Black);
            drawNode.DrawLine(new CCPoint(0, TakeoffNode.BoundingBoxTransformedToWorld.UpperRight.Y), TakeoffNode.BoundingBoxTransformedToWorld.UpperRight, 8f, CCColor4B.White);
            // listen to the TakeoffCollectionNode
            TakeoffCollectionNode.CollectionRemovalEvent += HangarLayer.ReceiveAircraftFromCollection;
        }

        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        if (TakeoffNode.BoundingBoxTransformedToParent.ContainsPoint(touches[0].Location))
                        {
                            // if an aircraft is selected deselect it and place it
                            var selectedAircraft = HangarLayer.SelectedAircraft;
                            if (selectedAircraft != null)
                            {
                                touchEvent.StopPropogation();
                                {
                                    HangarLayer.RemoveChild(selectedAircraft);
                                    TakeoffCollectionNode.AddToCollection(selectedAircraft);
                                    HangarLayer.SelectedAircraft = null;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
