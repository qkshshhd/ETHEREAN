using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerTraversalAbilities : MonoBehaviour
    {
        [SerializeField] private bool wallTraversalUnlocked;

        public bool WallTraversalUnlocked => wallTraversalUnlocked;

        public void UnlockWallTraversal()
        {
            wallTraversalUnlocked = true;
        }
    }
}