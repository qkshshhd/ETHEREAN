using System.Collections.Generic;
using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAttackEffects : MonoBehaviour
    {
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private PlayerAttackHitbox hitbox;
        [SerializeField] private Transform effectOrigin;
        private GameObject _phaseEffect;
        private readonly List<GameObject> _impacts = new List<GameObject>();
        private void OnEnable()
        {
            if (attack == null) attack = GetComponent<PlayerAttack>();
            if (hitbox == null) hitbox = GetComponent<PlayerAttackHitbox>();
            if (effectOrigin == null) effectOrigin = transform;
            if (attack != null) attack.PhaseChanged += OnPhaseChanged;
            if (hitbox != null) hitbox.HitLanded += OnHit;
        }
        private void OnPhaseChanged()
        {
            if (_phaseEffect != null) Destroy(_phaseEffect);
            _phaseEffect = null;
            if (attack == null || attack.CurrentData == null) return;
            GameObject prefab = attack.Phase == AttackPhase.Windup ? attack.CurrentData.WindupEffectPrefab :
                attack.Phase == AttackPhase.Active ? attack.CurrentData.SwingEffectPrefab : null;
            if (prefab != null)
            {
                _phaseEffect = Instantiate(prefab, effectOrigin, false);
                Vector3 position = _phaseEffect.transform.localPosition;
                position.x *= attack.FacingDirection;
                _phaseEffect.transform.localPosition = position;
                Vector3 scale = _phaseEffect.transform.localScale;
                scale.x *= attack.FacingDirection;
                _phaseEffect.transform.localScale = scale;
            }
        }
        private void OnHit(Vector2 point)
        {
            var data = attack != null ? attack.CurrentData : null;
            if (data == null || data.HitEffectPrefab == null) return;
            _impacts.RemoveAll(item => item == null);
            var instance = Instantiate(data.HitEffectPrefab, point, Quaternion.identity);
            Vector3 scale = instance.transform.localScale;
            scale.x *= attack.FacingDirection;
            instance.transform.localScale = scale;
            _impacts.Add(instance);
            Destroy(instance, data.HitEffectLifetime);
        }
        private void OnDisable()
        {
            if (attack != null) attack.PhaseChanged -= OnPhaseChanged;
            if (hitbox != null) hitbox.HitLanded -= OnHit;
            if (_phaseEffect != null) Destroy(_phaseEffect);
            foreach (var instance in _impacts) if (instance != null) Destroy(instance);
            _impacts.Clear();
        }
    }
}
