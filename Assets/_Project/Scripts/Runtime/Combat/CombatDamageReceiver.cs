using Etherean.Player;
using UnityEngine;

namespace Etherean.Combat
{
    [RequireComponent(typeof(CombatHealth), typeof(CombatReaction))]
    public sealed class CombatDamageReceiver : AttackHitReceiver
    {
        [SerializeField, Min(0f)] private float invulnerabilityTime = 0.15f;
        private float _invulnerableUntil = float.NegativeInfinity;
        private CombatHealth _health;
        private CombatReaction _reaction;
        public event System.Action<AttackHit> HitReceived;
        private void Awake() { _health = GetComponent<CombatHealth>(); _reaction = GetComponent<CombatReaction>(); }
        public void ResetForRespawn(float protectionTime)
        { _invulnerableUntil = Time.fixedTime + Mathf.Max(0, protectionTime); }
        public override bool TryReceiveHit(AttackHit hit)
        {
            if (!isActiveAndEnabled || Time.fixedTime < _invulnerableUntil || !_health.ApplyDamage(hit.Damage)) return false;
            _invulnerableUntil = Time.fixedTime + invulnerabilityTime;
            if (!_health.IsDead) _reaction.Begin(hit);
            HitReceived?.Invoke(hit);
            return true;
        }
    }
}
