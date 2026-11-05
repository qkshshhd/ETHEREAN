using System;
using UnityEngine;

namespace Etherean.Combat
{
    [DisallowMultipleComponent]
    public sealed class CombatHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 50;
        [SerializeField] private int currentHealth;
        public int Current => currentHealth;
        public int Maximum => maxHealth;
        public bool IsDead => Current <= 0;
        public event Action Died;
        private void Awake() { currentHealth = maxHealth; }
        public void RestoreFull() { currentHealth = maxHealth; }
        public int Heal(int amount)
        {
            if (!isActiveAndEnabled || IsDead || amount <= 0 || Current >= Maximum) return 0;
            int restored = Mathf.Min(amount, Maximum - Current);
            currentHealth += restored;
            Debug.Log($"[{name}] HP {Current}/{maxHealth}", this);
            return restored;
        }
        public bool ApplyDamage(int damage)
        {
            if (!isActiveAndEnabled || IsDead || damage <= 0) return false;
            currentHealth = Mathf.Max(0, Current - damage);
            Debug.Log($"[{name}] HP {Current}/{maxHealth}", this);
            if (IsDead) Died?.Invoke();
            return true;
        }
    }
}
