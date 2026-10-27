using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerWallJump : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float wallJumpHorizontalSpeed = 8f;
        [SerializeField, Min(0.01f)] private float wallJumpVerticalSpeed = 16f;
        [SerializeField, Min(0f)] private float wallJumpControlLockTime = 0.1f;
        [SerializeField, Min(0f)] private float wallCoyoteTime = 0.1f;

        private float _wallJumpControlUntil = float.NegativeInfinity;
        private bool _leftWallJumpBlocked;
        private bool _rightWallJumpBlocked;
        private float _lastLeftWallTime = float.NegativeInfinity;
        private float _lastRightWallTime = float.NegativeInfinity;

        internal void UpdateContacts(PlayerWallSensor.WallContacts walls, bool controlAvailable, float now)
        {
            _leftWallJumpBlocked &= walls.Left;
            _rightWallJumpBlocked &= walls.Right;
            if (!isActiveAndEnabled || !controlAvailable)
            {
                CancelPendingInput();
                return;
            }
            if (walls.Left && !_leftWallJumpBlocked)
                _lastLeftWallTime = now;
            if (walls.Right && !_rightWallJumpBlocked)
                _lastRightWallTime = now;
        }

        internal bool IsControlLocked(float now)
        {
            return isActiveAndEnabled && now < _wallJumpControlUntil;
        }

        internal bool TryJump(PlayerWallSensor.WallContacts walls, float input, float now, out Vector2 velocity)
        {
            velocity = default;
            if (!isActiveAndEnabled)
                return false;
            int side = walls.HasContact
                ? SelectSide(walls.Left && !_leftWallJumpBlocked, walls.Right && !_rightWallJumpBlocked, input)
                : SelectCoyoteSide(input, now);
            if (side == 0)
                return false;
            velocity = new Vector2(-side * Mathf.Max(0.01f, wallJumpHorizontalSpeed),
                Mathf.Max(0f, wallJumpVerticalSpeed));
            ClearWallHistory();
            _wallJumpControlUntil = now + Mathf.Max(0f, wallJumpControlLockTime);
            if (side < 0)
                _leftWallJumpBlocked = true;
            else
                _rightWallJumpBlocked = true;
            return true;
        }

        private int SelectCoyoteSide(float input, float now)
        {
            float duration = Mathf.Max(0f, wallCoyoteTime);
            if (duration <= 0f)
                return 0;
            bool left = now - _lastLeftWallTime <= duration;
            bool right = now - _lastRightWallTime <= duration;
            if (left && right && _lastLeftWallTime != _lastRightWallTime)
                return _lastLeftWallTime > _lastRightWallTime ? -1 : 1;
            return SelectSide(left, right, input);
        }

        private static int SelectSide(bool left, bool right, float input)
        {
            if (left && right)
                return input > 0f ? 1 : input < 0f ? -1 : 0;
            return left ? -1 : right ? 1 : 0;
        }

        private void ClearWallHistory()
        {
            _lastLeftWallTime = float.NegativeInfinity;
            _lastRightWallTime = float.NegativeInfinity;
        }

        internal void CancelPendingInput()
        {
            ClearWallHistory();
            CancelControlLock();
        }

        internal void CancelControlLock()
        {
            _wallJumpControlUntil = float.NegativeInfinity;
        }

        internal void ResetState()
        {
            CancelPendingInput();
            _leftWallJumpBlocked = false;
            _rightWallJumpBlocked = false;
        }

        private void OnDisable()
        {
            ResetState();
        }
    }
}
