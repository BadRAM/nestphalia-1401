using System.Diagnostics;
using System.Numerics;

namespace nestphalia;

public partial class Minion
{
    protected abstract class BaseState
    {
        protected Minion Me;
        
        public BaseState(Minion minion) { Me = minion; } 
        
        public abstract void Update();
        public abstract int GetAnimFrame();
        public new abstract string ToString();
    }

    // Move acts as the 'default' state, that other states can safely return to without arguments
    protected class Move : BaseState
    {
        private int _animFrame;
        private int _animCounter;
        
        public Move(Minion minion) : base(minion) { }
        public override string ToString() { return "Move"; }

        public override void Update()
        {
            // If we've reached the current node of the path, change to the next node. Also checks if we're lost
            Me.UpdateNextPos();
            
            // If we've gotten lost, ask for a new path
            if (Me.CheckIfLost())
            {
                Me.SetTarget(Me.NavPath.Destination);
            }
            
            // If there's something in our way, change to attacking state
            if (Me.CanAttack())
            {
                Me.State = new Attack(Me);
                return;
            }
            
            // If we've reached the end of our path, request a new path and change to waiting state
            if (Me.NavPath.TargetReached(Me.Position))
            {
                Me.OnTargetReached();
                return;
            }
            
            // If no state change is needed, move towards NextPos
            Me.Position = Me.Position.MoveTowards(Me.NextPos, Me.AdjustedSpeed() * Time.DeltaTime); // else: Move
            _animCounter++;
            if (_animCounter >= 2)
            {
                _animFrame = (_animFrame + 1) % 4;
                _animCounter = 0;
            }
        }

        public override int GetAnimFrame()
        {
            return _animFrame;
        }
    }

    protected class Attack : BaseState
    {
        private double _attackStartedTime;

        public Attack(Minion minion) : base(minion)
        {
            _attackStartedTime = Time.Scaled;
        }
        public override string ToString() { return "Attack"; }
        
        public override void Update()
        {
            // Change to Move if target invalid
            if (!Me.CanAttack())
            {
                Me.State = new Move(Me);
                return;
            }
            
            // Attack if ready
            Me._timeOfLastAction = Time.Scaled;
            if (Time.Scaled - _attackStartedTime >= Me.Template.AttackCooldown)
            {
                Me.OnAttack();
                _attackStartedTime = Time.Scaled;
            }
        }

        public override int GetAnimFrame()
        {
            return 0;
        }
    }

    protected class Wait : BaseState
    {
        private int _waitRemaining;
        private Action _finishAction;

        public Wait(Minion minion, double duration, Action finishAction) : base(minion)
        {
            _waitRemaining = (int)(duration / Time.DeltaTime);
            _finishAction = finishAction;
        }
        public override string ToString() { return "Wait"; }
        
        public override void Update()
        {
            _waitRemaining--;
            if (_waitRemaining == 0)
            {
                _finishAction();
            }
            Debug.Assert(_waitRemaining >= 0);
        }

        public override int GetAnimFrame()
        {
            return 0;
        }
    }

    protected class Jump : BaseState
    {
        private Vector2 _from;
        private Vector2 _to;
        
        private double _started;
        private double _squatDuration;
        private double _jumpDuration;
        private double _landingLag;
        private double _height = 24;

        public Jump(Minion minion, Vector2 to, double squatDuration, double jumpDuration, double landingLag) 
            : base(minion)
        {
            _to = to;
            _squatDuration = squatDuration;
            _jumpDuration = jumpDuration;
            _landingLag = landingLag;
            _started = Time.Scaled;
            
            Me.NavPath.Skip();
            _from = Me.Position;
            Me.NavPath.Skip();
            Me.UpdateNextPos();
        }
        
        public override string ToString() { return "Jump"; }

        public override void Update()
        {
            if (Time.Scaled - _started < _squatDuration) // Waiting in jumpSquat
            {
            }
            else if (Time.Scaled - _started < _jumpDuration + _squatDuration) // Jumping
            {
                Me.IsFlying = true;
            
                double t = (Time.Scaled - (_started + _squatDuration)) / _jumpDuration;
                double arcOffset = Math.Sin(t * Math.PI) * _height;
            
                Me.Position = Vector2.Lerp(_from, _to, (float)t);
                Me.Position.Y -= (float)arcOffset;
            }
            else // Jump finished
            {
                Me.IsFlying = false;
                Me.Position = _to;
                Me.State = new Wait(Me, _landingLag, () => { Me.State = new Move(Me); });
            }
        }

        public override int GetAnimFrame()
        {
            if (Time.Scaled - _started < _squatDuration) // Waiting in jumpSquat
            {
                return 5;
            }
            return 4;
        }
    }
}

