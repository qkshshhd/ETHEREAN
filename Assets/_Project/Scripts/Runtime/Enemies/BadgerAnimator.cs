using Etherean.Combat;
using UnityEngine;
namespace Etherean.Enemies
{
    [DefaultExecutionOrder(100), DisallowMultipleComponent]
    public sealed class BadgerAnimator : MonoBehaviour
    {
        [SerializeField] private BadgerBrain brain;
        [SerializeField] private BadgerMotor motor;
        [SerializeField] private CombatHealth health;
        [SerializeField] private CombatReaction reaction;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private SpriteRenderer characterRenderer;
        [SerializeField] private bool invertSpriteFacing = true;
        private int _state;
        private void LateUpdate()
        {
            if (brain == null || motor == null || health == null || reaction == null || characterAnimator == null ||
                characterRenderer == null || !characterAnimator.isActiveAndEnabled || characterAnimator.runtimeAnimatorController == null) return;
            bool dead = health.IsDead;
            bool reacting = reaction.IsActive;
            bool attacking = brain.State == BadgerState.Windup || brain.State == BadgerState.Active || brain.State == BadgerState.Recovery;
            string name = dead ? "Death" : reacting ? "Idle" : attacking ? "Attack" :
                Mathf.Abs(motor.Velocity.x) > 0.01f && (brain.State == BadgerState.Chase || brain.State == BadgerState.Return || brain.State == BadgerState.Roam) ? "Walk" : "Idle";
            int hash = Animator.StringToHash("Base Layer." + name);
            if (!characterAnimator.HasState(0, hash)) return;
            characterAnimator.speed = (reacting || attacking) && !dead ? 0 : 1;
            if (_state != hash || reacting && !dead || attacking && !reacting && !dead)
            {
                characterAnimator.Play(hash, 0, attacking && !reacting && !dead ? brain.AttackProgress : 0);
                characterAnimator.Update(0);
                _state = hash;
            }
            if (!dead) characterRenderer.flipX = (brain.FacingDirection < 0) ^ invertSpriteFacing;
        }
        private void OnDisable() { if (characterAnimator != null) characterAnimator.speed = 1; _state = 0; }
    }
}
