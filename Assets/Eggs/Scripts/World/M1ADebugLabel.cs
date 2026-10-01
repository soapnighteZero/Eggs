using System.Text;
using UnityEngine;

namespace Eggs.Gameplay
{
    // Optional presentation only: removing this component does not change assignment.
    public sealed class M1ADebugLabel : MonoBehaviour
    {
        [SerializeField] private TextMesh label;
        [SerializeField] private UnitActor unit;
        [SerializeField] private WorkZone zone;
        [SerializeField] private string heading;

        private readonly StringBuilder text = new StringBuilder();

        private void LateUpdate()
        {
            if (label == null)
                return;

            text.Clear();
            if (unit != null)
            {
                text.Append(unit.name).Append('\n').Append(unit.CurrentState);
            }
            else if (zone != null)
            {
                text.Append(heading).Append("\nMembers: ").Append(zone.MemberCount);
            }

            string value = text.ToString();
            if (label.text != value)
                label.text = value;
        }
    }
}
