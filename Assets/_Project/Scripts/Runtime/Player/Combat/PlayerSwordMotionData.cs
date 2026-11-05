using System;
using DG.Tweening;
using UnityEngine;

namespace Etherean.Player
{
    [Serializable]
    public struct SwordPose
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private int orderOffset;
        public Vector3 Position => position;
        public Vector3 Rotation => rotation;
        public int OrderOffset => orderOffset;
    }

    [Serializable]
    public sealed class SwordMotionStep
    {
        [SerializeField, Tooltip("Use one curve across Windup and Active. Lift/Ready and their eases are ignored in this mode.")]
        private bool continuousSwing;
        [SerializeField, Tooltip("Normalized time -> swing progress, spanning Windup + Active without restarting easing.")]
        private AnimationCurve swingProgress = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.5f, 0.1f, 0.6f, 0.6f),
            new Keyframe(0.8510638f, 0.65f, 3f, 3f),
            new Keyframe(1f, 1f, 0f, 0f));
        [SerializeField] private SwordPose lift;
        [SerializeField] private SwordPose ready;
        [SerializeField] private SwordPose strike;
        [SerializeField] private SwordPose recovery;
        [SerializeField, Range(0.05f, 0.95f)] private float liftFraction = 0.75f;
        [SerializeField] private Ease windupEase = Ease.InOutSine;
        [SerializeField] private Ease strikeEase = Ease.InQuad;
        [SerializeField] private Ease recoveryEase = Ease.OutSine;
        public SwordPose Lift => lift;
        public bool ContinuousSwing => continuousSwing;
        public AnimationCurve SwingProgress => swingProgress;
        public SwordPose Ready => ready;
        public SwordPose Strike => strike;
        public SwordPose Recovery => recovery;
        public float LiftFraction => Mathf.Clamp(liftFraction, 0.05f, 0.95f);
        public Ease WindupEase => windupEase;
        public Ease StrikeEase => strikeEase;
        public Ease RecoveryEase => recoveryEase;
    }

    [CreateAssetMenu(menuName = "ETHEREAN/Player/Sword Motion")]
    public sealed class PlayerSwordMotionData : ScriptableObject
    {
        [SerializeField] private SwordPose carry;
        [SerializeField, Min(0.01f)] private float returnDuration = 0.35f;
        [SerializeField] private Ease returnEase = Ease.OutSine;
        [SerializeField, Min(0.1f), Tooltip("Applied by Connect Greatsword Tween when sizing the prefab.")]
        private float lengthRatio = 1.2f;
        [SerializeField] private SwordMotionStep[] combo = Array.Empty<SwordMotionStep>();
        public SwordPose Carry => carry;
        public float ReturnDuration => Mathf.Max(0.01f, returnDuration);
        public Ease ReturnEase => returnEase;
        public float LengthRatio => Mathf.Max(0.1f, lengthRatio);
        public SwordMotionStep GetStep(int oneBasedIndex) => combo != null && oneBasedIndex > 0 && oneBasedIndex <= combo.Length ? combo[oneBasedIndex - 1] : null;
    }
}
