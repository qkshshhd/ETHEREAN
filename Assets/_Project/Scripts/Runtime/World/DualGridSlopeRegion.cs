using UnityEngine;

namespace Etherean.World
{
    [DisallowMultipleComponent]
    public sealed class DualGridSlopeRegion : MonoBehaviour
    {
        [SerializeField] private RectInt cells;
        [SerializeField] private bool risesRight = true;
        [SerializeField, HideInInspector] private BoxCollider2D cut;
        [SerializeField, HideInInspector] private PolygonCollider2D fill;

        public RectInt Cells => cells;
        public bool RisesRight => risesRight;
        public BoxCollider2D Cut => cut;
        public PolygonCollider2D Fill => fill;
    }
}
