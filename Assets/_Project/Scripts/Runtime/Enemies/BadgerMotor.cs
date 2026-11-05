using System.Collections.Generic;
using UnityEngine;
namespace Etherean.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class BadgerMotor : MonoBehaviour
    {
        [SerializeField] private LayerMask terrainLayers = 1;
        [SerializeField, Min(0.01f)] private float gravity = 32;
        private Rigidbody2D _body;
        private BoxCollider2D _collider;
        private readonly List<RaycastHit2D> _hits = new List<RaycastHit2D>();
        public Vector2 Velocity => _body.linearVelocity;
        public bool Grounded => Velocity.y <= 0 && Cast(Vector2.down, 0.05f);
        private void Awake() { _body = GetComponent<Rigidbody2D>(); _collider = GetComponent<BoxCollider2D>(); }
        private bool Cast(Vector2 direction, float distance)
        {
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(terrainLayers);
            _collider.Cast(direction, filter, _hits, distance);
            return _hits.Count > 0;
        }
        public bool CanMove(int direction)
        {
            if (Cast(Vector2.right * direction, 0.06f)) return false;
            Vector2 probe = new Vector2(_collider.bounds.center.x + direction * (_collider.bounds.extents.x + 0.10f), _collider.bounds.min.y + 0.05f);
            return Physics2D.Raycast(probe, Vector2.down, 0.3f, terrainLayers);
        }
        public void Step(float horizontal, bool launch = false, float vertical = 0)
        {
            var velocity = _body.linearVelocity;
            velocity.x = horizontal;
            velocity.y = launch ? vertical : Grounded ? 0 : Mathf.Max(-16, velocity.y - gravity * Time.fixedDeltaTime);
            _body.linearVelocity = velocity;
        }
        private void OnDisable() { if (_body != null) _body.linearVelocity = Vector2.zero; }
    }
}
