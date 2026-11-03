using UnityEngine;

namespace Etherean.Player
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PlayerCharacterAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerLocomotion locomotion;
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private PlayerAttackAnimator attackAnimator;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private SpriteRenderer characterRenderer;
        [SerializeField, Min(0f)] private float idleSpeedThreshold = 0.052083334f;
        [SerializeField, Min(0.01f)] private float runSpeedThreshold = 1.5625f;
        [SerializeField, Min(0.01f)] private float playbackSpeed = 1f;
        private int _state;

        private void LateUpdate()
        {
            if (movement == null || locomotion == null || characterAnimator == null || characterRenderer == null ||
                !characterAnimator.isActiveAndEnabled || characterAnimator.runtimeAnimatorController == null) return;
            float horizontalSpeed = movement.Velocity.x;
            if (attack != null && attack.IsAttacking)
                characterRenderer.flipX = attack.FacingDirection < 0;
            else if (Mathf.Abs(horizontalSpeed) > idleSpeedThreshold)
                characterRenderer.flipX = horizontalSpeed < 0f;
            if (attackAnimator != null && attackAnimator.isActiveAndEnabled && attackAnimator.IsDrivingAnimation)
            {
                _state = 0;
                return;
            }
            bool hold = !movement.isActiveAndEnabled || !locomotion.isActiveAndEnabled || !locomotion.IsGrounded ||
                attack != null && attack.IsAttacking;
            float speed = Mathf.Abs(horizontalSpeed);
            string state = hold || speed <= idleSpeedThreshold ? "Base Layer.Idle" :
                speed < runSpeedThreshold ? "Base Layer.Walk" : "Base Layer.Run";
            int hash = Animator.StringToHash(state);
            if (!characterAnimator.HasState(0, hash)) return;
            characterAnimator.speed = hold ? 0f : playbackSpeed;
            if (_state != hash || hold)
            {
                characterAnimator.Play(hash, 0, 0f);
                characterAnimator.Update(0f);
                _state = hash;
            }
        }

        private void OnDisable()
        {
            if (characterAnimator != null) characterAnimator.speed = 1f;
            _state = 0;
        }
    }
}
