using Eggs.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Eggs.Editor
{
    [CustomEditor(typeof(DogUnit))]
    public sealed class DogUnitInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DogUnit dog = (DogUnit)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Current State", dog.CurrentState);
                EditorGUILayout.ObjectField("Current Zone", dog.CurrentZone, typeof(WorkZone), true);
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
                foreach (DogUnit dog in zone.Members)
                    EditorGUILayout.ObjectField("Member", dog, typeof(DogUnit), true);
            }
            if (Application.isPlaying)
                Repaint();
        }
    }
}
