using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public partial class Minion
{
    public abstract class BaseState
    {
        protected Minion Me;
        
        public BaseState(Minion minion) { Me = minion; } 
        
        public abstract void Update();
        public abstract int GetAnimFrame();
        public virtual void DrawDecorators() {}
        public new abstract string ToString();

        // This is called to tell a state to exit early and load nextState. The state can return false to signal that it has ignored the request.
        public virtual bool Exit(BaseState nextState)
        {
            Me.State = nextState;
            return true;
        } 
    }

    // Move acts as the 'default' state, that other states can safely return to without arguments
    public class Move : BaseState
    {
        private int _animFrame;
        private int _animCounter;
        
        public Move(Minion minion) : base(minion) { }
        public override string ToString() { return $"Move, frame: {_animFrame}, counter: {_animCounter}"; }

        public override void Update()
        {
            // If we've reached the current node of the path, change to the next node
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
            Me.Position = Me.Position.MoveTowardsXY(Me.NextPos, Me.AdjustedSpeed() * Time.DeltaTime); // else: Move
            _animCounter++;
            if (_animCounter >= Me.Template.WalkAnimDelay)
            {
                _animFrame = (_animFrame + 1) % 4;
                _animCounter = 0;
            }
        }

        public override int GetAnimFrame()
        {
            return _animFrame + 1;
        }
    }

    public class Attack : BaseState
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
            if (Time.Scaled - _attackStartedTime >= Me.Template.AttackDuration)
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

    public class Wait : BaseState
    {
        private int _waitRemaining;
        private Action _finishAction;
        private Texture2D? _decorator;

        public Wait(Minion minion, double duration, Action finishAction, Texture2D? decorator = null) : base(minion)
        {
            _waitRemaining = (int)(duration / Time.DeltaTime);
            _finishAction = finishAction;
            _decorator = decorator;
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

        public override void DrawDecorators()
        {
            if (_decorator == null) return;
            Raylib.DrawTexture(_decorator ?? Resources.MissingTexture, (int)Me.Position.X, (int)(Me.Position.Y - (Me.Template.PhysicsRadius + 6)), Color.White);
        }
    }

    public class Jump : BaseState
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
            _from = Me.Position.XY();
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
            
                Me.Position = Vector2.Lerp(_from, _to, (float)t).XYZ();
                Me.Position.Z = (float)arcOffset;
            }
            else // Jump finished
            {
                Me.IsFlying = false;
                Me.Position = _to.XYZ();
                Me.State = new Wait(Me, _landingLag, () => { Me.State = new Move(Me); });
            }
        }

        public override int GetAnimFrame()
        {
            if (Time.Scaled - _started < _squatDuration) // Waiting in jumpSquat
            {
                return 5;
            }
            return 6;
        }
    }
    
    public class Cheer : BaseState
    {
        public Cheer(Minion minion) : base(minion) { }
        public override string ToString() { return $"Cheer"; }

        public override void Update()
        {
            // Do the wave!
            Me.DrawOffset = Vector3.UnitZ * (float)Math.Max((Math.Abs((Time.Scaled + Me.Position.X/150) % 3 - 1.5)-1.25)*4, 0) * 6;
        }

        public override int GetAnimFrame()
        {
            return 0;
        }
    }
    
    public class Flee : BaseState
    {
        private int _animFrame;
        private int _animCounter;

        public Flee(Minion minion) : base(minion) { }
        public override string ToString() { return $"Flee"; }
        
        public override void Update()
        {
            // If we've reached the current node of the path, change to the next node
            Me.UpdateNextPos();
            
            // If we've reached the end of our path, request a new path and change to waiting state
            if (Me.NavPath.TargetReached(Me.Position))
            {
                Me.Die();
                return;
            }
            
            // If no state change is needed, move towards NextPos
            Me.Position = Me.Position.MoveTowardsXY(Me.NextPos, Me.AdjustedSpeed() * Time.DeltaTime); // else: Move
            _animCounter++;
            if (_animCounter >= Me.Template.WalkAnimDelay)
            {
                _animFrame = (_animFrame + 1) % 4;
                _animCounter = 0;
            }
        }

        public override int GetAnimFrame()
        {
            return _animFrame + 1;
        }
    }
}