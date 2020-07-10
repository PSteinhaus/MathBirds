using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    public class CCMyRotateTo : CCFiniteTimeAction
    {
        public float FinalAngle { get; private set; }

        #region Constructors

        public CCMyRotateTo(float duration, float angle)
            : base(duration)
        {
            FinalAngle = angle;
        }

        #endregion Constructors

        protected override CCActionState StartAction(CCNode target)
        {
            return new CCMyRotateToState(this, target);
        }

        public override CCFiniteTimeAction Reverse()
        {
            throw new NotImplementedException();
        }
    }

    public class CCMyRotateToState : CCFiniteTimeActionState
    {
        protected float FinalAngle { get; set; }

        protected float StartAngle;
        protected float DiffAngle;
        public CCMyRotateToState(CCMyRotateTo action, CCNode target)
            : base(action, target)
        {
            FinalAngle = action.FinalAngle;
            StartAngle = ((IGameObject)target).MyRotation;

            // Now we work out how far we actually have to rotate
            DiffAngle = Constants.AngleFromToDeg(StartAngle, FinalAngle);
        }

        public override void Update(float progress)
        {
            if (Target != null)
            {
                var elapsedRotation = DiffAngle * Math.Min(progress, 1);
                ((IGameObject)Target).MyRotation = StartAngle + elapsedRotation;
            }
        }

    }
}