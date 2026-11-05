using UnityEngine;

namespace Etherean.Enemies
{
    [CreateAssetMenu(menuName = "ETHEREAN/Enemies/Boar Data")]
    public sealed class BoarData : ScriptableObject
    {
        [SerializeField, Min(.1f)] private float detectionRange = 5;
        [SerializeField, Min(.1f)] private float engageRange = 4;
        [SerializeField, Min(.1f)] private float homeLimit = 7;
        [SerializeField, Min(.1f)] private float maximumHeightDifference = 1.25f;
        [SerializeField, Min(.1f)] private float returnSpeed = 1.1f;
        [SerializeField, Min(.02f)] private float alertTime = .6f;
        [SerializeField, Min(.02f)] private float windupTime = .7f;
        [SerializeField, Min(.1f)] private float chargeSpeed = 8;
        [SerializeField, Min(.02f)] private float chargeTime = .9f;
        [SerializeField, Min(.02f)] private float recoveryTime = .8f;
        [SerializeField, Min(.02f)] private float stunTime = 1.6f;
        [SerializeField, Min(.02f)] private float attackCooldown = 2.6f;
        [SerializeField, Min(1)] private int damage = 12;
        [SerializeField] private Vector2 knockback = new Vector2(5, 4);
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.05f, .65f);
        [SerializeField] private Vector2 hitboxOffset = new Vector2(.15f, 0);
        [SerializeField] private GameObject warningEffectPrefab;
        public float DetectionRange => Mathf.Max(.1f, detectionRange);
        public float EngageRange => Mathf.Clamp(engageRange, .1f, DetectionRange);
        public float HomeLimit => Mathf.Max(.1f, homeLimit);
        public float MaximumHeightDifference => Mathf.Max(.1f, maximumHeightDifference);
        public float ReturnSpeed => Mathf.Max(.1f, returnSpeed);
        public float AlertTime => Mathf.Max(.02f, alertTime);
        public float WindupTime => Mathf.Max(.02f, windupTime);
        public float ChargeSpeed => Mathf.Max(.1f, chargeSpeed);
        public float ChargeTime => Mathf.Max(.02f, chargeTime);
        public float RecoveryTime => Mathf.Max(.02f, recoveryTime);
        public float StunTime => Mathf.Max(.02f, stunTime);
        public float AttackCooldown => Mathf.Max(.02f, attackCooldown);
        public int Damage => Mathf.Max(1, damage);
        public Vector2 Knockback => knockback;
        public Vector2 HitboxSize => new Vector2(Mathf.Max(.01f, hitboxSize.x), Mathf.Max(.01f, hitboxSize.y));
        public Vector2 HitboxOffset => hitboxOffset;
        public GameObject WarningEffectPrefab => warningEffectPrefab;
    }
}
