using System;
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
    internal class Aircraft : GameObjectNode, ICollidable
    {
        private Part body = null;
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
                    AddChild(body);
            }
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
    }
}
