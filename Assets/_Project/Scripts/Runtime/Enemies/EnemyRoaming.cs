using UnityEngine;

namespace Etherean.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyRoaming : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 0.55f;
        [SerializeField, Min(0.1f)] private float radius = 2.5f;
        [SerializeField, Range(0f, 0.5f)] private float homeBias = 0.45f;
        [SerializeField] private Vector2 moveDuration = new Vector2(0.7f, 1.6f);
        [SerializeField] private Vector2 pauseDuration = new Vector2(0.4f, 0.9f);
        [SerializeField, Range(0f, 1f)] private float pauseChance = 0.25f;
        private float _homeX;
        private float _nextDecision;
        private int _direction;
        public float MoveSpeed => Mathf.Max(0, moveSpeed);

        public void Initialize(float homeX, float time)
        {
            _homeX = homeX;
            Pause(time);
        }

        public void Pause(float time)
        {
            _direction = 0;
            _nextDecision = time + Duration(pauseDuration);
        }

        public int GetDirection(float positionX, float time)
        {
            float offset = positionX - _homeX;
            if (Mathf.Abs(offset) >= Mathf.Max(0.1f, radius) && _direction * offset >= 0)
            {
                _direction = offset > 0 ? -1 : 1;
                _nextDecision = time + Duration(moveDuration);
            }
            if (time < _nextDecision) return _direction;
            if (_direction != 0 && Random.value < pauseChance)
            {
                Pause(time);
                return 0;
            }
            float rightWeight = 0.5f - Mathf.Clamp(offset / Mathf.Max(0.1f, radius), -1, 1) * Mathf.Clamp(homeBias, 0, 0.5f);
            _direction = Random.value < rightWeight ? 1 : -1;
            if (Mathf.Abs(offset) >= Mathf.Max(0.1f, radius)) _direction = offset > 0 ? -1 : 1;
            _nextDecision = time + Duration(moveDuration);
            return _direction;
        }

        public int TurnAtObstacle(float time, bool oppositeIsSafe)
        {
            if (!oppositeIsSafe) { Pause(time); return 0; }
            _direction = -_direction;
            _nextDecision = time + Duration(moveDuration);
            return _direction;
        }

        private static float Duration(Vector2 range)
        {
            float minimum = Mathf.Max(0.02f, Mathf.Min(range.x, range.y));
            return Random.Range(minimum, Mathf.Max(minimum, Mathf.Max(range.x, range.y)));
        }
    }
}
