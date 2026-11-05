using System.Collections.Generic;
using UnityEngine;

namespace Etherean.Enemies
{
    public enum BoarObstacle { None, Wall, Ledge }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class BoarMotor : MonoBehaviour
    {
        [SerializeField] private LayerMask terrainLayers = 1;
        [SerializeField, Min(.01f)] private float gravity = 32;
        private Rigidbody2D _body;
        private BoxCollider2D _collider;
        private readonly List<RaycastHit2D> _hits = new List<RaycastHit2D>();
        public Vector2 Velocity => _body.linearVelocity;
        public bool Grounded
        {
            get
            {
                var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(terrainLayers);
                _collider.Cast(Vector2.down, filter, _hits, .06f);
                foreach (var hit in _hits) if (hit.normal.y > .5f && Velocity.y <= 0) return true;
                return false;
            }
        }
        private void Awake() { _body = GetComponent<Rigidbody2D>(); _collider = GetComponent<BoxCollider2D>(); }
        public BoarObstacle CheckAhead(int facing, float speed)
        {
            float step = Mathf.Abs(speed) * Time.fixedDeltaTime;
            var filter = new ContactFilter2D { useTriggers = false }; filter.SetLayerMask(terrainLayers);
            _collider.Cast(Vector2.right * facing, filter, _hits, step + .03f);
            foreach (var hit in _hits) if (hit.normal.x * facing < -.5f) return BoarObstacle.Wall;
            var bounds = _collider.bounds;
            Vector2 probe = new Vector2(bounds.center.x + facing * (bounds.extents.x + step + .03f), bounds.min.y + .08f);
            return Physics2D.Raycast(probe, Vector2.down, .28f, terrainLayers) ? BoarObstacle.None : BoarObstacle.Ledge;
        }
        public void Step(float horizontal, bool launch = false, float vertical = 0)
        {
            var velocity = _body.linearVelocity;
            velocity.x = horizontal;
            velocity.y = launch ? vertical : Grounded ? 0 : Mathf.Max(-16, velocity.y - gravity * Time.fixedDeltaTime);
            _body.linearVelocity = velocity;
        }
        public void Stop() { if (_body != null) _body.linearVelocity = Vector2.zero; }
        private void OnDisable() { Stop(); }
    }
}
