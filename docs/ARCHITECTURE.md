# Architecture - Đua Ruộng

> Kiến trúc code cho Unity project. Đọc kỹ trước khi thêm system mới.

---

## 1. Nguyên Tắc Kiến Trúc

1. **Composition over inheritance** — nhân vật, chướng ngại = các Component nhỏ ráp lại, không kế thừa lớp cha to
2. **ScriptableObject làm config** — mọi tuning value (speed, jump force, score, spawn rate) đều ở SO, KHÔNG hard-code
3. **Event-driven** — system giao tiếp qua C# event hoặc UnityEvent, tránh `FindObjectOfType` và direct reference giữa các module
4. **Single GameManager** — 1 singleton quản lý state game (Menu / Playing / Paused / GameOver). UI chỉ subscribe.
5. **Pooling cho mọi thứ spawn lặp lại** — chướng ngại, item, particle, audio, UI feedback
6. **Separation of concerns**:
   - `*Logic` script: rule, math, không touch transform/UI
   - `*View` script: render, animation, UI
   - `*Controller`: cầu nối giữa Logic ↔ View ↔ Input

---

## 2. Cấu Trúc Folder Trong `Assets/_Project/`

```
_Project/
├── Scripts/
│   ├── Core/                   # Singleton, EventBus, ServiceLocator (nếu cần)
│   │   ├── GameManager.cs
│   │   ├── ServiceLocator.cs
│   │   └── Events/
│   ├── Gameplay/
│   │   ├── Player/             # PlayerController, PlayerInput, PlayerAnimator
│   │   ├── Track/              # TrackGenerator, ChunkPool, LaneSystem
│   │   ├── Obstacles/          # ObstacleBase + Trau, Buffalo, MudPit, Fence...
│   │   ├── Items/              # ItemBase + Rice, GoldBuffalo, Hat, Stick...
│   │   └── Camera/             # FollowCamera, CameraShake
│   ├── Systems/
│   │   ├── Score/              # ScoreSystem, ComboSystem
│   │   ├── Save/               # SaveSystem (PlayerPrefs / JSON)
│   │   ├── Audio/              # AudioManager, SfxPlayer
│   │   ├── Pool/               # ObjectPool<T>
│   │   └── Input/              # InputReader (wraps Unity Input System)
│   ├── UI/
│   │   ├── HUD/                # ScoreHUD, ComboHUD, PauseButton
│   │   ├── Menus/              # MainMenu, GameOverMenu, SettingsMenu
│   │   └── Common/             # ButtonAnimator, ToastMessage
│   └── Utils/                  # Extensions, Helpers, Math
├── ScriptableObjects/
│   ├── Configs/
│   │   ├── PlayerConfig.asset
│   │   ├── DifficultyCurve.asset
│   │   ├── ScoringConfig.asset
│   │   └── SpawnConfig.asset
│   ├── Obstacles/              # Mỗi obstacle = 1 SO data
│   ├── Items/
│   └── Audio/                  # AudioClipDatabase
├── Prefabs/
│   ├── Player/
│   ├── Track/
│   ├── Obstacles/
│   ├── Items/
│   ├── UI/
│   └── VFX/
├── Art/
│   ├── Models/
│   ├── Textures/
│   ├── Materials/
│   └── Animations/
├── Audio/
│   ├── BGM/
│   └── SFX/
├── Scenes/
│   ├── Bootstrap.unity         # Init persistent systems
│   ├── MainMenu.unity
│   └── Gameplay.unity
└── UI/
    └── Sprites/
```

---

## 3. Core Systems

### 3.1. GameManager (singleton)
- Sở hữu state machine: `Boot → MainMenu → Playing → Paused → GameOver`
- Phát event khi đổi state (`OnGameStarted`, `OnGameOver`, ...)
- KHÔNG chứa logic gameplay (đẩy xuống Player, Track, Score system)

### 3.2. InputReader (ScriptableObject hoặc MonoBehaviour singleton)
- Bọc Unity Input System
- Phơi bày các event `OnSwipeLeft`, `OnSwipeRight`, `OnSwipeUp`, `OnSwipeDown`, `OnTap`
- Player subscribe → không cần biết thiết bị (touch / mouse / keyboard)

### 3.3. Track Generation (Chunk-based)
- Map chia thành **chunks** dài ~30-50m, mỗi chunk = 1 prefab
- `TrackGenerator` spawn chunk phía trước, despawn chunk phía sau
- Mỗi chunk có **spawn points** marker để place obstacles/items theo `SpawnConfig`
- Difficulty curve điều khiển spawn density theo distance

### 3.4. Object Pooling
- Generic `ObjectPool<T>` cho mọi prefab spawn thường xuyên
- KHÔNG bao giờ `Instantiate`/`Destroy` trong gameplay loop
- Pool size tính sẵn dựa trên max simultaneous

### 3.5. Score & Combo System
- `ScoreSystem` lắng nghe event `OnPickupItem`, `OnPerfectJump`, `OnObstacleAvoided`
- `ComboSystem` track streak, reset on `OnPlayerHit`
- Final score = base + items + (perfect_jumps × multiplier) + time_bonus

### 3.6. Save System
- MVP: `PlayerPrefs` cho high score, settings (audio on/off)
- Nếu cần phức tạp hơn: serialize JSON vào `Application.persistentDataPath`

---

## 4. Data Flow Trong 1 Frame Gameplay

```
[Touch Input]
    ↓ (Unity Input System)
[InputReader] → fires SwipeUp / SwipeDown / SwipeLeft / SwipeRight
    ↓
[PlayerController] → calls TryClimbTier / TryDropTier / TryDodge(-1|+1)
    ↓
[PlayerMovement] → lerps Y to new tier OR snaps X dodge (auto-returns to 0)
                    updates DistanceTravelled each frame
    ↓
[DistanceReporter] (Update) → reads DistanceTravelled → ScoreSystem.AddDistance
    ↓
[ScoreSystem] → accumulates score, fires ScoreChanged / DistanceChanged
    ↓
[HudController] (subscriber) → updates score / distance / combo text

[TrackGenerator] (Update every frame)
    → while chunk ahead < spawnAheadDistance → SpawnNextChunk from pool
    → while oldest chunk too far behind → ReleaseChunk → returns obs/items to pools
    → PopulateChunk → 1 obstacle or item per row max (≥2 tiers free)

[PlayerCollision.OnTriggerEnter]
    → if IsTierChanging: skip (suppress during tier lerp)
    → ItemBase.Collect() → RaiseRiceCollected / RaiseGoldBuffaloCollected
      → ComboSystem.Increment → ScoreSystem adds item score with multiplier
    → ObstacleBase.OnPlayerHit() → RaisePlayerHit
      → GameManager.EndGame() → RaiseGameOver
      → GameOverController shows score panel
      → ComboSystem resets
```

---

## 5. Quy Tắc Khi Thêm Feature Mới

1. **Hỏi**: feature này thuộc system nào? (Gameplay / UI / Core / Audio)
2. **Tạo folder + class** trong đúng folder, không vứt vào `Scripts/` root
3. **Config qua ScriptableObject** nếu có giá trị cần tuning
4. **Phơi bày qua event**, không let UI gọi trực tiếp gameplay code
5. **Nếu prefab spawn lặp lại** → đăng ký pool ngay từ đầu
6. **Test trên device thật** trước khi merge — Editor không phản ánh đúng performance mobile

---

## 6. Anti-Patterns Cần Tránh

- ❌ `GameObject.Find()` / `FindObjectOfType()` trong `Update()`
- ❌ `Camera.main` trong `Update()` (cache trong `Awake`)
- ❌ String-based event names — dùng C# `event` hoặc `Action`
- ❌ Singleton chained (A.instance.B.instance.C) — dùng ServiceLocator nếu cần nhiều
- ❌ `Resources.Load()` runtime — dùng Addressables hoặc reference trực tiếp qua SO
- ❌ Copy-paste prefab cho từng obstacle thay vì base class + SO data
- ❌ Update() empty hoặc chỉ for show — Unity vẫn tốn ms gọi nó

---

## 7. Performance Budget (Target)

| Metric | Target | Hard limit |
|--------|--------|------------|
| FPS | 60 | ≥ 30 trên máy yếu |
| Draw calls | < 50 | < 100 |
| Tris on screen | < 50k | < 100k |
| Texture memory | < 80MB | < 150MB |
| RAM total | < 200MB | < 400MB |
| APK / IPA size | < 80MB | < 150MB |

Nếu vi phạm hard limit → STOP feature, profile và fix trước khi thêm cái mới.
