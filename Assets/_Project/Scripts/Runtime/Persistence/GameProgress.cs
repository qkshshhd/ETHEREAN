using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Etherean.Persistence
{
    [DisallowMultipleComponent]
    public sealed class GameProgress : MonoBehaviour
    {
        private readonly HashSet<string> _flags = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _pickups = new Dictionary<string, int>(StringComparer.Ordinal);
        public event Action Changed;
        public bool Has(ProgressFlag flag) => _flags.Contains(flag.ToString());
        public void Set(ProgressFlag flag)
        { if (_flags.Add(flag.ToString())) Changed?.Invoke(); }
        public int PickupRemaining(string id, int initial) => _pickups.TryGetValue(id, out int value) ? value : initial;
        public void RecordPickup(string id, int remaining)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Persistent pickup needs a stable scene ID.");
            remaining = Mathf.Max(0, remaining);
            if (_pickups.TryGetValue(id, out int old) && old == remaining) return;
            _pickups[id] = remaining; Changed?.Invoke();
        }
        public void Restore(SaveData data)
        {
            _flags.Clear(); _pickups.Clear();
            if (data == null) return;
            foreach (var flag in data.flags) _flags.Add(flag);
            foreach (var item in data.pickups) _pickups[item.id] = item.remaining;
        }
        public string[] CaptureFlags() => _flags.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        public PickupSave[] CapturePickups() => _pickups.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new PickupSave { id = pair.Key, remaining = pair.Value }).ToArray();
    }
}
