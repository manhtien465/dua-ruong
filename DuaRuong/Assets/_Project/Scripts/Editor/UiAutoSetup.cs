using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuaRuong.UI;
using DuaRuong.UI.HUD;
using DuaRuong.UI.Menus;

namespace DuaRuong.Editor
{
    public static class UiAutoSetup
    {
        [MenuItem("Dua Ruong/UI/Auto Setup Juice")]
        public static void SetupJuice()
        {
            // 0. Fix Texture Settings (Transparency)
            string[] texturePaths = {
                "Assets/_Project/UI/Textures/Icon_Pause.png",
                "Assets/_Project/UI/Textures/Icon_Coin_New.png",
                "Assets/_Project/UI/Textures/Button_Green_New.png",
                "Assets/_Project/UI/Textures/Button_Orange_New.png",
                "Assets/_Project/UI/Textures/Panel_Wooden_New.png"
            };

            foreach (var path in texturePaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }

            // 1. Load Sprites
            Sprite pauseIcon = AssetDatabase.LoadAssetAtPath<Sprite>(texturePaths[0]);
            Sprite coinIcon = AssetDatabase.LoadAssetAtPath<Sprite>(texturePaths[1]);
            Sprite greenBtn = AssetDatabase.LoadAssetAtPath<Sprite>(texturePaths[2]);
            Sprite orangeBtn = AssetDatabase.LoadAssetAtPath<Sprite>(texturePaths[3]);
            Sprite woodPanel = AssetDatabase.LoadAssetAtPath<Sprite>(texturePaths[4]);

            // 2. Setup HUD Layout & Styling
            HudController hud = Object.FindObjectOfType<HudController>(true);
            if (hud != null)
            {
                // Score: Top Right
                var scoreText = hud.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
                if (scoreText != null)
                {
                    var rect = scoreText.GetComponent<RectTransform>();
                    rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 1);
                    rect.anchoredPosition = new Vector2(-100, -50);
                    scoreText.alignment = TextAlignmentOptions.Right;
                    scoreText.fontSize = 40;
                    
                    if (!scoreText.GetComponent<JuicyText>()) scoreText.gameObject.AddComponent<JuicyText>();
                    AddShadowAndOutline(scoreText.gameObject);
                    CreateIconNextToText(scoreText, coinIcon, new Vector2(40, 0), true);
                }

                // Distance: Top Left
                var distText = hud.transform.Find("DistText")?.GetComponent<TMP_Text>();
                if (distText != null)
                {
                    var rect = distText.GetComponent<RectTransform>();
                    rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 1);
                    rect.anchoredPosition = new Vector2(150, -50);
                    distText.fontSize = 30;
                    AddShadowAndOutline(distText.gameObject);
                }

                // Pause Button: Top Left
                var pauseBtn = hud.transform.Find("PauseButton")?.GetComponent<Button>();
                if (pauseBtn != null)
                {
                    var rect = pauseBtn.GetComponent<RectTransform>();
                    rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 1);
                    rect.anchoredPosition = new Vector2(30, -30);
                    rect.sizeDelta = new Vector2(80, 80);
                    
                    var img = pauseBtn.GetComponent<Image>();
                    img.sprite = pauseIcon;
                    img.color = Color.white;
                    pauseBtn.transition = Selectable.Transition.None;

                    var label = pauseBtn.transform.Find("Label");
                    if (label != null) label.gameObject.SetActive(false);
                    if (!pauseBtn.GetComponent<PauseButtonHandler>()) pauseBtn.gameObject.AddComponent<PauseButtonHandler>();
                    if (!pauseBtn.GetComponent<ButtonFeedback>()) pauseBtn.gameObject.AddComponent<ButtonFeedback>();
                }
                
                hud.SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
            }

            // 3. Setup Buttons in Menus
            Button[] allButtons = Object.FindObjectsOfType<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn.transform.parent != null && btn.transform.parent.name.Contains("HUD")) continue;

                string name = btn.name.ToLower();
                Image img = btn.GetComponent<Image>();
                if (img == null) continue;

                img.color = Color.white;
                btn.transition = Selectable.Transition.None;
                if (!btn.GetComponent<ButtonFeedback>()) btn.gameObject.AddComponent<ButtonFeedback>();

                if (name.Contains("again") || name.Contains("play") || name.Contains("tiep") || name.Contains("tuc"))
                {
                    if (greenBtn != null) img.sprite = greenBtn;
                    btn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100);
                }
                else if (name.Contains("menu"))
                {
                    if (orangeBtn != null) img.sprite = orangeBtn;
                    btn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100);
                }
            }

            // 4. Setup Panels
            GameOverController gameOver = Object.FindObjectOfType<GameOverController>(true);
            if (gameOver != null)
            {
                var panelImg = gameOver.transform.Find("GameOverPanel")?.GetComponent<Image>();
                if (panelImg != null && woodPanel != null) {
                    panelImg.sprite = woodPanel;
                    panelImg.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 700);
                }
                gameOver.SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
            }

            Debug.Log("UI Magic v4 Complete! Pro Layout applied.");
            EditorUtility.DisplayDialog("UI Magic v4", "Đã nâng cấp Layout chuẩn Pro và thay Asset sạch!", "OK");
        }

        private static void AddShadowAndOutline(GameObject target)
        {
            var text = target.GetComponent<TMP_Text>();
            if (text == null) return;
            text.color = Color.white;
            if (!target.GetComponent<Shadow>())
            {
                var s = target.AddComponent<Shadow>();
                s.effectColor = new Color(0, 0, 0, 0.8f);
                s.effectDistance = new Vector2(2, -2);
            }
        }

        private static void CreateIconNextToText(TMP_Text text, Sprite icon, Vector2 offset, bool rightSide = false)
        {
            string iconName = "Icon_" + text.name;
            Transform existing = text.transform.Find(iconName);
            GameObject iconObj = existing != null ? existing.gameObject : new GameObject(iconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(text.transform);
            
            var rect = iconObj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(rightSide ? 1 : 0, 0.5f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(50, 50);

            var img = iconObj.GetComponent<Image>();
            img.sprite = icon;
            img.raycastTarget = false;
        }
    }
}
