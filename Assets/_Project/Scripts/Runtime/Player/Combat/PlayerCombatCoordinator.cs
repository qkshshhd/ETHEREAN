using UnityEngine;

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
            if (locomotion == null) locomotion = GetComponent<PlayerLocomotion>();
            if (attack == null) attack = GetComponent<PlayerAttack>();
            if (attackInput == null) attackInput = GetComponent<NewInputAttackReader>();
            locomotion.PreparingMovement += Prepare;
            locomotion.JumpStarted += OnJump;
            locomotion.ControlCancelled += ClearCombat;
            ClearCombat();
        }
        private void Prepare(PlayerLocomotion.MovementFrame frame)
        {
            attack.Tick(frame.Grounded, frame.WallSliding, frame.HorizontalInput, frame.ControlEnabled, frame.Time);
            locomotion.SetActionConstraints(attack.isActiveAndEnabled ? attack.MoveMultiplier : 1f,
                !attack.isActiveAndEnabled || attack.CanJump);
        }
        private void OnJump() => attack.CancelAttack();
        private void ClearCombat()
        {
            if (attack != null) attack.CancelAttack();
            if (attackInput != null) attackInput.ConsumePress();
            if (locomotion != null) locomotion.SetActionConstraints(1f, true);
        }
        private void OnDisable()
        {
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
