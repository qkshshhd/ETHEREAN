using Etherean.Combat;
using UnityEngine;
namespace Etherean.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatDamageReceiver), typeof(PlayerInventory), typeof(NewInputHealingReader))]
    public sealed class PlayerHealing : MonoBehaviour
    {
        [SerializeField] private HealingItemData data;
        [SerializeField] private bool isHealing;
        private PlayerInventory _inventory;
        private CombatHealth _health;
        private CombatDamageReceiver _receiver;
        private NewInputHealingReader _input;
        private float _started, _now;
        private bool _completionPending;
        private GameObject _effect;
        public bool IsHealing => isActiveAndEnabled && isHealing;
        public event System.Action Ended;
        public float MoveMultiplier => data != null ? data.MoveMultiplier : 1;
        public float Progress => IsHealing && data != null ? Mathf.Clamp01((_now - _started) / data.UseTime) : 0;
        private void OnEnable()
        {
            _inventory = GetComponent<PlayerInventory>(); _health = GetComponent<CombatHealth>();
            _receiver = GetComponent<CombatDamageReceiver>(); _input = GetComponent<NewInputHealingReader>();
            _receiver.HitReceived += OnHit; _health.Died += Cancel; _input.InputDisabled += Cancel;
        }
        private void OnHit(AttackHit hit) { Cancel(); }
        private bool Available => data != null && _inventory != null && _inventory.isActiveAndEnabled && _inventory.Current > 0 &&
            _health != null && _health.isActiveAndEnabled && !_health.IsDead && _health.Current < _health.Maximum && _input != null && _input.isActiveAndEnabled;
        public void Tick(bool canStart, bool grounded, bool wallSliding, bool controlEnabled, float now)
        {
            _now = now;
            bool pressed = _input != null && _input.ConsumePress();
            bool valid = isActiveAndEnabled && Available && grounded && !wallSliding && controlEnabled;
            if (!valid) { Cancel(); return; }
            if (isHealing)
            {
                if (now - _started >= data.UseTime) _completionPending = true;
                return;
            }
            if (!pressed || !canStart) return;
            isHealing = true; _started = now; _completionPending = false;
            if (data.HealingEffectPrefab != null) _effect = Instantiate(data.HealingEffectPrefab, transform);
            Debug.Log("[Player] 회복 시작", this);
        }
        private void LateUpdate()
        {
            // Commit after all physics damage callbacks; a hit on the completion tick can still cancel.
            if (!_completionPending || !isHealing) return;
            if (!Available) { Cancel(); return; }
            isHealing = false; _completionPending = false; ClearEffect();
            _input.ConsumePress(); Ended?.Invoke();
            if (_health.Heal(data.HealAmount) > 0)
            {
                _inventory.TryConsumeOne(); Debug.Log("[Player] 회복 완료 — 아이템 1개 소비", this);
            }
        }
        public void Cancel()
        {
            bool wasHealing = isHealing;
            if (_input != null) _input.ConsumePress();
            if (isHealing) Debug.Log("[Player] 회복 취소 — 아이템 보존", this);
            isHealing = false; _completionPending = false; ClearEffect();
            if (wasHealing) Ended?.Invoke();
        }
        private void ClearEffect() { if (_effect != null) Destroy(_effect); _effect = null; }
        private void OnDisable()
        {
            if (_receiver != null) _receiver.HitReceived -= OnHit;
            if (_health != null) _health.Died -= Cancel;
            if (_input != null) _input.InputDisabled -= Cancel;
            Cancel();
        }
    }
}
