using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public partial class Minion
{
    public enum AnimationState
    {
        Base, // Used for single animation ImageSets
        // Minion related sets:
        Walking,
        Flying,
        Attacking,
        Jumping,
        Carrying
    }
    
    public enum StateType
    {
        Move,
        SwoopAttack,
        MeleeAttack,
        RangedAttack,
        Wait,
        Jump,
        Cheer,
        Flee
    }
    
    public abstract class BaseState
    {
        protected Minion Me;
        
        public BaseState(Minion minion) { Me = minion; } 
        
        public abstract void Update();
        public abstract Rectangle GetAnimFrame();
        public virtual void DrawDecorators() {}
        public new abstract string ToString();

        // This is called to tell a state to exit early and load nextState. The state can return false to signal that it has ignored the request.
        public virtual bool Exit(BaseState nextState)
        {
            Me.State = nextState;
            return true;
        } 
    }

    public class Move : BaseState
    {
        private double _animTimer;
        
        public Move(Minion minion) : base(minion) { }

        public override string ToString()
        {
            int frames = Me.Template.GetAnimationFrameCount(AnimationState.Walking);
            if (frames == 0) frames = 1;
            return $"Move, frame: {_animTimer % frames}";
        }

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
                Me.ResetState(Me.Template.AttackType);
                return;
            }
            
            // If we've reached the end of our path, request a new path and change to waiting state
            if (Me.NavPath.TargetReached(Me.Position))
            {
                Me.OnTargetReached();
                return;
            }
            
            // If no state change is needed, move towards NextPos
            Me.Position = Me.Position.MoveTowardsXY(Me.NextPos, Me.AdjustSpeed(Me.Template.Speed) * Time.DeltaTime); // else: Move

            _animTimer += Time.DeltaTime * Me.Template.AnimFrameRate;
        }

        public override Rectangle GetAnimFrame()
        {
            AnimationState animState = Me.IsFlying ? AnimationState.Flying : AnimationState.Walking;
            return Me.Template.GetAnimationFrame(animState, (int)_animTimer);
        }
    }
    
    public class SwoopAttack : BaseState
    {
        private double _attackStartedTime;
        private float _altitude;
        private Vector2 _startPos;
        private Vector2 _endPos;
        private double _duration;
        private bool _hasAttacked = false;
        private double _animTimer;

        public SwoopAttack(Minion minion) : base(minion)
        {
            _attackStartedTime = Time.Scaled;
            _altitude = minion.Position.Z;
            _startPos = minion.Position.XY();
            _endPos = minion.Position.XY() + (Me.NextPos - Me.Position.XY()) * 2;
            _duration = Me.Template.AttackDuration * 0.75f;
        }
        
        public override string ToString() { return "SwoopAttack"; }

        public override void Update()
        {
            float t = (float)((Time.Scaled - _attackStartedTime) / _duration);
            Me.Position = new Vector3(Vector2.Lerp(_startPos, _endPos, t), _altitude - (float)(_altitude * Easings.Ballistic(t) * 0.8f));
            if (!_hasAttacked && t >= 0.5)
            {
                _hasAttacked = true;
                Me.DoAttack();
            }
            if (t >= 1)
            {
                Me.Position = _endPos.XYZ(_altitude);
                Me.State = new Wait(Me, Me.Template.AttackDuration * 0.25f, () => { Me.ResetState(Me.Template.DefaultState); });
                return;
            }
            _animTimer += Time.DeltaTime * Me.Template.AnimFrameRate;
        }

        public override Rectangle GetAnimFrame()
        {
            return Me.Template.GetAnimationFrame(AnimationState.Flying, (int)_animTimer);
        }
    }
    
    public class MeleeAttack : BaseState
    {
        private double _attackStartedTime;
        private double _animTimer;

        public MeleeAttack(Minion minion) : base(minion)
        {
            _attackStartedTime = Time.Scaled;
        }
        public override string ToString() { return "MeleeAttack"; }
        
        public override void Update()
        {
            // Abort if attack invalid
            if (!Me.CanAttack())
            {
                Me.ResetState(Me.Template.DefaultState);
                return;
            }
            
            // ROCK THE FENCE
            Me.Position = Me.Position.MoveTowardsXY(Me.NextPos, Me.AdjustSpeed(Me.Template.Speed) * Time.DeltaTime);
            
            // Attack when it's time
            Me._timeOfLastAction = Time.Scaled;
            if (Time.Scaled - _attackStartedTime >= Me.Template.AttackDuration)
            {
                Me.DoAttack();
                _attackStartedTime = Time.Scaled;
            }
            _animTimer += Time.DeltaTime * Me.Template.AnimFrameRate;
        }

        public override Rectangle GetAnimFrame()
        {
            return Me.Template.GetAnimationFrame(AnimationState.Walking, (int)_animTimer);
        }
    }

    public class RangedAttack : BaseState
    {
        private double _attackStartedTime;

        public RangedAttack(Minion minion) : base(minion)
        {
            _attackStartedTime = Time.Scaled;
        }
        public override string ToString() { return "Attack"; }
        
        public override void Update()
        {
            if (Me.IsOnTopOfStructure)
            {
                GameConsole.WriteLine($"{Me.Template.Name} tried to attack while climbing!");
                Me.ResetState(Me.Template.DefaultState);
            }
            
            // Change to Move if target invalid
            if (!Me.CanAttack())
            {
                Me.ResetState(Me.Template.DefaultState);
                return;
            }
            
            // Attack if ready
            Me._timeOfLastAction = Time.Scaled;
            if (Time.Scaled - _attackStartedTime >= Me.Template.AttackDuration)
            {
                Me.DoAttack();
                _attackStartedTime = Time.Scaled;
            }
        }

        public override Rectangle GetAnimFrame()
        {
            return Me.Template.GetAnimationFrame(AnimationState.Base, 0);
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
            // Debug.Assert(_waitRemaining >= 0);
        }

        public override Rectangle GetAnimFrame()
        {
            return Me.Template.GetAnimationFrame(AnimationState.Base, 0);
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
        private double _height;

        public Jump(Minion minion, Vector2 to, double squatDuration, double jumpDuration, double landingLag, double height = 24) 
            : base(minion)
        {
            _to = to;
            _squatDuration = squatDuration;
            _jumpDuration = jumpDuration;
            _landingLag = landingLag;
            _started = Time.Scaled;
            _height = height;
            
            _from = Me.Position.XY();
            Me.NextPos = _to + (_to - _from);
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
            
                Me.Position = Vector2.Lerp(_from, _to, (float)t).XYZ((float)arcOffset);
            }
            else // Jump finished
            {
                Me.IsFlying = false;
                Me.Position = _to.XYZ();
                Structure? landingOn = World.GetTile(World.PosToTilePos(Me.Position));
                Me.IsOnTopOfStructure = landingOn?.PhysSolid(Me) ?? false;
                Me.NavPath.TargetNearestPoint(Me.Position.XY());
                Me.UpdateNextPos();
                
                // Fall Damage
                if (Me is not HopperMinion && landingOn is not SpringBoard)
                {
                    // scale with maxhealth
                    double fallDamage = Me.Template.MaxHealth; 
                    // scale with drop height. 0% damage from 0-1 tile drop, 100% damage at 4 tile drop
                    fallDamage *= Math.Max((_height - 24) / 96, 0); 
                    fallDamage -= 4; // flat reduction to help small guys
                    if (fallDamage >= 1) Me.Hurt(fallDamage, ignoreArmor:true); // Don't hurt below minimum damage
                }
                
                if (Vector2.Distance(Me.Position.XY(), Me.NextPos) > 48)
                {
                    Me.SetTarget(Me.NavPath.Destination, _landingLag);
                }
                else
                {
                    if (_landingLag == 0)
                    {
                        Me.ResetState(Me.Template.DefaultState);
                    }
                    else
                    {
                        Me.State = new Wait(Me, _landingLag, () => { Me.ResetState(Me.Template.DefaultState); });
                    }
                }
            }
        }

        public override Rectangle GetAnimFrame()
        {
            if (Time.Scaled - _started < _squatDuration) // Waiting in jumpSquat
            {
                return Me.Template.GetAnimationFrame(AnimationState.Jumping, 0);
            }
            return Me.Template.GetAnimationFrame(AnimationState.Jumping, 1);
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

        public override Rectangle GetAnimFrame()
        {
            return Me.Template.GetAnimationFrame(AnimationState.Base, 0);
        }
    }
    
    public class Flee : BaseState
    {
        private double _animTimer;

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
            Me.Position = Me.Position.MoveTowardsXY(Me.NextPos, Me.AdjustSpeed(Me.Template.Speed) * Time.DeltaTime); // else: Move
            _animTimer += Time.DeltaTime * Me.Template.AnimFrameRate;
        }

        public override Rectangle GetAnimFrame()
        {
            AnimationState animState = Me.IsFlying ? AnimationState.Flying : AnimationState.Walking;
            return Me.Template.GetAnimationFrame(animState, (int)_animTimer);
        }
    }
}