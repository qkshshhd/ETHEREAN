using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider2D), typeof(Rigidbody2D))]
    public sealed class PlayerWallSensor : MonoBehaviour
    {
        [SerializeField] private LayerMask wallLayers = Physics2D.DefaultRaycastLayers;
        [SerializeField, Min(0f)] private float wallCheckDistance = 0.05f;
        [SerializeField, Range(0f, 89f)] private float maxWallAngle = 15f;

        private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[8];
        private CapsuleCollider2D _collider;

        public readonly struct WallContacts
        {
            public bool Left { get; }
            public bool Right { get; }
            public bool HasContact => Left || Right;

            public WallContacts(bool left, bool right)
            {
                Left = left;
                Right = right;
            }
        }

        private void Awake()
        {
            _collider = GetComponent<CapsuleCollider2D>();
        }

        public WallContacts CheckWalls()
        {
            if (!isActiveAndEnabled || _collider == null || !_collider.enabled)
                return default;

            return new WallContacts(CheckWall(Vector2.left), CheckWall(Vector2.right));
        }

        private bool CheckWall(Vector2 direction)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(wallLayers);
            int count = _collider.Cast(direction, filter, _wallHits, Mathf.Max(0f, wallCheckDistance));
            float minimumNormalAlignment = Mathf.Cos(Mathf.Clamp(maxWallAngle, 0f, 89f) * Mathf.Deg2Rad);
            for (int i = 0; i < count; i++)
            {
                Vector2 normal = _wallHits[i].normal;
                if (_wallHits[i].distance <= 0f)
                {
                    ColliderDistance2D separation = _collider.Distance(_wallHits[i].collider);
                    if (!separation.isValid)
                        continue;
                    if (separation.isOverlapped)
                        normal = -separation.normal;
                }
                if (Vector2.Dot(normal, -direction) >= minimumNormalAlignment)
                    return true;
            }
            return false;
        }
    }
}