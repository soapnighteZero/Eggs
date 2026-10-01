using System;
using System.IO;
using Eggs.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Eggs.Editor
{
    public static class M1APrototypeBuilder
    {
        private const string MenuPath = "Tools/Eggs/Build M1A Prototype";
        private const string ScenePath = "Assets/Eggs/Scenes/Eggs_M1A.unity";
        private const string PrefabPath = "Assets/Eggs/Prefabs/Unit_M1A.prefab";
        private const string ArtFolder = "Assets/Eggs/Generated/M1A";

        [MenuItem(MenuPath)]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            // Never save or discard unrelated scenes on behalf of the user.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                {
                    EditorUtility.DisplayDialog("M1A setup paused",
                        "An open scene has unsaved changes. Save or close it yourself, "
                        + "then run this menu again. No scene has been changed.", "OK");
                    return;
                }
            }

            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Rebuild M1A scene?",
                $"Replace {ScenePath} with a fresh scene containing exactly four units and three zones? "
                + "Scene edits will be lost. Existing Unit prefab and placeholder assets are reused. "
                + "Save a copy of a customized scene before rebuilding.", "Rebuild", "Cancel"))
                return;

            try
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (font == null || shader == null)
                    throw new InvalidOperationException("The built-in font or installed URP sprite shader is missing.");

                EnsureFolder("Assets/Eggs/Scenes");
                EnsureFolder("Assets/Eggs/Prefabs");
                EnsureFolder(ArtFolder);
                Sprite square = GetOrCreateSprite(ArtFolder + "/Square.png", false);
                Sprite circle = GetOrCreateSprite(ArtFolder + "/Circle.png", true);
                Material material = GetOrCreateMaterial(shader);

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.orthographic = true;
                camera.orthographicSize = 5.5f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 100f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
                camera.allowHDR = false;

                WorkZone[] zones =
                {
                    CreateZone("Food Zone", "FOOD", UnitWorkState.Gathering,
                        -4.8f, new Color(0.24f, 0.72f, 0.40f, 0.55f), square, material, font),
                    CreateZone("Love Nest Zone", "LOVE NEST", UnitWorkState.Breeding,
                        0f, new Color(0.86f, 0.38f, 0.54f, 0.55f), square, material, font),
                    CreateZone("Defense Zone", "DEFENSE", UnitWorkState.Defending,
                        4.8f, new Color(0.28f, 0.52f, 0.90f, 0.55f), square, material, font)
                };

                GameObject prefab = GetOrCreateUnitPrefab(circle, material, font);
                Color[] colors =
                {
                    new Color(1f, 0.75f, 0.25f), new Color(0.45f, 0.9f, 0.95f),
                    new Color(0.85f, 0.65f, 1f), new Color(1f, 0.6f, 0.35f)
                };
                for (int i = 0; i < 4; i++)
                {
                    GameObject unit = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    unit.name = $"Unit {i + 1}";
                    unit.transform.position = new Vector3(-3.6f + i * 2.4f, -3f, 0f);
                    unit.GetComponent<SpriteRenderer>().color = colors[i];
                    TextMesh label = unit.GetComponentInChildren<TextMesh>();
                    if (label != null)
                        label.text = unit.name + "\nIdle";
                    PrefabUtility.RecordPrefabInstancePropertyModifications(unit);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(unit.transform);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(unit.GetComponent<SpriteRenderer>());
                    if (label != null)
                        PrefabUtility.RecordPrefabInstancePropertyModifications(label);
                }

                UnitDragController controller = new GameObject("Unit Drag Controller").AddComponent<UnitDragController>();
                SerializedObject controllerData = new SerializedObject(controller);
                controllerData.FindProperty("inputCamera").objectReferenceValue = camera;
                SerializedProperty zoneReferences = controllerData.FindProperty("workZones");
                zoneReferences.arraySize = zones.Length;
                for (int i = 0; i < zones.Length; i++)
                    zoneReferences.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
                controllerData.ApplyModifiedPropertiesWithoutUndo();

                CreateLabel("Instructions", null, new Vector3(0f, 4.6f, 0f),
                    "M1A - Drag units between work zones", font, 0.09f, TextAnchor.MiddleCenter);
                CreateLabel("Idle Hint", null, new Vector3(0f, -4.6f, 0f),
                    "Pick up = Idle    |    Drop outside zones = Idle", font, 0.075f, TextAnchor.MiddleCenter);

                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Unity could not save " + ScenePath);

                Debug.Log("M1A scene saved: " + ScenePath
                    + ". Enter Play and verify drag/state/membership changes. This is not a Play test.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("M1A setup failed", "See the Console for details. "
                    + "Do not treat this build as verified.", "OK");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool CanBuild() => !EditorApplication.isPlayingOrWillChangePlaymode;

        private static WorkZone CreateZone(string name, string heading, UnitWorkState state,
            float x, Color color, Sprite sprite, Material material, Font font)
        {
            GameObject root = new GameObject(name);
            root.transform.position = new Vector3(x, 0.5f, 0f);
            BoxCollider2D area = root.AddComponent<BoxCollider2D>();
            area.size = new Vector2(4f, 3.2f);
            area.isTrigger = true;
            WorkZone zone = root.AddComponent<WorkZone>();
            SerializedObject data = new SerializedObject(zone);
            data.FindProperty("targetState").enumValueIndex = (int)state;
            data.FindProperty("dropArea").objectReferenceValue = area;
            data.FindProperty("gizmoColor").colorValue = color;
            data.ApplyModifiedPropertiesWithoutUndo();

            GameObject visual = new GameObject("Placeholder (replace via SpriteRenderer)");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(4f, 3.2f, 1f);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.color = color;
            renderer.sortingOrder = -10;

            TextMesh label = CreateLabel("Zone Label", root.transform, new Vector3(0f, 2.9f, 0f),
                heading + "\nMembers: 0", font, 0.09f, TextAnchor.UpperCenter);
            WireDebugLabel(root.AddComponent<M1ADebugLabel>(), label, null, zone, heading);
            return zone;
        }

        private static GameObject GetOrCreateUnitPrefab(Sprite sprite, Material material, Font font)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                if (existing.GetComponent<UnitActor>() == null || existing.GetComponent<Collider2D>() == null
                    || existing.GetComponent<SpriteRenderer>() == null)
                    throw new InvalidOperationException("Existing Unit_M1A prefab needs UnitActor, Collider2D and SpriteRenderer. "
                        + "Repair it or move it to another path before rebuilding.");
                return existing;
            }
            if (File.Exists(PrefabPath))
                throw new InvalidOperationException("Cannot read existing prefab: " + PrefabPath);

            GameObject root = new GameObject("Unit_M1A");
            try
            {
                UnitActor unit = root.AddComponent<UnitActor>();
                CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
                collider.radius = 0.48f;
                collider.isTrigger = true;
                SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sharedMaterial = material;
                root.AddComponent<SortingGroup>().sortingOrder = 10;
                TextMesh label = CreateLabel("State Label", root.transform, new Vector3(0f, 0.9f, 0f),
                    "Unit\nIdle", font, 0.065f, TextAnchor.MiddleCenter);
                WireDebugLabel(root.AddComponent<M1ADebugLabel>(), label, unit, null, string.Empty);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                if (prefab == null)
                    throw new InvalidOperationException("Could not create " + PrefabPath);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static TextMesh CreateLabel(string name, Transform parent, Vector3 position,
            string text, Font font, float size, TextAnchor anchor)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            TextMesh label = root.AddComponent<TextMesh>();
            label.font = font;
            label.fontSize = 48;
            label.characterSize = size;
            label.anchor = anchor;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
            label.text = text;
            MeshRenderer renderer = root.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingOrder = 20;
            return label;
        }

        private static void WireDebugLabel(M1ADebugLabel display, TextMesh label,
            UnitActor unit, WorkZone zone, string heading)
        {
            SerializedObject data = new SerializedObject(display);
            data.FindProperty("label").objectReferenceValue = label;
            data.FindProperty("unit").objectReferenceValue = unit;
            data.FindProperty("zone").objectReferenceValue = zone;
            data.FindProperty("heading").stringValue = heading;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite GetOrCreateSprite(string path, bool circle)
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;
            if (File.Exists(path))
                throw new InvalidOperationException("Existing placeholder is not a Sprite: " + path);

            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            try
            {
                Color[] pixels = new Color[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - (size - 1) * 0.5f;
                    float dy = y - (size - 1) * 0.5f;
                    pixels[y * size + x] = !circle || dx * dx + dy * dy <= 15f * 15f
                        ? Color.white : Color.clear;
                }
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            Sprite result = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (result == null)
                throw new InvalidOperationException("Could not import placeholder sprite: " + path);
            return result;
        }

        private static Material GetOrCreateMaterial(Shader shader)
        {
            string path = ArtFolder + "/SpriteUnlit.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            if (File.Exists(path))
                throw new InvalidOperationException("Cannot read existing material: " + path);

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, Path.GetFileName(path))))
                throw new IOException("Could not create asset folder: " + path);
        }
    }
}
