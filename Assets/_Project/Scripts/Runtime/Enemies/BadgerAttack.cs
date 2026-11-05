using System.Collections.Generic;
using Etherean.Combat;
using Etherean.Player;
using UnityEngine;
namespace Etherean.Enemies
{
    public sealed class BadgerAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask playerLayers = 1 << 8;
        private readonly List<Collider2D> _hits = new List<Collider2D>();
        private readonly HashSet<CombatDamageReceiver> _receivers = new HashSet<CombatDamageReceiver>();
        private bool _open;
        public void Begin() { _receivers.Clear(); _open = true; }
        public void Cancel() { _open = false; _receivers.Clear(); }
        public void Sample(BadgerData data, int facing)
        {
            if (!_open || !isActiveAndEnabled) return;
            var filter = new ContactFilter2D { useTriggers = true }; filter.SetLayerMask(playerLayers);
            float reach = Mathf.Max(1f, data.AttackRange);
            Vector2 center = (Vector2)transform.position + Vector2.right * (facing * (reach + 0.1f) * 0.5f);
            Physics2D.OverlapBox(center, new Vector2(reach - 0.1f, 0.65f), 0, filter, _hits);
            foreach (var collider in _hits)
            {
                if (!_open) break;
                var target = collider.GetComponentInParent<CombatDamageReceiver>();
                if (target == null || target.GetComponent<PlayerLocomotion>() == null || _receivers.Contains(target)) continue;
                if (target.TryReceiveHit(new AttackHit(data.Damage, HitReaction.Stagger,
                    new Vector2(data.knockback.x * facing, data.knockback.y), collider.ClosestPoint(transform.position)))) _receivers.Add(target);
            }
        }
        private void OnDisable() { Cancel(); }
    }
}
