using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Can "use" the two drawNodes of the PlayLayer
    /// </summary>
    interface IDrawNodeUser
    {
        void UseDrawNodes(CCDrawNode highNode, CCDrawNode lowNode);
    }
}
