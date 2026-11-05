using Etherean.Combat;
using Etherean.Enemies;
using Etherean.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Etherean.Persistence
{
    public static class CheckpointWorldReset
    {
        public static void Reset(Scene scene, CombatHealth player)
        {
            foreach (var pickup in Object.FindObjectsByType<HealingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (pickup.gameObject.scene == scene && pickup.IsMonsterDrop)
                { pickup.gameObject.SetActive(false); Object.Destroy(pickup.gameObject); }
            foreach (var anchor in Object.FindObjectsByType<EnemySpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (anchor.gameObject.scene == scene && anchor.isActiveAndEnabled) anchor.Respawn(player);
        }
    }
}
