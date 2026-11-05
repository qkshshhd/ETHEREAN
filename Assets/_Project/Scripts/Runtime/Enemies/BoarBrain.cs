using Etherean.Combat;
using Etherean.Player;
using UnityEngine;

namespace Etherean.Enemies
{
    public enum BoarState { Idle, Roam, Alert, Windup, Charge, Recovery, Stun, Reacting, Return, Dead }

    [RequireComponent(typeof(BoarMotor), typeof(BoarAttack), typeof(CombatDamageReceiver))]
    [RequireComponent(typeof(EnemyRoaming))]
    public sealed class BoarBrain : MonoBehaviour
    {
        [SerializeField] private BoarData data;
        [SerializeField] private CombatHealth target;
        [SerializeField] private BoarState state;
        private BoarMotor _motor;
        private BoarAttack _attack;
        private CombatHealth _health;
        private CombatReaction _reaction;
        private CombatDamageReceiver _receiver;
        private EnemyRoaming _roaming;
        private Vector2 _home;
        private float _until, _stateStarted;
        private float _nextChargeAt = float.NegativeInfinity;
        private int _facing = 1;
        private bool _provoked;
        private GameObject _warning;
        public BoarState State => state;
        public int FacingDirection => _facing;
        public bool IsSuperArmored => isActiveAndEnabled && (State == BoarState.Windup || State == BoarState.Charge);
        public float StateProgress => Mathf.Clamp01((Time.fixedTime - _stateStarted) / Mathf.Max(.02f, _until - _stateStarted));
        public float AttackCooldownRemaining => Mathf.Max(0, _nextChargeAt - Time.fixedTime);
        public void SetTarget(CombatHealth value) { target = value; }
        private void Awake()
        {
            _motor = GetComponent<BoarMotor>(); _attack = GetComponent<BoarAttack>();
            _health = GetComponent<CombatHealth>(); _reaction = GetComponent<CombatReaction>();
            _receiver = GetComponent<CombatDamageReceiver>(); _roaming = GetComponent<EnemyRoaming>();
            _home = transform.position; _roaming.Initialize(_home.x, Time.fixedTime);
        }
        private void OnEnable()
        {
            _receiver.HitReceived += OnHit; _health.Died += Die;
            state = BoarState.Idle; _reaction.SuppressNewReactions = false; _attack.Cancel();
            _roaming.Pause(Time.fixedTime);
        }
        private void FaceTarget()
        {
            if (target == null) return;
            float delta = target.transform.position.x - transform.position.x;
            if (Mathf.Abs(delta) > .08f) _facing = delta > 0 ? 1 : -1;
        }
        private void OnHit(AttackHit hit)
        {
            _provoked = true;
            if (_health.IsDead || IsSuperArmored || State == BoarState.Stun) return;
            if (_reaction.IsActive) Change(BoarState.Reacting);
        }
        private void Die() { Change(BoarState.Dead); _motor.Stop(); }
        private void Change(BoarState next)
        {
            if (State == next) return;
            _attack.Cancel(); ClearWarning();
            state = next; _stateStarted = Time.fixedTime;
            // Stun owns a fixed deadline: incoming hits do damage without replacing it with Stagger.
            _reaction.SuppressNewReactions = IsSuperArmored || State == BoarState.Stun;
            if (next != BoarState.Idle && next != BoarState.Roam) _roaming.Pause(Time.fixedTime);
            if (next == BoarState.Alert)
            {
                FaceTarget(); _until = Time.fixedTime + data.AlertTime;
                Debug.Log("[Boar] 경고 시작", this);
            }
            else if (next == BoarState.Windup)
            {
                FaceTarget(); _until = Time.fixedTime + data.WindupTime;
                _nextChargeAt = Time.fixedTime + data.AttackCooldown;
                Debug.Log("[Boar] 돌진 준비 — 방향 확정", this);
                if (data.WarningEffectPrefab != null) _warning = Instantiate(data.WarningEffectPrefab, transform);
            }
            else if (next == BoarState.Charge) { _until = Time.fixedTime + data.ChargeTime; _attack.Begin(); }
            else if (next == BoarState.Recovery) _until = Mathf.Max(Time.fixedTime + data.RecoveryTime, _nextChargeAt);
            else if (next == BoarState.Stun) { _until = Time.fixedTime + data.StunTime; Debug.Log("[Boar] 벽 충돌 기절", this); }
        }
        private void FixedUpdate()
        {
            if (data == null) { _attack.Cancel(); _reaction.SuppressNewReactions = false; _motor.Stop(); return; }
            if (_health.IsDead) { Change(BoarState.Dead); _motor.Step(0); return; }
            _reaction.SuppressNewReactions = IsSuperArmored || State == BoarState.Stun;
            _reaction.Tick(_motor.Grounded);
            if (_reaction.IsActive)
            {
                Change(BoarState.Reacting);
                Vector2 motion = _reaction.TakeMotion(out bool launch); _motor.Step(motion.x, launch, motion.y); return;
            }
            if (State == BoarState.Reacting) Change(BoarState.Recovery);
            bool valid = target != null && target.isActiveAndEnabled && !target.IsDead;
            float distance = valid ? Mathf.Abs(target.transform.position.x - transform.position.x) : float.PositiveInfinity;
            var targetCollider = valid ? target.GetComponent<Collider2D>() : null;
            float targetY = targetCollider != null ? targetCollider.bounds.center.y : valid ? target.transform.position.y : 0;
            bool reachable = valid && Mathf.Abs(targetY - transform.position.y) <= data.MaximumHeightDifference;
            bool withinHome = valid && Mathf.Abs(target.transform.position.x - _home.x) <= data.HomeLimit && Mathf.Abs(transform.position.x - _home.x) <= data.HomeLimit;
            float horizontal = 0;
            switch (State)
            {
                case BoarState.Idle:
                case BoarState.Roam:
                    if (reachable && withinHome && (distance <= data.DetectionRange || _provoked)) Change(BoarState.Alert);
                    else
                    {
                        int direction = _roaming.GetDirection(transform.position.x, Time.fixedTime);
                        if (direction != 0 && _motor.CheckAhead(direction, _roaming.MoveSpeed) != BoarObstacle.None)
                            direction = _roaming.TurnAtObstacle(Time.fixedTime, _motor.CheckAhead(-direction, _roaming.MoveSpeed) == BoarObstacle.None);
                        Change(direction == 0 ? BoarState.Idle : BoarState.Roam);
                        if (direction != 0) { _facing = direction; horizontal = direction * _roaming.MoveSpeed; }
                    }
                    break;
                case BoarState.Alert:
                    if (!reachable || !withinHome || distance > data.DetectionRange) Change(BoarState.Return);
                    else
                    {
                        FaceTarget();
                        if (_motor.Grounded && Time.fixedTime >= _until && Time.fixedTime >= _nextChargeAt && distance <= data.EngageRange) Change(BoarState.Windup);
                    }
                    break;
                case BoarState.Windup:
                    if (Time.fixedTime >= _until) Change(BoarState.Charge);
                    break;
                case BoarState.Charge:
                    var obstacle = _motor.CheckAhead(_facing, data.ChargeSpeed);
                    if (obstacle == BoarObstacle.Wall) Change(BoarState.Stun);
                    else if (obstacle == BoarObstacle.Ledge || Time.fixedTime >= _until || Mathf.Abs(transform.position.x - _home.x) > data.HomeLimit) Change(BoarState.Recovery);
                    else { _attack.Sample(data, _facing); horizontal = _facing * data.ChargeSpeed; }
                    break;
                case BoarState.Recovery:
                case BoarState.Stun:
                    if (Time.fixedTime >= _until) Change(reachable && withinHome && distance <= data.DetectionRange ? BoarState.Alert : BoarState.Return);
                    break;
                case BoarState.Return:
                    _provoked = false;
                    if (Mathf.Abs(transform.position.x - _home.x) <= .08f) Change(BoarState.Idle);
                    else
                    {
                        int direction = transform.position.x > _home.x ? -1 : 1;
                        if (_motor.CheckAhead(direction, data.ReturnSpeed) == BoarObstacle.None) { _facing = direction; horizontal = direction * data.ReturnSpeed; }
                    }
                    break;
            }
            _motor.Step(horizontal);
        }
        private void ClearWarning() { if (_warning != null) Destroy(_warning); _warning = null; }
        private void OnDisable()
        {
            _receiver.HitReceived -= OnHit; _health.Died -= Die;
            _reaction.SuppressNewReactions = false; _attack.Cancel(); ClearWarning(); _motor.Stop();
        }
    }
}
