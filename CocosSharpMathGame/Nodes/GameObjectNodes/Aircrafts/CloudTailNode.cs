using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A cloud has a lifetime, a total lifetime, a reference size, a position (pivot point of the polygon) and a rotation, a polygon and a color.
    /// </summary>
    internal class Cloud
    {
        public bool DrawLow;
        public Polygon Polygon;
        public float LifeTime;
        public float TotalLifeTime;
        public float CCRotation;
        public float ReferenceSize;
        public CCColor4B Color;
        /// <summary>
        /// clouds usually stay in one place; set this property to let it follow a node instead
        /// </summary>
        internal CCNode FollowTarget { get; set; } = null;
        public Cloud(CCPoint position, float CCrotation, CCColor4B color, bool drawLow, float referenceSize, float totalLifeTime, float lifeTime = 0)
        {
            Polygon = new Polygon(new CCPoint[0]);
            Polygon.PivotPoint = position;
            LifeTime = lifeTime;
            TotalLifeTime = totalLifeTime;
            Color = color;
            CCRotation = CCrotation;
            ReferenceSize = referenceSize;
            DrawLow = drawLow;
            MatchLifeCycle();
        }
        /// <summary>
        /// change according to your lifecycle
        /// </summary>
        public virtual void MatchLifeCycle()
        {
            // follow your target if you have one
            Follow();
            // construct the polygon
            var center = Polygon.PivotPoint;
            CCPoint[] cloudPoints = new CCPoint[]
            {    new CCPoint(center.X - ReferenceSize / 2, center.Y - ReferenceSize / 2),
                 new CCPoint(center.X - ReferenceSize / 2, center.Y + ReferenceSize / 2),
                 new CCPoint(center.X + ReferenceSize / 2, center.Y + ReferenceSize / 2),
                 new CCPoint(center.X + ReferenceSize / 2, center.Y - ReferenceSize / 2)};
            Polygon.Points = cloudPoints;
            Polygon.RotateBy(CCRotation);
            // scale according to life cycle
            ScaleAndColor(out float scale, out CCColor4B color);
            Polygon.Scale(scale);
            // adapt the color (fade over time)
            Color = color;
        }

        public virtual void ScaleAndColor(out float scale, out CCColor4B color)
        {
            float lifePercentage = LifeTime / TotalLifeTime;
            float max = 0.3f;
            scale = (float)Math.Sin(Math.PI * lifePercentage);
            if (lifePercentage <= max)
                scale = (float)Math.Sin(Math.PI / 2 * lifePercentage * (1 / max));
            else
                scale = (float)Math.Cos(Math.PI / 2 * (lifePercentage - max));
            byte value = (lifePercentage <= 0.5) ? (byte)255 : (byte)(255f * 2 * (1 - lifePercentage));
            color = Color;
            color.A = value;
        }

        internal virtual void UseDrawNode(CCDrawNode highNode, CCDrawNode lowNode)
        {
            (DrawLow ? lowNode : highNode).DrawPolygon(Polygon.Points, Polygon.Points.Length, Color, 0, CCColor4B.Transparent);
        }

        protected void Follow()
        {
            // follow your target if you have one
            if (FollowTarget != null)
                Polygon.PivotPoint = FollowTarget.PositionWorldspace;
        }
    }
    /// <summary>
    /// Draws some nice clouds behind you
    /// </summary>
    internal class CloudTailNode : IDrawNodeUser
    {
        internal bool DrawLow { get; set; } = true;
        internal float CloudDelay { get; set; } = 0.1f;
        internal float CloudLifeTime { get; set; } = 2.5f;
        internal CCColor4B CloudColor { get; set; } = CCColor4B.White;
        internal float ReferenceSize { get; set; } = 18f;
        protected float TimeSinceLastCloud { get; set; } = 0;

        internal bool AutoAddClouds { get; set; } = true;
        protected List<Cloud> clouds = new List<Cloud>();

        /// <summary>
        /// Advance the life cycles of all clouds and possibly add a new cloud
        /// </summary>
        /// <param name="dt">how far to advance the lifecycle</param>
        /// <param name="currentPosition">the position at which to add a possible new cloud</param>
        /// <param name="decayOnly">if true, no new clouds are added (i.e. this just advances the cloud life cycles)</param>
        internal virtual void Advance(float dt, CCPoint currentPosition, float currentCCRotation, bool decayOnly=false)
        {
            // advance the lifecycle of each cloud
            foreach (var cloud in clouds)
            {
                cloud.LifeTime += dt;
                cloud.MatchLifeCycle();
            }
            // check if it is time for a new cloud
            float timePlusDt = TimeSinceLastCloud + dt;
            if (AutoAddClouds && timePlusDt > CloudDelay)
            {
                // reset the timer to 0 + overshoot
                TimeSinceLastCloud = timePlusDt % CloudDelay;
                // add a new cloud
                if (!decayOnly)
                {
                    var cloud = new Cloud(currentPosition, currentCCRotation, CloudColor, DrawLow, ReferenceSize, CloudLifeTime);
                    clouds.Add(cloud);
                }
            }
            else
            {
                // let the timer run
                TimeSinceLastCloud = timePlusDt;
            }
            // remove all clouds that have gone beyond their lifecycle
            for (int i=0; i<clouds.Count; i++)
            {
                if (clouds[i].LifeTime > clouds[i].TotalLifeTime)
                    clouds.RemoveAt(i--);
            }
        }

        internal void AddCloud(Cloud cloud)
        {
            clouds.Add(cloud);
        }
        public void UseDrawNodes(CCDrawNode highNode, CCDrawNode lowNode)
        {
            foreach (var cloud in clouds)
            {
                if (cloud != null)
                    cloud.UseDrawNode(highNode, lowNode);
            }
        }
    }
}
