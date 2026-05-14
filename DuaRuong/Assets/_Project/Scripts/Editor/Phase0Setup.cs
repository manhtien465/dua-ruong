#if UNITY_EDITOR
using System.IO;
using System.Linq;
using DuaRuong.Core;
using DuaRuong.Gameplay.Cam;
using DuaRuong.Gameplay.Player;
using DuaRuong.Gameplay.Track;
using DuaRuong.Systems.Input;
using DuaRuong.Systems.Score;
using DuaRuong.UI.HUD;
using DuaRuong.UI.Menus;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DuaRuong.Editor
{
    public static class Phase0Setup
    {
        private const string ROOT     = "Assets/_Project";
        private const string SO_CFG   = ROOT + "/ScriptableObjects/Configs";
        private const string PREFABS  = ROOT + "/Prefabs";
        private const string SCENES   = ROOT + "/Scenes";

        [MenuItem("DuaRuong/Build Phase 0 ▶", priority = 0)]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!EditorUtility.DisplayDialog("Phase 0 Setup",
                    "Tạo SOs, Prefabs, Scenes cho Phase 0.\n" +
                    "File đã tồn tại sẽ bị OVERWRITE để cập nhật logic mới.",
                    "Chạy", "Huỷ"))
                return;

            try
            {
                Step("Folders", 0.00f);
                EnsureFolders();

                Step("ScriptableObjects", 0.10f);
                var playerCfg  = GetOrCreateSO<PlayerConfig> (SO_CFG, "PlayerConfig");
                var scoringCfg = GetOrCreateSO<ScoringConfig>(SO_CFG, "ScoringConfig");
                var spawnCfg   = GetOrCreateSO<SpawnConfig>  (SO_CFG, "SpawnConfig");

                Step("GameManager prefab", 0.25f);
                var gmPrefab = CreateGameManagerPrefab();

                Step("TrackChunk prefab", 0.35f);
                var chunkPrefab = CreateChunkPrefab();
                WireChunkIntoSpawnConfig(spawnCfg, chunkPrefab);

                Step("Bootstrap scene", 0.50f);
                CreateBootstrapScene(gmPrefab);

                Step("Gameplay scene", 0.65f);
                CreateGameplayScene(playerCfg, scoringCfg, spawnCfg);

                Step("Build Settings", 0.90f);
                AddScenesToBuildSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorSceneManager.OpenScene($"{SCENES}/Gameplay.unity");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Done ✅",
                "Phase 0 setup xong!\n\n" +
                "Bước tiếp theo:\n" +
                "1. Double-click Assets/_Project/Scenes/Gameplay.unity\n" +
                "2. Nhấn Play\n\n" +
                "Nếu TMP lỗi: Window → TextMeshPro → Import TMP Essential Resources",
                "OK");
        }

        // ─────────────────────────────────────────────────────────
        // Folders
        // ─────────────────────────────────────────────────────────

        private static void EnsureFolders()
        {
            foreach (var path in new[]
            {
                ROOT + "/ScriptableObjects/Configs",
                PREFABS + "/Player",
                PREFABS + "/Track",
                PREFABS + "/Obstacles",
                PREFABS + "/Items",
                SCENES,
            })
                EnsureFolder(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ─────────────────────────────────────────────────────────
        // ScriptableObjects
        // ─────────────────────────────────────────────────────────

        private static T GetOrCreateSO<T>(string dir, string fileName) where T : ScriptableObject
        {
            var assetPath = $"{dir}/{fileName}.asset";
            var existing  = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null) return existing;

            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, assetPath);
            return so;
        }

        // ─────────────────────────────────────────────────────────
        // Prefabs
        // ─────────────────────────────────────────────────────────

        private static GameManager CreateGameManagerPrefab()
        {
            var path = $"{PREFABS}/Player/GameManager.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<GameManager>();

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<GameManager>();
        }

        private static TrackChunk CreateChunkPrefab()
        {
            var path = $"{PREFABS}/Track/Chunk_A.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<TrackChunk>();

            var root  = new GameObject("Chunk_A");
            var chunk = root.AddComponent<TrackChunk>();

            // Ground plane: Unity Plane = 10×10. Scale (0.6, 1, 3) → 6 wide × 30 long.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localPosition = new Vector3(0f, 0f, 15f);
            ground.transform.localScale    = new Vector3(0.6f, 1f, 3f);
            Object.DestroyImmediate(ground.GetComponent<MeshCollider>());
            var groundCol  = ground.AddComponent<BoxCollider>();
            groundCol.size   = new Vector3(10f, 0.05f, 10f);
            groundCol.center = new Vector3(0f, -0.025f, 0f);

            var startPt = Marker(root.transform, "StartPoint", Vector3.zero);
            var endPt   = Marker(root.transform, "EndPoint",   new Vector3(0f, 0f, 30f));

            // SpawnPoints: 4 rows × 3 tiers = 12 points
            // Each tier is at a different Y (floor of that terrace level).
            // Obstacles placed here use a collider that covers only that tier's player hitbox.
            var spawnParent = new GameObject("SpawnPoints");
            spawnParent.transform.SetParent(root.transform);
            spawnParent.transform.localPosition = Vector3.zero;

            float[] tierSpawnY = { 0f, 2f, 4f };           // floor Y of each terrace tier
            float[] rowZ       = { 7f, 13f, 19f, 25f };
            var spawnPts = new Transform[tierSpawnY.Length * rowZ.Length];
            int idx = 0;
            foreach (var z in rowZ)
                foreach (var y in tierSpawnY)
                    spawnPts[idx++] = Marker(spawnParent.transform, $"SP_{idx}", new Vector3(0f, y, z));

            // Wire via SerializedObject before saving as prefab
            var so = new SerializedObject(chunk);
            so.FindProperty("_startPoint").objectReferenceValue = startPt;
            so.FindProperty("_endPoint").objectReferenceValue   = endPt;
            var spArr = so.FindProperty("_spawnPoints");
            spArr.arraySize = spawnPts.Length;
            for (int i = 0; i < spawnPts.Length; i++)
                spArr.GetArrayElementAtIndex(i).objectReferenceValue = spawnPts[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<TrackChunk>();
        }

        private static void WireChunkIntoSpawnConfig(SpawnConfig cfg, TrackChunk chunk)
        {
            if (cfg == null || chunk == null) return;
            var so   = new SerializedObject(cfg);
            var prop = so.FindProperty("_chunkPrefabs");
            prop.arraySize = 1;
            prop.GetArrayElementAtIndex(0).objectReferenceValue = chunk;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cfg);
        }

        // ─────────────────────────────────────────────────────────
        // Bootstrap scene
        // ─────────────────────────────────────────────────────────

        private static void CreateBootstrapScene(GameManager gmPrefab)
        {
            var path = $"{SCENES}/Bootstrap.unity";
            
            // Just create it as the only scene (Single)
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go        = new GameObject("Bootstrap");
            var bootstrap = go.AddComponent<Bootstrap>();
            var so        = new SerializedObject(bootstrap);
            so.FindProperty("_gameManagerPrefab").objectReferenceValue = gmPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, path);
        }

        // ─────────────────────────────────────────────────────────
        // Gameplay scene
        // ─────────────────────────────────────────────────────────

        private static void CreateGameplayScene(
            PlayerConfig playerCfg, ScoringConfig scoringCfg, SpawnConfig spawnCfg)
        {
            var path = $"{SCENES}/Gameplay.unity";
            
            // Just create it as the only scene (Single)
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Dev starter (Editor-only auto-start) ──
            new GameObject("DevBootstrap").AddComponent<DevBootstrap>();

            // ── InputReader ──
            var inputGo   = new GameObject("InputReader");
            var inputRdr  = inputGo.AddComponent<InputReader>();

            // ── Systems ──
            var sysGo    = new GameObject("Systems");
            sysGo.AddComponent<ComboSystem>();
            var scoreSys = sysGo.AddComponent<ScoreSystem>();
            Set(scoreSys, "_config", scoringCfg);

            // ── Player ──
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 0.5f, 0f);

            // Visual capsule (no collider — moved to root)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(playerGo.transform);
            visual.transform.localPosition = Vector3.zero;
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());

            // Trigger hitbox on root
            var hitbox         = playerGo.AddComponent<CapsuleCollider>();
            hitbox.isTrigger   = true;
            hitbox.height      = 2f;
            hitbox.center      = new Vector3(0f, 0.5f, 0f);

            // Kinematic Rigidbody required for OnTriggerEnter to fire (Unity trigger rule).
            var rb            = playerGo.AddComponent<Rigidbody>();
            rb.isKinematic    = true;
            rb.useGravity     = false;
            rb.interpolation  = RigidbodyInterpolation.Interpolate;

            var movement   = playerGo.AddComponent<PlayerMovement>();
            playerGo.AddComponent<PlayerCollision>();
            var controller = playerGo.AddComponent<PlayerController>();
            var distRep    = playerGo.AddComponent<DistanceReporter>();

            Set(movement,   "_config",      playerCfg);
            Set(controller, "_input",       inputRdr);
            Set(distRep,    "_movement",    movement);
            Set(distRep,    "_scoreSystem", scoreSys);

            // ── Camera ──
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            var followCam = camGo.AddComponent<FollowCamera>();
            camGo.transform.position = new Vector3(-1f, 6f, -10f);
            camGo.transform.rotation = Quaternion.Euler(18f, 5f, 0f);
            Set(followCam, "_target", playerGo.transform);
            // Slightly left + higher angle to show the stepped terrace tiers beside the player.
            SetVec3(followCam, "_localOffset", new Vector3(-1f, 6f, -10f));
            SetFloat(followCam, "_lookDownAngle", 18f);

            // ── TrackGenerator ──
            var chunkParent = new GameObject("Chunks");
            var trkGenGo    = new GameObject("TrackGenerator");
            var trackGen    = trkGenGo.AddComponent<TrackGenerator>();
            Set(trackGen, "_config",      spawnCfg);
            Set(trackGen, "_player",      playerGo.transform);
            Set(trackGen, "_chunkParent", chunkParent.transform);

            // ── Directional Light ──
            var lightGo = new GameObject("Directional Light");
            var lt      = lightGo.AddComponent<Light>();
            lt.type         = LightType.Directional;
            lt.intensity    = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // ── Canvas & UI ──
            var evSysGo = new GameObject("EventSystem");
            evSysGo.AddComponent<EventSystem>();
            
            // Use reflection to add New Input System module if package exists, to avoid compile errors
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                evSysGo.AddComponent(inputModuleType);
            }
            else
            {
                evSysGo.AddComponent<StandaloneInputModule>();
            }

            var canvasGo    = new GameObject("Canvas");
            var canvas      = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler      = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();

            // HUD
            var hudGo  = Child(canvasGo.transform, "HUD", true);
            Stretch(hudGo);
            var hudCtrl = hudGo.AddComponent<HudController>();

            var scoreText = TMP(hudGo.transform, "ScoreText", "0",
                Anchor.TopCenter, new Vector2(0, -80), new Vector2(400, 60), 48);
            var distText = TMP(hudGo.transform, "DistText", "0m",
                Anchor.TopLeft, new Vector2(120, -80), new Vector2(200, 50), 30);
            var comboGo  = Child(hudGo.transform, "ComboContainer", true);
            PlaceRt(comboGo, Anchor.TopRight, new Vector2(-120, -80), new Vector2(200, 50));
            var comboGroup = comboGo.AddComponent<CanvasGroup>();
            comboGroup.alpha = 0f;
            var comboText = TMP(comboGo.transform, "ComboText", "x1",
                Anchor.Stretch, Vector2.zero, Vector2.zero, 30);

            Set(hudCtrl, "_scoreText",    scoreText);
            Set(hudCtrl, "_distanceText", distText);
            Set(hudCtrl, "_comboText",    comboText);
            Set(hudCtrl, "_comboGroup",   comboGroup);

            // Game Over
            // GameOverController on a persistent parent; _root is the panel that gets shown/hidden.
            var goSysGo  = Child(canvasGo.transform, "GameOverSystem", true);
            Stretch(goSysGo);
            var goCtrl = goSysGo.AddComponent<GameOverController>();

            var goPanelGo = Child(goSysGo.transform, "GameOverPanel", true);
            Stretch(goPanelGo);
            var goBg = goPanelGo.AddComponent<Image>();
            goBg.color = new Color(0f, 0f, 0f, 0.85f);

            var goScore = TMP(goPanelGo.transform, "ScoreText", "0",
                Anchor.Center, new Vector2(0, 150), new Vector2(500, 80), 64);
            var goBest = TMP(goPanelGo.transform, "BestText", "Best: 0",
                Anchor.Center, new Vector2(0, 70), new Vector2(400, 60), 36);
            var playAgainBtn = Btn(goPanelGo.transform, "PlayAgainBtn", "CHƠI LẠI",
                new Vector2(0, -30), new Vector2(300, 80));
            var menuBtn = Btn(goPanelGo.transform, "MenuBtn", "MENU",
                new Vector2(0, -130), new Vector2(200, 60));

            Set(goCtrl, "_root",           goPanelGo);
            Set(goCtrl, "_scoreSystem",    scoreSys);
            Set(goCtrl, "_scoreText",      goScore);
            Set(goCtrl, "_bestText",       goBest);
            Set(goCtrl, "_playAgainButton",playAgainBtn);
            Set(goCtrl, "_menuButton",     menuBtn);

            // Main Menu panel (shows at start, hidden when Play pressed)
            var mmPanelGo = Child(canvasGo.transform, "MainMenuPanel", true);
            Stretch(mmPanelGo);
            mmPanelGo.AddComponent<Image>().color = new Color(0.08f, 0.22f, 0.08f, 0.95f);
            var mmCtrl = mmPanelGo.AddComponent<MainMenuController>();

            TMP(mmPanelGo.transform, "Title", "ĐUA RUỘNG",
                Anchor.Center, new Vector2(0, 300), new Vector2(600, 110), 72);
            var mmHiScore = TMP(mmPanelGo.transform, "HighScoreText", "Best: 0",
                Anchor.Center, new Vector2(0, 170), new Vector2(400, 60), 36);
            var mmPlayBtn = Btn(mmPanelGo.transform, "PlayButton", "CHƠI",
                new Vector2(0, 50), new Vector2(280, 90));

            Set(mmCtrl, "_playButton",     mmPlayBtn);
            Set(mmCtrl, "_highScoreText",  mmHiScore);

            // DevBootstrap + MainMenu are both in scene.
            // DevBootstrap calls StartGame() in Editor → player runs immediately.
            // In real build, Bootstrap → Gameplay → MainMenu shows first.

            EditorSceneManager.SaveScene(scene, path);
        }

        // ─────────────────────────────────────────────────────────
        // Build Settings
        // ─────────────────────────────────────────────────────────

        private static void AddScenesToBuildSettings()
        {
            var toAdd   = new[] { $"{SCENES}/Bootstrap.unity", $"{SCENES}/Gameplay.unity" };
            var current = EditorBuildSettings.scenes.ToList();
            bool changed = false;
            foreach (var s in toAdd)
            {
                if (current.Any(x => x.path == s)) continue;
                current.Add(new EditorBuildSettingsScene(s, true));
                changed = true;
            }
            if (changed) EditorBuildSettings.scenes = current.ToArray();
        }

        // ─────────────────────────────────────────────────────────
        // Helpers — SerializedObject setter
        // ─────────────────────────────────────────────────────────

        private static void Set(Object target, string propName, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVec3(Object target, string propName, Vector3 value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propName).vector3Value = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string propName, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propName).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─────────────────────────────────────────────────────────
        // Helpers — Scene hierarchy
        // ─────────────────────────────────────────────────────────

        private static Transform Marker(Transform parent, string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        private static GameObject Child(Transform parent, string name, bool isUI = false)
        {
            var go = new GameObject(name);
            if (isUI) go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        // ─────────────────────────────────────────────────────────
        // Helpers — UI / RectTransform
        // ─────────────────────────────────────────────────────────

        private enum Anchor { TopLeft, TopCenter, TopRight, Center, Stretch }

        private static void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            
            rt.anchorMin    = Vector2.zero;
            rt.anchorMax    = Vector2.one;
            rt.sizeDelta    = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void PlaceRt(GameObject go, Anchor anchor, Vector2 pos, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            var (min, max) = AnchorVectors(anchor);
            rt.anchorMin    = min;
            rt.anchorMax    = max;
            rt.pivot        = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta    = size;
        }

        private static TMP_Text TMP(Transform parent, string name, string text,
            Anchor anchor, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = Child(parent, name, true);
            PlaceRt(go, anchor, pos, size);
            var t           = go.AddComponent<TextMeshProUGUI>();
            t.text          = text;
            t.fontSize      = fontSize;
            t.alignment     = TextAlignmentOptions.Center;
            t.color         = Color.white;
            return t;
        }

        private static Button Btn(Transform parent, string name, string label,
            Vector2 pos, Vector2 size)
        {
            var go = Child(parent, name, true);
            PlaceRt(go, Anchor.Center, pos, size);
            go.AddComponent<Image>().color = new Color(0.18f, 0.55f, 0.18f);
            var btn = go.AddComponent<Button>();

            var lbl = Child(go.transform, "Label", true);
            PlaceRt(lbl, Anchor.Stretch, Vector2.zero, Vector2.zero);
            var t           = lbl.AddComponent<TextMeshProUGUI>();
            t.text          = label;
            t.fontSize      = 34;
            t.fontStyle     = FontStyles.Bold;
            t.alignment     = TextAlignmentOptions.Center;
            t.color         = Color.white;

            return btn;
        }

        private static (Vector2 min, Vector2 max) AnchorVectors(Anchor a) => a switch
        {
            Anchor.TopLeft   => (new Vector2(0f, 1f),   new Vector2(0f, 1f)),
            Anchor.TopCenter => (new Vector2(0.5f, 1f), new Vector2(0.5f, 1f)),
            Anchor.TopRight  => (new Vector2(1f, 1f),   new Vector2(1f, 1f)),
            Anchor.Center    => (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)),
            Anchor.Stretch   => (Vector2.zero, Vector2.one),
            _                => (Vector2.zero, Vector2.one),
        };

        private static void Step(string msg, float t)
            => EditorUtility.DisplayProgressBar("Phase 0 Setup", msg + "…", t);
    }
}
#endif
