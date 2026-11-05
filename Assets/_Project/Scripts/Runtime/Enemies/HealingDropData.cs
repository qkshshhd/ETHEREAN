using Etherean.World;
using UnityEngine;
namespace Etherean.Enemies
{
    [CreateAssetMenu(menuName = "ETHEREAN/Enemies/Healing Drop")]
    public sealed class HealingDropData : ScriptableObject
    {
        [SerializeField] private HealingPickup pickupPrefab;
        [SerializeField, Range(0, 1)] private float dropChance = .35f;
        [SerializeField] private Vector2 spawnOffset = new Vector2(0, .1f);
        [SerializeField, Min(.1f)] private float despawnDistance = 16;
        public HealingPickup PickupPrefab => pickupPrefab;
        public float DropChance => Mathf.Clamp01(dropChance);
        public Vector2 SpawnOffset => spawnOffset;
        public float DespawnDistance => Mathf.Max(.1f, despawnDistance);
    }
}
