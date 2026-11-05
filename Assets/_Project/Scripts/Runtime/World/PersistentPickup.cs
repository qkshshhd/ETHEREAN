using Etherean.Persistence;
using UnityEngine;

namespace Etherean.World
{
    [DisallowMultipleComponent, RequireComponent(typeof(HealingPickup))]
    public sealed class PersistentPickup : MonoBehaviour
    {
        [SerializeField] private string persistentId;
        public string Id => persistentId;
        public int Remaining(int initial)
        {
            var session = CheckpointSession.InScene(gameObject.scene);
            return session != null ? session.Progress.PickupRemaining(Id, initial) : initial;
        }
        public void Record(int remaining)
        {
            var session = CheckpointSession.InScene(gameObject.scene);
            if (session != null) session.Progress.RecordPickup(Id, remaining);
        }
    }
}
