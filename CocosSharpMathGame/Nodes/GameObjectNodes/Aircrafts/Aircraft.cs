﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Aircrafts are objects in the sky that are assembled from parts
    /// which react to collision
    /// </summary>
    internal abstract class Aircraft : GameObjectNode, ICollidable
    {
        protected FlightPathControlNode FlightPathControlNode { get; set; }
        protected CloudTailNode CloudTailNode { get; set; } = new CloudTailNode();
        internal Team Team { get; set; }
        private AI ai;
        internal AI AI {
            get { return ai; }
            set
            {
                ai = value;
                if (ai != null)
                    ai.Aircraft = this;
            }
        }
        private bool controlledByPlayer = false;
        internal bool ControlledByPlayer { 
            get { return controlledByPlayer; }
            set
            {
                controlledByPlayer = value;
                if (!controlledByPlayer)
                    FlightPathControlNode.Visible = false;
            }
        }
        /// <summary>
        /// DEBUG
        /// This drawnode draws the manveuver polygon (if IsManeuverPolygonDrawn == true)
        /// </summary>
        private CCDrawNode maneuverPolygonDrawNode = new CCDrawNode();
        internal bool IsManeuverPolygonDrawn {
            get
            {
                return maneuverPolygonDrawNode.Visible;
            }
            set
            {
                maneuverPolygonDrawNode.Visible = value;
            }
        }

        /// <summary>
        /// defines where the aircraft can move to this turn
        /// </summary>
        internal PolygonWithSplines ManeuverPolygon { get; private set; }

        protected Part body = null;
        /// <summary>
        /// the head of the part-hierarchy
        /// </summary>
        internal Part Body
        {
            get
            {
                return body;
            }
            private protected set
            {
                // first remove the old body if there is one
                if (body != null)
                    RemoveChild(body);
                body = value;
                if (value != null)
                {
                    AddChild(body);
                    // update ContentSize
                    ContentSize = body.ScaledContentSize;
                }
            }
        }

        internal void RotateBy(float degree)
        {
            MyRotation += degree;
            // update the ManeuverPolygon
            ManeuverPolygon.RotateBy(degree);
        }

        internal void MoveBy(float dx, float dy)
        {
            PositionX += dx;
            PositionY += dy;
            // update the ManeuverPolygon
            ManeuverPolygon.MoveBy(dx, dy);
        }

        internal void MoveTo(CCPoint destination)
        {
            float dx = destination.X - PositionX;
            float dy = destination.Y - PositionY;
            MoveBy(dx, dy);
        }

        internal void UpdateManeuverPolygonToThis(PolygonWithSplines untransformedPolygon)
        {
            ManeuverPolygon = untransformedPolygon;
            // transform it to where it belongs
            ManeuverPolygon.RotateBy(MyRotation);
            ManeuverPolygon.MoveBy(PositionX, PositionY);
            // draw it (DEBUG)
            maneuverPolygonDrawNode.Clear();
            maneuverPolygonDrawNode.DrawPolygon(untransformedPolygon.Points, untransformedPolygon.Points.Length, CCColor4B.Transparent ,2f, CCColor4B.White);
        }

        internal void RotateTo(float direction)
        {
            RotateBy(direction - MyRotation);
        }

        /// <summary>
        /// Execute your orders for dt seconds
        /// </summary>
        /// <param name="dt">the time since the last Update call</param>
        /// <returns>whether the aircraft is done executing it orders</returns>
        internal bool ExecuteOrders(float dt)
        {
            // first make the controls invisible
            FlightPathControlNode.Visible = false;
            // advance the cloud tail lifecycle
            CloudTailNode.Advance(dt, Position, MyRotation);
            // for now all aircrafts can do is follow their flight path
            // advance dt seconds on the path
            bool finished = FlightPathControlNode.Advanche(dt);
            return finished;
        }

        internal Aircraft() : base()
        {
            FlightPathControlNode = new FlightPathControlNode(this);
            ControlledByPlayer = false;
            // DEBUG
            AddChild(maneuverPolygonDrawNode);
            maneuverPolygonDrawNode.AnchorPoint = CCPoint.AnchorLowerLeft;
            maneuverPolygonDrawNode.Scale = 1 / Constants.STANDARD_SCALE;
            IsManeuverPolygonDrawn = false;
        }

        internal void TryToSetFlightPathHeadTo(CCPoint position)
        {
            FlightPathControlNode.MoveHeadToClosestPointInsideManeuverPolygon(position);
        }

        internal void ChangeColor(CCColor3B newColor)
        {
            foreach (var part in TotalParts)
                part.Color = newColor;
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            // DrawNodes have no Size, therefore we need to position them correctly at the center of the node
            maneuverPolygonDrawNode.Position = new CCPoint(ContentSize.Width/2, ContentSize.Height / 2);
            // add the FlightPathControlNode as a brother below you
            Parent.AddChild(FlightPathControlNode, ZOrder - 1);
            // add the CloudTailNode as a brother below you
            Parent.AddChild(CloudTailNode, ZOrder - 2);
        }
        internal void PrepareForRemoval()
        {
            // remove your brothers (FlightPathControlNode & CloudTailNode)
            Parent.RemoveChild(FlightPathControlNode);
            Parent.RemoveChild(CloudTailNode);
        }

        /// <summary>
        /// searches and returns all parts that this aircraft is made of
        /// starting at the body and then searching recursively
        /// </summary>
        protected IEnumerable<Part> TotalParts {
            get
            {
                return Body.TotalParts;
            }
        }

        /// <summary>
        /// Set into a state so that the planning phase can act properly on this aircraft
        /// </summary>
        internal void PrepareForPlanningPhase()
        {
            FlightPathControlNode.ResetHeadPosition();
            if (AI != null)
                AI.ActInPlanningPhase(AircraftsInLevel());
            if (ControlledByPlayer)
                FlightPathControlNode.Visible = true;
        }

        internal IEnumerable<Aircraft> AircraftsInLevel()
        {
            var playLayer = (Layer as PlayLayer);
            if (playLayer != null)
                return playLayer.Aircrafts;
            else
                return null;
        }
    }
}
