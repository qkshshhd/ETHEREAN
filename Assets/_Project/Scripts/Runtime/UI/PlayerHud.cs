using Etherean.Combat;
using Etherean.Player;
using UnityEngine;

namespace Etherean.UI
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerHudView))]
    public sealed class PlayerHud : MonoBehaviour
    {
        [SerializeField] private PlayerHudView view;
        [SerializeField] private CombatHealth health;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerHealing healing;

        private void OnEnable()
        {
            if (view == null) view = GetComponent<PlayerHudView>();
            var owner = GetComponentInParent<PlayerInventory>();
            if (owner != null && health == null)
                Bind(owner.GetComponent<CombatHealth>(), owner, owner.GetComponent<PlayerHealing>());
            Refresh();
        }

        public void Bind(CombatHealth targetHealth, PlayerInventory targetInventory, PlayerHealing targetHealing)
        {
            health = targetHealth; inventory = targetInventory; healing = targetHealing;
            if (view == null) view = GetComponent<PlayerHudView>();
            view.Hide();
            Refresh();
        }

        private void LateUpdate() { Refresh(); }

        public void Refresh()
        {
            if (view == null) return;
            if (!isActiveAndEnabled || health == null) { view.Hide(); return; }
            view.Show(health.Current, health.Maximum, inventory != null ? inventory.Current : 0,
                inventory != null ? inventory.Capacity : 0, healing != null && healing.IsHealing,
                healing != null ? healing.Progress : 0);
        }

        private void OnDisable() { if (view != null) view.Hide(); }
    }
}
