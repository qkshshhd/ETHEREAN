using UnityEngine;
using UnityEngine.InputSystem;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class NewInputMovementReader : PlayerMovementInput
    {
        [SerializeField] private InputActionReference moveActionReference;
        [SerializeField] private InputActionReference jumpActionReference;

        private InputAction _ownedMoveAction;
        private InputAction _ownedJumpAction;
        private bool _jumpRequested;
        private bool _jumpReleaseRequested;

        public override float HorizontalInput => isActiveAndEnabled && _ownedMoveAction != null
            ? Mathf.Clamp(_ownedMoveAction.ReadValue<Vector2>().x, -1f, 1f)
            : 0f;

        public override bool IsJumpHeld => isActiveAndEnabled && _ownedJumpAction != null &&
            _ownedJumpAction.IsPressed();

        public override bool ConsumeJumpPress()
        {
            bool requested = isActiveAndEnabled && _jumpRequested;
            _jumpRequested = false;
            return requested;
        }

        public override bool ConsumeJumpRelease()
        {
            bool requested = isActiveAndEnabled && _jumpReleaseRequested;
            _jumpReleaseRequested = false;
            return requested;
        }

        private void OnEnable()
        {
            InputAction sourceMoveAction = moveActionReference != null ? moveActionReference.action : null;
            if (sourceMoveAction == null || sourceMoveAction.type != InputActionType.Value ||
                sourceMoveAction.expectedControlType != "Vector2")
            {
                Debug.LogError("Assign a Value / Vector2 Move action to Move Action Reference.", this);
                enabled = false;
                return;
            }

            InputAction sourceJumpAction = jumpActionReference != null ? jumpActionReference.action : null;
            if (sourceJumpAction == null || sourceJumpAction.type != InputActionType.Button)
            {
                Debug.LogError("Assign a Button Jump action to Jump Action Reference.", this);
                enabled = false;
                return;
            }

            _ownedMoveAction = sourceMoveAction.Clone();
            _ownedJumpAction = sourceJumpAction.Clone();
            _ownedJumpAction.performed += OnJumpPerformed;
            _ownedJumpAction.canceled += OnJumpCanceled;
            _ownedMoveAction.Enable();
            _ownedJumpAction.Enable();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _jumpRequested = true;
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            _jumpReleaseRequested = true;
        }

        private void OnDisable()
        {
            _ownedMoveAction?.Disable();
            _ownedMoveAction?.Dispose();
            _ownedMoveAction = null;
            if (_ownedJumpAction != null)
            {
                _ownedJumpAction.performed -= OnJumpPerformed;
                _ownedJumpAction.canceled -= OnJumpCanceled;
                _ownedJumpAction.Disable();
                _ownedJumpAction.Dispose();
                _ownedJumpAction = null;
            }
            _jumpRequested = false;
            _jumpReleaseRequested = false;
            NotifyInputDisabled();
        }
    }
}
