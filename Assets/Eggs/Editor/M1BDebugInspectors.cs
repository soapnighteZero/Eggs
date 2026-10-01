using Eggs.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Eggs.Editor
{
    [CustomEditor(typeof(GameState))]
    public sealed class GameStateInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
                DrawDefaultInspector();
            GameState state = (GameState)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Current Food", state.Food);
                EditorGUILayout.IntField("Current Population", state.Population);
            }
            if (Application.isPlaying)
                Repaint();
        }
    }

    [CustomEditor(typeof(BreedingSystem))]
    public sealed class BreedingSystemInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
                DrawDefaultInspector();
            BreedingSystem breeding = (BreedingSystem)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("Cycle Active", breeding.IsBreeding);
                EditorGUILayout.FloatField("Remaining Seconds", breeding.RemainingSeconds);
                EditorGUILayout.ObjectField("Active Egg", breeding.ActiveEgg, typeof(EggHatch), true);
            }
            if (Application.isPlaying)
                Repaint();
        }
    }
}
