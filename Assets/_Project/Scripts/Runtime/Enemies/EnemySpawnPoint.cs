using Etherean.Combat;
using Etherean.Persistence;
using UnityEngine;

namespace Etherean.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        private GameObject _instance;
        public GameObject Instance => _instance;
        private void Start()
        {
            var session = CheckpointSession.InScene(gameObject.scene);
            if (_instance == null && (session == null || session.IsReady)) Respawn(session != null ? session.PlayerHealth : null);
        }
        private void OnEnable()
        {
            var session = CheckpointSession.InScene(gameObject.scene);
            if (session != null && session.IsReady) Respawn(session.PlayerHealth);
        }
        public void Respawn(CombatHealth target)
        {
            Clear();
            if (!isActiveAndEnabled || enemyPrefab == null) return;
            _instance = Instantiate(enemyPrefab, transform.position, transform.rotation, transform);
            _instance.GetComponent<BadgerBrain>()?.SetTarget(target);
            _instance.GetComponent<BoarBrain>()?.SetTarget(target);
        }
        private void Clear()
        { if (_instance != null) { _instance.SetActive(false); Destroy(_instance); _instance = null; } }
        private void OnDisable() { Clear(); }
    }
}
