using System.Collections.Generic;
using Etherean.Combat;
using Etherean.Player;
using UnityEngine;
namespace Etherean.World
{
    [RequireComponent(typeof(BoxCollider2D)), DisallowMultipleComponent]
    public sealed class HealingPickup : MonoBehaviour
    {
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField] private bool monsterDrop;
        [SerializeField, Min(.1f)] private float despawnDistance = 16;
        private int _remaining;
        private bool _collecting, _claimed;
        private PlayerInventory _inventory;
        private CombatHealth _collectorHealth;
        private readonly HashSet<Collider2D> _contacts = new HashSet<Collider2D>();
        private readonly Plane[] _planes = new Plane[6];
        private Transform _player;
        private Camera _camera;
        private Renderer _renderer;
        private BoxCollider2D _collider;
        private float _nextLookup;
        public bool IsMonsterDrop => monsterDrop;
        private void Awake() { _remaining = Mathf.Max(1, quantity); _renderer = GetComponentInChildren<Renderer>(); _collider = GetComponent<BoxCollider2D>(); }
        private void Start()
        {
            var persistent = GetComponent<PersistentPickup>();
            if (persistent == null || monsterDrop) return;
            _remaining = Mathf.Clamp(persistent.Remaining(_remaining), 0, _remaining);
            if (_remaining == 0) { _claimed = true; _collider.enabled = false; gameObject.SetActive(false); }
        }
        public void ConfigureAsDrop(float distance)
        {
            monsterDrop = true; despawnDistance = Mathf.Max(.1f, distance); FindReferences();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            var inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null || _inventory != null && inventory != _inventory) return;
            if (_inventory == null)
            {
                Unbind(); _inventory = inventory; _collectorHealth = inventory.GetComponent<CombatHealth>(); _inventory.Changed += TryCollect;
            }
            _contacts.Add(other); TryCollect();
        }
        private void OnTriggerStay2D(Collider2D other) { OnTriggerEnter2D(other); }
        private void OnTriggerExit2D(Collider2D other)
        {
            _contacts.Remove(other); if (_contacts.Count == 0) Unbind();
        }
        private void TryCollect()
        {
            if (_claimed || _collecting || !isActiveAndEnabled || _inventory == null || !_inventory.isActiveAndEnabled ||
                _contacts.Count == 0 || _collectorHealth != null && _collectorHealth.IsDead) return;
            _contacts.RemoveWhere(contact => contact == null || !contact.enabled || !contact.gameObject.activeInHierarchy);
            bool touching = false;
            foreach (var contact in _contacts)
            {
                var distance = _collider.Distance(contact);
                if (distance.isValid && distance.distance <= .001f) { touching = true; break; }
            }
            if (!touching) return;
            _collecting = true;
            try
            {
                int accepted = _inventory.TryAdd(_remaining);
                _remaining -= accepted;
                if (accepted > 0 && !monsterDrop) GetComponent<PersistentPickup>()?.Record(_remaining);
                if (accepted > 0) Debug.Log("[Player] 회복 아이템 획득", this);
                if (_remaining <= 0)
                {
                    _claimed = true; _collider.enabled = false; Destroy(gameObject);
                }
            }
            finally { _collecting = false; }
        }
        private void FindReferences()
        {
            if (_player == null)
            {
                var inventory = FindAnyObjectByType<PlayerInventory>(); if (inventory != null) _player = inventory.transform;
            }
            if (_camera == null || !_camera.isActiveAndEnabled) _camera = Camera.main;
            _nextLookup = Time.time + 1;
        }
        private void LateUpdate()
        {
            if (!monsterDrop || _claimed) return;
            if ((_player == null || _camera == null || !_camera.isActiveAndEnabled) && Time.time >= _nextLookup) FindReferences();
            if (_player == null || _camera == null || !_camera.isActiveAndEnabled ||
                ((Vector2)(_player.position - transform.position)).sqrMagnitude < despawnDistance * despawnDistance) return;
            GeometryUtility.CalculateFrustumPlanes(_camera, _planes);
            var bounds = _renderer != null ? _renderer.bounds : GetComponent<BoxCollider2D>().bounds;
            if (!GeometryUtility.TestPlanesAABB(_planes, bounds)) Destroy(gameObject);
        }
        private void Unbind()
        {
            if (_inventory != null) _inventory.Changed -= TryCollect;
            _inventory = null; _collectorHealth = null; _contacts.Clear();
        }
        private void OnDisable() { Unbind(); }
    }
}
