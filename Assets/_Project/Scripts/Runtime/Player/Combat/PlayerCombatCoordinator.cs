using UnityEngine;
using Etherean.Combat;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerLocomotion), typeof(PlayerAttack))]
    public sealed class PlayerCombatCoordinator : MonoBehaviour
    {
        [SerializeField] private PlayerLocomotion locomotion;
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private NewInputAttackReader attackInput;
        private void OnEnable()
        {
            _healing = GetComponent<PlayerHealing>();
            if (_healing != null) _healing.Ended += OnHealingEnded;
            _health = GetComponent<CombatHealth>(); _reaction = GetComponent<CombatReaction>();
            if (_health != null) _health.Died += ClearCombat;
            if (_reaction != null) _reaction.Started += ClearCombat;
            if (locomotion == null) locomotion = GetComponent<PlayerLocomotion>();
            if (attack == null) attack = GetComponent<PlayerAttack>();
            if (attackInput == null) attackInput = GetComponent<NewInputAttackReader>();
            locomotion.PreparingMovement += Prepare;
            locomotion.JumpStarted += OnJump;
            locomotion.ControlCancelled += ClearCombat;
            ClearCombat();
        }
        private CombatHealth _health;
        private CombatReaction _reaction;
        private PlayerHealing _healing;
        private void Prepare(PlayerLocomotion.MovementFrame frame)
        {
            if (_reaction != null) _reaction.Tick(frame.Grounded);
            if (_health != null && _health.IsDead || _reaction != null && _reaction.IsActive)
            {
                ClearCombat();
                bool launches = false;
                Vector2 motion = _health != null && _health.IsDead ? Vector2.zero : _reaction.TakeMotion(out launches);
                locomotion.SetExternalMotion(motion, launches);
                return;
            }
            if (_healing != null && _healing.isActiveAndEnabled)
            {
                bool wasHealing = _healing.IsHealing;
                _healing.Tick(!attack.IsAttacking, frame.Grounded, frame.WallSliding, frame.ControlEnabled, frame.Time);
                if (wasHealing || _healing.IsHealing)
                {
                    attack.CancelAttack();
                    if (attackInput != null) attackInput.ConsumePress();
                    locomotion.DiscardJumpRequests();
                    locomotion.SetActionConstraints(_healing.IsHealing ? _healing.MoveMultiplier : 1, !_healing.IsHealing);
                    return;
                }
            }
            attack.Tick(frame.Grounded, frame.WallSliding, frame.HorizontalInput, frame.ControlEnabled, frame.Time);
            locomotion.SetActionConstraints(attack.isActiveAndEnabled ? attack.MoveMultiplier : 1f,
                !attack.isActiveAndEnabled || attack.CanJump);
        }
        private void OnHealingEnded()
        {
            if (attackInput != null) attackInput.ConsumePress();
            if (locomotion != null) locomotion.DiscardJumpRequests();
        }
        private void OnJump() => attack.CancelAttack();
        private void ClearCombat()
        {
            if (_healing != null) _healing.Cancel();
            if (attack != null) attack.CancelAttack();
            if (attackInput != null) attackInput.ConsumePress();
            if (locomotion != null) locomotion.SetActionConstraints(1f, true);
        }
        private void OnDisable()
        {
            if (_healing != null) _healing.Ended -= OnHealingEnded;
            if (_health != null) _health.Died -= ClearCombat;
            if (_reaction != null) _reaction.Started -= ClearCombat;
            if (locomotion != null)
            {
                locomotion.PreparingMovement -= Prepare;
                locomotion.JumpStarted -= OnJump;
                locomotion.ControlCancelled -= ClearCombat;
            }
            ClearCombat();
        }
    }
}
