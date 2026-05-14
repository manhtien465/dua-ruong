#if UNITY_EDITOR
using System.IO;
using DuaRuong.Gameplay.Items;
using DuaRuong.Gameplay.Obstacles;
using DuaRuong.Gameplay.Track;
using DuaRuong.Systems.Audio;
using DuaRuong.UI;
using DuaRuong.UI.HUD;
using DuaRuong.UI.Menus;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DuaRuong.Editor
{
    /// <summary>
    /// Creates placeholder obstacle and item prefabs, wires them into SpawnConfig,
    /// and removes the hardcoded test obstacle from the Gameplay scene.
    /// Run this after Phase 0 setup: DuaRuong → Build Phase 1 ▶
    /// </summary>
    public static class Phase1Setup
    {
        private const string ROOT    = "Assets/_Project";
        private const string SO_CFG  = ROOT + "/ScriptableObjects/Configs";
        private const string PREFABS = ROOT + "/Prefabs";
        private const string SCENES  = ROOT + "/Scenes";
        private const string MATS    = ROOT + "/Art/Materials";

        [MenuItem("DuaRuong/Build Phase 1 ▶", priority = 1)]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (!EditorUtility.DisplayDialog("Phase 1 Setup",
                    "Tạo obstacle/item prefabs và wire vào SpawnConfig.\n" +
                    "Prefab đã tồn tại sẽ bị OVERWRITE.",
                    "Chạy", "Huỷ"))
                return;

            try
            {
                Step("Folders + Materials", 0.05f);
                EnsureFolders();
                var matBrown  = GetOrCreateMat("Mat_Obstacle_Buffalo",     new Color(0.35f, 0.20f, 0.05f));
                var matTan    = GetOrCreateMat("Mat_Obstacle_BambooFence", new Color(0.75f, 0.65f, 0.35f));
                var matMud    = GetOrCreateMat("Mat_Obstacle_MudPit",      new Color(0.35f, 0.25f, 0.15f));
                var matRice   = GetOrCreateMat("Mat_Item_Rice",            new Color(1.00f, 0.90f, 0.10f));
                var matGold   = GetOrCreateMat("Mat_Item_GoldBuffalo",     new Color(1.00f, 0.75f, 0.00f));

                Step("Obstacle prefabs", 0.30f);
                var buffaloPrefab = CreateBuffaloPrefab(matBrown);
                var fencePrefab   = CreateBambooFencePrefab(matTan);
                var mudPrefab     = CreateMudPitPrefab(matMud);

                Step("Item prefabs", 0.55f);
                var ricePrefab  = CreateRicePrefab(matRice);
                var goldPrefab  = CreateGoldBuffaloPrefab(matGold);

                Step("SpawnConfig", 0.70f);
                WireSpawnConfig(
                    new ObstacleBase[] { buffaloPrefab, fencePrefab, mudPrefab },
                    new ItemBase[]     { ricePrefab, goldPrefab });

                Step("AudioDatabase", 0.78f);
                GetOrCreateAudioDatabase();

                Step("Chunk prefab — tier spawn points", 0.82f);
                UpdateChunkPrefabForTiers();

                Step("Gameplay scene cleanup", 0.88f);
                CleanupGameplayScene();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Done ✅",
                "Phase 1 xong! Tier system đã được áp dụng.\n\n" +
                "Bước tiếp theo:\n" +
                "1. Mở Assets/_Project/Scenes/Gameplay.unity\n" +
                "2. Nhấn Play\n" +
                "   - Swipe UP/W  = leo lên bậc trên\n" +
                "   - Swipe DOWN/S = tụt xuống bậc dưới\n" +
                "   - Swipe LEFT/A / RIGHT/D = dodge ngang\n\n" +
                "Obstacles giờ chặn từng bậc — phải đổi tier để tránh.\n" +
                "Tuning: TierY, TierChangeDuration, DodgeDistance trong PlayerConfig asset.",
                "OK");
        }

        // ─────────────────────────────────────────────────────────
        // Folders
        // ─────────────────────────────────────────────────────────

        private static void EnsureFolders()
        {
            foreach (var path in new[]
            {
                ROOT + "/Art",
                ROOT + "/Art/Materials",
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
        // Materials
        // ─────────────────────────────────────────────────────────

        private static Material GetOrCreateMat(string name, Color color)
        {
            var path = $"{MATS}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                SetMatColor(existing, color);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Standard");
            var mat = new Material(shader);
            SetMatColor(mat, color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void SetMatColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        }

        // ─────────────────────────────────────────────────────────
        // Obstacle prefabs
        // ─────────────────────────────────────────────────────────

        // Buffalo: blocking a terrace tier. Player must climb/drop to a different tier.
        // Collider covers y=[0.2, 1.8] local — fits inside one tier's 2m window with 0.2m margin
        // so adjacent-tier players can't accidentally trigger it during transition.
        private static ObstacleBase CreateBuffaloPrefab(Material mat)
        {
            var path = $"{PREFABS}/Obstacles/Buffalo_Obstacle.prefab";

            var root = new GameObject("Buffalo_Obstacle");

            var col      = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center    = new Vector3(0f, 1.0f, 0f);
            col.size      = new Vector3(1.2f, 1.6f, 0.9f);

            var comp = root.AddComponent<BuffaloObstacle>();
            SetEnum(comp, "_type", (int)ObstacleType.BuffaloStanding);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            visual.transform.localScale    = new Vector3(1.2f, 1.6f, 0.9f);
            if (mat != null) visual.GetComponent<Renderer>().sharedMaterial = mat;

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<BuffaloObstacle>();
        }

        // BambooFence: wide barrier across a tier, same dodge mechanic (climb/drop tier).
        private static ObstacleBase CreateBambooFencePrefab(Material mat)
        {
            var path = $"{PREFABS}/Obstacles/BambooFence_Obstacle.prefab";

            var root = new GameObject("BambooFence_Obstacle");

            var col      = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center    = new Vector3(0f, 1.0f, 0f);
            col.size      = new Vector3(1.8f, 1.6f, 0.3f);

            var comp = root.AddComponent<BambooFenceObstacle>();
            SetEnum(comp, "_type", (int)ObstacleType.BambooFenceHigh);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            visual.transform.localScale    = new Vector3(1.8f, 1.6f, 0.3f);
            if (mat != null) visual.GetComponent<Renderer>().sharedMaterial = mat;

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<BambooFenceObstacle>();
        }

        // MudPit: low-profile terrain hazard on a tier. Dodge by lateral dodge or switching tier.
        private static ObstacleBase CreateMudPitPrefab(Material mat)
        {
            var path = $"{PREFABS}/Obstacles/MudPit_Obstacle.prefab";

            var root = new GameObject("MudPit_Obstacle");

            var col      = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center    = new Vector3(0f, 0.8f, 0f);
            col.size      = new Vector3(1.8f, 1.2f, 1.6f);

            var comp = root.AddComponent<MudPitObstacle>();
            SetEnum(comp, "_type", (int)ObstacleType.MudPit);

            // Flat visual sitting on the tier floor
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            visual.transform.localScale    = new Vector3(1.8f, 0.4f, 1.6f);
            if (mat != null) visual.GetComponent<Renderer>().sharedMaterial = mat;

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<MudPitObstacle>();
        }

        // ─────────────────────────────────────────────────────────
        // Item prefabs
        // ─────────────────────────────────────────────────────────

        private static ItemBase CreateRicePrefab(Material mat)
        {
            var path = $"{PREFABS}/Items/Rice_Item.prefab";

            var root = new GameObject("Rice_Item");

            var col      = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center    = new Vector3(0f, 0.5f, 0f);
            col.radius    = 0.35f;

            var comp = root.AddComponent<RiceItem>();
            SetEnum(comp, "_type", (int)ItemType.Rice);
            SetInt(comp, "_value", 10);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<SphereCollider>());
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            visual.transform.localScale    = new Vector3(0.5f, 0.5f, 0.5f);
            if (mat != null) visual.GetComponent<Renderer>().sharedMaterial = mat;

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<RiceItem>();
        }

        private static ItemBase CreateGoldBuffaloPrefab(Material mat)
        {
            var path = $"{PREFABS}/Items/GoldBuffalo_Item.prefab";

            var root = new GameObject("GoldBuffalo_Item");

            var col      = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center    = new Vector3(0f, 0.5f, 0f);
            col.radius    = 0.5f;

            var comp = root.AddComponent<GoldBuffaloItem>();
            SetEnum(comp, "_type", (int)ItemType.GoldBuffalo);
            SetInt(comp, "_value", 1);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<SphereCollider>());
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            visual.transform.localScale    = new Vector3(0.8f, 0.8f, 0.8f);
            if (mat != null) visual.GetComponent<Renderer>().sharedMaterial = mat;

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<GoldBuffaloItem>();
        }

        // ─────────────────────────────────────────────────────────
        // Chunk prefab — rebuild spawn points for tier system
        // ─────────────────────────────────────────────────────────

        private static void UpdateChunkPrefabForTiers()
        {
            var prefabPath = $"{PREFABS}/Track/Chunk_A.prefab";
            var prefabGo   = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabGo == null)
            {
                Debug.LogWarning("[Phase1Setup] Chunk_A.prefab not found — run Phase 0 first.");
                return;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var root = scope.prefabContentsRoot;

                // Destroy old SpawnPoints GO and rebuild.
                var oldSp = root.transform.Find("SpawnPoints");
                if (oldSp != null) Object.DestroyImmediate(oldSp.gameObject);

                var spawnParent = new GameObject("SpawnPoints");
                spawnParent.transform.SetParent(root.transform);
                spawnParent.transform.localPosition = Vector3.zero;

                // 4 rows × 3 tiers = 12 points. All centered at x=0; Y = floor of each tier.
                float[] tierSpawnY = { 0f, 2f, 4f };
                float[] rowZ       = { 7f, 13f, 19f, 25f };
                var spawnPts = new Transform[tierSpawnY.Length * rowZ.Length];
                int idx = 0;
                foreach (var z in rowZ)
                    foreach (var y in tierSpawnY)
                    {
                        var go = new GameObject($"SP_{idx}");
                        go.transform.SetParent(spawnParent.transform);
                        go.transform.localPosition = new Vector3(0f, y, z);
                        spawnPts[idx++] = go.transform;
                    }

                // Wire into TrackChunk via SerializedObject.
                var chunk = root.GetComponent<TrackChunk>();
                if (chunk != null)
                {
                    var so   = new SerializedObject(chunk);
                    var arr  = so.FindProperty("_spawnPoints");
                    arr.arraySize = spawnPts.Length;
                    for (int i = 0; i < spawnPts.Length; i++)
                        arr.GetArrayElementAtIndex(i).objectReferenceValue = spawnPts[i];
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // AudioDatabase
        // ─────────────────────────────────────────────────────────

        private static AudioDatabase GetOrCreateAudioDatabase()
        {
            var path = $"{SO_CFG}/AudioDatabase.asset";
            var existing = AssetDatabase.LoadAssetAtPath<AudioDatabase>(path);
            if (existing != null) return existing;

            var db = ScriptableObject.CreateInstance<AudioDatabase>();
            AssetDatabase.CreateAsset(db, path);
            return db;
        }

        // ─────────────────────────────────────────────────────────
        // Wire SpawnConfig
        // ─────────────────────────────────────────────────────────

        private static void WireSpawnConfig(ObstacleBase[] obstacles, ItemBase[] items)
        {
            var cfg = AssetDatabase.LoadAssetAtPath<SpawnConfig>($"{SO_CFG}/SpawnConfig.asset");
            if (cfg == null)
            {
                Debug.LogError("[Phase1Setup] SpawnConfig.asset not found. Run Phase 0 first.");
                return;
            }

            var so = new SerializedObject(cfg);

            var obsArr = so.FindProperty("_obstaclePrefabs");
            obsArr.arraySize = obstacles.Length;
            for (int i = 0; i < obstacles.Length; i++)
                obsArr.GetArrayElementAtIndex(i).objectReferenceValue = obstacles[i];

            var itmArr = so.FindProperty("_itemPrefabs");
            itmArr.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                itmArr.GetArrayElementAtIndex(i).objectReferenceValue = items[i];

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cfg);
        }

        // ─────────────────────────────────────────────────────────
        // Gameplay scene cleanup
        // ─────────────────────────────────────────────────────────

        private static void CleanupGameplayScene()
        {
            var scenePath = $"{SCENES}/Gameplay.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool dirty = false;

            // Remove placeholder test obstacle if still present.
            var testObs = GameObject.Find("TestObstacle_Buffalo");
            if (testObs != null) { Object.DestroyImmediate(testObs); dirty = true; }

            // Add kinematic Rigidbody to player (required for OnTriggerEnter to fire).
            dirty |= FixPlayerRigidbody();

            // Remove deprecated PlayerJump / PlayerSlide stubs from Player GO.
            dirty |= MigratePlayerToTierSystem();

            // Add AudioManager + AudioSources to Systems GO if missing.
            dirty |= FixAudioManager();

            // Migrate MainMenuController to a persistent parent (MainMenuSystem)
            // so it can react to StateChanged events even after hiding the panel.
            dirty |= MigrateMainMenuHierarchy();

            // Add full-screen red HitFlash overlay to Canvas.
            dirty |= AddHitFlash();

            // Add combo milestone popup to HUD.
            dirty |= AddComboPopup();

            // Add "KỶ LỤC MỚI!" badge to GameOver panel.
            dirty |= AddNewRecordBadge();

            // Add mute toggle button to Main Menu panel.
            dirty |= AddMuteButton();

            // Add pause button to HUD + full pause panel to Canvas.
            dirty |= AddPauseSystem();

            if (dirty) EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static bool FixAudioManager()
        {
            var systems = GameObject.Find("Systems");
            if (systems == null) return false;
            if (systems.GetComponent<AudioManager>() != null) return false;

            // Music source
            var musicSrcGo = new GameObject("MusicSource");
            musicSrcGo.transform.SetParent(systems.transform);
            var musicSrc = musicSrcGo.AddComponent<AudioSource>();
            musicSrc.playOnAwake = false;
            musicSrc.loop = true;
            musicSrc.volume = 0.6f;

            // SFX ring (3 sources)
            const int SfxCount = 3;
            var sfxSources = new AudioSource[SfxCount];
            for (int i = 0; i < SfxCount; i++)
            {
                var sfxGo = new GameObject($"SfxSource_{i}");
                sfxGo.transform.SetParent(systems.transform);
                var src = sfxGo.AddComponent<AudioSource>();
                src.playOnAwake = false;
                sfxSources[i] = src;
            }

            var db = AssetDatabase.LoadAssetAtPath<AudioDatabase>($"{SO_CFG}/AudioDatabase.asset");
            var mgr = systems.AddComponent<AudioManager>();
            var so  = new SerializedObject(mgr);
            so.FindProperty("_database").objectReferenceValue = db;
            so.FindProperty("_musicSource").objectReferenceValue = musicSrc;

            var sfxArr = so.FindProperty("_sfxSources");
            sfxArr.arraySize = SfxCount;
            for (int i = 0; i < SfxCount; i++)
                sfxArr.GetArrayElementAtIndex(i).objectReferenceValue = sfxSources[i];

            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool FixPlayerRigidbody()
        {
            var player = GameObject.Find("Player");
            if (player == null) return false;
            if (player.GetComponent<Rigidbody>() != null) return false;

            var rb           = player.AddComponent<Rigidbody>();
            rb.isKinematic   = true;
            rb.useGravity    = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            return true;
        }

        private static bool MigratePlayerToTierSystem()
        {
            var player = GameObject.Find("Player");
            if (player == null) return false;
            bool changed = false;

            // Remove deprecated stubs — jump/slide are now tier change in PlayerMovement.
            var jump = player.GetComponent<DuaRuong.Gameplay.Player.PlayerJump>();
            if (jump != null) { Object.DestroyImmediate(jump); changed = true; }
            var slide = player.GetComponent<DuaRuong.Gameplay.Player.PlayerSlide>();
            if (slide != null) { Object.DestroyImmediate(slide); changed = true; }

            return changed;
        }

        // Restructures:
        //   Canvas/MainMenuPanel [MainMenuController]
        // into:
        //   Canvas/MainMenuSystem [MainMenuController, _root→MainMenuPanel]
        //     MainMenuPanel [Image, buttons, texts]
        private static bool MigrateMainMenuHierarchy()
        {
            // If already migrated, skip.
            var existing = GameObject.Find("MainMenuSystem");
            if (existing != null) return false;

            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return false;

            var mmPanelT = canvas.transform.Find("MainMenuPanel");
            if (mmPanelT == null) return false;

            var oldCtrl = mmPanelT.GetComponent<MainMenuController>();
            if (oldCtrl == null) return false;  // already migrated or different setup

            // Read refs from old controller before destroying it.
            var oldSo    = new SerializedObject(oldCtrl);
            var playBtn  = oldSo.FindProperty("_playButton").objectReferenceValue  as Button;
            var hiScore  = oldSo.FindProperty("_highScoreText").objectReferenceValue as TMP_Text;

            // Create persistent system parent, stretched to fill canvas.
            var sysGo = new GameObject("MainMenuSystem");
            sysGo.transform.SetParent(canvas.transform, worldPositionStays: false);
            var sysRt           = sysGo.AddComponent<RectTransform>();
            sysRt.anchorMin     = Vector2.zero;
            sysRt.anchorMax     = Vector2.one;
            sysRt.sizeDelta     = Vector2.zero;
            sysRt.anchoredPosition = Vector2.zero;

            // Re-parent the panel under the new system GO.
            mmPanelT.SetParent(sysGo.transform, worldPositionStays: false);

            // Add controller to system parent and wire references.
            var newCtrl = sysGo.AddComponent<MainMenuController>();
            var newSo   = new SerializedObject(newCtrl);
            newSo.FindProperty("_root").objectReferenceValue         = mmPanelT.gameObject;
            newSo.FindProperty("_playButton").objectReferenceValue   = playBtn;
            newSo.FindProperty("_highScoreText").objectReferenceValue = hiScore;
            newSo.ApplyModifiedPropertiesWithoutUndo();

            // Remove old controller from the panel.
            Object.DestroyImmediate(oldCtrl);

            return true;
        }

        private static bool AddComboPopup()
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return false;
            if (hud.GetComponentInChildren<ComboPopup>() != null) return false;

            var go = new GameObject("ComboPopup");
            go.transform.SetParent(hud.transform, worldPositionStays: false);

            var rt              = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 200f);
            rt.sizeDelta        = new Vector2(460f, 90f);

            var group = go.AddComponent<CanvasGroup>();
            group.alpha = 0f;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, worldPositionStays: false);
            var txtRt       = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var t           = txtGo.AddComponent<TextMeshProUGUI>();
            t.text          = "COMBO ×2!";
            t.fontSize      = 56;
            t.fontStyle     = FontStyles.Bold;
            t.alignment     = TextAlignmentOptions.Center;
            t.color         = new Color(1f, 0.92f, 0.1f);

            var popup = go.AddComponent<ComboPopup>();
            var so    = new SerializedObject(popup);
            so.FindProperty("_text").objectReferenceValue  = t;
            so.FindProperty("_group").objectReferenceValue = group;
            so.ApplyModifiedPropertiesWithoutUndo();

            return true;
        }

        private static bool AddNewRecordBadge()
        {
            var goPanel = GameObject.Find("GameOverPanel");
            if (goPanel == null) return false;

            var sysGo = GameObject.Find("GameOverSystem");
            if (sysGo == null) return false;
            var ctrl = sysGo.GetComponent<GameOverController>();
            if (ctrl == null) return false;

            var ctrlSo = new SerializedObject(ctrl);
            if (ctrlSo.FindProperty("_newRecordBadge").objectReferenceValue != null) return false;

            var badge = new GameObject("NewRecordBadge");
            badge.transform.SetParent(goPanel.transform, worldPositionStays: false);

            var rt              = badge.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 260f);
            rt.sizeDelta        = new Vector2(340f, 64f);

            badge.AddComponent<Image>().color = new Color(1f, 0.82f, 0f);

            var lbl = new GameObject("Label");
            lbl.transform.SetParent(badge.transform, worldPositionStays: false);
            var lblRt       = lbl.AddComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.sizeDelta = Vector2.zero;
            var t           = lbl.AddComponent<TextMeshProUGUI>();
            t.text          = "KY LUC MOI!";
            t.fontSize      = 28;
            t.fontStyle     = FontStyles.Bold;
            t.alignment     = TextAlignmentOptions.Center;
            t.color         = Color.black;

            ctrlSo.FindProperty("_newRecordBadge").objectReferenceValue = badge;
            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            return true;
        }

        private static bool AddMuteButton()
        {
            var mmPanel = GameObject.Find("MainMenuPanel");
            if (mmPanel == null) return false;

            var sysGo = GameObject.Find("MainMenuSystem");
            if (sysGo == null) return false;
            var ctrl = sysGo.GetComponent<MainMenuController>();
            if (ctrl == null) return false;

            var ctrlSo = new SerializedObject(ctrl);
            if (ctrlSo.FindProperty("_muteButton").objectReferenceValue != null) return false;

            var go = new GameObject("MuteButton");
            go.transform.SetParent(mmPanel.transform, worldPositionStays: false);

            var rt              = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -80f);
            rt.sizeDelta        = new Vector2(200f, 56f);

            go.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);
            var btn = go.AddComponent<Button>();

            var lbl = new GameObject("Label");
            lbl.transform.SetParent(go.transform, worldPositionStays: false);
            var lblRt       = lbl.AddComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.sizeDelta = Vector2.zero;
            var t           = lbl.AddComponent<TextMeshProUGUI>();
            t.text          = "AM THANH";
            t.fontSize      = 26;
            t.fontStyle     = FontStyles.Bold;
            t.alignment     = TextAlignmentOptions.Center;
            t.color         = Color.white;

            ctrlSo.FindProperty("_muteButton").objectReferenceValue = btn;
            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            return true;
        }

        private static bool AddPauseSystem()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return false;
            if (canvas.GetComponentInChildren<PauseController>() != null) return false;

            // ── Pause button on HUD ──────────────────────────────
            var hud = GameObject.Find("HUD");
            if (hud != null)
            {
                var btnGo = new GameObject("PauseButton");
                btnGo.transform.SetParent(hud.transform, worldPositionStays: false);

                var rt              = btnGo.AddComponent<RectTransform>();
                rt.anchorMin        = new Vector2(0f, 1f);
                rt.anchorMax        = new Vector2(0f, 1f);
                rt.pivot            = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(30f, -30f);
                rt.sizeDelta        = new Vector2(90f, 70f);

                btnGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
                btnGo.AddComponent<Button>();

                var lbl = new GameObject("Label");
                lbl.transform.SetParent(btnGo.transform, worldPositionStays: false);
                var lblRt       = lbl.AddComponent<RectTransform>();
                lblRt.anchorMin = Vector2.zero;
                lblRt.anchorMax = Vector2.one;
                lblRt.sizeDelta = Vector2.zero;
                var t           = lbl.AddComponent<TextMeshProUGUI>();
                t.text          = "| |";
                t.fontSize      = 28;
                t.fontStyle     = FontStyles.Bold;
                t.alignment     = TextAlignmentOptions.Center;
                t.color         = Color.white;

                // PauseButtonHandler wires Button.onClick → GameManager.Pause() at runtime.
                btnGo.AddComponent<PauseButtonHandler>();
            }

            // ── PauseSystem persistent container ─────────────────
            var sysGo = new GameObject("PauseSystem");
            sysGo.transform.SetParent(canvas.transform, worldPositionStays: false);
            var sysRt           = sysGo.AddComponent<RectTransform>();
            sysRt.anchorMin     = Vector2.zero;
            sysRt.anchorMax     = Vector2.one;
            sysRt.sizeDelta     = Vector2.zero;
            sysRt.anchoredPosition = Vector2.zero;

            // ── Pause panel ───────────────────────────────────────
            var panelGo = new GameObject("PausePanel");
            panelGo.transform.SetParent(sysGo.transform, worldPositionStays: false);
            var panelRt           = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin     = Vector2.zero;
            panelRt.anchorMax     = Vector2.one;
            panelRt.sizeDelta     = Vector2.zero;
            panelRt.anchoredPosition = Vector2.zero;
            panelGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

            var titleText = new GameObject("TitleText");
            titleText.transform.SetParent(panelGo.transform, worldPositionStays: false);
            var titleRt           = titleText.AddComponent<RectTransform>();
            titleRt.anchorMin     = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax     = new Vector2(0.5f, 0.5f);
            titleRt.pivot         = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, 200f);
            titleRt.sizeDelta     = new Vector2(500f, 90f);
            var titleTmp          = titleText.AddComponent<TextMeshProUGUI>();
            titleTmp.text         = "TAM DUNG";
            titleTmp.fontSize     = 64;
            titleTmp.fontStyle    = FontStyles.Bold;
            titleTmp.alignment    = TextAlignmentOptions.Center;
            titleTmp.color        = Color.white;

            var resumeBtn = CreatePauseMenuBtn(panelGo.transform, "ResumeBtn",  "TIEP TUC",  new Vector2(0f,  30f), new Vector2(300f, 90f));
            var menuBtn   = CreatePauseMenuBtn(panelGo.transform, "MenuBtn",    "MENU",      new Vector2(0f, -80f), new Vector2(200f, 70f));

            var ctrl = sysGo.AddComponent<PauseController>();
            var so   = new SerializedObject(ctrl);
            so.FindProperty("_root").objectReferenceValue          = panelGo;
            so.FindProperty("_resumeButton").objectReferenceValue  = resumeBtn;
            so.FindProperty("_menuButton").objectReferenceValue    = menuBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            return true;
        }

        private static Button CreatePauseMenuBtn(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, worldPositionStays: false);

            var rt              = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;

            go.AddComponent<Image>().color = new Color(0.15f, 0.45f, 0.15f);
            var btn = go.AddComponent<Button>();

            var lbl = new GameObject("Label");
            lbl.transform.SetParent(go.transform, worldPositionStays: false);
            var lblRt       = lbl.AddComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.sizeDelta = Vector2.zero;
            var t           = lbl.AddComponent<TextMeshProUGUI>();
            t.text          = label;
            t.fontSize      = 36;
            t.fontStyle     = FontStyles.Bold;
            t.alignment     = TextAlignmentOptions.Center;
            t.color         = Color.white;

            return btn;
        }

        private static bool AddHitFlash()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return false;
            if (canvas.GetComponentInChildren<HitFlash>() != null) return false;

            var go = new GameObject("HitFlash");
            go.transform.SetParent(canvas.transform, worldPositionStays: false);

            var rt               = go.AddComponent<RectTransform>();
            rt.anchorMin         = Vector2.zero;
            rt.anchorMax         = Vector2.one;
            rt.sizeDelta         = Vector2.zero;
            rt.anchoredPosition  = Vector2.zero;

            var img              = go.AddComponent<Image>();
            img.color            = new Color(1f, 0.08f, 0.08f, 0f);
            img.raycastTarget    = false;

            go.AddComponent<HitFlash>();
            return true;
        }

        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        private static void SetEnum(Object target, string prop, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(prop).enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string prop, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(prop).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Step(string msg, float t)
            => EditorUtility.DisplayProgressBar("Phase 1 Setup", msg + "…", t);
    }
}
#endif
