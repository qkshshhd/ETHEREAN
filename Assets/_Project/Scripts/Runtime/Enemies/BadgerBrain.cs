using Etherean.Combat;
using UnityEngine;
namespace Etherean.Enemies
{
    public enum BadgerState { Idle, Alert, Chase, Windup, Active, Recovery, Return, Reacting, Dead, Roam, Cooldown }
    [RequireComponent(typeof(BadgerMotor), typeof(BadgerAttack), typeof(CombatDamageReceiver))]
    [RequireComponent(typeof(EnemyRoaming))]
    public sealed class BadgerBrain : MonoBehaviour
    {
        [SerializeField] private BadgerData data;
        [SerializeField] private CombatHealth target;
        [SerializeField, Min(0f)] private float facingDeadZone = 0.08f;
        private BadgerMotor _motor;
        private BadgerAttack _attack;
        private CombatHealth _health;
        private CombatReaction _reaction;
        private CombatDamageReceiver _receiver;
        private EnemyRoaming _roaming;
        private Vector2 _home;
        private float _until;
        private float _stateStarted;
        private float _nextAttackAt = float.NegativeInfinity;
        private int _facing = 1;
        private bool _provoked;
        private GameObject _warning;
        [SerializeField] private BadgerState state;
        public BadgerState State => state;
        public int FacingDirection => _facing;
        public float AttackCooldownRemaining => Mathf.Max(0, _nextAttackAt - Time.fixedTime);
        public float AttackProgress => data == null ? 0 : Mathf.Clamp01(
            ((State == BadgerState.Windup ? 0 : data.WindupTime) +
            (State == BadgerState.Recovery ? data.ActiveTime : 0) +
            Time.fixedTime - _stateStarted) / (data.WindupTime + data.ActiveTime + data.RecoveryTime));
        public void SetTarget(CombatHealth value) { target = value; }
        private void FaceTarget()
        {
            if (target == null) return;
            float delta = target.transform.position.x - transform.position.x;
            if (Mathf.Abs(delta) > facingDeadZone) _facing = delta > 0 ? 1 : -1;
        }
        private void Awake()
        {
            _motor = GetComponent<BadgerMotor>(); _attack = GetComponent<BadgerAttack>();
            _health = GetComponent<CombatHealth>(); _reaction = GetComponent<CombatReaction>(); _receiver = GetComponent<CombatDamageReceiver>();
            _home = transform.position;
            _roaming = GetComponent<EnemyRoaming>();
            _roaming.Initialize(_home.x, Time.fixedTime);
        }
        private void OnEnable()
        {
            _receiver.HitReceived += OnHit; _health.Died += Die;
        }
        private void OnHit(Etherean.Player.AttackHit hit)
        {
            _provoked = true;
            _attack.Cancel();
            if (!_health.IsDead && _reaction.IsActive) Change(BadgerState.Reacting);
        }
        private void Die() { Change(BadgerState.Dead); }
        private void Change(BadgerState state)
        {
            if (State == state) return;
            _attack.Cancel();
            if (_warning != null) Destroy(_warning);
            this.state = state;
            _stateStarted = Time.fixedTime;
            if (state != BadgerState.Idle && state != BadgerState.Roam) _roaming.Pause(Time.fixedTime);
            if (state == BadgerState.Alert)
            {
                FaceTarget();
                Debug.Log("[Badger] 경고 시작", this);
                _until = Time.fixedTime + data.AlertTime;
                if (data.WarningEffectPrefab != null) _warning = Instantiate(data.WarningEffectPrefab, transform);
            }
            else if (state == BadgerState.Windup)
            {
                _until = Time.fixedTime + data.WindupTime;
                _nextAttackAt = Time.fixedTime + data.AttackCooldown;
            }
            else if (state == BadgerState.Active) { _until = Time.fixedTime + data.ActiveTime; _attack.Begin(); _attack.Sample(data, _facing); }
            else if (state == BadgerState.Recovery) _until = Time.fixedTime + data.RecoveryTime;
        }
        private void FixedUpdate()
        {
            if (data == null) { _attack.Cancel(); _motor.Step(0); return; }
            if (_health.IsDead) { Change(BadgerState.Dead); _motor.Step(0); return; }
            _reaction.Tick(_motor.Grounded);
            if (_reaction.IsActive)
            {
                Change(BadgerState.Reacting); Vector2 motion = _reaction.TakeMotion(out bool launch); _motor.Step(motion.x, launch, motion.y); return;
            }
            if (State == BadgerState.Reacting) Change(AttackCooldownRemaining > 0 ? BadgerState.Cooldown : BadgerState.Chase);
            bool valid = target != null && target.isActiveAndEnabled && !target.IsDead;
            float distance = valid ? Mathf.Abs(target.transform.position.x - transform.position.x) : float.PositiveInfinity;
            bool reachable = valid && Mathf.Abs(target.GetComponent<Collider2D>().bounds.center.y - transform.position.y) <= data.MaximumHeightDifference;
            bool withinHome = valid && Mathf.Abs(target.transform.position.x - _home.x) <= data.ChaseLimit && Mathf.Abs(transform.position.x - _home.x) <= data.ChaseLimit;
            float horizontal = 0;
            switch (State)
            {
                case BadgerState.Idle:
                case BadgerState.Roam:
                    if (reachable && distance <= data.DetectionRange || _provoked && valid) Change(BadgerState.Alert);
                    else
                    {
                        int roamDirection = _roaming.GetDirection(transform.position.x, Time.fixedTime);
                        if (roamDirection != 0 && !_motor.CanMove(roamDirection))
                            roamDirection = _roaming.TurnAtObstacle(Time.fixedTime, _motor.CanMove(-roamDirection));
                        Change(roamDirection == 0 ? BadgerState.Idle : BadgerState.Roam);
                        if (roamDirection != 0) { _facing = roamDirection; horizontal = roamDirection * _roaming.MoveSpeed; }
                    }
                    break;
                case BadgerState.Alert:
                    if (!reachable || !withinHome || distance > data.DetectionRange) Change(BadgerState.Return);
                    else
                    {
                        FaceTarget();
                        if (Time.fixedTime >= _until && distance <= data.EngageRange) Change(BadgerState.Chase);
                    }
                    break;
                case BadgerState.Chase:
                    if (!reachable || !withinHome) Change(BadgerState.Return);
                    else
                    {
                        FaceTarget();
                        if (distance <= data.AttackRange) Change(AttackCooldownRemaining > 0 ? BadgerState.Cooldown : BadgerState.Windup);
                        else if (_motor.CanMove(_facing)) horizontal = _facing * data.MoveSpeed;
                        else Change(BadgerState.Return);
                    }
                    break;
                case BadgerState.Windup:
                    if (!valid) Change(BadgerState.Return);
                    else if (Time.fixedTime >= _until) Change(BadgerState.Active);
                    break;
                case BadgerState.Active:
                    if (!valid) Change(BadgerState.Return);
                    else if (Time.fixedTime >= _until) Change(BadgerState.Recovery);
                    else _attack.Sample(data, _facing);
                    break;
                case BadgerState.Recovery:
                    if (valid) FaceTarget();
                    if (Time.fixedTime >= _until) Change(!valid ? BadgerState.Return : AttackCooldownRemaining > 0 ? BadgerState.Cooldown : BadgerState.Chase);
                    break;
                case BadgerState.Cooldown:
                    if (!reachable || !withinHome) Change(BadgerState.Return);
                    else
                    {
                        FaceTarget();
                        if (AttackCooldownRemaining <= 0) Change(BadgerState.Chase);
                    }
                    break;
                case BadgerState.Return:
                    _provoked = false;
                    int direction = transform.position.x > _home.x ? -1 : 1;
                    if (Mathf.Abs(transform.position.x - _home.x) <= 0.08f) Change(BadgerState.Idle);
                    else if (_motor.CanMove(direction)) { _facing = direction; horizontal = direction * data.MoveSpeed; }
                    break;
            }
            _motor.Step(horizontal);
        }
        private void OnDisable()
        {
            _receiver.HitReceived -= OnHit; _health.Died -= Die; _attack.Cancel();
            if (_warning != null) Destroy(_warning);
        }
    }
}
