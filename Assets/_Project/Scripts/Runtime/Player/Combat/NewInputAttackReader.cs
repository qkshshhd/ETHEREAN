using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class NewInputAttackReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference attackActionReference;
        private InputAction _ownedAction;
        private bool _requested;
        public event Action InputDisabled;

        public bool ConsumePress()
        {
            bool result = isActiveAndEnabled && _requested;
            _requested = false;
            return result;
        }

        private void OnEnable()
        {
            var source = attackActionReference != null ? attackActionReference.action : null;
            if (source == null || source.type != InputActionType.Button)
            {
                Debug.LogError("Assign a Button Attack action to Attack Action Reference.", this);
                enabled = false;
                return;
            }
            _ownedAction = source.Clone();
            _ownedAction.performed += OnPerformed;
            _ownedAction.Enable();
        }

        private void OnPerformed(InputAction.CallbackContext context) => _requested = true;

        private void OnDisable()
        {
            if (_ownedAction != null)
            {
                _ownedAction.performed -= OnPerformed;
                _ownedAction.Disable();
                _ownedAction.Dispose();
                _ownedAction = null;
            }
            _requested = false;
            InputDisabled?.Invoke();
        }
    }
}
