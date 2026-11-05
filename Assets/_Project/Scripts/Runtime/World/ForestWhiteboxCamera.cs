using System;
using System.Collections.Generic;
using Etherean.Enemies;
using UnityEngine;

namespace Etherean.World
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public sealed class ForestWhiteboxCamera : MonoBehaviour
    {
        [Serializable]
        private struct RoomView
        {
            public string name;
            public Rect entryBounds;
            public Rect viewBounds;
            public Transform[] visibleRoots;
            public bool openSky;
        }

        [SerializeField] private Transform target;
        [SerializeField] private Rect worldBounds;
        [SerializeField] private RoomView[] rooms = Array.Empty<RoomView>();
        [SerializeField] private Vector2 followOffset = new Vector2(0f, 1.5f);
        [SerializeField, Min(0f)] private float followTime = .14f;
        [SerializeField] private Color interiorBackground = new(.08f, .10f, .11f, 1f);
        private Camera _camera;
        private Vector2 _velocity;
        private float _defaultSize;
        private Color _defaultBackground;
        private int _roomIndex = -2;
        private ForestRoomVisibility _visibility;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _defaultSize = _camera.orthographicSize;
            _defaultBackground = _camera.backgroundColor;
        }

        private void OnEnable()
        {
            var groups = new Transform[rooms.Length][];
            for (int i = 0; i < rooms.Length; i++) groups[i] = rooms[i].visibleRoots;
            _visibility = new ForestRoomVisibility(groups);
            _roomIndex = -2;
        }

        private void OnDisable()
        {
            _visibility?.Restore();
            _visibility = null;
            if (_camera == null) return;
            _camera.orthographicSize = _defaultSize;
            _camera.backgroundColor = _defaultBackground;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector2 position = target.position;
            int nextRoom = FindRoom(position);
            _visibility?.Update(nextRoom);
            if (nextRoom != _roomIndex)
                _camera.backgroundColor = _visibility != null && _visibility.HasRooms &&
                    nextRoom >= 0 && !rooms[nextRoom].openSky ? interiorBackground : _defaultBackground;
            Rect bounds = nextRoom < 0 ? worldBounds : rooms[nextRoom].viewBounds;
            if (nextRoom >= 0)
                _camera.orthographicSize = Mathf.Min(_defaultSize, Mathf.Min(bounds.height * .5f, bounds.width * .5f / _camera.aspect));
            else
                _camera.orthographicSize = _defaultSize;

            Vector2 desired = position + followOffset;
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            desired = ClampView(desired, bounds, halfWidth, halfHeight);
            Vector2 current = transform.position;
            if (nextRoom != _roomIndex || Vector2.SqrMagnitude(current - desired) > 64f)
                _velocity = Vector2.zero;
            else desired = Vector2.SmoothDamp(current, desired, ref _velocity, followTime);
            _roomIndex = nextRoom;
            desired = ClampView(desired, bounds, halfWidth, halfHeight);
            transform.position = new Vector3(desired.x, desired.y, transform.position.z);
        }

        private int FindRoom(Vector2 position)
        {
            for (int i = 0; i < rooms.Length; i++)
                if (rooms[i].entryBounds.Contains(position)) return i;
            if (_visibility == null || !_visibility.HasRooms) return -1;
            if (_roomIndex >= 0) return _roomIndex;
            int nearest = -1;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < rooms.Length; i++)
            {
                var bounds = rooms[i].entryBounds;
                var point = new Vector2(Mathf.Clamp(position.x, bounds.xMin, bounds.xMax),
                                        Mathf.Clamp(position.y, bounds.yMin, bounds.yMax));
                float distance = (position - point).sqrMagnitude;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                nearest = i;
            }
            return nearest;
        }

        private static Vector2 ClampView(Vector2 position, Rect bounds, float halfWidth, float halfHeight) =>
            new Vector2(ClampAxis(position.x, bounds.xMin, bounds.xMax, halfWidth),
                        ClampAxis(position.y, bounds.yMin, bounds.yMax, halfHeight));

        private static float ClampAxis(float position, float min, float max, float halfView) =>
            max - min > halfView * 2f ? Mathf.Clamp(position, min + halfView, max - halfView) : (min + max) * .5f;
    }

    internal sealed class ForestRoomVisibility
    {
        private sealed class Group
        {
            internal readonly Transform Root;
            internal readonly EnemySpawnPoint SpawnPoint;
            internal GameObject Instance;
            internal readonly Dictionary<Renderer, bool> Original = new();
            internal bool Visible = true;

            internal Group(Transform root)
            {
                Root = root;
                SpawnPoint = root.GetComponent<EnemySpawnPoint>();
                Cache(root);
            }

            private void Cache(Transform root)
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    Original.TryAdd(renderer, renderer.forceRenderingOff);
            }

            internal void RefreshInstance()
            {
                if (SpawnPoint == null || SpawnPoint.Instance == Instance) return;
                foreach (var renderer in new List<Renderer>(Original.Keys))
                    if (renderer == null) Original.Remove(renderer);
                Instance = SpawnPoint.Instance;
                if (Instance != null) Cache(Instance.transform);
                SetVisible(Visible);
            }

            internal void SetVisible(bool visible)
            {
                Visible = visible;
                foreach (var pair in Original)
                    if (pair.Key != null) pair.Key.forceRenderingOff = pair.Value || !visible;
            }
        }

        private readonly Dictionary<Transform, Group> _groups = new();
        private readonly List<Group> _spawns = new();
        private readonly HashSet<Transform>[] _visibleByRoom;
        private int _activeRoom = int.MinValue;

        internal bool HasRooms => _groups.Count > 0;

        internal ForestRoomVisibility(Transform[][] rootsByRoom)
        {
            _visibleByRoom = new HashSet<Transform>[rootsByRoom.Length];
            for (int i = 0; i < rootsByRoom.Length; i++)
            {
                var visible = _visibleByRoom[i] = new HashSet<Transform>();
                if (rootsByRoom[i] == null) continue;
                foreach (var root in rootsByRoom[i])
                {
                    if (root == null || !visible.Add(root) || _groups.ContainsKey(root)) continue;
                    var group = _groups[root] = new Group(root);
                    if (group.SpawnPoint != null) _spawns.Add(group);
                }
            }
        }

        internal void Update(int room)
        {
            if (_activeRoom != room)
            {
                _activeRoom = room;
                foreach (var group in _groups.Values)
                    group.SetVisible(room < 0 || _visibleByRoom[room].Contains(group.Root));
            }
            foreach (var spawn in _spawns) spawn.RefreshInstance();
        }

        internal void Restore()
        {
            foreach (var group in _groups.Values) group.SetVisible(true);
        }
    }
}
