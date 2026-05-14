# Tech Stack - Đua Ruộng

> Versions cố định. Không tự upgrade nếu chưa test compat.

---

## 1. Engine

| Tool | Version | Lý do |
|------|---------|-------|
| Unity | **2022.3 LTS** (cụ thể: 2022.3.40f1 hoặc mới hơn trong dòng 2022.3) | LTS = ổn định 2 năm, không break API |
| Visual Studio / Rider | Mới nhất | IDE chính cho C# |

**Cài Unity qua Unity Hub.** Modules cần thiết:
- Android Build Support (+ Android SDK & NDK Tools, OpenJDK)
- iOS Build Support (chỉ trên Mac)
- Documentation

---

## 2. Render & Pipeline

| Component | Lựa chọn |
|-----------|----------|
| Render Pipeline | **URP (Universal Render Pipeline)** |
| Color Space | Linear |
| Graphics API Android | Vulkan + GLES3 fallback |
| Graphics API iOS | Metal |
| Quality preset MVP | 1 preset duy nhất "Mobile" |

URP version: gói qua Package Manager, version match với Unity LTS.

---

## 3. Unity Packages (Required)

```
com.unity.inputsystem        — touch, swipe, multi-platform input
com.unity.render-pipelines.universal  — URP
com.unity.cinemachine         — camera follow & shake
com.unity.textmeshpro         — UI text (built-in từ 2022)
com.unity.addressables        — asset loading (Phase 2, không cho MVP)
```

---

## 4. Third-Party Packages (Optional)

| Package | Mục đích | MVP? |
|---------|----------|------|
| [DOTween](http://dotween.demigiant.com/) | Tweening UI & gameplay | ✅ MVP |
| [UniTask](https://github.com/Cysharp/UniTask) | Async không alloc | ✅ MVP |
| [More Mountains Feel](https://feel.moremountains.com/) | Game feel (juice) | ❌ chỉ khi cần |
| Odin Inspector | Inspector mạnh hơn | ❌ paid, không cần |

---

## 5. Build Targets

### Android
- Min API: **24** (Android 7.0)
- Target API: 34
- Architecture: **ARM64 only** (drop ARMv7 cho MVP để giảm size)
- Scripting backend: **IL2CPP**
- Format: AAB cho Play Store, APK cho test

### iOS
- Min iOS: **13.0**
- Architecture: ARM64
- Scripting backend: IL2CPP (mặc định)
- Xcode mới nhất

---

## 6. Art Pipeline

### 3D models
- Tool: **Blender 4.x** (free)
- Export: `.fbx` (binary, Y-up convert sang Z-up tự động)
- Naming: `m_Player_Mong`, `m_Obstacle_Trau`, `m_Item_RiceGold`
- Tris budget:
  - Player: < 3000 tris
  - Obstacle: < 500 tris
  - Item: < 200 tris
  - Background prop: < 1000 tris

### Textures
- Tool: **Krita** hoặc Photoshop / Figma
- Format: `.png` source, Unity import sang ASTC (Android) / ASTC (iOS)
- Size:
  - Character albedo: 1024x1024 max
  - Obstacle: 512x512
  - UI: 256x256 hoặc dùng SDF cho icon scale-friendly
- **Atlas hoá** UI sprites bằng Sprite Atlas

### Animation
- Skeletal animation từ Blender (Mecanim Humanoid nếu nhân vật người)
- Loop run + jump + slide + die là đủ cho MVP

---

## 7. Audio

- Format source: `.wav` 44.1kHz
- Unity import:
  - **BGM**: Vorbis, quality 70, Streaming
  - **SFX**: ADPCM nếu < 1s, Vorbis quality 50 nếu dài hơn, Decompress on Load
- Tool: **Audacity** (free) để cắt/normalize
- Source âm thanh free:
  - [freesound.org](https://freesound.org)
  - [zapsplat.com](https://zapsplat.com) (free with account)
  - [pixabay.com/music](https://pixabay.com/music)

---

## 8. Source Control

- **Git** + **Git LFS** cho:
  - `*.fbx`, `*.blend`, `*.png`, `*.psd`, `*.wav`, `*.mp3`, `*.unity` (scene), `*.asset` (binary)
- `.gitignore` (xem repo root)
- Smart Merge: setup `mergetool` của Unity vào git config

```bash
git lfs install
git lfs track "*.fbx" "*.blend" "*.png" "*.psd" "*.wav" "*.mp3"
```

---

## 9. CI / Build (Phase 2, sau MVP)

- **GitHub Actions** + [GameCI Unity Builder](https://game.ci/docs/github/getting-started)
- Trigger: push to `main` → build Android APK + iOS Xcode project
- Cache `Library/` để build nhanh

KHÔNG setup CI cho MVP — manual build từ Unity Editor đủ.

---

## 10. Versioning

- Game version: **SemVer** `MAJOR.MINOR.PATCH`
- MVP target: `0.1.0` (alpha) → `0.5.0` (closed beta) → `1.0.0` (launch)
- Bundle version code Android: monotonic int (1, 2, 3, ...)
- Build number iOS: monotonic int

---

## 11. Tools Workflow Đề Xuất

| Việc | Tool |
|------|------|
| Code | VSCode hoặc JetBrains Rider |
| 3D model | Blender |
| Texture / 2D | Krita |
| Audio edit | Audacity |
| Audio source | freesound.org, pixabay |
| Reference research | Pinterest (board "ruộng bậc thang") |
| Task tracking | GitHub Issues hoặc Notion |
| Version control | Git + GitHub |
