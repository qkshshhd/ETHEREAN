using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public sealed class PlayerGroundSensor : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayers = Physics2D.DefaultRaycastLayers;
        [SerializeField, Min(0f)] private float groundCheckDistance = 0.026041667f;
        [SerializeField, Range(0f, 89f)] private float maxGroundAngle = 50f;

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[8];
        private readonly ContactPoint2D[] _groundContacts = new ContactPoint2D[8];
        private const float ContactSeparationSpeed = 0.05f;
        private CapsuleCollider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<CapsuleCollider2D>();
        }

        public bool CheckGrounded(Vector2 velocity)
        {
            if (!isActiveAndEnabled || _collider == null || !_collider.enabled)
                return false;

            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(groundLayers);
            filter.SetNormalAngle(90f - maxGroundAngle, 90f + maxGroundAngle);
            int contactCount = _collider.GetContacts(filter, _groundContacts);
            for (int i = 0; i < contactCount; i++)
                if (Vector2.Dot(velocity, _groundContacts[i].normal) <= ContactSeparationSpeed)
                    return true;

            return velocity.y <= 0f &&
                _collider.Cast(Vector2.down, filter, _groundHits, Mathf.Max(0f, groundCheckDistance)) > 0;
        }
    }
}
