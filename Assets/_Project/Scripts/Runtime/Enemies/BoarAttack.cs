using System.Collections.Generic;
using Etherean.Combat;
using Etherean.Player;
using UnityEngine;

namespace Etherean.Enemies
{
    public sealed class BoarAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask playerLayers = 1 << 8;
        private readonly List<Collider2D> _hits = new List<Collider2D>();
        private readonly HashSet<CombatDamageReceiver> _receivers = new HashSet<CombatDamageReceiver>();
        private Vector2 _previousPosition;
        public bool IsOpen { get; private set; }
        public void Begin() { _receivers.Clear(); _previousPosition = transform.position; IsOpen = true; }
        public void Cancel() { IsOpen = false; _receivers.Clear(); }
        public void Sample(BoarData data, int facing)
        {
            if (!IsOpen || !isActiveAndEnabled) return;
            Vector2 position = transform.position, delta = position - _previousPosition;
            Vector2 center = (position + _previousPosition) * .5f + new Vector2(data.HitboxOffset.x * facing, data.HitboxOffset.y);
            Vector2 size = data.HitboxSize + new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            _previousPosition = position;
            var filter = new ContactFilter2D { useTriggers = true }; filter.SetLayerMask(playerLayers);
            // Sweep the previous physics step as a box so a fast, non-colliding charge cannot skip the player.
            Physics2D.OverlapBox(center, size, 0, filter, _hits);
            foreach (var collider in _hits)
            {
                if (!IsOpen) break;
                var receiver = collider.GetComponentInParent<CombatDamageReceiver>();
                if (receiver == null || receiver.GetComponent<PlayerLocomotion>() == null || _receivers.Contains(receiver)) continue;
                if (receiver.TryReceiveHit(new AttackHit(data.Damage, HitReaction.Knockdown,
                    new Vector2(data.Knockback.x * facing, data.Knockback.y), collider.ClosestPoint(position)))) _receivers.Add(receiver);
            }
        }
        private void OnDisable() { Cancel(); }
    }
}
