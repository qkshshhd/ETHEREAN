using UnityEngine;
namespace Etherean.Player
{
    [CreateAssetMenu(menuName = "ETHEREAN/Player/Healing Item")]
    public sealed class HealingItemData : ScriptableObject
    {
        [SerializeField, Min(1)] private int maximumCount = 3;
        [SerializeField, Min(1)] private int healAmount = 20;
        [SerializeField, Min(.02f)] private float useTime = 1.2f;
        [SerializeField, Range(0, 1)] private float moveMultiplier = .35f;
        [SerializeField] private GameObject healingEffectPrefab;
        public int MaximumCount => Mathf.Max(1, maximumCount);
        public int HealAmount => Mathf.Max(1, healAmount);
        public float UseTime => Mathf.Max(.02f, useTime);
        public float MoveMultiplier => Mathf.Clamp01(moveMultiplier);
        public GameObject HealingEffectPrefab => healingEffectPrefab;
    }
}
