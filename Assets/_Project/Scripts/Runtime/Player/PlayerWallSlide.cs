using System.Collections;
using UnityEngine;

namespace Etherean.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerWallSlide : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float wallSlideSpeed = 3f;

        public bool IsWallSliding { get; private set; }

        internal float LimitFallSpeed(float currentSpeed, float nextSpeed, bool wallContact, bool abilityUnlocked)
        {
            IsWallSliding = isActiveAndEnabled && abilityUnlocked && wallContact && currentSpeed <= 0f;
            return IsWallSliding ? Mathf.Max(nextSpeed, -Mathf.Max(0.01f, wallSlideSpeed)) : nextSpeed;
        }

        internal void ResetState()
        {
            IsWallSliding = false;
        }

        private void OnDisable()
        {
            ResetState();
        }
    }
    
}
