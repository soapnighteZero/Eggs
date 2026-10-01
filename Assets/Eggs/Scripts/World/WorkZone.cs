using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class WorkZone : MonoBehaviour
    {
        // Zero/default keeps existing M1A scene data in Collider mode.
        [SerializeField] private WorkZoneShape shape = WorkZoneShape.Collider;
        [SerializeField] private UnitWorkState targetState = UnitWorkState.Gathering;
        [SerializeField] private bool acceptUnits = true;
        [SerializeField] private Collider2D dropArea;
        [SerializeField] private Transform ringCenter;
        [Min(0f)] [SerializeField] private float innerRadius;
        [Min(0f)] [SerializeField] private float outerRadius = 1f;
        [Tooltip("Disable on inner rings so their shared outer edge belongs only to the next ring.")]
        [SerializeField] private bool includeOuterBoundary = true;
        [Tooltip("Food-production availability only. Does not change geometry or membership.")]
        [SerializeField] private bool resourceAvailable = true;
        [Tooltip("Optional. Leave empty to keep each unit at its drop position.")]
        [SerializeField] private Transform snapPoint;
        [SerializeField] private Color gizmoColor = new Color(0.3f, 0.8f, 0.5f, 0.8f);

        private readonly List<UnitActor> members = new List<UnitActor>();
        private ReadOnlyCollection<UnitActor> readOnlyMembers;

        public UnitWorkState TargetState => targetState;
        public WorkZoneShape Shape => shape;
        public Transform RingCenter => ringCenter;
        public float InnerRadius => innerRadius;
        public float OuterRadius => outerRadius;
        public bool ResourceAvailable => resourceAvailable;
        public IReadOnlyList<UnitActor> Members => readOnlyMembers ??= members.AsReadOnly();
        public int MemberCount => members.Count;
        public bool HasValidGeometry => shape == WorkZoneShape.Collider
            ? dropArea != null && dropArea.enabled && dropArea.gameObject.activeInHierarchy
            : shape == WorkZoneShape.Ring && ringCenter != null
                && IsFinite(innerRadius) && IsFinite(outerRadius)
                && innerRadius >= 0f && outerRadius > innerRadius;
        public bool CanAcceptUnits => isActiveAndEnabled && acceptUnits && HasValidGeometry;

        // A future resource-refresh system can use this without changing drag or economy ownership.
        public void SetResourceAvailable(bool available)
        {
            resourceAvailable = available;
        }

        public bool Contains(Vector2 worldPoint)
        {
            if (!CanAcceptUnits)
                return false;
            return shape == WorkZoneShape.Collider ? dropArea.OverlapPoint(worldPoint) : ContainsRingPoint(worldPoint);
        }

        private bool ContainsRingPoint(Vector2 worldPoint)
        {
            float distanceSquared = (worldPoint - (Vector2)ringCenter.position).sqrMagnitude;
            return distanceSquared >= innerRadius * innerRadius
                && (includeOuterBoundary ? distanceSquared <= outerRadius * outerRadius
                    : distanceSquared < outerRadius * outerRadius);
        }

        public Vector3 GetDropPosition(Vector3 position)
        {
            if (snapPoint != null && Contains(snapPoint.position))
            {
                position.x = snapPoint.position.x;
                position.y = snapPoint.position.y;
            }
            return shape == WorkZoneShape.Ring ? GetSafePosition(position) : position;
        }

        public Vector3 GetSafePosition(Vector3 position)
        {
            if (shape != WorkZoneShape.Ring || !HasValidGeometry || ContainsRingPoint(position))
                return position;

            Vector2 direction = (Vector2)position - (Vector2)ringCenter.position;
            // A point at the center uses a deterministic direction instead of randomness.
            direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            Vector2 point = (Vector2)ringCenter.position + direction * ((innerRadius + outerRadius) * 0.5f);
            return new Vector3(point.x, point.y, position.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        // Only UnitActor can change membership, after changing its authoritative owner.
        internal void AddMember(UnitActor unit)
        {
            if (unit != null && unit.CurrentZone == this && !members.Contains(unit))
                members.Add(unit);
        }

        internal void RemoveMember(UnitActor unit)
        {
            members.Remove(unit);
        }

        private void OnDisable()
        {
            while (members.Count > 0)
            {
                UnitActor unit = members[members.Count - 1];
                if (unit != null && unit.CurrentZone == this)
                    unit.AssignTo(null);
                else
                    members.RemoveAt(members.Count - 1);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!HasValidGeometry)
                return;

            Gizmos.color = gizmoColor;
            if (shape == WorkZoneShape.Collider)
                Gizmos.DrawWireCube(dropArea.bounds.center, dropArea.bounds.size);
            else
            {
                DrawRingGizmo(innerRadius);
                DrawRingGizmo(outerRadius);
            }
        }

        private void DrawRingGizmo(float radius)
        {
            const int segments = 64;
            Vector3 previous = ringCenter.position + Vector3.right * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * (Mathf.PI * 2f / segments);
                Vector3 next = ringCenter.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
