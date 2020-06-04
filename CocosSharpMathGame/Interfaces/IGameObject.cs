using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// basically an extension of CCNode with stuff that I like
    /// </summary>
    internal interface IGameObject
    {
        bool MyVisible { get; }
        float MyRotation { get; set; }
        float TotalRotation { get; }
        void RotateTowards(float angle, float maxRotationAngle);
        float GetScale();
        /// <summary>
        /// sets the scaling to fit a certain width in pixels
        /// </summary>
        /// <param name="width">how wide the sprite shall be (in world pixels)</param>
        void FitToWidth(float desiredWidth);

        /// <summary>
        /// sets the scaling to fit a certain height in pixels
        /// </summary>
        /// <param name="height">how high the sprite shall be (in world pixels)</param>
        void FitToHeight(float desiredHeight);

        /// <summary>
        /// Returns a simple collision polygon, that is a diamond based on the ContentSize
        /// </summary>
        /// <returns></returns>
        CCPoint[] DiamondCollisionPoints();
    }
}
