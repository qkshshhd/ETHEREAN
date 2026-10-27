using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerJump : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float jumpSpeed = 12f;
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.1f;

        private float _lastGroundedTime = float.NegativeInfinity;
        private float _lastJumpPressedTime = float.NegativeInfinity;
        private bool _bufferedJumpReleased;
        private bool _jumpAvailable;
        private bool _jumpCutAvailable;

        internal float JumpSpeed => jumpSpeed;

        internal void UpdateState(bool grounded, bool pressed, bool released, bool held, bool inputEnabled, float now)
        {
            if (!isActiveAndEnabled)
                return;
            if (grounded)
            {
                _lastGroundedTime = now;
                _jumpAvailable = true;
                _jumpCutAvailable = false;
            }
            if (!inputEnabled)
            {
                ClearBuffer();
                return;
            }
            if (pressed)
            {
                _lastJumpPressedTime = now;
                _bufferedJumpReleased = false;
            }
            if (released || !held)
                _bufferedJumpReleased = true;
            if (now - _lastJumpPressedTime > Mathf.Max(0f, jumpBufferTime))
                ClearBuffer();
        }

        internal bool HasBufferedJump(float now)
        {
            return isActiveAndEnabled && now - _lastJumpPressedTime <= Mathf.Max(0f, jumpBufferTime);
        }

        internal bool CanGroundJump(bool grounded, float now)
        {
            return isActiveAndEnabled && _jumpAvailable &&
                (grounded || now - _lastGroundedTime <= Mathf.Max(0f, coyoteTime));
        }

        internal float StartJump(float speed, bool held)
        {
            _jumpAvailable = false;
            _jumpCutAvailable = true;
            _lastGroundedTime = float.NegativeInfinity;
            bool releasedBeforeJump = _bufferedJumpReleased;
            ClearBuffer();
            return ApplyJumpCut(Mathf.Max(0f, speed), releasedBeforeJump, held);
        }

        internal float ApplyJumpCut(float currentSpeed, bool released, bool held)
        {
            if (!isActiveAndEnabled)
                return currentSpeed;
            if (currentSpeed <= 0f)
                _jumpCutAvailable = false;
            if (!_jumpCutAvailable || (!released && held))
                return currentSpeed;
            _jumpCutAvailable = false;
            return currentSpeed * Mathf.Clamp01(jumpCutMultiplier);
        }

        internal void ClearBuffer()
        {
            _lastJumpPressedTime = float.NegativeInfinity;
            _bufferedJumpReleased = false;
        }

        internal void ResetState()
        {
            ClearBuffer();
            _lastGroundedTime = float.NegativeInfinity;
            _jumpAvailable = false;
            _jumpCutAvailable = false;
        }

        private void OnDisable()
        {
            ResetState();
        }
    }
}
