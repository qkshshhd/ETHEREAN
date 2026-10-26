using System;
using UnityEngine;

namespace Etherean.Player
{
    public abstract class PlayerMovementInput : MonoBehaviour
    {
        public event Action InputDisabled;

        public abstract float HorizontalInput { get; }
        public abstract bool IsJumpHeld { get; }
        public abstract bool ConsumeJumpPress();
        public abstract bool ConsumeJumpRelease();

        protected void NotifyInputDisabled()
        {
            InputDisabled?.Invoke();
        }
    }
}
