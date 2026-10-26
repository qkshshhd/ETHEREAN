using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public sealed class PlayerGroundSensor : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayers = Physics2D.DefaultRaycastLayers;
        [SerializeField, Min(0f)] private float groundCheckDistance = 0.05f;
        [SerializeField, Range(0f, 89f)] private float maxGroundAngle = 50f;

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[8];
        private CapsuleCollider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<CapsuleCollider2D>();
        }

        public bool CheckGrounded()
        {
            if (!isActiveAndEnabled || _collider == null || !_collider.enabled)
                return false;

            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(groundLayers);
            filter.SetNormalAngle(90f - maxGroundAngle, 90f + maxGroundAngle);
            return _collider.Cast(Vector2.down, filter, _groundHits, Mathf.Max(0f, groundCheckDistance)) > 0;
        }
    }
}
