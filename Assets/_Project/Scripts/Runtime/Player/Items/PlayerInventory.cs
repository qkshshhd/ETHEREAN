using System;
using UnityEngine;
namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private HealingItemData data;
        [SerializeField, Min(0)] private int currentCount;
        [SerializeField, Min(0)] private int totalConsumed;
        public int Capacity => data != null ? data.MaximumCount : 0;
        public int Current => Mathf.Clamp(currentCount, 0, Capacity);
        public int TotalConsumed => totalConsumed;
        public event Action Changed;
        private void Awake() { currentCount = Current; }
        public void RestoreState(int count, int consumed)
        {
            currentCount = Mathf.Clamp(count, 0, Capacity); totalConsumed = Mathf.Max(0, consumed);
            Changed?.Invoke();
        }
        public int TryAdd(int amount)
        {
            if (!isActiveAndEnabled || amount <= 0) return 0;
            int accepted = Mathf.Min(amount, Capacity - Current);
            if (accepted <= 0) return 0;
            currentCount = Current + accepted; Changed?.Invoke(); return accepted;
        }
        public bool TryConsumeOne()
        {
            if (!isActiveAndEnabled || Current <= 0) return false;
            currentCount = Current - 1; totalConsumed++; Changed?.Invoke(); return true;
        }
    }
}
