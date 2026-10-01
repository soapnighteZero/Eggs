using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class WorkZone : MonoBehaviour
    {
        [SerializeField] private DogWorkState targetState = DogWorkState.Gathering;
        [SerializeField] private Collider2D dropArea;
        [Tooltip("Optional. Leave empty to keep each dog at its drop position.")]
        [SerializeField] private Transform snapPoint;
        [SerializeField] private Color gizmoColor = new Color(0.3f, 0.8f, 0.5f, 0.8f);

        private readonly List<DogUnit> members = new List<DogUnit>();
        private ReadOnlyCollection<DogUnit> readOnlyMembers;

        public DogWorkState TargetState => targetState;
        public IReadOnlyList<DogUnit> Members => readOnlyMembers ??= members.AsReadOnly();
        public int MemberCount => members.Count;
        public bool CanAcceptDogs => isActiveAndEnabled && targetState != DogWorkState.Idle
            && dropArea != null && dropArea.enabled && dropArea.gameObject.activeInHierarchy;

        public bool Contains(Vector2 worldPoint)
        {
            return CanAcceptDogs && dropArea.OverlapPoint(worldPoint);
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

        // Only DogUnit can change membership, after changing its authoritative owner.
        internal void AddMember(DogUnit dog)
        {
            if (dog != null && dog.CurrentZone == this && !members.Contains(dog))
                members.Add(dog);
        }

        internal void RemoveMember(DogUnit dog)
        {
            members.Remove(dog);
        }

        private void OnDisable()
        {
            while (members.Count > 0)
            {
                DogUnit dog = members[members.Count - 1];
                if (dog != null && dog.CurrentZone == this)
                    dog.AssignTo(null);
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
