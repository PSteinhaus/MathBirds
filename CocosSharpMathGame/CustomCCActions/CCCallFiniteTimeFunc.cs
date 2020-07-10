using CocosSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    public class CCCallFiniteTimeFunc : CCFiniteTimeAction
    {
        public Action<float,float> MyFunction;

        #region Constructors

        public CCCallFiniteTimeFunc(float duration, Action<float,float> myFunction) : base(duration)
        {
            MyFunction = myFunction;
        }

        #endregion Constructors

        protected override CCActionState StartAction(CCNode target)
        {
            return new CCCallFiniteTimeFuncState(this, target);
        }

        public override CCFiniteTimeAction Reverse()
        {
            throw new NotImplementedException();
        }
    }
    public class CCCallFiniteTimeFuncState : CCFiniteTimeActionState
    {
        protected Action<float,float> MyFunction;
        /// <summary>
        /// dirty bug fix (for some reason "Elapsed" doesn't work here)
        /// </summary>
        protected float MyElapsed = 0f;
        public CCCallFiniteTimeFuncState(CCCallFiniteTimeFunc action, CCNode target) : base(action, target)
        {
            MyFunction = action.MyFunction;
        }

        public override void Update(float progress) // for some reason it's progress now...
        {
            if (Target != null)
            {
                MyFunction(progress, Duration);
            }
        }
    }
}
