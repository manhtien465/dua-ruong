#if UNITY_EDITOR
using DuaRuong.UI;
using DuaRuong.UI.HUD;
using DuaRuong.UI.Menus;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace DuaRuong.Editor
{
    /// <summary>
    /// UI/UX polish pass. Run after Phase 1.
    /// Adds animations + applies a high-contrast Vietnamese-themed colour scheme.
    /// Safe to run multiple times (idempotent for components; always re-applies colours).
    /// </summary>
    public static class Phase2Setup
    {
        private const string SCENES = "Assets/_Project/Scenes";

        // ── Cached sprites (loaded once per Run) ─────────────────
        private static Sprite _sprBtn;
        private static Sprite _sprPanel;
        private static Sprite _sprPill;
        private static Sprite _sprBadge;

        // ── Palette ──────────────────────────────────────────────
        // Inspired by Mù Cang Chải: green terraces, golden rice, dark mountain sky.
        private static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        [MenuItem("DuaRuong/Build Phase 2 UI ▶", priority = 2)]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (!EditorUtility.DisplayDialog("Phase 2 UI Setup",
                    "Áp dụng colour theme + animations.\nChạy SAU Phase 1.",
                    "Chạy", "Huỷ"))
                return;

            try
            {
                Step("Mở scene", 0.05f);
                EditorSceneManager.OpenScene($"{SCENES}/Gameplay.unity", OpenSceneMode.Single);

                Step("Load sprites", 0.10f);
                LoadSprites();

                Step("Camera background", 0.10f);
                FixCamera();

                Step("Screen fade", 0.16f);
                AddScreenFade();

                Step("Panel fade-in", 0.22f);
                AddPanelFadeIns();

                Step("Button feedback", 0.28f);
                AddButtonFeedbacks();

                Step("Score pop", 0.34f);
                AddScorePop();

                Step("Juicy text", 0.40f);
                AddJuicyText();

                Step("Rolling score", 0.46f);
                AddRollingScore();

                Step("Floating score labels", 0.52f);
                AddFloatingScoreLauncher();

                Step("Star rating", 0.58f);
                AddStarRating();

                Step("Combo bar", 0.64f);
                AddComboBar();

                Step("Apply sprites", 0.70f);
                ApplySprites();

                Step("Màu sắc + font", 0.80f);
                ApplyColours();

                Step("HUD backgrounds", 0.90f);
                AddHudPillBackgrounds();

                Step("Lưu scene", 0.95f);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Done ✅",
                "Phase 2 UI xong!\n\n" +
                "Nhấn Play để xem:\n" +
                "• Camera nền xanh đậm ruộng bậc thang\n" +
                "• Màn fade đen → sáng khi bắt đầu\n" +
                "• Panel mờ dần + scale khi xuất hiện\n" +
                "• Nút thu nhỏ khi nhấn (spring physics)\n" +
                "• Score giật nhẹ khi nhặt item\n" +
                "• Score text bounce khi cộng điểm\n" +
                "• Floating +X text khi nhặt item\n" +
                "• Thanh combo bar (đổi màu theo tier)\n" +
                "• Sao 1-3 reveal tuần tự ở Game Over\n" +
                "• Score đếm lên ở Game Over",
                "OK");
        }

        // ─────────────────────────────────────────────────────────
        // Camera background — biggest single visual win
        // ─────────────────────────────────────────────────────────

        private static void FixCamera()
        {
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null) return;

            var cam = camGo.GetComponent<Camera>();
            if (cam == null) return;

            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = C("#050F05");   // near-black dark green
            EditorUtility.SetDirty(camGo);
        }

        // ─────────────────────────────────────────────────────────
        // Animations
        // ─────────────────────────────────────────────────────────

        private static void AddScreenFade()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return;
            if (canvas.GetComponentInChildren<ScreenFade>() != null) return;

            var go = new GameObject("ScreenFade");
            go.transform.SetParent(canvas.transform, worldPositionStays: false);
            go.transform.SetAsLastSibling();

            var rt              = go.AddComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            go.AddComponent<Image>().color   = Color.black;
            go.AddComponent<ScreenFade>();
        }

        private static void AddPanelFadeIns()
        {
            foreach (var name in new[] { "GameOverPanel", "PausePanel", "MainMenuPanel" })
            {
                var go = GameObject.Find(name);
                if (go == null) continue;
                if (go.GetComponent<CanvasGroup>()  == null) go.AddComponent<CanvasGroup>();
                if (go.GetComponent<PanelFadeIn>()  == null) go.AddComponent<PanelFadeIn>();
            }
        }

        private static void AddButtonFeedbacks()
        {
            foreach (var btn in Object.FindObjectsOfType<Button>())
                if (btn.GetComponent<ButtonFeedback>() == null)
                    btn.gameObject.AddComponent<ButtonFeedback>();
        }

        private static void AddScorePop()
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;
            var scoreT = hud.transform.Find("ScoreText");
            if (scoreT == null) return;
            if (scoreT.GetComponent<ScorePop>() == null)
                scoreT.gameObject.AddComponent<ScorePop>();
        }

        // Adds JuicyText to ScoreText + ComboText, then wires HudController references.
        private static void AddJuicyText()
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;

            var hudCtrl = hud.GetComponent<HudController>();
            var so      = hudCtrl != null ? new SerializedObject(hudCtrl) : null;

            var scoreT = hud.transform.Find("ScoreText");
            if (scoreT != null)
            {
                var jt = scoreT.GetComponent<JuicyText>() ?? scoreT.gameObject.AddComponent<JuicyText>();
                var scoreProp = so?.FindProperty("_scoreJuice");
                if (scoreProp != null) scoreProp.objectReferenceValue = jt;
            }

            var comboContainer = hud.transform.Find("ComboContainer");
            if (comboContainer != null)
            {
                var comboT = comboContainer.Find("ComboText");
                if (comboT != null)
                {
                    var jt = comboT.GetComponent<JuicyText>() ?? comboT.gameObject.AddComponent<JuicyText>();
                    var comboProp = so?.FindProperty("_comboJuice");
                    if (comboProp != null) comboProp.objectReferenceValue = jt;
                }
            }

            so?.ApplyModifiedPropertiesWithoutUndo();
        }

        // Adds RollingNumber to GameOver's score text and wires it into GameOverController.
        private static void AddRollingScore()
        {
            var sysGo = GameObject.Find("GameOverSystem");
            if (sysGo == null) return;
            var ctrl = sysGo.GetComponent<GameOverController>();
            if (ctrl == null) return;

            var goPanel = sysGo.transform.Find("GameOverPanel");
            if (goPanel == null) return;
            var scoreT = goPanel.Find("ScoreText");
            if (scoreT == null) return;

            var rolling = scoreT.GetComponent<RollingNumber>() ?? scoreT.gameObject.AddComponent<RollingNumber>();

            var so = new SerializedObject(ctrl);
            so.FindProperty("_rollingScore").objectReferenceValue = rolling;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Adds FloatingScoreLauncher to HUD at a good spawn position.
        private static void AddFloatingScoreLauncher()
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;
            if (hud.GetComponentInChildren<FloatingScoreLauncher>() != null) return;

            var go = new GameObject("FloatingScoreLauncher");
            go.transform.SetParent(hud.transform, worldPositionStays: false);

            var rt              = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.35f); // roughly where player character is on screen
            rt.anchorMax        = new Vector2(0.5f, 0.35f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = Vector2.zero;

            go.AddComponent<FloatingScoreLauncher>();
        }

        // Adds StarRatingDisplay to GameOverPanel and wires GameOverController._starRating.
        private static void AddStarRating()
        {
            var sysGo = GameObject.Find("GameOverSystem");
            if (sysGo == null) return;
            var ctrl = sysGo.GetComponent<GameOverController>();
            if (ctrl == null) return;

            var goPanel = sysGo.transform.Find("GameOverPanel");
            if (goPanel == null) return;
            if (goPanel.GetComponentInChildren<StarRatingDisplay>() != null) return;

            var go = new GameObject("StarRating");
            go.transform.SetParent(goPanel, worldPositionStays: false);

            var rt              = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 240f); // above score text (y=150), below badge (y=260)
            rt.sizeDelta        = new Vector2(300f, 70f);

            var starRating = go.AddComponent<StarRatingDisplay>();

            var so = new SerializedObject(ctrl);
            so.FindProperty("_starRating").objectReferenceValue = starRating;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Adds ComboBar to HUD at the bottom of the screen.
        private static void AddComboBar()
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;
            if (hud.GetComponentInChildren<ComboBar>() != null) return;

            var go = new GameObject("ComboBar");
            go.transform.SetParent(hud.transform, worldPositionStays: false);

            var rt              = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 0f);
            rt.anchorMax        = new Vector2(1f, 0f);
            rt.pivot            = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = new Vector2(0f, 10f);   // full-width thin bar

            go.AddComponent<CanvasGroup>();
            go.AddComponent<ComboBar>();
        }

        // ─────────────────────────────────────────────────────────
        // Sprites — load generated assets, apply to scene objects
        // ─────────────────────────────────────────────────────────

        private static void LoadSprites()
        {
            _sprBtn   = AssetDatabase.LoadAssetAtPath<Sprite>(UIAssetGenerator.ROUNDED_BTN);
            _sprPanel = AssetDatabase.LoadAssetAtPath<Sprite>(UIAssetGenerator.ROUNDED_PANEL);
            _sprPill  = AssetDatabase.LoadAssetAtPath<Sprite>(UIAssetGenerator.ROUNDED_PILL);
            _sprBadge = AssetDatabase.LoadAssetAtPath<Sprite>(UIAssetGenerator.ROUNDED_BADGE);

            if (_sprBtn == null || _sprPanel == null || _sprPill == null || _sprBadge == null)
                Debug.LogWarning("[Phase2Setup] Sprites chưa tồn tại — hãy chạy \"DuaRuong → Generate UI Assets ▶\" trước.");
        }

        private static void ApplySprites()
        {
            if (_sprBtn != null)
            {
                // Apply rounded button sprite to every Button in scene
                foreach (var btn in Object.FindObjectsOfType<Button>())
                {
                    var img = btn.GetComponent<Image>();
                    if (img == null) continue;
                    img.sprite                 = _sprBtn;
                    img.type                   = Image.Type.Sliced;
                    img.pixelsPerUnitMultiplier = 1f;
                    EditorUtility.SetDirty(btn.gameObject);
                }
            }

            if (_sprPanel != null)
            {
                foreach (var name in new[] { "GameOverPanel", "PausePanel", "MainMenuPanel" })
                {
                    var go = GameObject.Find(name);
                    if (go == null) continue;
                    var img = go.GetComponent<Image>();
                    if (img == null) continue;
                    img.sprite                 = _sprPanel;
                    img.type                   = Image.Type.Sliced;
                    img.pixelsPerUnitMultiplier = 1f;
                    EditorUtility.SetDirty(go);
                }
            }

            if (_sprBadge != null)
            {
                var badgeGo = GameObject.Find("NewRecordBadge");
                if (badgeGo != null)
                {
                    var img = badgeGo.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite                 = _sprBadge;
                        img.type                   = Image.Type.Sliced;
                        img.pixelsPerUnitMultiplier = 1f;
                        EditorUtility.SetDirty(badgeGo);
                    }
                }
            }
        }

        private static void AddHudPillBackgrounds()
        {
            if (_sprPill == null) return;

            var hud = GameObject.Find("HUD");
            if (hud == null) return;

            foreach (var childName in new[] { "ScoreText", "DistText" })
            {
                var child = hud.transform.Find(childName);
                if (child == null) continue;

                // Skip if pill background already exists
                if (child.Find("BG") != null) continue;

                var bg   = new GameObject("BG");
                bg.transform.SetParent(child, worldPositionStays: false);
                bg.transform.SetAsFirstSibling();

                var rt           = bg.AddComponent<RectTransform>();
                rt.anchorMin     = Vector2.zero;
                rt.anchorMax     = Vector2.one;
                rt.sizeDelta     = new Vector2(16f, 8f);   // slight padding beyond text
                rt.anchoredPosition = Vector2.zero;

                var img                    = bg.AddComponent<Image>();
                img.sprite                 = _sprPill;
                img.type                   = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                img.color                  = C("#00000060");   // semi-transparent dark pill
                EditorUtility.SetDirty(hud);
            }
        }

        // ─────────────────────────────────────────────────────────
        // Colours + typography — always re-applied
        // ─────────────────────────────────────────────────────────

        private static void ApplyColours()
        {
            // ── Main Menu ────────────────────────────────────────
            SetImg ("MainMenuPanel",     C("#0B2B0C"));        // dark forest green
            SetTmp ("MainMenuPanel/Title",
                    color: C("#FFD700"), size: 96f, style: FontStyles.Bold);
            SetTmp ("MainMenuPanel/HighScoreText",
                    color: C("#A5D6A7"), size: 34f, style: FontStyles.Normal);
            SetImg ("PlayButton",        C("#2E7D32"));        // Material Green 800
            SetTmp ("PlayButton/Label",  color: Color.white, size: 42f, style: FontStyles.Bold);
            SetImg ("MuteButton",        C("#37474F"));        // Blue Grey 800
            SetTmp ("MuteButton/Label",  color: C("#B0BEC5"), size: 26f, style: FontStyles.Normal);

            // ── HUD ──────────────────────────────────────────────
            SetTmp ("ScoreText",  color: C("#FFFDE7"), size: 56f, style: FontStyles.Bold);
            SetTmp ("DistText",   color: C("#C8E6C9"), size: 30f, style: FontStyles.Normal);
            SetTmp ("ComboText",  color: C("#FFD54F"), size: 32f, style: FontStyles.Bold);
            SetImg ("PauseButton",C("#00000080"));

            // ── Game Over ────────────────────────────────────────
            SetImg ("GameOverPanel",      C("#050505E6"));     // near-black, high alpha
            SetTmp ("GameOverPanel/ScoreText",
                    color: C("#FFFDE7"), size: 80f, style: FontStyles.Bold);
            SetTmp ("GameOverPanel/BestText",
                    color: C("#A5D6A7"), size: 36f, style: FontStyles.Normal);
            SetImg ("PlayAgainBtn",       C("#2E7D32"));
            SetTmp ("PlayAgainBtn/Label", color: Color.white, size: 38f, style: FontStyles.Bold);
            SetImg ("MenuBtn",            C("#37474F"));
            SetTmp ("MenuBtn/Label",      color: C("#B0BEC5"), size: 30f, style: FontStyles.Normal);
            SetImg ("NewRecordBadge",     C("#FFC107"));
            SetTmp ("NewRecordBadge/Label",
                    color: C("#212121"), size: 26f, style: FontStyles.Bold);

            // ── Pause ────────────────────────────────────────────
            SetImg ("PausePanel",         C("#050F05E0"));
            SetTmp ("PausePanel/TitleText",
                    color: Color.white,   size: 64f, style: FontStyles.Bold);
            SetImg ("ResumeBtn",          C("#2E7D32"));
            SetTmp ("ResumeBtn/Label",    color: Color.white, size: 38f, style: FontStyles.Bold);
            // MenuBtn inside PausePanel shares name with GameOver's MenuBtn — both already handled.

            // ── Combo popup ──────────────────────────────────────
            SetTmp ("ComboPopup/Text",    color: C("#FFD700"), size: 56f, style: FontStyles.Bold);
        }

        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        // path = "GOName" or "ParentName/ChildName"
        private static GameObject Resolve(string path)
        {
            var slash = path.IndexOf('/');
            if (slash < 0) return GameObject.Find(path);

            var parent = GameObject.Find(path[..slash]);
            if (parent == null) return null;
            var child = parent.transform.Find(path[(slash + 1)..]);
            return child != null ? child.gameObject : null;
        }

        private static void SetImg(string path, Color colour)
        {
            var go = Resolve(path);
            if (go == null) { Warn(path, "Image"); return; }
            var img = go.GetComponent<Image>();
            if (img == null) return;
            img.color = colour;
            EditorUtility.SetDirty(go);
        }

        private static void SetTmp(string path, Color color, float size, FontStyles style)
        {
            var go = Resolve(path);
            if (go == null) { Warn(path, "TMP_Text"); return; }
            var tmp = go.GetComponent<TMP_Text>();
            if (tmp == null) return;
            tmp.color     = color;
            tmp.fontSize  = size;
            tmp.fontStyle = style;
            EditorUtility.SetDirty(go);
        }

        private static void Warn(string path, string component)
            => Debug.LogWarning($"[Phase2Setup] '{path}' not found — {component} not updated.");

        private static void Step(string msg, float t)
            => EditorUtility.DisplayProgressBar("Phase 2 UI", msg + "…", t);
    }
}
#endif
