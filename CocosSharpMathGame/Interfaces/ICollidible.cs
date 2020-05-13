using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    /// <summary>
    /// implemented by all objects in the sky that can collide with things like shoots, rockets, etc.
    /// </summary>
    interface ICollidible : IGameObject
    {
        /// <summary>
        /// Returns an object representing the collision type (and perhaps holding data necessary to compute the collision).
        /// </summary>
        /// <returns></returns>
        CollisionType CollisionType { get; set; }
    }
}
