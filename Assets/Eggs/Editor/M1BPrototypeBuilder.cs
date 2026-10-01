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
        // Builder defaults. Generated WorkZone radii remain editable in the Inspector.
        private const float NestRadius = 1.1f;
        private const float GuardRadius = 2.5f;
        private const float FoodARadius = 3.7f;
        private const float FoodBRadius = 4.9f;

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
                if (!(NestRadius > 0f && GuardRadius > NestRadius && FoodARadius > GuardRadius
                    && FoodBRadius > FoodARadius && !float.IsInfinity(FoodBRadius)))
                    throw new InvalidOperationException("Ring radii must increase from Nest to Food B.");
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
                Material lineMaterial = GetOrCreateLineMaterial();
                EggHatch eggPrefab = GetOrCreateEggPrefab(unitPrefab, eggSprite, material, font);

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                Camera camera = CreateCamera();
                GameState state = new GameObject("Game State").AddComponent<GameState>();
                SerializedObject stateData = new SerializedObject(state);
                int initialPopulation = stateData.FindProperty("startingPopulation").intValue;
                int initialFood = stateData.FindProperty("startingFood").intValue;

                Transform center = new GameObject("Love Nest Center").transform;
                center.position = Vector3.zero;
                WorkZone[] zones =
                {
                    CreateRing("Nest Core", "LOVE NEST", UnitWorkState.Breeding, center,
                        0f, NestRadius, false, new Vector3(7f, 3.2f, 0f),
                        new Color(1f, 0.52f, 0.7f), lineMaterial, font),
                    CreateRing("Guard Ring", "GUARD / IDLE", UnitWorkState.Idle, center,
                        NestRadius, GuardRadius, false, new Vector3(7f, 1.4f, 0f),
                        new Color(0.4f, 0.75f, 1f), lineMaterial, font),
                    CreateRing("Food Ring A", "FOOD A", UnitWorkState.Gathering, center,
                        GuardRadius, FoodARadius, false, new Vector3(7f, -0.4f, 0f),
                        new Color(0.4f, 0.95f, 0.5f), lineMaterial, font),
                    CreateRing("Food Ring B", "FOOD B", UnitWorkState.Gathering, center,
                        FoodARadius, FoodBRadius, true, new Vector3(7f, -2.2f, 0f),
                        new Color(1f, 0.84f, 0.35f), lineMaterial, font)
                };
                WorkZone guard = zones[1];
                SpriteRenderer nest = CreateSprite("Nest Visual", center, Vector3.zero,
                    nestSprite, material, -8);
                nest.transform.localScale = Vector3.one * (NestRadius * 2f / nestSprite.bounds.size.x);

                for (int i = 0; i < initialPopulation; i++)
                {
                    GameObject unit = (GameObject)PrefabUtility.InstantiatePrefab(unitPrefab.gameObject, scene);
                    unit.name = $"Unit {i + 1}";
                    unit.transform.position = GuardPosition(guard, i * (360f / initialPopulation));
                    UnitActor actor = unit.GetComponent<UnitActor>();
                    Configure(actor, data => data.FindProperty("initialZone").objectReferenceValue = guard);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(actor);
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
                    SerializedProperty foodZones = data.FindProperty("foodZones");
                    foodZones.arraySize = 2;
                    foodZones.GetArrayElementAtIndex(0).objectReferenceValue = zones[2];
                    foodZones.GetArrayElementAtIndex(1).objectReferenceValue = zones[3];
                });

                Transform spawn = new GameObject("Egg Spawn Point").transform;
                spawn.SetParent(center, false);
                Transform returnA = CreatePoint("Post Breed Return A", center, GuardPosition(guard, 45f));
                Transform returnB = CreatePoint("Post Breed Return B", center, GuardPosition(guard, 135f));
                Transform newborn = CreatePoint("Newborn Spawn Point", center, GuardPosition(guard, 225f));
                TextMesh status = CreateLabel("Breeding Status", null, new Vector3(-7f, 0f, 0f),
                    "DROP 2 UNITS TO BREED", font, 0.06f);
                BreedingSystem breeding = new GameObject("Breeding System").AddComponent<BreedingSystem>();
                Configure(breeding, data =>
                {
                    data.FindProperty("gameState").objectReferenceValue = state;
                    data.FindProperty("loveNestZone").objectReferenceValue = zones[0];
                    data.FindProperty("standbyZone").objectReferenceValue = guard;
                    data.FindProperty("postBreedReturnPointA").objectReferenceValue = returnA;
                    data.FindProperty("postBreedReturnPointB").objectReferenceValue = returnB;
                    data.FindProperty("newbornSpawnPoint").objectReferenceValue = newborn;
                    data.FindProperty("eggPrefab").objectReferenceValue = eggPrefab;
                    data.FindProperty("eggSpawnPoint").objectReferenceValue = spawn;
                    data.FindProperty("statusLabel").objectReferenceValue = status;
                });

                CreateHud(state, foodSprite, populationSprite, initialFood, initialPopulation, square, material, font);
                CreateLabel("Instructions", null, new Vector3(0f, -5.65f, 0f),
                    "M1B - Food rings > Central Nest > Egg > Population", font, 0.075f);
                CreateLabel("Idle Hint", null, new Vector3(0f, -6.25f, 0f),
                    "Guard = Idle   |   Nest locks 2 units   |   Outside rings = unassigned Idle", font, 0.052f);
                CreateLabel("Ring Legend", null, new Vector3(7f, 4.5f, 0f), "INNER > OUTER", font, 0.055f);

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
            camera.orthographicSize = Mathf.Max(6.8f, FoodBRadius + 1.9f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
            camera.allowHDR = false;
            return camera;
        }

        private static WorkZone CreateRing(string name, string heading, UnitWorkState state, Transform center,
            float inner, float outer, bool includeOuter, Vector3 labelPosition, Color color, Material material, Font font)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(center, false);
            WorkZone zone = root.AddComponent<WorkZone>();
            Configure(zone, data =>
            {
                data.FindProperty("shape").enumValueIndex = (int)WorkZoneShape.Ring;
                data.FindProperty("targetState").enumValueIndex = (int)state;
                data.FindProperty("ringCenter").objectReferenceValue = center;
                data.FindProperty("innerRadius").floatValue = inner;
                data.FindProperty("outerRadius").floatValue = outer;
                data.FindProperty("includeOuterBoundary").boolValue = includeOuter;
                data.FindProperty("acceptUnits").boolValue = true;
                data.FindProperty("resourceAvailable").boolValue = true;
                data.FindProperty("gizmoColor").colorValue = color;
            });

            // One outer outline per concentric zone; adjacent zones share the same seam.
            LineRenderer line = new GameObject("Ring Outline").AddComponent<LineRenderer>();
            line.transform.SetParent(root.transform, false);
            line.sharedMaterial = material;
            line.startColor = color;
            line.endColor = color;
            line.widthMultiplier = 0.035f;
            line.sortingOrder = -5;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            RingZoneVisual visual = root.AddComponent<RingZoneVisual>();
            Configure(visual, data =>
            {
                data.FindProperty("zone").objectReferenceValue = zone;
                data.FindProperty("outline").objectReferenceValue = line;
            });
            visual.Refresh();
            TextMesh label = CreateLabel("Zone Label", root.transform, labelPosition,
                heading + "\nMembers: 0", font, 0.075f);
            label.color = color;
            Configure(root.AddComponent<M1ADebugLabel>(), data =>
            {
                data.FindProperty("label").objectReferenceValue = label;
                data.FindProperty("zone").objectReferenceValue = zone;
                data.FindProperty("heading").stringValue = heading;
            });
            return zone;
        }

        private static Vector3 GuardPosition(WorkZone guard, float degrees)
        {
            float angle = degrees * Mathf.Deg2Rad;
            float radius = (guard.InnerRadius + guard.OuterRadius) * 0.5f;
            return guard.RingCenter.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }

        private static Transform CreatePoint(string name, Transform parent, Vector3 position)
        {
            Transform point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.position = position;
            return point;
        }

        private static void CreateHud(GameState state, Sprite food, Sprite population, int initialFood,
            int initialPopulation, Sprite square, Material material, Font font)
        {
            GameObject root = new GameObject("HUD");
            root.transform.position = new Vector3(0f, 0.95f, 0f);
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

        private static Material GetOrCreateLineMaterial()
        {
            const string path = "Assets/Eggs/Generated/M1B/RingOutline.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;
            if (File.Exists(path))
                throw new InvalidOperationException("Cannot read existing ring outline material: " + path);

            // LineRenderer needs a vertex-color shader without SpriteRenderer-only properties.
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
                throw new InvalidOperationException("Missing installed URP Particles/Unlit shader for ring outlines.");
            EnsureFolder("Assets/Eggs/Generated/M1B");
            Material material = new Material(shader);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
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
