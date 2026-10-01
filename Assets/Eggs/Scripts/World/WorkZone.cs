using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class WorkZone : MonoBehaviour
    {
        [SerializeField] private UnitWorkState targetState = UnitWorkState.Gathering;
        [SerializeField] private Collider2D dropArea;
        [Tooltip("Optional. Leave empty to keep each unit at its drop position.")]
        [SerializeField] private Transform snapPoint;
        [SerializeField] private Color gizmoColor = new Color(0.3f, 0.8f, 0.5f, 0.8f);

        private readonly List<UnitActor> members = new List<UnitActor>();
        private ReadOnlyCollection<UnitActor> readOnlyMembers;

        public UnitWorkState TargetState => targetState;
        public IReadOnlyList<UnitActor> Members => readOnlyMembers ??= members.AsReadOnly();
        public int MemberCount => members.Count;
        public bool CanAcceptUnits => isActiveAndEnabled && targetState != UnitWorkState.Idle
            && dropArea != null && dropArea.enabled && dropArea.gameObject.activeInHierarchy;

        public bool Contains(Vector2 worldPoint)
        {
            return CanAcceptUnits && dropArea.OverlapPoint(worldPoint);
        }

        public Vector3 GetDropPosition(Vector3 position)
        {
            if (snapPoint != null && Contains(snapPoint.position))
            {
                position.x = snapPoint.position.x;
                position.y = snapPoint.position.y;
            }
            return position;
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
            if (dropArea == null)
                return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(dropArea.bounds.center, dropArea.bounds.size);
        }
    }
}
