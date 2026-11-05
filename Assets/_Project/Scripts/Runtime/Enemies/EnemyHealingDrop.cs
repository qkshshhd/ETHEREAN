using Etherean.Combat;
using UnityEngine;
namespace Etherean.Enemies
{
    [RequireComponent(typeof(CombatHealth)), DisallowMultipleComponent]
    public sealed class EnemyHealingDrop : MonoBehaviour
    {
        [SerializeField] private HealingDropData data;
        private CombatHealth _health;
        private bool _attempted;
        private void OnEnable() { _health = GetComponent<CombatHealth>(); _health.Died += Drop; }
        private void Drop()
        {
            if (_attempted) return;
            _attempted = true;
            if (data == null || data.PickupPrefab == null || data.DropChance <= 0 || data.DropChance < 1 && Random.value >= data.DropChance) return;
            var pickup = Instantiate(data.PickupPrefab, (Vector2)transform.position + data.SpawnOffset, Quaternion.identity);
            pickup.ConfigureAsDrop(data.DespawnDistance);
        }
        private void OnDisable() { if (_health != null) _health.Died -= Drop; }
    }
}
