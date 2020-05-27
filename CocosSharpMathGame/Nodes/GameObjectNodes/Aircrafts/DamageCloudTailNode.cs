using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class DamageCloud : Cloud
    {
        internal static readonly CCColor4B CLOUD_COLOR = CCColor4B.Gray;
        internal CCColor4B FireColor { get; set; } = CCColor4B.Gray;
        internal DamageCloud(CCPoint position, float CCrotation, CCColor4B color, float referenceSize, bool drawLow, float totalLifeTime, float lifeTime = 0) : base(position, CCrotation, color, drawLow, referenceSize, totalLifeTime, lifeTime)
        { }
        /// <summary>
        /// Damage clouds have a different color behaviour (fire for example)
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        public override void ScaleAndColor(out float scale, out CCColor4B color)
        {
            base.ScaleAndColor(out scale, out color);
            color = CLOUD_COLOR;
            // if you have a special FireColor (anything but gray) factor it in
            float lifePercentage = LifeTime / TotalLifeTime;
            if (!FireColor.Equals(CLOUD_COLOR))
            {
                float factor = (float)Math.Cos(Math.PI / 2 * lifePercentage);
                color = CCColor4B.Lerp(CLOUD_COLOR, FireColor, factor);
            }
        }
    }
    internal class RingCloud : Cloud
    {
        internal RingCloud(CCPoint position, float CCrotation, CCColor4B color, bool drawLow, float referenceSize, float totalLifeTime, float lifeTime = 0) : base(position, CCrotation, color, drawLow, referenceSize, totalLifeTime, lifeTime)
        { }

        /// <summary>
        /// Ring clouds have a different scale behaviour (they are growing)
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        public override void ScaleAndColor(out float scale, out CCColor4B color)
        {
            base.ScaleAndColor(out scale, out color);
            // grow with time
            scale = 1 - (float)(Math.Pow(LifeTime / TotalLifeTime - 1, 4));
        }
        /// <summary>
        /// change according to your lifecycle
        /// </summary>
        public override void MatchLifeCycle()
        {
            // follow your target if you have one
            Follow();
        }
        internal override void UseDrawNode(CCDrawNode highNode, CCDrawNode lowNode)
        {
            ScaleAndColor(out float scale, out CCColor4B color);
            (DrawLow ? lowNode : highNode).DrawCircle(Polygon.PivotPoint, ReferenceSize/2*scale, color);
        }
    }
    internal class CircleCloud : Cloud
    {
        internal CircleCloud(CCPoint position, float CCrotation, CCColor4B color, bool drawLow, float referenceSize, float totalLifeTime, float lifeTime = 0) : base(position, CCrotation, color, drawLow, referenceSize, totalLifeTime, lifeTime)
        { }

        /// <summary>
        /// Ring clouds have a different scale behaviour (they are growing)
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        public override void ScaleAndColor(out float scale, out CCColor4B color)
        {
            color = Color;
            //color.A = (byte)(255 *(1 - LifeTime / TotalLifeTime));
            scale = 1 - (float)(Math.Pow(LifeTime / TotalLifeTime - 1, 4));
            color.A = (byte)(255 * (Math.Pow(LifeTime / TotalLifeTime - 1, 2)));
        }
        /// <summary>
        /// change according to your lifecycle
        /// </summary>
        public override void MatchLifeCycle()
        {
            // follow your target if you have one
            Follow();
        }
        internal override void UseDrawNode(CCDrawNode highNode, CCDrawNode lowNode)
        {
            ScaleAndColor(out float scale, out CCColor4B color);
            (DrawLow ? lowNode : highNode).DrawSolidCircle(Polygon.PivotPoint, ReferenceSize * scale, color);
        }
    }
    internal class DamageCloudTailNode : CloudTailNode
    {
        internal CCPoint RelativePosition;
        internal bool AddRing = true;
        internal DamageCloudTailNode(float referenceSize, CCPoint relPosistion)
        {
            RelativePosition = relPosistion;
            ReferenceSize = referenceSize;
            DrawLow = false;
            CloudDelay = 0.25f;
            CloudLifeTime = 0.8f;
            CloudColor = CCColor4B.Gray;
        }

        /// <summary>
        /// Advance the life cycles of all clouds and possibly add a new cloud.
        /// Rotate the given angle by 45° before creating clouds.
        /// </summary>
        /// <param name="dt">how far to advance the lifecycle</param>
        /// <param name="currentPosition">the position at which to add a possible new cloud</param>
        /// <param name="decayOnly">if true, no new clouds are added (i.e. this just advances the cloud life cycles)</param>
        internal override void Advance(float dt, CCPoint currentPosition, float currentCCRotation, bool decayOnly = false)
        {
            // advance the lifecycle of each cloud 
            foreach (var cloud in clouds)
            {
                cloud.LifeTime += dt;
                cloud.MatchLifeCycle();
            }
            CCPoint pos;
            if (AddRing)   // add a special ring cloud, only one
            {
                pos = CCPoint.RotateByAngle(RelativePosition, CCPoint.Zero, Constants.CCDegreesToMathRadians(currentCCRotation));
                pos += currentPosition;
                AddRing = false;
                clouds.Add(new RingCloud(pos, 0, CCColor4B.White, DrawLow, 100f, 1f));
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
                    // calculate the total position based on the saved relative position, the currentPosition and the current CCrotation
                    pos = CCPoint.RotateByAngle(RelativePosition, CCPoint.Zero, Constants.CCDegreesToMathRadians(currentCCRotation));
                    pos += currentPosition;
                    // move the spawn of the cloud a little in a random direction for a nice little bonus effect
                    // also make the cloud size a bit random
                    var cloud = new Cloud(Constants.RandomPointNear(pos, ReferenceSize), currentCCRotation + 45f, CloudColor, DrawLow, ReferenceSize + (float)(new Random()).NextDouble() * ReferenceSize*2, CloudLifeTime);
                    //var cloud = new DamageCloud(Constants.RandomPointNear(pos, ReferenceSize), currentCCRotation+45f, CloudColor, ReferenceSize + (float)(new Random()).NextDouble() * ReferenceSize, CloudLifeTime);
                    //cloud.FireColor = FireColor;
                    clouds.Add(cloud);
                }
            }
            else
            {
                // let the timer run
                TimeSinceLastCloud = timePlusDt;
            }
            // remove all clouds that have gone beyond their lifecycle
            for (int i = 0; i < clouds.Count; i++)
            {
                if (clouds[i].LifeTime > clouds[i].TotalLifeTime)
                    clouds.RemoveAt(i--);
            }
        }
    }
}
