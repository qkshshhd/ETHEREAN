using System;
using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float horizontalMoveSpeed = 6f;
        [SerializeField, Min(0.01f)] private float acceleration = 40f;
        [SerializeField, Min(0.01f)] private float deceleration = 60f;
        [SerializeField, Min(0.01f)] private float riseGravity = 24f;
        [SerializeField, Min(0.01f)] private float fallGravity = 48f;
        [SerializeField, Min(0.01f)] private float maxFallSpeed = 20f;

        private Rigidbody2D _rigidbody2D;
        private float _originalGravityScale;
        private bool _ownsGravity;

        public event Action MovementEnabled;
        public event Action MovementDisabled;
        public Vector2 Velocity => Body.linearVelocity;
        private Rigidbody2D Body => _rigidbody2D != null ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();

        private void OnEnable()
        {
            MovementEnabled?.Invoke();
        }

        private void OnDisable()
        {
            EndMovement();
            MovementDisabled?.Invoke();
        }

        internal void BeginMovement()
        {
            if (_ownsGravity || !isActiveAndEnabled)
                return;
            _originalGravityScale = Body.gravityScale;
            Body.gravityScale = 0f;
            _ownsGravity = true;
        }

        internal void EndMovement()
        {
            if (_ownsGravity)
            {
                Body.gravityScale = _originalGravityScale;
                _ownsGravity = false;
            }
            Vector2 velocity = Body.linearVelocity;
            velocity.x = 0f;
            Body.linearVelocity = velocity;
        }

        internal float CalculateHorizontalSpeed(float currentSpeed, float input, float deltaTime)
        {
            float targetSpeed = Mathf.Clamp(input, -1f, 1f) * Mathf.Max(0f, horizontalMoveSpeed);
            bool isReversing = currentSpeed * targetSpeed < 0f;
            bool isSlowingDown = isReversing || Mathf.Abs(targetSpeed) < Mathf.Abs(currentSpeed);
            float changeRate = isSlowingDown ? deceleration : acceleration;
            return Mathf.MoveTowards(currentSpeed, isReversing ? 0f : targetSpeed,
                Mathf.Max(0f, changeRate) * deltaTime);
        }

        internal float CalculateGravitySpeed(float currentSpeed, float deltaTime)
        {
            float gravity = currentSpeed > 0f ? riseGravity : fallGravity;
            return Mathf.Max(currentSpeed - Mathf.Max(0f, gravity) * deltaTime, -Mathf.Max(0f, maxFallSpeed));
        }

        internal void ApplyVelocity(Vector2 velocity)
        {
            if (isActiveAndEnabled && _ownsGravity)
                Body.linearVelocity = velocity;
        }
    }
}
