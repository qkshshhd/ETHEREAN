using UnityEngine;
namespace Etherean.Enemies
{
    [CreateAssetMenu(menuName = "ETHEREAN/Enemies/Badger Data")]
    public sealed class BadgerData : ScriptableObject
    {
        [Min(0.1f)] [SerializeField] private float detectionRange = 4;
        public float DetectionRange => detectionRange;
        [SerializeField, Min(0.1f)] private float engageRange = 2.5f;
        public float EngageRange => Mathf.Clamp(engageRange, AttackRange, DetectionRange);
        [Min(0.1f)] [SerializeField] private float chaseLimit = 6;
        public float ChaseLimit => chaseLimit;
        [Min(0.1f)] [SerializeField] private float maximumHeightDifference = 1.25f;
        public float MaximumHeightDifference => maximumHeightDifference;
        [Min(0.1f)] [SerializeField] private float moveSpeed = 1.4f;
        public float MoveSpeed => moveSpeed;
        [Min(0.1f)] [SerializeField] private float attackRange = 0.9f;
        public float AttackRange => attackRange;
        [Min(0.02f)] [SerializeField] private float alertTime = 0.6f;
        public float AlertTime => alertTime;
        [Min(0.02f)] [SerializeField] private float windupTime = 0.35f;
        public float WindupTime => windupTime;
        [Min(0.02f)] [SerializeField] private float activeTime = 0.12f;
        public float ActiveTime => activeTime;
        [Min(0.02f)] [SerializeField] private float recoveryTime = 0.65f;
        public float RecoveryTime => recoveryTime;
        [SerializeField, Min(0.02f)] private float attackCooldown = 1.4f;
        public float AttackCooldown => Mathf.Max(0.02f, attackCooldown);
        [Min(1)] [SerializeField] private int damage = 8;
        public int Damage => damage;
        public Vector2 knockback = new Vector2(3.5f, 2.5f);
        [SerializeField] private GameObject warningEffectPrefab;
        public GameObject WarningEffectPrefab => warningEffectPrefab;
    }
}
