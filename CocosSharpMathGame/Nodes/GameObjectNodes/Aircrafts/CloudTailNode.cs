using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Draws some nice clouds behind you
    /// </summary>
    internal class CloudTailNode
    {
        /// <summary>
        /// A cloud has a lifetime, a total lifetime, a reference size, a position (pivot point of the polygon) and a rotation, a polygon and a color.
        /// </summary>
        class Cloud
        {
            public Polygon Polygon;
            public float LifeTime;
            public float TotalLifeTime;
            public float CCRotation;
            public float ReferenceSize;
            public CCColor4B Color;
            public Cloud(CCPoint position, float CCrotation, CCColor4B color, float referenceSize, float totalLifeTime, float lifeTime=0)
            {
                Polygon = new Polygon(new CCPoint[0]);
                Polygon.PivotPoint = position;
                LifeTime = lifeTime;
                TotalLifeTime = totalLifeTime;
                Color = color;
                CCRotation = CCrotation;
                ReferenceSize = referenceSize;
                MatchLifeCycle();
            }
            /// <summary>
            /// change according to your lifecycle
            /// </summary>
            public void MatchLifeCycle()
            {
                // construct the polygon
                var center = Polygon.PivotPoint;
                CCPoint[] cloudPoints = new CCPoint[]
                {new CCPoint(center.X - ReferenceSize / 2, center.Y - ReferenceSize / 2),
                 new CCPoint(center.X - ReferenceSize / 2, center.Y + ReferenceSize / 2),
                 new CCPoint(center.X + ReferenceSize / 2, center.Y + ReferenceSize / 2),
                 new CCPoint(center.X + ReferenceSize / 2, center.Y - ReferenceSize / 2)};
                Polygon.Points = cloudPoints;
                Polygon.RotateBy(CCRotation);
                // scale according to life cycle
                float lifePercentage = LifeTime / TotalLifeTime;
                float scale;
                float max = 0.3f;
                scale = (float)Math.Sin(Math.PI * lifePercentage);
                if (lifePercentage <= max)
                    scale = (float)Math.Sin(Math.PI/2 * lifePercentage * (1 / max));
                else
                    scale = (float)Math.Cos(Math.PI/2 * (lifePercentage - max));
                Polygon.Scale(scale);
                // adapt the color (fade over time)
                byte value = (lifePercentage <= 0.5) ? (byte)255 : (byte)(255f * 2 * (1 - lifePercentage));
                Color.A = value; 
                //Color.R = value;
                //Color.G = value;
                //Color.B = value;
            }
        }
        internal float CloudDelay { get; set; } = 0.3f;
        internal float CloudLifeTime { get; set; } = 2.5f;
        internal CCColor4B CloudColor { get; set; } = CCColor4B.White;
        internal float ReferenceSize { get; set; } = 18f;
        private float TimeSinceLastCloud { get; set; } = 0;
        private List<Cloud> clouds = new List<Cloud>();

        /// <summary>
        /// Advance the life cycles of all clouds and possibly add a new cloud
        /// </summary>
        /// <param name="dt">how far to advance the lifecycle</param>
        /// <param name="currentPosition">the position at which to add a possible new cloud</param>
        /// <param name="decayOnly">if true, no new clouds are added (i.e. this just advances the cloud life cycles)</param>
        internal void Advance(float dt, CCPoint currentPosition, float currentCCRotation, bool decayOnly=false)
        {
            // check if it is time for a new cloud
            float timePlusDt = TimeSinceLastCloud + dt;
            if (timePlusDt > CloudDelay)
            {
                // reset the timer to 0 + overshoot
                TimeSinceLastCloud = timePlusDt % CloudDelay;
                // add a new cloud
                if (!decayOnly)
                {
                    var cloud = new Cloud(currentPosition, currentCCRotation, CloudColor, ReferenceSize, CloudLifeTime);
                    clouds.Add(cloud);
                }
            }
            else
            {
                // let the timer run
                TimeSinceLastCloud = timePlusDt;
            }
            // advance the lifecycle of each cloud (exept the last one, if it is new)
            foreach (var cloud in clouds)
            {
                cloud.LifeTime += dt;
                cloud.MatchLifeCycle();
            }
            // remove all clouds that have gone beyond their lifecycle
            for (int i=0; i<clouds.Count; i++)
            {
                if (clouds[i].LifeTime > clouds[i].TotalLifeTime)
                    clouds.RemoveAt(i--);
            }
        }
        internal void DrawClouds(CCDrawNode drawNode)
        {
            foreach (var cloud in clouds)
            {
                if (cloud == null) continue;
                var cloudPoly = cloud.Polygon;
                drawNode.DrawPolygon(cloudPoly.Points, cloudPoly.Points.Length, cloud.Color, 0, CCColor4B.Transparent);
            }
        }
    }
}
