using UnityEngine;

namespace Etherean.Player
{
    public enum HitReaction { Stagger, Knockdown }

    [CreateAssetMenu(menuName = "ETHEREAN/Player/Attack Data")]
    public sealed class PlayerAttackData : ScriptableObject
    {
        [SerializeField, Min(0.02f)] private float windupTime = 0.24f;
        [SerializeField, Min(0.02f)] private float activeTime = 0.12f;
        [SerializeField, Min(0.02f)] private float recoveryTime = 0.4f;
        [SerializeField, Range(0f, 1f)] private float jumpCancelStart = 0.6f;
        [SerializeField, Range(0f, 1f)] private float windupMoveMultiplier = 0.35f;
        [SerializeField, Range(0f, 1f)] private float activeMoveMultiplier = 0.05f;
        [SerializeField, Min(1)] private int damage = 10;
        [SerializeField] private HitReaction reaction = HitReaction.Stagger;
        [SerializeField] private Vector2 knockback = new Vector2(1.5625f, 1.041667f);
        [SerializeField, Range(0f, 1f)] private float comboLinkStart = 0.25f;
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.520833334f, 0.208333334f);
        [SerializeField] private Vector2 hitboxSize = new Vector2(0.833333334f, 1.041667f);
        [SerializeField, Range(-180f, 180f)] private float hitboxAngle;
        [SerializeField] private string animationStateName;
        [SerializeField] private GameObject windupEffectPrefab;
        [SerializeField] private GameObject swingEffectPrefab;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField, Min(0.01f)] private float hitEffectLifetime = 1.5f;

        public float WindupTime => Mathf.Max(0.02f, windupTime);
        public float ActiveTime => Mathf.Max(0.02f, activeTime);
        public float RecoveryTime => Mathf.Max(0.02f, recoveryTime);
        public float JumpCancelStart => Mathf.Clamp01(jumpCancelStart);
        public float WindupMoveMultiplier => Mathf.Clamp01(windupMoveMultiplier);
        public float ActiveMoveMultiplier => Mathf.Clamp01(activeMoveMultiplier);
        public int Damage => Mathf.Max(1, damage);
        public HitReaction Reaction => reaction;
        public Vector2 Knockback => knockback;
        public float ComboLinkStart => Mathf.Clamp01(comboLinkStart);
        public Vector2 HitboxOffset => hitboxOffset;
        public Vector2 HitboxSize => new Vector2(Mathf.Max(0.01f, hitboxSize.x), Mathf.Max(0.01f, hitboxSize.y));
        public float HitboxAngle => hitboxAngle;
        public string AnimationStateName => animationStateName;
        public float TotalTime => WindupTime + ActiveTime + RecoveryTime;
        public GameObject WindupEffectPrefab => windupEffectPrefab;
        public GameObject SwingEffectPrefab => swingEffectPrefab;
        public GameObject HitEffectPrefab => hitEffectPrefab;
        public float HitEffectLifetime => Mathf.Max(0.01f, hitEffectLifetime);
    }
}
