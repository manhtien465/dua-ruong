# Game Design - Đua Ruộng

> Doc chi tiết về mechanics, balance, skills. Nguồn cho ScriptableObject configs.

---

## 1. Vòng Lặp Cốt Lõi (Core Loop)

```
Run forward (auto)
  ↓
Swipe → đổi bậc thang / dodge ngang
  ↓
Né obstacle, nhặt item → cộng điểm + combo
  ↓
Va chạm 1 lần → game over (không có hệ thống mạng trong MVP)
  ↓
Show score + best → "Play Again"
```

**Một run kéo dài**: 30s - 3 phút (tuỳ skill). Mỗi run cảm thấy "thua nhanh, chơi lại liền".

---

## 2. Tier System (Bậc Thang)

- **3 bậc thang** (tiers): dưới / giữa / trên — player đứng trên 1 bậc tại 1 thời điểm
- Y position (GO): tier 0 = **0.5**, tier 1 = **2.5**, tier 2 = **4.5** (unit)
- Bậc cách nhau **2 unit** theo trục Y
- Player auto chạy thẳng theo trục Z (forward)
- **SwipeUp** = leo lên bậc trên: lerp Y trong **0.2s** (EaseOutQuad)
- **SwipeDown** = tụt xuống bậc dưới: lerp Y trong **0.2s**
- Không cho chuyển tiếp nếu đang ở bậc cao nhất / thấp nhất
- Trong lúc lerp tier: collision bị suppress (IsTierChanging guard) → tránh false hit với obstacle ở bậc kế

---

## 3. Movement

### 3.1. Auto run
- Tốc độ ban đầu: **8 m/s**
- Tăng tốc theo distance: `StartSpeed + DistanceTravelled × AccelerationPerMeter`
- Tốc độ cap: **18 m/s**

### 3.2. Tier climb / drop
- SwipeUp = `TryClimbTier()` — tăng tier +1, lerp Y trong `TierChangeDuration` (0.2s)
- SwipeDown = `TryDropTier()` — giảm tier -1, lerp Y trong `TierChangeDuration`
- Không có double-move: có thể queue tier change kế tiếp ngay khi lerp đang chạy (BeginTierChange cập nhật từ mid-lerp position)

### 3.3. Lateral dodge
- SwipeLeft = `TryDodge(-1)` — lệch X **-0.8 unit**, tự trở về 0 trong **0.25s** (SmoothDamp)
- SwipeRight = `TryDodge(+1)` — lệch X **+0.8 unit**, tự trở về 0 trong **0.25s**
- Dodge không thay đổi tier, chỉ offset X nhỏ để né góc hẹp của MudPit

---

## 4. Track Generation

### 4.1. Chunks
- Mỗi chunk = 1 prefab dài **30 unit** (= ~3.5s ở tốc độ 8m/s)
- 1 chunk có **5-8 spawn points** (marker placeholder)
- Pool 6 chunk types khác nhau cho MVP (xếp xen kẽ)

### 4.2. Difficulty curve

| Distance | Spawn density | Item ratio | Speed |
|----------|---------------|------------|-------|
| 0-200m | Thấp (3-4 obstacle/chunk) | 50% | 8 m/s |
| 200-500m | Trung bình (5-6) | 35% | 10 m/s |
| 500-1000m | Khá khó (7-8) | 25% | 13 m/s |
| 1000m+ | Khó (8-10) | 15% | 15-18 m/s |

### 4.3. Spawn rules
- Tối đa **1 obstacle hoặc item mỗi row** (row = 3 tiers trên cùng Z) → luôn ≥2 tiers trống để né
- Obstacle spawn random vào 1 tier trong row → player phải đổi sang tier khác
- Khoảng cách obstacle tối thiểu: **5 unit** (đủ thời gian phản xạ)

---

## 5. Obstacles (MVP: 3 loại)

Tất cả obstacle đều **chặn 1 bậc thang** — player phải SwipeUp/Down sang bậc khác để né.

| Tên | Hành động né | Collider (local) | Visual |
|-----|--------------|-----------------|--------|
| **Trâu đứng** | SwipeUp/Down đổi tier | center=(0,1,0), size=(1.2,1.6,0.9) | Cube nâu placeholder |
| **Hàng rào tre** | SwipeUp/Down đổi tier | center=(0,1,0), size=(1.8,1.6,0.3) | Cube tan dài placeholder |
| **Vũng bùn** | SwipeUp/Down đổi tier | center=(0,0.8,0), size=(1.8,1.2,1.6) | Cube nâu thấp placeholder |

Collider luôn nằm trong window [0.2, 1.8] local Y của bậc → không "chạm" player ở bậc kế (cách 2m).

**Phase 2 thêm:** đá lăn, sương mù, cô bán hàng rong, mưa rào.

---

## 6. Items (MVP: 2 loại)

| Tên | Effect | Visual | Spawn rate |
|-----|--------|--------|------------|
| **Hạt thóc vàng** | +10 điểm | Hạt vàng nhỏ glow | Common (cụm 5 hạt) |
| **Trâu vàng nhỏ** | +200 điểm | Trâu chibi vàng | Rare (1 / 30s) |

**Phase 2 thêm:** Nón lá (shield), cây gậy trúc (boost), giày Mông (jump x2), bát phở (revive).

---

## 7. Scoring System

### 7.1. Công thức
```
TotalScore = DistanceScore + ItemScore + ComboBonus + TimeBonus
```

### 7.2. Distance score
- **+1 điểm** mỗi 1m đi qua

### 7.3. Item score
- Hạt thóc: **+10**
- Trâu vàng: **+200**

### 7.4. Combo (key feature game feel)
- "Combo" = nhặt liên tục item / né obstacle không miss
- **+1 combo** mỗi lần nhặt thóc hoặc né perfect
- **Reset** khi va chạm hoặc miss item trong tầm với
- Multiplier:
  - Combo 0-9: x1.0
  - Combo 10-24: x1.5
  - Combo 25-49: x2.0
  - Combo 50+: x3.0
- Combo apply lên item score (không apply distance)

### 7.5. Visual feedback combo
- Mỗi mốc 10 combo: hiện popup "Combo x1.5!" + screen shake nhẹ
- Combo bar trên HUD, đầy lên dần

---

## 8. Game Over Conditions

MVP đơn giản: **1 hit = game over**.

Phase 2 có thể thêm:
- 3 lives + revive bằng nón lá
- Continue bằng watch ad (sau khi gắn ads)

---

## 9. UI Flow

```
[Splash] (1s logo)
   ↓
[Main Menu]
  - Play (big button)
  - Best Score: XXX
  - Settings (icon)
   ↓
[Tutorial Overlay] (chỉ lần đầu)
  - "Vuốt trái/phải để đổi lane"
  - "Vuốt lên để nhảy"
  - "Vuốt xuống để cúi"
   ↓
[Gameplay HUD]
  - Score (top center)
  - Combo (top right, bar fill)
  - Distance (top left)
  - Pause button (top left corner)
   ↓
[Game Over]
  - Score: XXX
  - Best: XXX (highlight nếu mới)
  - "Play Again" button
  - "Main Menu" button
```

---

## 10. Audio Cue (MVP)

| Sự kiện | Audio |
|---------|-------|
| Tap menu | UI click ngắn |
| Bắt đầu run | "Bắt đầu!" voice (tiếng Việt, vui) |
| Nhặt thóc | Tinkle ngắn |
| Nhặt trâu vàng | Fanfare 1s |
| Nhảy | Whoosh nhẹ |
| Slide | Slide ngắn |
| Combo milestone | Ding 3 nốt (cao dần) |
| Va chạm | Bụp + thump trống |
| Game over | Khèn Mông sad short |

BGM: 1 track loop, 60-90 BPM, theme khèn Mông + EDM nhẹ.

---

## 11. Difficulty & Tuning Knobs (ScriptableObject)

Tất cả các giá trị sau **PHẢI** nằm trong ScriptableObject để tuning:

```csharp
PlayerConfig:
  StartSpeed, MaxSpeed, AccelerationPerMeter
  TierY[3] = {0.5, 2.5, 4.5}, TierChangeDuration
  DodgeDistance, DodgeReturnDuration

DifficultyCurve:
  AnimationCurve speedByDistance
  AnimationCurve obstacleDensityByDistance
  AnimationCurve itemRatioByDistance

ScoringConfig:
  DistanceScorePerMeter
  RiceScore, GoldBuffaloScore
  ComboThresholds[], ComboMultipliers[]

SpawnConfig:
  ChunkPrefabs[], ChunkLength, MinObstacleSpacing
  ObstaclePrefabs[], ItemPrefabs[]
```

→ Tuning chỉ sửa SO asset trong Unity Editor, KHÔNG sửa code.

---

## 12. Skills System (Phase 2 — chưa làm trong MVP)

Khi unlock thêm nhân vật, mỗi nhân vật có 1 passive skill:

| Nhân vật | Skill | Mô tả |
|----------|-------|-------|
| Em bé Mông | "Nhanh như sóc" | +5% tốc độ |
| Cô gái Thái | "Tay khéo" | Hút thóc trong 1 lane |
| Ông già Dao | "Đôi chân khoẻ" | Jump cao hơn 20% |
| Du khách Tây | "Camera xịn" | Combo decay chậm hơn |

Active skill (1 lần / run, hồi qua nhặt item):
- Boost tốc độ 5s
- Magnet (hút thóc) 5s
- Shield 1 hit
- Double score 10s

---

## 13. Mở Rộng Sau MVP (Roadmap Game Design)

1. **Story Mode**: 5 vùng × 8 màn hand-designed
2. **Boss**: cuối mỗi vùng — ví dụ "Đua với chú trâu già"
3. **Daily Challenge**: 1 seed/ngày, leaderboard tuần
4. **Versus PvP**: 2-4 người đua đường giống nhau (server hoặc P2P)
5. **Sự kiện theo mùa**: Tết (lì xì + skin áo dài), mùa lúa chín (gấp đôi điểm thóc)
