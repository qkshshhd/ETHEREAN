using System;
using Etherean.Player;
using UnityEngine;

namespace Etherean.Combat
{
    public enum ReactionPhase { None, Stagger, Launch, Down, GetUp }
    [DisallowMultipleComponent]
    public sealed class CombatReaction : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] private float staggerTime = 0.2f;
        [SerializeField, Min(0.02f)] private float downTime = 0.45f;
        [SerializeField, Min(0.02f)] private float getUpTime = 0.25f;
        [SerializeField, Min(0f)] private float knockbackDrag = 10f;
        private float _until, _horizontal, _vertical;
        private bool _launchPending;
        [SerializeField] private ReactionPhase phase;
        public ReactionPhase Phase => phase;
        public bool IsActive => isActiveAndEnabled && Phase != ReactionPhase.None;
        public event Action Started;
        public bool SuppressNewReactions { get; set; }
        public void Begin(AttackHit hit)
        {
            if (!isActiveAndEnabled || SuppressNewReactions) return;
            if (Phase == ReactionPhase.Down || Phase == ReactionPhase.GetUp) return;
            phase = hit.Reaction == HitReaction.Knockdown ? ReactionPhase.Launch : ReactionPhase.Stagger;
            _until = Time.fixedTime + staggerTime;
            _horizontal = hit.Knockback.x;
            _vertical = Mathf.Max(0, hit.Knockback.y);
            _launchPending = true;
            Started?.Invoke();
        }
        public void Tick(bool grounded)
        {
            if (!IsActive) return;
            if (Phase == ReactionPhase.Launch && grounded && !_launchPending) { phase = ReactionPhase.Down; _until = Time.fixedTime + downTime; }
            else if (Phase == ReactionPhase.Down && Time.fixedTime >= _until) { phase = ReactionPhase.GetUp; _until = Time.fixedTime + getUpTime; }
            else if ((Phase == ReactionPhase.Stagger || Phase == ReactionPhase.GetUp) && Time.fixedTime >= _until) phase = ReactionPhase.None;
        }
        public Vector2 TakeMotion(out bool launches)
        {
            launches = _launchPending; _launchPending = false;
            Vector2 motion = new Vector2(_horizontal, _vertical);
            _horizontal = Mathf.MoveTowards(_horizontal, 0, knockbackDrag * Time.fixedDeltaTime);
            return Phase == ReactionPhase.Down || Phase == ReactionPhase.GetUp ? Vector2.zero : motion;
        }
        public void ResetState()
        {
            phase = ReactionPhase.None; _launchPending = false; SuppressNewReactions = false;
            _horizontal = _vertical = _until = 0;
        }
        private void OnDisable() { ResetState(); }
    }
}
