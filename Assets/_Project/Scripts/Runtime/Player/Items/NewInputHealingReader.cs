using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class NewInputHealingReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference healActionReference;
        private InputAction _ownedAction;
        private bool _requested;
        public event Action InputDisabled;
        public bool ConsumePress() { bool result = isActiveAndEnabled && _requested; _requested = false; return result; }
        private void OnEnable()
        {
            var source = healActionReference != null ? healActionReference.action : null;
            if (source == null || source.type != InputActionType.Button)
            {
                Debug.LogError("Assign a Button Heal action to Heal Action Reference.", this); enabled = false; return;
            }
            _ownedAction = source.Clone(); _ownedAction.performed += OnPerformed; _ownedAction.Enable();
        }
        private void OnPerformed(InputAction.CallbackContext context) => _requested = true;
        private void OnDisable()
        {
            if (_ownedAction != null)
            {
                _ownedAction.performed -= OnPerformed; _ownedAction.Disable(); _ownedAction.Dispose(); _ownedAction = null;
            }
            _requested = false; InputDisabled?.Invoke();
        }
    }
}
