using DG.Tweening;
using Etherean.Combat;
using UnityEngine;

namespace Etherean.Player
{
    [DefaultExecutionOrder(200), DisallowMultipleComponent]
    public sealed class PlayerSwordMotion : MonoBehaviour
    {
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private PlayerSwordMotionData motionData;
        [SerializeField] private Transform facingRoot;
        [SerializeField] private Transform motionRoot;
        [SerializeField] private SpriteRenderer characterRenderer;
        [SerializeField] private SpriteRenderer swordRenderer;
        [SerializeField, Min(0.01f)] private float motionScale = 1f;
        private CombatHealth _health;
        private Sequence _swing;
        private SwordMotionStep _step;
        private PlayerAttackData _attackData;
        private int _comboStep;
        private Vector3 _position;
        private Vector3 _rotation;

        private void OnEnable()
        {
            if (attack == null) attack = GetComponent<PlayerAttack>();
            _health = GetComponent<CombatHealth>();
            if (attack != null) attack.PhaseChanged += OnPhaseChanged;
            if (_health != null) _health.Died += Hide;
            Hide();
            OnPhaseChanged();
        }

        private void OnPhaseChanged()
        {
            if (!isActiveAndEnabled || attack == null || motionData == null || motionRoot == null) return;
            if (!attack.WeaponUnlocked || !attack.IsAttacking || _health != null && _health.IsDead)
            {
                Hide();
                return;
            }
            if (_swing != null && _attackData == attack.CurrentData && _comboStep == attack.ComboStep) return;
            KillSwing();
            _step = motionData.GetStep(attack.ComboStep);
            if (_step == null || attack.CurrentData == null) return;
            _comboStep = attack.ComboStep;
            _attackData = attack.CurrentData;
            _swing = DOTween.Sequence().SetTarget(this).SetUpdate(UpdateType.Manual).SetAutoKill(false).Pause();
            Vector3 startPosition = _position, startRotation = _rotation;
            if (_step.ContinuousSwing)
            {
                AppendContinuousSwing(ref startPosition, ref startRotation);
            }
            else
            {
                Append(_step.Lift, _attackData.WindupTime * _step.LiftFraction, _step.WindupEase, ref startPosition, ref startRotation);
                Append(_step.Ready, _attackData.WindupTime * (1 - _step.LiftFraction), _step.WindupEase, ref startPosition, ref startRotation);
                Append(_step.Strike, _attackData.ActiveTime, _step.StrikeEase, ref startPosition, ref startRotation);
            }
            Append(_step.Recovery, _attackData.RecoveryTime, _step.RecoveryEase, ref startPosition, ref startRotation);
        }

        private void AppendContinuousSwing(ref Vector3 startPosition, ref Vector3 startRotation)
        {
            // One easing clock keeps angular velocity continuous across preparation and strike.
            Vector3 fromPosition = startPosition, fromRotation = startRotation;
            SwordPose strike = _step.Strike;
            Tween swing = DOTween.To(() => 0f, progress => SetPose(
                Vector3.LerpUnclamped(fromPosition, strike.Position, progress),
                Vector3.LerpUnclamped(fromRotation, strike.Rotation, progress)), 1f,
                _attackData.WindupTime + _attackData.ActiveTime);
            if (_step.SwingProgress != null && _step.SwingProgress.length >= 2) swing.SetEase(_step.SwingProgress);
            else swing.SetEase(Ease.InQuad);
            _swing.Append(swing);
            startPosition = strike.Position;
            startRotation = strike.Rotation;
        }

        private void Append(SwordPose target, float duration, Ease ease, ref Vector3 startPosition, ref Vector3 startRotation)
        {
            // Keep authored Euler turns unwrapped: a downward cut must not take a shorter, reversed arc.
            Vector3 fromPosition = startPosition, fromRotation = startRotation;
            _swing.Append(DOTween.To(() => 0f, progress => SetPose(
                Vector3.LerpUnclamped(fromPosition, target.Position, progress),
                Vector3.LerpUnclamped(fromRotation, target.Rotation, progress)), 1f, duration).SetEase(ease));
            startPosition = target.Position;
            startRotation = target.Rotation;
        }

        private void LateUpdate()
        {
            if (attack == null || motionData == null || motionRoot == null || facingRoot == null || characterRenderer == null || swordRenderer == null)
            {
                Hide();
                return;
            }
            if (!attack.WeaponUnlocked || !attack.IsAttacking || _health != null && _health.IsDead)
            {
                if (_swing != null || swordRenderer.enabled) Hide();
                return;
            }

            OnPhaseChanged();
            if (_swing == null || _attackData == null || _step == null)
            {
                swordRenderer.enabled = false;
                return;
            }
            facingRoot.localScale = new Vector3(attack.FacingDirection, 1, 1);
            float duration = attack.Phase == AttackPhase.Windup ? _attackData.WindupTime : attack.Phase == AttackPhase.Active ? _attackData.ActiveTime : _attackData.RecoveryTime;
            float offset = attack.Phase == AttackPhase.Windup ? 0 : _attackData.WindupTime + (attack.Phase == AttackPhase.Recovery ? _attackData.ActiveTime : 0);
            float remainder = Mathf.Clamp(Time.time - Time.fixedTime, 0, Time.fixedDeltaTime);
            // Sample the existing combat clock; the visual tween never opens a damage window.
            float elapsed = offset + Mathf.Min(duration, attack.PhaseProgress * duration + remainder);
            _swing.Goto(elapsed, false);
            int order = attack.Phase == AttackPhase.Windup
                ? (attack.PhaseProgress < _step.LiftFraction ? _step.Lift.OrderOffset : _step.Ready.OrderOffset)
                : attack.Phase == AttackPhase.Active ? _step.Strike.OrderOffset : _step.Recovery.OrderOffset;
            swordRenderer.sortingLayerID = characterRenderer.sortingLayerID;
            swordRenderer.sortingOrder = Mathf.Clamp(characterRenderer.sortingOrder + order, short.MinValue, short.MaxValue);
            swordRenderer.enabled = true;
        }

        private void SetPose(Vector3 position, Vector3 rotation)
        {
            _position = position;
            _rotation = rotation;
            if (motionRoot == null) return;
            motionRoot.localPosition = position * motionScale;
            motionRoot.localRotation = Quaternion.Euler(rotation);
        }

        private void ResetCarry()
        {
            if (motionData != null) SetPose(motionData.Carry.Position, motionData.Carry.Rotation);
        }

        private void KillSwing()
        {
            _swing?.Kill(false);
            _swing = null;
            _step = null;
            _attackData = null;
            _comboStep = 0;
        }

        private void Hide() { KillSwing(); ResetCarry(); if (swordRenderer != null) swordRenderer.enabled = false; }

        private void OnDisable()
        {
            if (attack != null) attack.PhaseChanged -= OnPhaseChanged;
            if (_health != null) _health.Died -= Hide;
            Hide();
        }
    }
}
