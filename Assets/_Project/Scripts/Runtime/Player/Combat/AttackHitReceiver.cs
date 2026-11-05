using UnityEngine;

namespace Etherean.Player
{
    public readonly struct AttackHit
    {
        public readonly int Damage;
        public readonly HitReaction Reaction;
        public readonly Vector2 Knockback;
        public readonly Vector2 Point;
        public AttackHit(int damage, HitReaction reaction, Vector2 knockback, Vector2 point)
        {
            Damage = damage; Reaction = reaction; Knockback = knockback; Point = point;
        }
        public AttackHit(PlayerAttackData data, int direction, Vector2 point)
        {
            Damage = data.Damage;
            Reaction = data.Reaction;
            Knockback = new Vector2(data.Knockback.x * direction, data.Knockback.y);
            Point = point;
        }
    }

    public abstract class AttackHitReceiver : MonoBehaviour
    {
        public abstract bool TryReceiveHit(AttackHit hit);
    }
}
