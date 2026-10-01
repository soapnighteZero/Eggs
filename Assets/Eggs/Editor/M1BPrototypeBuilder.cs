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
    public static class M1BPrototypeBuilder
    {
        private const string MenuPath = "Tools/Eggs/Build M1B Prototype";
        private const string ScenePath = "Assets/Eggs/Scenes/Eggs_M1B.unity";
        private const string UnitPath = "Assets/Eggs/Prefabs/Unit_M1A.prefab";
        private const string EggPath = "Assets/Eggs/Prefabs/Egg_M1B.prefab";
        private const string ArtRoot = "Assets/Eggs/Art/";

        [MenuItem(MenuPath)]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                {
                    EditorUtility.DisplayDialog("M1B setup paused",
                        "An open scene has unsaved changes. Save or close it yourself, "
                        + "then run this menu again. No scene has been changed.", "OK");
                    return;
                }
            }

            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Rebuild M1B scene?",
                $"Replace {ScenePath} with a fresh M1B scene? Scene edits will be lost. "
                + "Existing Unit and Egg prefabs are reused. Save a copy of a customized scene first.",
                "Rebuild", "Cancel"))
                return;

            try
            {
                // Resolve every existing dependency before replacing the open scene.
                UnitActor unitPrefab = RequireUnitPrefab();
                Sprite square = RequireSprite("Assets/Eggs/Generated/M1A/Square.png");
                Material material = RequireAsset<Material>("Assets/Eggs/Generated/M1A/SpriteUnlit.mat");
                Sprite eggSprite = RequireSprite(ArtRoot + "Props/Egg/egg_neutral.png");
                Sprite nestSprite = RequireSprite(ArtRoot + "Props/LoveNest/love_nest_base.png");
                Sprite foodSprite = RequireSprite(ArtRoot + "UI/Labels/food_label_zh.png");
                Sprite populationSprite = RequireSprite(ArtRoot + "UI/Labels/population_label_zh.png");
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null)
                    throw new InvalidOperationException("Missing built-in LegacyRuntime.ttf font.");

                EnsureFolder("Assets/Eggs/Scenes");
                EnsureFolder("Assets/Eggs/Prefabs");
                EggHatch eggPrefab = GetOrCreateEggPrefab(unitPrefab, eggSprite, material, font);

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                Camera camera = CreateCamera();
                GameState state = new GameObject("Game State").AddComponent<GameState>();
                SerializedObject stateData = new SerializedObject(state);
                int initialPopulation = stateData.FindProperty("startingPopulation").intValue;
                int initialFood = stateData.FindProperty("startingFood").intValue;

                WorkZone[] zones =
                {
                    CreateZone("Food Zone", "FOOD", UnitWorkState.Gathering, -5.8f,
                        new Color(0.24f, 0.72f, 0.40f, 0.55f), square, material, font),
                    CreateZone("Love Nest Zone", "LOVE NEST", UnitWorkState.Breeding, 0f,
                        new Color(0.86f, 0.38f, 0.54f, 0.30f), square, material, font),
                    CreateZone("Defense Zone", "DEFENSE", UnitWorkState.Defending, 5.8f,
                        new Color(0.28f, 0.52f, 0.90f, 0.55f), square, material, font)
                };
                SpriteRenderer nest = CreateSprite("Nest Visual", zones[1].transform, Vector3.zero,
                    nestSprite, material, -8);
                nest.transform.localScale = Vector3.one * 1.25f;

                for (int i = 0; i < initialPopulation; i++)
                {
                    GameObject unit = (GameObject)PrefabUtility.InstantiatePrefab(unitPrefab.gameObject, scene);
                    unit.name = $"Unit {i + 1}";
                    unit.transform.position = new Vector3(-3.6f + i * 2.4f, -3.7f, 0f);
                    TextMesh label = unit.GetComponentInChildren<TextMesh>();
                    if (label != null)
                    {
                        label.text = unit.name + "\nIdle";
                        PrefabUtility.RecordPrefabInstancePropertyModifications(label);
                    }
                    PrefabUtility.RecordPrefabInstancePropertyModifications(unit);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(unit.transform);
                }

                UnitDragController controller = new GameObject("Unit Drag Controller").AddComponent<UnitDragController>();
                Configure(controller, data =>
                {
                    data.FindProperty("inputCamera").objectReferenceValue = camera;
                    SerializedProperty references = data.FindProperty("workZones");
                    references.arraySize = zones.Length;
                    for (int i = 0; i < zones.Length; i++)
                        references.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
                });

                FoodProductionSystem food = new GameObject("Food Production").AddComponent<FoodProductionSystem>();
                Configure(food, data =>
                {
                    data.FindProperty("gameState").objectReferenceValue = state;
                    data.FindProperty("foodZone").objectReferenceValue = zones[0];
                });

                Transform spawn = new GameObject("Egg Spawn Point").transform;
                spawn.position = new Vector3(0f, 0.5f, 0f);
                TextMesh status = CreateLabel("Breeding Status", null, new Vector3(0f, -1.65f, 0f),
                    "DROP 2 UNITS TO BREED", font, 0.06f);
                BreedingSystem breeding = new GameObject("Breeding System").AddComponent<BreedingSystem>();
                Configure(breeding, data =>
                {
                    data.FindProperty("gameState").objectReferenceValue = state;
                    data.FindProperty("loveNestZone").objectReferenceValue = zones[1];
                    data.FindProperty("eggPrefab").objectReferenceValue = eggPrefab;
                    data.FindProperty("eggSpawnPoint").objectReferenceValue = spawn;
                    data.FindProperty("statusLabel").objectReferenceValue = status;
                });

                CreateHud(state, foodSprite, populationSprite, initialFood, initialPopulation, square, material, font);
                CreateLabel("Instructions", null, new Vector3(0f, 4f, 0f),
                    "M1B - Gather Food > Breed > Egg > Grow", font, 0.075f);
                CreateLabel("Idle Hint", null, new Vector3(0f, -5.1f, 0f),
                    "Pick up = Idle   |   Breeding locks 2 units   |   Drop again for another cycle", font, 0.052f);

                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new IOException("Could not save " + ScenePath);
                Selection.activeObject = state.gameObject;
                Debug.Log("M1B scene saved. Pending Unity Play Verification: "
                    + "test Food, one breeding cycle, hatch and draggable Unit 5.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("M1B setup failed",
                    "See the Console for the missing asset or setup error. Do not treat this build as verified.", "OK");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool CanBuild() => !EditorApplication.isPlayingOrWillChangePlaymode;

        private static Camera CreateCamera()
        {
            GameObject root = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            root.tag = "MainCamera";
            Camera camera = root.GetComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
            camera.allowHDR = false;
            return camera;
        }

        private static WorkZone CreateZone(string name, string heading, UnitWorkState state, float x,
            Color color, Sprite square, Material material, Font font)
        {
            GameObject root = new GameObject(name);
            root.transform.position = new Vector3(x, 0.5f, 0f);
            BoxCollider2D area = root.AddComponent<BoxCollider2D>();
            area.size = new Vector2(4.6f, 3.6f);
            area.isTrigger = true;
            WorkZone zone = root.AddComponent<WorkZone>();
            Configure(zone, data =>
            {
                data.FindProperty("targetState").enumValueIndex = (int)state;
                data.FindProperty("dropArea").objectReferenceValue = area;
                data.FindProperty("gizmoColor").colorValue = color;
            });

            SpriteRenderer visual = CreateSprite("Zone Area", root.transform, Vector3.zero, square, material, -10);
            visual.transform.localScale = new Vector3(4.6f, 3.6f, 1f);
            visual.color = color;
            TextMesh label = CreateLabel("Zone Label", root.transform, new Vector3(0f, 2.55f, 0f),
                heading + "\nMembers: 0", font, 0.075f);
            Configure(root.AddComponent<M1ADebugLabel>(), data =>
            {
                data.FindProperty("label").objectReferenceValue = label;
                data.FindProperty("zone").objectReferenceValue = zone;
                data.FindProperty("heading").stringValue = heading;
            });
            return zone;
        }

        private static void CreateHud(GameState state, Sprite food, Sprite population, int initialFood,
            int initialPopulation, Sprite square, Material material, Font font)
        {
            GameObject root = new GameObject("HUD");
            SpriteRenderer panel = CreateSprite("HUD Background", root.transform, new Vector3(0f, 5f, 0f),
                square, material, 25);
            panel.transform.localScale = new Vector3(17.6f, 1.3f, 1f);
            panel.color = new Color(0.88f, 0.9f, 0.84f);
            CreateSprite("Food Label", root.transform, new Vector3(-6.5f, 5f, 0f), food, material, 30);
            CreateSprite("Population Label", root.transform, new Vector3(2.1f, 5f, 0f), population, material, 30);
            TextMesh foodValue = CreateLabel("Food Value", root.transform, new Vector3(-4.7f, 5f, 0f),
                initialFood.ToString(), font, 0.12f);
            TextMesh populationValue = CreateLabel("Population Value", root.transform, new Vector3(4.5f, 5f, 0f),
                initialPopulation.ToString(), font, 0.12f);
            foodValue.color = Color.black;
            populationValue.color = Color.black;
            Configure(root.AddComponent<M1BHud>(), data =>
            {
                data.FindProperty("gameState").objectReferenceValue = state;
                data.FindProperty("foodValue").objectReferenceValue = foodValue;
                data.FindProperty("populationValue").objectReferenceValue = populationValue;
            });
        }

        private static EggHatch GetOrCreateEggPrefab(UnitActor unit, Sprite sprite, Material material, Font font)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(EggPath);
            if (existing != null)
            {
                EggHatch egg = existing.GetComponent<EggHatch>();
                if (egg == null || !egg.enabled || !existing.activeSelf || !egg.HasValidUnitPrefab
                    || existing.GetComponentInChildren<SpriteRenderer>() == null
                    || new SerializedObject(egg).FindProperty("unitPrefab").objectReferenceValue != unit)
                    throw new InvalidOperationException("Repair the existing Egg prefab and its Unit reference: " + EggPath);
                return egg;
            }
            if (File.Exists(EggPath))
                throw new InvalidOperationException("Cannot read existing prefab: " + EggPath);

            GameObject root = new GameObject("Egg_M1B");
            try
            {
                root.AddComponent<SortingGroup>().sortingOrder = 15;
                CreateSprite("Visual", root.transform, Vector3.zero, sprite, material, 0);
                TextMesh label = CreateLabel("Countdown", root.transform, new Vector3(0f, 0.95f, 0f),
                    "EGG 4.0s", font, 0.06f);
                EggHatch egg = root.AddComponent<EggHatch>();
                Configure(egg, data =>
                {
                    data.FindProperty("unitPrefab").objectReferenceValue = unit;
                    data.FindProperty("countdownLabel").objectReferenceValue = label;
                });
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, EggPath);
                if (saved == null)
                    throw new IOException("Could not create " + EggPath);
                return saved.GetComponent<EggHatch>();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static UnitActor RequireUnitPrefab()
        {
            GameObject prefab = RequireAsset<GameObject>(UnitPath);
            UnitActor unit = prefab.GetComponent<UnitActor>();
            Collider2D collider = prefab.GetComponent<Collider2D>();
            if (!prefab.activeSelf || unit == null || !unit.enabled || collider == null || !collider.enabled
                || prefab.GetComponentInChildren<SpriteRenderer>() == null || prefab.GetComponent<SortingGroup>() == null)
                throw new InvalidOperationException("Unit prefab needs active UnitActor, Collider2D, SpriteRenderer "
                    + "and SortingGroup: " + UnitPath);
            return unit;
        }

        private static T RequireAsset<T>(string path) where T : Object
        {
            T result = AssetDatabase.LoadAssetAtPath<T>(path);
            if (result == null)
                throw new InvalidOperationException("Missing or invalid " + typeof(T).Name + " asset: " + path);
            return result;
        }

        private static Sprite RequireSprite(string path)
        {
            // Supports either Single import or a single named Sprite subasset; no importer edits.
            Sprite result = null;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (!(asset is Sprite sprite))
                    continue;
                if (result != null)
                    throw new InvalidOperationException("Expected one Sprite, found multiple: " + path);
                result = sprite;
            }
            if (result == null)
                throw new InvalidOperationException("Missing Sprite. Check its Unity import settings: " + path);
            return result;
        }

        private static SpriteRenderer CreateSprite(string name, Transform parent, Vector3 position,
            Sprite sprite, Material material, int order)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.color = Color.white;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static TextMesh CreateLabel(string name, Transform parent, Vector3 position,
            string text, Font font, float size)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            TextMesh label = root.AddComponent<TextMesh>();
            label.font = font;
            label.fontSize = 48;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
            label.text = text;
            MeshRenderer renderer = root.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingOrder = 40;
            return label;
        }

        private static void Configure(Object target, Action<SerializedObject> configure)
        {
            SerializedObject data = new SerializedObject(target);
            configure(data);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'),
                Path.GetFileName(path))))
                throw new IOException("Could not create asset folder: " + path);
        }
    }
}
