using Eggs.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Eggs.Editor
{
    [CustomEditor(typeof(UnitActor))]
    public sealed class UnitActorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UnitActor unit = (UnitActor)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Current State", unit.CurrentState);
                EditorGUILayout.ObjectField("Current Zone", unit.CurrentZone, typeof(WorkZone), true);
            }
            if (Application.isPlaying)
                Repaint();
        }
    }

    [CustomEditor(typeof(WorkZone))]
    public sealed class WorkZoneInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Configuration stays fixed during Play; runtime membership is read-only.
            using (new EditorGUI.DisabledScope(Application.isPlaying))
                DrawDefaultInspector();

            WorkZone zone = (WorkZone)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Member Count", zone.MemberCount);
                foreach (UnitActor unit in zone.Members)
                    EditorGUILayout.ObjectField("Member", unit, typeof(UnitActor), true);
            }
            if (Application.isPlaying)
                Repaint();
        }
    }
}
