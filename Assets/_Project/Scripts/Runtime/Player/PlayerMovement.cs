using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerGroundSensor))]
    [RequireComponent(typeof(PlayerWallSensor), typeof(PlayerTraversalAbilities))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerMovementInput movementInputSource;
        [SerializeField] private PlayerGroundSensor groundSensor;
        [SerializeField] private PlayerWallSensor wallSensor;
        [SerializeField] private PlayerTraversalAbilities traversalAbilities;
        [SerializeField, Min(0f)] private float horizontalMoveSpeed = 6f;
        [SerializeField, Min(0.01f)] private float acceleration = 40f;
        [SerializeField, Min(0.01f)] private float deceleration = 60f;
        [SerializeField, Min(0.01f)] private float jumpSpeed = 12f;
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.1f;
        [SerializeField, Min(0.01f)] private float riseGravity = 24f;
        [SerializeField, Min(0.01f)] private float fallGravity = 48f;
        [SerializeField, Min(0.01f)] private float maxFallSpeed = 20f;
        [SerializeField, Min(0.01f)] private float wallSlideSpeed = 3f;

        private Rigidbody2D _rigidbody2D;
        private float _lastGroundedTime = float.NegativeInfinity;
        private float _lastJumpPressedTime = float.NegativeInfinity;
        private bool _bufferedJumpReleased;
        private float _originalGravityScale;
        private bool _jumpAvailable;
        private bool _jumpCutAvailable;
        private bool _ownsGravity;

        public bool IsGrounded { get; private set; }
        public bool IsWallSliding { get; private set; }

        private void Reset()
        {
            movementInputSource = GetComponent<PlayerMovementInput>();
            groundSensor = GetComponent<PlayerGroundSensor>();
            wallSensor = GetComponent<PlayerWallSensor>();
            traversalAbilities = GetComponent<PlayerTraversalAbilities>();
        }

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            if (groundSensor == null)
                groundSensor = GetComponent<PlayerGroundSensor>();
            if (wallSensor == null)
                wallSensor = GetComponent<PlayerWallSensor>();
            if (traversalAbilities == null)
                traversalAbilities = GetComponent<PlayerTraversalAbilities>();
            if (movementInputSource == null)
            {
                movementInputSource = GetComponent<PlayerMovementInput>();
            }

            if (movementInputSource == null || groundSensor == null || wallSensor == null || traversalAbilities == null)
            {
                Debug.LogError("PlayerMovement requires input, ground sensor, wall sensor and traversal abilities.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_rigidbody2D == null || movementInputSource == null || groundSensor == null || wallSensor == null || traversalAbilities == null)
            {
                enabled = false;
                return;
            }
            movementInputSource.InputDisabled += ClearJumpBuffer;
            ClearJumpBuffer();
            _originalGravityScale = _rigidbody2D.gravityScale;
            _rigidbody2D.gravityScale = 0f;
            _ownsGravity = true;
            _lastGroundedTime = float.NegativeInfinity;
            _jumpAvailable = false;
            _jumpCutAvailable = false;
            IsGrounded = false;
            IsWallSliding = false;
            movementInputSource.ConsumeJumpPress();
            movementInputSource.ConsumeJumpRelease();
        }

        private void FixedUpdate()
        {
            Vector2 linearVelocity = _rigidbody2D.linearVelocity;
            linearVelocity.x = CalculateHorizontalSpeed(linearVelocity.x);
            linearVelocity.y = CalculateVerticalSpeed(linearVelocity.y);
            _rigidbody2D.linearVelocity = linearVelocity;
        }

        private float CalculateHorizontalSpeed(float currentSpeed)
        {
            float horizontalInput = movementInputSource != null ? movementInputSource.HorizontalInput : 0f;
            float targetSpeed = Mathf.Clamp(horizontalInput, -1f, 1f) * Mathf.Max(0f, horizontalMoveSpeed);
            bool isReversing = currentSpeed * targetSpeed < 0f;
            bool isSlowingDown = isReversing || Mathf.Abs(targetSpeed) < Mathf.Abs(currentSpeed);
            float changeRate = isSlowingDown ? deceleration : acceleration;
            return Mathf.MoveTowards(currentSpeed, isReversing ? 0f : targetSpeed,
                Mathf.Max(0f, changeRate) * Time.fixedDeltaTime);
        }

        private float CalculateVerticalSpeed(float currentSpeed)
        {
            IsWallSliding = false;
            IsGrounded = currentSpeed <= 0f && groundSensor.CheckGrounded();
            if (IsGrounded)
            {
                _lastGroundedTime = Time.fixedTime;
                _jumpAvailable = true;
            }

            bool jumpPressed = movementInputSource != null && movementInputSource.ConsumeJumpPress();
            bool jumpReleased = movementInputSource != null && movementInputSource.ConsumeJumpRelease();
            UpdateJumpBuffer(jumpPressed, jumpReleased);
            bool hasBufferedJump = Time.fixedTime - _lastJumpPressedTime <= Mathf.Max(0f, jumpBufferTime);
            if (hasBufferedJump && _jumpAvailable &&
                (IsGrounded || Time.fixedTime - _lastGroundedTime <= Mathf.Max(0f, coyoteTime)))
            {
                IsGrounded = false;
                IsWallSliding = false;
                _jumpAvailable = false;
                _jumpCutAvailable = true;
                _lastGroundedTime = float.NegativeInfinity;
                bool releasedBeforeJump = _bufferedJumpReleased;
                ClearJumpBuffer();
                return ApplyJumpCut(Mathf.Max(0f, jumpSpeed), releasedBeforeJump);
            }

            if (IsGrounded)
            {
                _jumpCutAvailable = false;
                return 0f;
            }

            currentSpeed = ApplyJumpCut(currentSpeed, jumpReleased);
            float gravity = currentSpeed > 0f ? riseGravity : fallGravity;
            float nextSpeed = Mathf.Max(currentSpeed - Mathf.Max(0f, gravity) * Time.fixedDeltaTime,
                -Mathf.Max(0f, maxFallSpeed));
            if (currentSpeed <= 0f && traversalAbilities.isActiveAndEnabled &&
                traversalAbilities.WallTraversalUnlocked && wallSensor.CheckWalls().HasContact)
            {
                IsWallSliding = true;
                nextSpeed = Mathf.Max(nextSpeed, -Mathf.Max(0.01f, wallSlideSpeed));
            }
            return nextSpeed;
        }

        private void UpdateJumpBuffer(bool jumpPressed, bool jumpReleased)
        {
            if (movementInputSource == null || !movementInputSource.isActiveAndEnabled)
            {
                ClearJumpBuffer();
                return;
            }

            if (jumpPressed)
            {
                _lastJumpPressedTime = Time.fixedTime;
                _bufferedJumpReleased = false;
            }
            if (jumpReleased || !movementInputSource.IsJumpHeld)
                _bufferedJumpReleased = true;

            if (Time.fixedTime - _lastJumpPressedTime > Mathf.Max(0f, jumpBufferTime))
                ClearJumpBuffer();
        }

        private void ClearJumpBuffer()
        {
            _lastJumpPressedTime = float.NegativeInfinity;
            _bufferedJumpReleased = false;
        }

        private float ApplyJumpCut(float currentSpeed, bool jumpReleased)
        {
            if (currentSpeed <= 0f)
                _jumpCutAvailable = false;

            bool isJumpHeld = movementInputSource != null && movementInputSource.IsJumpHeld;
            if (!_jumpCutAvailable || (!jumpReleased && isJumpHeld))
                return currentSpeed;

            _jumpCutAvailable = false;
            return currentSpeed * Mathf.Clamp01(jumpCutMultiplier);
        }

        private void OnDisable()
        {
            if (movementInputSource != null)
                movementInputSource.InputDisabled -= ClearJumpBuffer;
            ClearJumpBuffer();
            if (_rigidbody2D == null) return;

            if (_ownsGravity)
            {
                _rigidbody2D.gravityScale = _originalGravityScale;
                _ownsGravity = false;
            }
            IsGrounded = false;
            IsWallSliding = false;
            _jumpAvailable = false;
            _jumpCutAvailable = false;
            _lastGroundedTime = float.NegativeInfinity;
            if (movementInputSource != null)
            {
                movementInputSource.ConsumeJumpPress();
                movementInputSource.ConsumeJumpRelease();
            }

            Vector2 linearVelocity = _rigidbody2D.linearVelocity;
            linearVelocity.x = 0f;
            _rigidbody2D.linearVelocity = linearVelocity;
        }
    }
}
