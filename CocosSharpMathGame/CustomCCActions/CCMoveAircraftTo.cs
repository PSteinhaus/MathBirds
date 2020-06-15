using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// DON'T USE. This is currently not necessary, as Aircraft.MoveTo doesn't do anything special yet.
    /// </summary>
    public class CCMoveAircraftTo : CCFiniteTimeAction
    {
        public CCPoint FinalPosition { get; private set; }

        #region Constructors

        public CCMoveAircraftTo(float duration, CCPoint position)
            : base(duration)
        {
            FinalPosition = position;
        }

        #endregion Constructors

        protected override CCActionState StartAction(CCNode target)
        {
            return new CCMoveAircraftToState(this, target);
        }

        public override CCFiniteTimeAction Reverse()
        {
            throw new NotImplementedException();
        }
    }

    public class CCMoveAircraftToState : CCFiniteTimeActionState
    {
        protected CCPoint FinalPosition { get; private set; }
        protected CCPoint StartPosition;
        protected CCPoint DiffPosition;
        public CCMoveAircraftToState(CCMoveAircraftTo action, CCNode target)
            : base(action, target)
        {
            FinalPosition = action.FinalPosition;
            StartPosition = target.Position;
            DiffPosition  = FinalPosition - StartPosition;
        }

        public override void Update(float deltaTime)
        {
            if (Target != null)
            {
                var elapsedMove = DiffPosition * Math.Min(Elapsed / Duration, 1);
                ((Aircraft)Target).MoveTo(StartPosition + elapsedMove);
            }
        }

    }
}