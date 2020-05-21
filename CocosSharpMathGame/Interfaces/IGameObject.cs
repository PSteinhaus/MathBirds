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
        float MyRotation { get; set; }
        float TotalRotation { get; }
        void RotateTowards(float angle, float maxRotationAngle);
        float GetScale();

        /// <summary>
        /// Returns a simple collision polygon, that is a diamond based on the ContentSize
        /// </summary>
        /// <returns></returns>
        CCPoint[] DiamondCollisionPoints();
    }
}
