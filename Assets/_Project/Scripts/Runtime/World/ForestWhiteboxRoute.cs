using Etherean.Combat;
using Etherean.Enemies;
using Etherean.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Etherean.World
{
    [DefaultExecutionOrder(210), DisallowMultipleComponent]
    public sealed class ForestWhiteboxRoute : MonoBehaviour
    {
        [SerializeField] private CheckpointSession session;
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private EnemySpawnPoint encounterBoar;
        [SerializeField] private GameObject encounterGate;
        [SerializeField] private GameObject shortcutGate;
        [SerializeField] private GameObject exitClimbGate;
        [SerializeField] private BoxCollider2D[] oneWaySteps;
        [SerializeField] private Rect encounterArea;
        [SerializeField] private Rect encounterExitUseArea;
        [SerializeField] private Rect shortcutUseArea;
        [SerializeField] private Rect traversalUnlockArea;
        [SerializeField] private Rect returnArea;
        [SerializeField] private Rect exitArea;
        [SerializeField] private float recoveryHeight = -24f;

        private InputAction _interact;
        private CombatHealth _encounterHealth;

        private void Awake()
        {
            foreach (var step in oneWaySteps)
            {
                var effector = step.GetComponent<PlatformEffector2D>();
                if (effector == null) effector = step.gameObject.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                step.usedByEffector = true;
            }
        }

        private void OnEnable()
        {
            _interact = interactAction.action.Clone();
            _interact.Enable();
        }

        private void Update()
        {
            if (!session.IsReady) return;
            TrackEncounter();
            var progress = session.Progress;
            encounterGate.SetActive(!progress.Has(ProgressFlag.DExitOpened));
            shortcutGate.SetActive(!progress.Has(ProgressFlag.ShortcutCEOpened));
            if (exitClimbGate != null)
                exitClimbGate.SetActive(!progress.Has(ProgressFlag.WallTraversalUnlocked));

            var player = session.PlayerHealth;
            if (player == null || player.IsDead || session.IsSeated || Time.timeScale == 0f) return;
            Vector2 position = player.transform.position;
            if (position.y < recoveryHeight)
            {
                session.RespawnPlayer();
                return;
            }

            if (encounterArea.Contains(position)) Record(ProgressFlag.BoarIntroSeen);
            if (traversalUnlockArea.Contains(position)) Record(ProgressFlag.WallTraversalUnlocked);
            if (returnArea.Contains(position) && progress.Has(ProgressFlag.WallTraversalUnlocked))
                Record(ProgressFlag.ShortcutHBOpened);
            if (exitArea.Contains(position) && progress.Has(ProgressFlag.WallTraversalUnlocked))
                Record(ProgressFlag.ForestExitReached);

            if (!_interact.WasPressedThisFrame()) return;
            if (encounterExitUseArea.Contains(position) && progress.Has(ProgressFlag.BoarEncounterCleared))
                Record(ProgressFlag.DExitOpened);
            if (shortcutUseArea.Contains(position) && progress.Has(ProgressFlag.DExitOpened))
                Record(ProgressFlag.ShortcutCEOpened);
        }

        private void TrackEncounter()
        {
            var instance = encounterBoar.Instance;
            var health = instance != null ? instance.GetComponent<CombatHealth>() : null;
            if (_encounterHealth == health) return;
            if (_encounterHealth != null) _encounterHealth.Died -= OnEncounterCleared;
            _encounterHealth = health;
            if (_encounterHealth != null)
            {
                _encounterHealth.Died += OnEncounterCleared;
                if (_encounterHealth.IsDead) OnEncounterCleared();
            }
        }

        private void OnEncounterCleared() => Record(ProgressFlag.BoarEncounterCleared);

        private void Record(ProgressFlag flag)
        {
            if (session.Progress.Has(flag)) return;
            session.Progress.Set(flag);
            Debug.Log("[Forest Whitebox] " + flag, this);
        }

        private void OnDisable()
        {
            if (_encounterHealth != null) _encounterHealth.Died -= OnEncounterCleared;
            _encounterHealth = null;
            _interact?.Disable();
            _interact?.Dispose();
            _interact = null;
        }
    }
}
