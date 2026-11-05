using UnityEngine;

namespace Etherean.World
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private string checkpointId;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform seatPoint;
        private BoxCollider2D _interactionArea;
        public string Id => checkpointId;
        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;
        public Vector3 SeatPosition => seatPoint != null ? seatPoint.position : SpawnPosition;

        public bool IsInReach(Collider2D visitor)
        {
            if (!isActiveAndEnabled || visitor == null || !visitor.isActiveAndEnabled) return false;
            if (_interactionArea == null) _interactionArea = GetComponent<BoxCollider2D>();
            if (!_interactionArea.enabled) return false;
            var distance = _interactionArea.Distance(visitor);
            return distance.isValid && distance.distance <= 0;
        }
    }
}
