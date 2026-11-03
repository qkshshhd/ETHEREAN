using System;
using UnityEngine;

namespace Etherean.Player
{
    public enum AttackPhase { None, Windup, Active, Recovery }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(NewInputAttackReader), typeof(PlayerAttackHitbox))]
    public sealed class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private NewInputAttackReader attackInput;
        [SerializeField] private PlayerAttackHitbox hitbox;
        [SerializeField] private PlayerAttackData[] groundCombo;
        [SerializeField] private PlayerAttackData[] airCombo;
        [SerializeField] private bool weaponUnlocked;
        private PlayerAttackData[] _sequence;
        private PlayerAttackData _currentData;
        private int _comboIndex;
        private bool _comboQueued;
        private float _phaseStarted;
        private float _now;

        public event Action PhaseChanged;
        public AttackPhase Phase { get; private set; }
        public bool IsAttacking => Phase != AttackPhase.None;
        public int ComboStep => IsAttacking ? _comboIndex + 1 : 0;
        public bool ComboQueued => _comboQueued;
        public int FacingDirection { get; private set; } = 1;
        public bool StartedInAir { get; private set; }
        public bool WeaponUnlocked => weaponUnlocked;
        public PlayerAttackData CurrentData => _currentData;
        public float PhaseProgress => _currentData == null ? 0f : Mathf.Clamp01((_now - _phaseStarted) / PhaseDuration);
        public float AttackProgress => _currentData == null ? 0f : Mathf.Clamp01(
            ((Phase == AttackPhase.Windup ? 0f : _currentData.WindupTime) +
             (Phase == AttackPhase.Recovery ? _currentData.ActiveTime : 0f) +
             PhaseProgress * PhaseDuration) / _currentData.TotalTime);
        public bool CanJump => !IsAttacking || Phase == AttackPhase.Recovery && PhaseProgress >= _currentData.JumpCancelStart;
        public float MoveMultiplier => !IsAttacking ? 1f : Phase == AttackPhase.Windup ? _currentData.WindupMoveMultiplier :
            Phase == AttackPhase.Active ? _currentData.ActiveMoveMultiplier : Mathf.Lerp(_currentData.ActiveMoveMultiplier, 1f, PhaseProgress);
        private float PhaseDuration => Phase == AttackPhase.Windup ? _currentData.WindupTime :
            Phase == AttackPhase.Active ? _currentData.ActiveTime : _currentData.RecoveryTime;

        private void Reset() => ResolveReferences();
        private void ResolveReferences()
        {
            if (attackInput == null) attackInput = GetComponent<NewInputAttackReader>();
            if (hitbox == null) hitbox = GetComponent<PlayerAttackHitbox>();
        }
        private void OnEnable()
        {
            ResolveReferences();
            if (attackInput != null) attackInput.InputDisabled += CancelAttack;
        }
        public void SetWeaponUnlocked(bool unlocked)
        {
            weaponUnlocked = unlocked;
            if (!unlocked) CancelAttack();
        }

        internal void Tick(bool grounded, bool wallSliding, float horizontalInput, bool controlEnabled, float now)
        {
            _now = now;
            bool pressed = attackInput != null && attackInput.ConsumePress();
            if (!isActiveAndEnabled || !controlEnabled || attackInput == null || !attackInput.isActiveAndEnabled ||
                hitbox == null || !hitbox.isActiveAndEnabled ||
                !weaponUnlocked || IsAttacking && _currentData == null)
            {
                CancelAttack();
                return;
            }
            bool wasAttacking = IsAttacking;
            if (IsAttacking)
            {
                if (pressed && Phase != AttackPhase.Windup && _comboIndex + 1 < _sequence.Length) _comboQueued = true;
                while (IsAttacking && now - _phaseStarted >= PhaseDuration) AdvancePhase();
                if (Phase == AttackPhase.Recovery && _comboQueued && PhaseProgress >= _currentData.ComboLinkStart)
                    StartStep(_comboIndex + 1);
            }
            if (!wasAttacking)
            {
                if (Mathf.Abs(horizontalInput) > 0.01f) FacingDirection = horizontalInput > 0f ? 1 : -1;
                if (pressed && !wallSliding)
                {
                    _sequence = grounded ? groundCombo : airCombo;
                    StartedInAir = !grounded;
                    if (_sequence != null && _sequence.Length > 0) StartStep(0);
                }
            }
            if (!IsAttacking) return;
            if (Phase == AttackPhase.Active) hitbox.Sample(_currentData, FacingDirection);
        }
        private void AdvancePhase()
        {
            float nextStart = _phaseStarted + PhaseDuration;
            if (Phase == AttackPhase.Windup) EnterPhase(AttackPhase.Active, nextStart);
            else if (Phase == AttackPhase.Active) EnterPhase(AttackPhase.Recovery, nextStart);
            else CancelAttack();
        }
        private void StartStep(int index)
        {
            if (index >= _sequence.Length || _sequence[index] == null) { CancelAttack(); return; }
            _comboIndex = index;
            _currentData = _sequence[index];
            _comboQueued = false;
            hitbox.BeginAttack();
            EnterPhase(AttackPhase.Windup, _now);
        }
        private void EnterPhase(AttackPhase phase, float started)
        {
            Phase = phase;
            _phaseStarted = started;
            PhaseChanged?.Invoke();
        }
        public void CancelAttack()
        {
            bool changed = IsAttacking;
            Phase = AttackPhase.None;
            _currentData = null;
            _sequence = null;
            _comboQueued = false;
            if (hitbox != null) hitbox.EndAttack();
            if (changed) PhaseChanged?.Invoke();
        }
        private void OnDisable()
        {
            if (attackInput != null) attackInput.InputDisabled -= CancelAttack;
            CancelAttack();
            if (attackInput != null) attackInput.ConsumePress();
        }
    }
}
