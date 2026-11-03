using System;
using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerJump), typeof(PlayerWallJump))]
    [RequireComponent(typeof(PlayerWallSlide), typeof(PlayerGroundSensor), typeof(PlayerWallSensor))]
    [RequireComponent(typeof(PlayerTraversalAbilities))]
    public sealed class PlayerLocomotion : MonoBehaviour
    {
        [SerializeField] private PlayerMovementInput movementInputSource;
        [SerializeField] private PlayerGroundSensor groundSensor;
        [SerializeField] private PlayerWallSensor wallSensor;
        [SerializeField] private PlayerTraversalAbilities traversalAbilities;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerJump jump;
        [SerializeField] private PlayerWallJump wallJump;
        [SerializeField] private PlayerWallSlide wallSlide;
        private float _actionSpeedMultiplier = 1f;
        private bool _actionAllowsJump = true;

        public event Action<MovementFrame> PreparingMovement;
        public event Action JumpStarted;
        public event Action ControlCancelled;

        public readonly struct MovementFrame
        {
            public readonly bool Grounded, WallSliding, ControlEnabled;
            public readonly float HorizontalInput, Time;
            public MovementFrame(bool grounded, bool wallSliding, bool controlEnabled, float input, float time)
            {
                Grounded = grounded; WallSliding = wallSliding; ControlEnabled = controlEnabled;
                HorizontalInput = input; Time = time;
            }
        }

        public void SetActionConstraints(float speedMultiplier, bool allowsJump)
        {
            _actionSpeedMultiplier = Mathf.Clamp01(speedMultiplier);
            _actionAllowsJump = allowsJump;
        }

        public bool IsGrounded { get; private set; }
        public bool IsWallSliding => wallSlide != null && wallSlide.IsWallSliding;

        private void Reset()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (movementInputSource == null) movementInputSource = GetComponent<PlayerMovementInput>();
            if (groundSensor == null) groundSensor = GetComponent<PlayerGroundSensor>();
            if (wallSensor == null) wallSensor = GetComponent<PlayerWallSensor>();
            if (traversalAbilities == null) traversalAbilities = GetComponent<PlayerTraversalAbilities>();
            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (jump == null) jump = GetComponent<PlayerJump>();
            if (wallJump == null) wallJump = GetComponent<PlayerWallJump>();
            if (wallSlide == null) wallSlide = GetComponent<PlayerWallSlide>();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (movementInputSource == null || groundSensor == null || wallSensor == null || traversalAbilities == null ||
                movement == null || jump == null || wallJump == null || wallSlide == null)
            {
                Debug.LogError("PlayerLocomotion requires input, sensors, abilities and all movement features.", this);
                enabled = false;
                return;
            }
            movementInputSource.InputDisabled += CancelPendingInput;
            movement.MovementEnabled += OnMovementEnabled;
            movement.MovementDisabled += ResetRuntimeState;
            OnMovementEnabled();
        }

        private void OnMovementEnabled()
        {
            ResetRuntimeState();
            movement.BeginMovement();
        }

        private void FixedUpdate()
        {
            if (!movement.isActiveAndEnabled)
                return;
            movement.BeginMovement();
            PlayerWallSensor.WallContacts walls = wallSensor.CheckWalls();
            bool abilityUnlocked = traversalAbilities.isActiveAndEnabled && traversalAbilities.WallTraversalUnlocked;
            wallJump.UpdateContacts(walls, abilityUnlocked && wallSensor.isActiveAndEnabled && movementInputSource.isActiveAndEnabled, Time.fixedTime);
            movement.ApplyVelocity(CalculateVelocity(movement.Velocity, walls, abilityUnlocked));
        }

        private Vector2 CalculateVelocity(Vector2 velocity, PlayerWallSensor.WallContacts walls, bool abilityUnlocked)
        {
            float now = Time.fixedTime;
            bool pressed = movementInputSource.ConsumeJumpPress();
            bool released = movementInputSource.ConsumeJumpRelease();
            bool held = movementInputSource.IsJumpHeld;
            float horizontalInput = movementInputSource.HorizontalInput;
            wallSlide.ResetState();
            IsGrounded = velocity.y <= 0f && groundSensor.CheckGrounded();
            if (IsGrounded)
                wallJump.ResetState();
            jump.UpdateState(IsGrounded, pressed, released, held, movementInputSource.isActiveAndEnabled, now);
            bool sliding = !IsGrounded && velocity.y <= 0f && walls.HasContact && abilityUnlocked && wallSlide.isActiveAndEnabled;
            SetActionConstraints(1f, true);
            PreparingMovement?.Invoke(new MovementFrame(IsGrounded, sliding, movementInputSource.isActiveAndEnabled, horizontalInput, now));
            if (!wallJump.IsControlLocked(now))
                velocity.x = movement.CalculateHorizontalSpeed(horizontalInput, _actionSpeedMultiplier);
            if (TryStartJump(walls, abilityUnlocked, horizontalInput, held, now, ref velocity))
                return velocity;
            if (IsGrounded)
            {
                velocity.y = 0f;
                return velocity;
            }
            velocity.y = jump.ApplyJumpCut(velocity.y, released, held);
            float fallingSpeed = movement.CalculateGravitySpeed(velocity.y, Time.fixedDeltaTime);
            velocity.y = wallSlide.LimitFallSpeed(velocity.y, fallingSpeed, walls.HasContact, abilityUnlocked);
            return velocity;
        }

        private bool TryStartJump(PlayerWallSensor.WallContacts walls, bool abilityUnlocked, float input,
            bool held, float now, ref Vector2 velocity)
        {
            if (!jump.HasBufferedJump(now))
                return false;
            if (!_actionAllowsJump)
                return false;
            if (!IsGrounded && abilityUnlocked && wallJump.TryJump(walls, input, now, out Vector2 launch))
            {
                velocity.x = launch.x;
                velocity.y = jump.StartJump(launch.y, held);
            }
            else if (jump.CanGroundJump(IsGrounded, now))
            {
                velocity.y = jump.StartJump(jump.JumpSpeed, held);
            }
            else
            {
                return false;
            }
            IsGrounded = false;
            JumpStarted?.Invoke();
            return true;
        }

        private void CancelPendingInput()
        {
            if (jump != null) jump.ClearBuffer();
            if (wallJump != null) wallJump.CancelPendingInput();
            SetActionConstraints(1f, true);
            ControlCancelled?.Invoke();
        }

        private void ResetRuntimeState()
        {
            IsGrounded = false;
            if (jump != null) jump.ResetState();
            if (wallJump != null) wallJump.ResetState();
            if (wallSlide != null) wallSlide.ResetState();
            SetActionConstraints(1f, true);
            ControlCancelled?.Invoke();
            if (movementInputSource != null)
            {
                movementInputSource.ConsumeJumpPress();
                movementInputSource.ConsumeJumpRelease();
            }
        }

        private void OnDisable()
        {
            if (movementInputSource != null) movementInputSource.InputDisabled -= CancelPendingInput;
            if (movement != null)
            {
                movement.MovementEnabled -= OnMovementEnabled;
                movement.MovementDisabled -= ResetRuntimeState;
            }
            ResetRuntimeState();
            if (movement != null) movement.EndMovement();
        }
    }
}
