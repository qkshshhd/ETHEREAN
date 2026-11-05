using Etherean.Player;
using UnityEngine;

namespace Etherean.Persistence
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInventory), typeof(PlayerTraversalAbilities), typeof(PlayerAttack))]
    public sealed class PlayerSaveBinding : MonoBehaviour
    {
        private CheckpointSession _session;
        private PlayerInventory _inventory;
        private PlayerTraversalAbilities _abilities;
        private PlayerAttack _attack;
        private bool _subscribed;

        public void Initialize(CheckpointSession session, SaveData loaded)
        {
            Unsubscribe(); _session = session;
            _inventory = GetComponent<PlayerInventory>(); _abilities = GetComponent<PlayerTraversalAbilities>(); _attack = GetComponent<PlayerAttack>();
            if (loaded != null)
            {
                _inventory.RestoreState(loaded.healingItems, loaded.totalConsumed);
                ApplyProgress();
            }
            else
            {
                if (_abilities.WallTraversalUnlocked) session.Progress.Set(ProgressFlag.WallTraversalUnlocked);
                if (_attack.WeaponUnlocked) session.Progress.Set(ProgressFlag.WeaponUnlocked);
            }
            Subscribe();
        }
        private void OnEnable() { Subscribe(); }
        private void Subscribe()
        {
            if (_session == null || _subscribed) return;
            _abilities.Changed += RecordAbilities; _attack.WeaponUnlockChanged += RecordAbilities;
            _session.Progress.Changed += ApplyProgress; _subscribed = true;
        }
        private void RecordAbilities()
        {
            if (_abilities.WallTraversalUnlocked) _session.Progress.Set(ProgressFlag.WallTraversalUnlocked);
            if (_attack.WeaponUnlocked) _session.Progress.Set(ProgressFlag.WeaponUnlocked);
        }
        private void ApplyProgress()
        {
            _abilities.SetWallTraversalUnlocked(_session.Progress.Has(ProgressFlag.WallTraversalUnlocked));
            _attack.SetWeaponUnlocked(_session.Progress.Has(ProgressFlag.WeaponUnlocked));
        }
        private void Unsubscribe()
        {
            if (!_subscribed) return;
            if (_session != null) _session.Progress.Changed -= ApplyProgress;
            _abilities.Changed -= RecordAbilities; _attack.WeaponUnlockChanged -= RecordAbilities; _subscribed = false;
        }
        private void OnDisable() { Unsubscribe(); }
    }
}
