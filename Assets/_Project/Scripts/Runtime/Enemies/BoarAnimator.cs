using Etherean.Combat;
using UnityEngine;

namespace Etherean.Enemies
{
    [DefaultExecutionOrder(100), DisallowMultipleComponent]
    public sealed class BoarAnimator : MonoBehaviour
    {
        [SerializeField] private BoarBrain brain;
        [SerializeField] private BoarMotor motor;
        [SerializeField] private CombatHealth health;
        [SerializeField] private CombatReaction reaction;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private SpriteRenderer characterRenderer;
        [SerializeField] private bool invertSpriteFacing;
        [SerializeField] private string idleState = "Base Layer.Idle";
        [SerializeField] private string walkState = "Base Layer.Walk";
        [SerializeField] private string alertState = "Base Layer.Alert";
        [SerializeField] private string windupState = "Base Layer.Windup";
        [SerializeField] private string chargeState = "Base Layer.Charge";
        [SerializeField] private string recoveryState = "Base Layer.Recovery";
        [SerializeField] private string stunState = "Base Layer.Stun";
        [SerializeField] private string staggerState = "Base Layer.Stagger";
        [SerializeField] private string launchState = "Base Layer.KnockdownLaunch";
        [SerializeField] private string downState = "Base Layer.KnockdownDown";
        [SerializeField] private string getUpState = "Base Layer.GetUp";
        [SerializeField] private string deathState = "Base Layer.Death";
        private int _state;
        private void LateUpdate()
        {
            if (brain == null || motor == null || health == null || reaction == null || characterAnimator == null || characterRenderer == null ||
                !characterAnimator.isActiveAndEnabled || characterAnimator.runtimeAnimatorController == null) return;
            string name = health.IsDead ? deathState : reaction.IsActive ? ReactionState() :
                brain.State == BoarState.Alert ? alertState : brain.State == BoarState.Windup ? windupState :
                brain.State == BoarState.Charge ? chargeState : brain.State == BoarState.Recovery ? recoveryState :
                brain.State == BoarState.Stun ? stunState : Mathf.Abs(motor.Velocity.x) > .01f ? walkState : idleState;
            bool hold = !health.IsDead && (reaction.IsActive || brain.State == BoarState.Windup || brain.State == BoarState.Stun);
            int hash = Animator.StringToHash(name);
            if (!characterAnimator.HasState(0, hash)) { hash = Animator.StringToHash(idleState); hold = true; }
            if (!characterAnimator.HasState(0, hash)) return;
            characterAnimator.speed = hold ? 0 : 1;
            if (_state != hash || hold)
            {
                characterAnimator.Play(hash, 0, 0); characterAnimator.Update(0); _state = hash;
            }
            if (!health.IsDead) characterRenderer.flipX = (brain.FacingDirection < 0) ^ invertSpriteFacing;
        }
        private string ReactionState() => reaction.Phase == ReactionPhase.Launch ? launchState :
            reaction.Phase == ReactionPhase.Down ? downState : reaction.Phase == ReactionPhase.GetUp ? getUpState : staggerState;
        private void OnDisable() { if (characterAnimator != null) characterAnimator.speed = 1; _state = 0; }
    }
}
