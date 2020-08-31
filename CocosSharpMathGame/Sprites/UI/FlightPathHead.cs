using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Symbolism;

namespace CocosSharpMathGame
{
    /// <summary>
    /// The FlightPathHead is positioned at the end of a flight path.
    /// It can be moved (for example by touching and dragging it) to change the flight path.
    /// </summary>
    internal class FlightPathHead : UIElementNode
    {
        internal const float MAX_DISTANCE_FROM_CENTER = 14000;
        private CCLabel PowerUpAmountLabel = new CCLabel("", "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
        private FlightPathHeadOption head = new FlightPathHeadOption("flightPathHead.png", PowerUp.PowerType.NORMAL);
        internal FlightPathHeadOption Head
        {
            get { return head; }
            set
            {
                var oldPower = PowerUp.PowerType.NORMAL;
                if (head != null && head.Parent == this)
                {
                    oldPower = head.pType;
                    RemoveChild(head);
                }
                head = value;
                head.Position = (CCPoint)ContentSize / 2;
                AddChild(head);
                if (Parent != null && Parent is FlightPathControlNode f)
                {
                    f.Aircraft.PowerChanged(oldPower);
                    PowerUpAmountLabel.Text = head.pType != PowerUp.PowerType.NORMAL ? f.Aircraft.GetPowerUpCount(head.pType).ToString() : "";
                }
            }
        }
        internal PowerUp.PowerType SelectedPower { get { return Head != null ? Head.pType : PowerUp.PowerType.NORMAL; } }
        internal FlightPathHead() : base()
        {
            ContentSize = head.ContentSize;
            Head = head;
            AddChild(PowerUpAmountLabel);
            PowerUpAmountLabel.Color = CCColor3B.White;
            PowerUpAmountLabel.IsAntialiased = false;
            PowerUpAmountLabel.Position = new CCPoint(ContentSize.Width * 1.75f, ContentSize.Width / 2);
            PowerUpAmountLabel.Rotation = 90f;
            Scale = Constants.STANDARD_SCALE * 1.45f;
        }

        internal void MoveTo(float x, float y)
        {
            PositionX = x;
            PositionY = y;
        }

        internal Aircraft Aircraft
        {
            get
            {
                return ((FlightPathControlNode)Parent).Aircraft;
            }
        }

        internal void TouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                var touch = touches[0];
                // show alternative orders (super-speed, shield, ...)
                ShowAvailableOrders();
                // make sure no other aircrafts have their options exposed at the same time
                ((PlayLayer)Layer).TellFlightPathHeadsToShowHeadsOnly(Aircraft);
            }
        }

        internal void ShowAvailableOrders()
        {
            PowerUpAmountLabel.Visible = false;
            // first get the available power-ups from the aircraft
            var availablePowerUps = Aircraft.AvailablePowerUps();
            //Console.WriteLine("availablePowerUps: " + availablePowerUps);
            // then create fitting FlightPathHeadOptions
            var availableHeadOptions = new List<FlightPathHeadOption>();
            foreach (var powUpT in availablePowerUps)
            {
                if (powUpT != Head.pType)
                    availableHeadOptions.Add(PowerUp.FlightPathHeadOptionFromType(powUpT));
            }
            // then place them
            float l = ContentSize.Width * 1.25f;
            for (int i=0; i<availableHeadOptions.Count; i++)
            {
                var option = availableHeadOptions[i];
                AddChild(option);
                CCPoint relPos = CCPoint.Zero;
                switch (i)
                {
                    case 0:
                        relPos = new CCPoint(0, -l);
                        break;
                    case 1:
                        relPos = new CCPoint(0, l);
                        break;
                    case 2:
                        relPos = new CCPoint(l, -l/2);
                        break;
                    case 3:
                        relPos = new CCPoint(l, l/2);
                        break;
                }
                option.Position = Head.Position + relPos;
            }
        }

        internal void ShowOnlyHead()
        {
            RemoveAllChildren();
            AddChild(Head);
            AddChild(PowerUpAmountLabel);
            PowerUpAmountLabel.Visible = true;
        }
        /// <summary>
        /// Returns the geometrical center of mass of all the flight path heads of the player
        /// </summary>
        /// <returns></returns>
        internal CCPoint PlayerHeadCenter()
        {
            var center = CCPoint.Zero;
            if (Layer is PlayLayer pl)
            {
                foreach (var aircraft in pl.PlayerAircrafts)
                {
                    center += aircraft.FlightPathHeadPos;
                }
                center /= pl.PlayerAircrafts.Count;
            }
            return center;
        }

        internal void EnsureProximityToOtherPlayerHeads()
        {
            bool testing = true;
            while (testing)
            {
                // additionally calc the geometrical center of mass (of all player flightPathHeads)
                CCPoint headCenter = PlayerHeadCenter();
                // and make sure that you're close enough to it
                CCPoint vecToCenter = headCenter - Position;
                float delta = vecToCenter.Length - MAX_DISTANCE_FROM_CENTER;
                if (delta > 0)
                {
                    Position += CCPoint.Normalize(vecToCenter) * (delta + 5f);
                }
                else
                    testing = false;
            }
        }

        internal void TouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                var touch = touches[0];
                // move to the position that is allowed and closest to the touch (the closest point that is still inside the ManeuverPolygon)
                MoveHeadToClosestPointInsideManeuverPolygon(touch.Location);
            }
        }

        private void MoveHeadToClosestPointInsideManeuverPolygon(CCPoint point)
        {
            (Parent as FlightPathControlNode).MoveHeadToClosestPointInsideManeuverPolygon(point);
        }
    }

    internal class FlightPathHeadOption : UIElement
    {
        internal PowerUp.PowerType pType { get; private set; }
        internal FlightPathHeadOption(string textureName, PowerUp.PowerType powerType) : base(textureName)
        {
            pType = powerType;
            Scale = 1f;
            RadiusFactor = 0.7f;  // make the button a bit easier to hit
            MakeClickable(IsCircleButton: true);
        }

        private protected override void OnTouchesBeganUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            //Console.WriteLine("touched");
            var parent = (FlightPathHead)Parent;
            if (parent.Head == this)
                parent.TouchesBegan(touches, touchEvent);
            else
            {
                parent.Head = this;
                parent.ShowOnlyHead();
            }
        }

        private protected override void OnTouchesMovedUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            var parent = (FlightPathHead)Parent;
            if (parent.Head == this)
                parent.TouchesMoved(touches, touchEvent);
        }
    }
}
