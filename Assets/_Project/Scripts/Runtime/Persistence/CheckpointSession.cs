using System;
using System.IO;
using System.Linq;
using Etherean.Combat;
using Etherean.Player;
using Etherean.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Etherean.Persistence
{
    [DefaultExecutionOrder(200), DisallowMultipleComponent, RequireComponent(typeof(GameProgress))]
    public sealed class CheckpointSession : MonoBehaviour
    {
        [SerializeField] private PlayerInventory player;
        [SerializeField] private Checkpoint defaultCheckpoint;
        [SerializeField] private InputActionReference interactActionReference;
        [SerializeField] private string saveFileName = "etherean-save.json";
        [SerializeField] private string editorSaveFileName = "etherean-editor-save.json";
        private JsonSaveStore _store;
        private SaveData _loaded;
        private Checkpoint _active;
        private InputAction _interact;
        private Checkpoint[] _checkpoints;
        private Checkpoint _seat;
        private PlayerLocomotion _locomotion;
        private Collider2D[] _playerColliders;
        private CombatHealth _health;
        private CombatDamageReceiver _damageReceiver;
        public bool IsReady { get; private set; }
        public GameProgress Progress { get; private set; }
        public CombatHealth PlayerHealth => player != null ? player.GetComponent<CombatHealth>() : null;
        public string SavePath => _store != null ? _store.FilePath : null;
        public string ActiveCheckpointId => _active != null ? _active.Id : null;
        public bool IsSeated => _seat != null && _locomotion != null && _locomotion.IsSeated;

        private void OnEnable()
        {
            var source = interactActionReference != null ? interactActionReference.action : null;
            if (source == null || source.type != InputActionType.Button) return;
            _interact = source.Clone();
            _interact.Enable();
        }

        private void OnDisable()
        {
            LeaveSeat(false);
            _interact?.Disable();
            _interact?.Dispose();
            _interact = null;
        }

        public static CheckpointSession InScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var session = root.GetComponentInChildren<CheckpointSession>(true);
                if (session != null) return session;
            }
            return null;
        }

        private void Awake()
        {
            Progress = GetComponent<GameProgress>();
            string name = Application.isEditor ? editorSaveFileName : saveFileName;
            if (string.IsNullOrWhiteSpace(name) || Path.GetFileName(name) != name || !name.EndsWith(".json", StringComparison.Ordinal))
            { Debug.LogError("Save filename must be a local .json filename.", this); enabled = false; return; }
            _store = new JsonSaveStore(Path.Combine(Application.persistentDataPath, name));
            _loaded = _store.Load(out string warning);
            if (warning != null) Debug.LogWarning(warning, this);
            Progress.Restore(_loaded);
        }

        private void Start()
        {
            if (_store == null || player == null || defaultCheckpoint == null) { Debug.LogError("Connect session Player and Default Checkpoint.", this); return; }
            _checkpoints = FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(item => item.gameObject.scene == gameObject.scene).ToArray();
            if (_checkpoints.Any(item => string.IsNullOrWhiteSpace(item.Id)) || _checkpoints.Select(item => item.Id).Distinct().Count() != _checkpoints.Length)
            { Debug.LogError("Checkpoint scene IDs must be nonempty and unique. Assign IDs in the Inspector.", this); return; }
            var pickups = FindObjectsByType<PersistentPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(item => item.gameObject.scene == gameObject.scene).ToArray();
            if (pickups.Any(item => string.IsNullOrWhiteSpace(item.Id)) || pickups.Select(item => item.Id).Distinct().Count() != pickups.Length)
            { Debug.LogError("Persistent pickup scene IDs must be nonempty and unique.", this); return; }
            if (_loaded != null && _loaded.scenePath != gameObject.scene.path)
            {
                if (Application.CanStreamedLevelBeLoaded(_loaded.scenePath))
                { player.gameObject.SetActive(false); SceneManager.LoadSceneAsync(_loaded.scenePath); return; }
                Debug.LogError("Saved checkpoint scene is not in the build. Save writes suspended.", this); return;
            }
            _active = _loaded != null ? _checkpoints.FirstOrDefault(item => item.Id == _loaded.checkpointId) : defaultCheckpoint;
            if (_active == null) { _active = defaultCheckpoint; Debug.LogWarning("Saved checkpoint missing; using scene default with progress retained.", this); }
            player.GetComponent<PlayerSaveBinding>().Initialize(this, _loaded);
            player.GetComponent<PlayerRespawn>().Initialize(this);
            _locomotion = player.GetComponent<PlayerLocomotion>();
            _playerColliders = player.GetComponentsInChildren<Collider2D>(true);
            _health = PlayerHealth;
            _damageReceiver = player.GetComponent<CombatDamageReceiver>();
            _health.Died += OnPlayerDied;
            _damageReceiver.HitReceived += OnPlayerHit;
            if (_interact == null) Debug.LogError("Connect Player/Interact to CheckpointSession.", this);
            IsReady = true;
            player.GetComponent<PlayerRespawn>().RestoreAt(_active.SpawnPosition);
            CheckpointWorldReset.Reset(gameObject.scene, PlayerHealth);
            Debug.Log("[Save] " + SavePath, this);
        }

        private void Update()
        {
            if (_seat != null && (!_seat.isActiveAndEnabled || !IsSeated)) LeaveSeat(false);
            if (!IsReady || !player.isActiveAndEnabled || _health.IsDead || Time.timeScale == 0 ||
                _interact == null || !_interact.WasPressedThisFrame()) return;
            if (IsSeated) { LeaveSeat(true); return; }
            if (!_locomotion.isActiveAndEnabled || !_locomotion.IsGrounded || player.GetComponent<CombatReaction>().IsActive) return;

            Checkpoint nearest = null;
            float closest = float.PositiveInfinity;
            foreach (var checkpoint in _checkpoints)
            {
                if (checkpoint == null) continue;
                float distance = (checkpoint.SeatPosition - player.transform.position).sqrMagnitude;
                if (distance >= closest) continue;
                foreach (var collider in _playerColliders)
                {
                    if (collider == null || collider.isTrigger || !checkpoint.IsInReach(collider)) continue;
                    closest = distance; nearest = checkpoint; break;
                }
            }
            if (nearest != null) SitAt(nearest);
        }

        private void SitAt(Checkpoint checkpoint)
        {
            _seat = checkpoint;
            _locomotion.SetSeated(true);
            player.GetComponent<PlayerMovement>().Teleport(checkpoint.SeatPosition);
            player.GetComponent<PlayerRespawn>().Rest();
            CheckpointWorldReset.Reset(gameObject.scene, PlayerHealth);
            if (SaveAt(checkpoint))
            {
                _active = checkpoint;
                Debug.Log("[Checkpoint] Saved at " + checkpoint.Id, checkpoint);
            }
        }

        private void LeaveSeat(bool moveToSpawn)
        {
            var seat = _seat;
            _seat = null;
            if (_locomotion == null || !_locomotion.IsSeated) return;
            _locomotion.SetSeated(false);
            if (moveToSpawn && seat != null) player.GetComponent<PlayerMovement>().Teleport(seat.SpawnPosition);
        }
        private void OnPlayerHit(AttackHit _) => LeaveSeat(false);
        private void OnPlayerDied() => LeaveSeat(false);

        public void RespawnPlayer()
        {
            if (!IsReady || _active == null) return;
            LeaveSeat(false);
            player.GetComponent<PlayerRespawn>().RestoreAt(_active.SpawnPosition);
            CheckpointWorldReset.Reset(gameObject.scene, PlayerHealth);
        }

        private bool SaveAt(Checkpoint checkpoint)
        {
            var data = new SaveData
            {
                signature = "ETHEREAN", version = JsonSaveStore.CurrentVersion,
                checkpointId = checkpoint.Id, scenePath = gameObject.scene.path,
                healingItems = player.Current, totalConsumed = player.TotalConsumed,
                flags = Progress.CaptureFlags(), pickups = Progress.CapturePickups()
            };
            if (_store.Save(data, out string error)) return true;
            Debug.LogError("[Save] " + error, this); return false;
        }
        private void OnDestroy()
        {
            if (_health != null) _health.Died -= OnPlayerDied;
            if (_damageReceiver != null) _damageReceiver.HitReceived -= OnPlayerHit;
        }
    }
}
