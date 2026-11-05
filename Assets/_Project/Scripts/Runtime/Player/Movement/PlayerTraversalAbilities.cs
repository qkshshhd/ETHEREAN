using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerTraversalAbilities : MonoBehaviour
    {
        [SerializeField] private bool wallTraversalUnlocked;

        public bool WallTraversalUnlocked => wallTraversalUnlocked;
        public event System.Action Changed;

        public void UnlockWallTraversal()
        {
            SetWallTraversalUnlocked(true);
        }
        public void SetWallTraversalUnlocked(bool unlocked)
        {
            if (wallTraversalUnlocked == unlocked) return;
            wallTraversalUnlocked = unlocked; Changed?.Invoke();
        }
    }
}
