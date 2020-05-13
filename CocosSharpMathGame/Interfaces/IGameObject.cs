﻿using System;
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
        float GetScale();
    }
}
