#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DuaRuong.Editor
{
    /// <summary>
    /// Generates all UI sprites programmatically — no external art tools needed.
    /// Run once: DuaRuong → Generate UI Assets ▶
    /// Sprites are saved as PNG assets with 9-slice metadata set by TextureImporter.
    /// Phase2Setup applies them to the scene.
    /// </summary>
    public static class UIAssetGenerator
    {
        private const string OUT = "Assets/_Project/Art/UI/Generated";

        // Sprite names (used by Phase2Setup to load them)
        public const string ROUNDED_BTN    = OUT + "/RoundedBtn.png";
        public const string ROUNDED_PANEL  = OUT + "/RoundedPanel.png";
        public const string ROUNDED_PILL   = OUT + "/RoundedPill.png";
        public const string ROUNDED_BADGE  = OUT + "/RoundedBadge.png";

        [MenuItem("DuaRuong/Generate UI Assets ▶", priority = 3)]
        public static void Run()
        {
            EnsureFolders();

            // Button: 64×64, radius 20, gradient (lighter top)
            GenerateAndSave(
                CreateRoundedTex(64, 64, 20, gradient: true),
                ROUNDED_BTN, new Vector4(20, 20, 20, 20));

            // Panel background: 32×32, radius 8, flat
            GenerateAndSave(
                CreateRoundedTex(32, 32, 8, gradient: false),
                ROUNDED_PANEL, new Vector4(8, 8, 8, 8));

            // Pill: 128×48, full radius (pill shape), flat
            GenerateAndSave(
                CreateRoundedTex(128, 48, 24, gradient: false),
                ROUNDED_PILL, new Vector4(24, 24, 24, 24));

            // Badge (wider pill): 128×56, radius 12
            GenerateAndSave(
                CreateRoundedTex(128, 56, 14, gradient: false),
                ROUNDED_BADGE, new Vector4(14, 14, 14, 14));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Done ✅",
                "UI sprites đã tạo xong!\n\n" +
                "Bước tiếp: DuaRuong → Build Phase 2 UI ▶",
                "OK");
        }

        // ─────────────────────────────────────────────────────────
        // Texture generation
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a white rounded-rectangle texture, optionally with a
        /// subtle top-lighter gradient so the button appears to have depth.
        /// </summary>
        private static Texture2D CreateRoundedTex(int w, int h, int r, bool gradient)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode   = TextureWrapMode.Clamp,
            };

            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float alpha = RoundedRectAlpha(x, y, w, h, r);
                    if (alpha <= 0f) { pixels[y * w + x] = Color.clear; continue; }

                    // Vertical brightness gradient — 0.82 at bottom (y=0), 1.0 at top (y=h-1)
                    float bright = gradient
                        ? Mathf.Lerp(0.82f, 1.00f, (float)y / (h - 1))
                        : 1f;

                    pixels[y * w + x] = new Color(bright, bright, bright, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(updateMipmaps: false);
            return tex;
        }

        /// <summary>
        /// Returns alpha for anti-aliased rounded-rectangle edge.
        /// 1.0 = fully inside, 0.0 = outside, 0–1 = soft AA edge.
        /// </summary>
        private static float RoundedRectAlpha(int x, int y, int w, int h, int r)
        {
            // Corner circle centres
            int cx = -1, cy = -1;
            if      (x <  r     && y <  r    ) { cx = r;     cy = r;     }
            else if (x >= w - r && y <  r    ) { cx = w-r-1; cy = r;     }
            else if (x <  r     && y >= h - r) { cx = r;     cy = h-r-1; }
            else if (x >= w - r && y >= h - r) { cx = w-r-1; cy = h-r-1; }
            else return 1f; // straight section — fully inside

            float dx   = x - cx;
            float dy   = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(r + 0.5f - dist);
        }

        // ─────────────────────────────────────────────────────────
        // Save + import as 9-sliced sprite
        // ─────────────────────────────────────────────────────────

        private static void GenerateAndSave(Texture2D tex, string assetPath, Vector4 border)
        {
            // Write PNG to disk
            var fullPath = Application.dataPath + assetPath["Assets".Length..];
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            // Force import
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            // Configure as 9-sliced sprite
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter imp) return;

            imp.textureType          = TextureImporterType.Sprite;
            imp.spriteImportMode     = SpriteImportMode.Single;
            imp.spriteBorder         = border;   // left, bottom, right, top
            imp.filterMode           = FilterMode.Bilinear;
            imp.alphaIsTransparency  = true;
            imp.spritePivot          = Vector2.one * 0.5f;
            imp.spritePixelsPerUnit  = 100f;
            imp.maxTextureSize       = 128;
            imp.textureCompression   = TextureImporterCompression.Uncompressed;

            var settings = imp.GetDefaultPlatformTextureSettings();
            settings.format = TextureImporterFormat.RGBA32;
            imp.SetPlatformTextureSettings(settings);

            imp.SaveAndReimport();
        }

        // ─────────────────────────────────────────────────────────
        // Folder setup
        // ─────────────────────────────────────────────────────────

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/UI");
            EnsureFolder("Assets/_Project/Art/UI/Generated");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
            var name   = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
