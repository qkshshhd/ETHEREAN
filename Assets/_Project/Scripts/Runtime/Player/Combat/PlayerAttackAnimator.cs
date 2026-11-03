using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAttackAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private Animator characterAnimator;
        [SerializeField, Min(0)] private int layer;
        [SerializeField] private string returnStateName;
        private bool _driving;
        public bool IsDrivingAnimation => _driving;

        private void OnEnable()
        {
            if (attack == null) attack = GetComponent<PlayerAttack>();
            if (attack != null) attack.PhaseChanged += OnPhaseChanged;
        }
        private void OnPhaseChanged()
        {
            if (attack == null || !attack.IsAttacking) ReleaseAnimation();
        }
        private void LateUpdate()
        {
            if (attack == null || !attack.IsAttacking || characterAnimator == null ||
                !characterAnimator.isActiveAndEnabled || characterAnimator.runtimeAnimatorController == null ||
                layer >= characterAnimator.layerCount) return;
            string state = attack.CurrentData.AnimationStateName;
            if (string.IsNullOrEmpty(state)) return;
            int hash = Animator.StringToHash(state);
            if (!characterAnimator.HasState(layer, hash)) return;
            characterAnimator.Play(hash, layer, attack.AttackProgress);
            characterAnimator.Update(0f);
            _driving = true;
        }
        private void ReleaseAnimation()
        {
            if (_driving && characterAnimator != null && characterAnimator.isActiveAndEnabled &&
                characterAnimator.runtimeAnimatorController != null && layer < characterAnimator.layerCount &&
                !string.IsNullOrEmpty(returnStateName))
            {
                int hash = Animator.StringToHash(returnStateName);
                if (characterAnimator.HasState(layer, hash)) characterAnimator.Play(hash, layer, 0f);
            }
            _driving = false;
        }
        private void OnDisable()
        {
            if (attack != null) attack.PhaseChanged -= OnPhaseChanged;
            ReleaseAnimation();
        }
    }
}
