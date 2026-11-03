using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class CombatTestTarget : AttackHitReceiver
    {
        [SerializeField, Min(1)] private int maxHealth = 50;
        public int Health { get; private set; }
        public int HitCount { get; private set; }
        public AttackHit LastHit { get; private set; }
        private void OnEnable()
        {
            Health = maxHealth;
            HitCount = 0;
        }
        public override bool TryReceiveHit(AttackHit hit)
        {
            if (!isActiveAndEnabled || Health <= 0) return false;
            Health = Mathf.Max(0, Health - hit.Damage);
            HitCount++;
            LastHit = hit;
            return true;
        }
    }
}
