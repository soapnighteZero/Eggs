using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class DogUnit : MonoBehaviour
    {
        public DogWorkState CurrentState { get; private set; } = DogWorkState.Idle;
        public WorkZone CurrentZone { get; private set; }

        // The only entry point for both state changes and zone membership changes.
        public void AssignTo(WorkZone zone)
        {
            if (zone != null && (!isActiveAndEnabled || !zone.CanAcceptDogs))
                zone = null;

            DogWorkState nextState = zone != null ? zone.TargetState : DogWorkState.Idle;
            if (CurrentZone == zone && CurrentState == nextState)
                return;

            if (CurrentZone != null)
                CurrentZone.RemoveMember(this);

            CurrentZone = zone;
            CurrentState = nextState;

            if (CurrentZone != null)
                CurrentZone.AddMember(this);
        }

        private void OnDisable()
        {
            AssignTo(null);
        }
    }
}
