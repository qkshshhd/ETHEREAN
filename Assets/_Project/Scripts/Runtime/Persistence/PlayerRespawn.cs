using System.Collections;
using Etherean.Combat;
using Etherean.Player;
using UnityEngine;

namespace Etherean.Persistence
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatHealth), typeof(PlayerMovement))]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        [SerializeField, Min(0)] private float deathDelay = 1f;
        [SerializeField, Min(0)] private float spawnProtection = 1f;
        private CheckpointSession _session;
        private CombatHealth _health;
        private bool _pending;
        public void Initialize(CheckpointSession session) { _session = session; }
        private void OnEnable() { _health = GetComponent<CombatHealth>(); _health.Died += OnDeath; }
        private void OnDeath()
        { if (_session != null && _session.IsReady && !_pending) { _pending = true; StartCoroutine(ReturnAfterDeath()); } }
        private IEnumerator ReturnAfterDeath()
        {
            yield return null;
            if (deathDelay > 0) yield return new WaitForSeconds(deathDelay);
            _pending = false; _session.RespawnPlayer();
        }
        public void RestoreAt(Vector3 position)
        {
            _pending = false;
            gameObject.SetActive(false);
            GetComponent<PlayerMovement>().Teleport(position);
            _health.RestoreFull();
            GetComponent<CombatDamageReceiver>().ResetForRespawn(spawnProtection);
            gameObject.SetActive(true);
        }
        public void Rest()
        {
            GetComponent<PlayerHealing>()?.Cancel(); GetComponent<PlayerAttack>()?.CancelAttack();
            GetComponent<CombatReaction>().ResetState();
            GetComponent<PlayerLocomotion>().ResetForRespawn();
            GetComponent<PlayerMovement>().Teleport(transform.position);
            _health.RestoreFull(); GetComponent<CombatDamageReceiver>().ResetForRespawn(spawnProtection);
        }
        private void OnDisable()
        { StopAllCoroutines(); if (_health != null) _health.Died -= OnDeath; _pending = false; }
    }
}
