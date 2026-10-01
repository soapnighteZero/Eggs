using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class UnitActor : MonoBehaviour
    {
        public UnitWorkState CurrentState { get; private set; } = UnitWorkState.Idle;
        public WorkZone CurrentZone { get; private set; }

        // The only entry point for both state changes and zone membership changes.
        public void AssignTo(WorkZone zone)
        {
            if (zone != null && (!isActiveAndEnabled || !zone.CanAcceptUnits))
                zone = null;

            UnitWorkState nextState = zone != null ? zone.TargetState : UnitWorkState.Idle;
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
