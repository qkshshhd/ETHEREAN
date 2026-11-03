using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAttackHitbox : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayers = Physics2D.DefaultRaycastLayers;
        private readonly List<Collider2D> _overlaps = new List<Collider2D>(16);
        private readonly HashSet<AttackHitReceiver> _hitTargets = new HashSet<AttackHitReceiver>();
        private bool _attackOpen;
        public event Action<Vector2> HitLanded;
        internal void BeginAttack() { _hitTargets.Clear(); _attackOpen = true; }
        internal void EndAttack() { _attackOpen = false; _hitTargets.Clear(); }

        internal void Sample(PlayerAttackData data, int direction)
        {
            if (!isActiveAndEnabled || !_attackOpen || data == null) return;
            var filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(targetLayers);
            Vector2 center = (Vector2)transform.position + new Vector2(data.HitboxOffset.x * direction, data.HitboxOffset.y);
            Physics2D.OverlapBox(center, data.HitboxSize, data.HitboxAngle * direction, filter, _overlaps);
            foreach (var collider in _overlaps)
            {
                if (!_attackOpen || !isActiveAndEnabled) break;
                if (collider == null) continue;
                var target = collider.GetComponentInParent<AttackHitReceiver>();
                if (target == null || !target.isActiveAndEnabled || target.transform.IsChildOf(transform) || _hitTargets.Contains(target)) continue;
                Vector2 point = collider.ClosestPoint(center);
                if (!target.TryReceiveHit(new AttackHit(data, direction, point))) continue;
                _hitTargets.Add(target);
                HitLanded?.Invoke(point);
            }
        }
        private void OnDisable() => EndAttack();
    }
}
