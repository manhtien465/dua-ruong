# Game Đua Ruộng Bậc Thang - Plan Chi Tiết

## 1. Ý Tưởng & Cảm Hứng

Lấy cảm hứng từ cuộc thi đua leo ruộng bậc thang đang viral ở Việt Nam (Mù Cang Chải, Hoàng Su Phì, Sa Pa). Người chơi điều khiển một nhân vật (nông dân, em bé Mông, du khách...) chạy/leo qua các bậc ruộng để về đích nhanh nhất, vừa né chướng ngại vừa nhặt vật phẩm.

**Tone & art style:**
- 2D side-scrolling hoặc 2.5D isometric
- Màu sắc: vàng lúa chín, xanh mạ non, nâu đất, trời xanh núi tím
- Nhân vật chibi dễ thương, mặc đồ dân tộc (Mông, Dao, Thái...)
- Nhạc nền: khèn Mông remix lo-fi / EDM nhẹ

---

## 2. Core Gameplay Loop

```
Start race → Leo bậc thang → Né chướng ngại → Nhặt item
   ↑                                                 ↓
   └────────── Về đích → Tính điểm → Replay ←───────┘
```

**Mỗi màn 30s - 90s.** Mục tiêu: về đích càng nhanh càng tốt + điểm combo cao.

---

## 3. Cơ Chế Điều Khiển

### 3.1. Tap (chạm màn hình)
- **Tap 1 lần**: nhảy lên bậc tiếp theo
- **Tap nhanh liên tục (tap-tap-tap)**: tăng tốc chạy ngang trên cùng 1 bậc
- **Tap giữ (hold)**: tích lực để nhảy xa, nhảy vượt 2-3 bậc cùng lúc
- **Double tap**: né nhanh sang bên (dodge)

### 3.2. Swipe (vuốt)
- **Swipe lên**: nhảy bậc cao
- **Swipe xuống**: trượt / cúi né chướng ngại trên cao
- **Swipe trái/phải**: đổi làn (lane) ngang
- **Swipe chéo lên-phải / lên-trái**: nhảy chéo qua bậc xa

### 3.3. Combo input
- **Tap + Swipe lên**: nhảy đôi (double jump)
- **Hold + thả**: nhảy bay qua 1 đoạn ruộng (powered jump)

**Tao đề xuất chọn 1 trong 2 schemes làm chủ đạo, schemes còn lại làm tuỳ chọn:**
- Scheme A (Tap-only): kiểu Flappy/Jetpack — dễ tiếp cận, dành cho casual
- Scheme B (Swipe-based): kiểu Subway Surfers — chiều sâu hơn, dành cho người chơi quen

→ **MVP CHỐT: Scheme B (Swipe-based, 3-lane Subway Surfers style).** Tap-only chỉ làm fallback / accessibility option.

---

## 4. Hệ Thống Tính Điểm

### 4.1. Điểm cơ bản
| Hành động | Điểm |
|-----------|------|
| Nhảy đúng bậc (perfect timing) | +100 |
| Nhảy sớm/muộn (vẫn qua được) | +30 |
| Nhặt thóc / hạt lúa | +50 |
| Nhặt trâu vàng (rare) | +500 |
| Né chướng ngại | +20 |
| Hoàn thành màn | +1000 |

### 4.2. Hệ số nhân (Multiplier)
- **Combo x1.5** sau 5 nhảy perfect liên tiếp
- **Combo x2** sau 10 nhảy perfect liên tiếp
- **Combo x3** sau 20 nhảy perfect liên tiếp
- **Reset combo** khi: vấp, ngã, miss item

### 4.3. Bonus thời gian
- Về đích < 30s: **+2000 (Tốc Độ Sét)**
- Về đích < 45s: **+1000 (Nhanh Như Ngựa)**
- Về đích < 60s: **+500 (Chân Khoẻ)**

### 4.4. Star rating cuối màn
- ⭐ : hoàn thành
- ⭐⭐ : > 50% điểm tối đa
- ⭐⭐⭐ : > 80% điểm tối đa + không ngã lần nào

---

## 5. Chướng Ngại & Vật Phẩm

### 5.1. Chướng ngại (obstacles)
- **Trâu đứng cản đường** → swipe trái/phải né
- **Vũng bùn trơn** → trượt nếu không nhảy
- **Cọc tre / hàng rào** → swipe xuống cúi né
- **Đá lăn từ trên xuống** → tap nhảy đúng nhịp
- **Cô bán hàng rong** (haha) → né hoặc đụng mất 2s
- **Sương mù vùng cao** → tầm nhìn giảm 3s
- **Cơn mưa rào** → trơn, nhảy khó hơn

### 5.2. Vật phẩm (items)
- **Thóc vàng**: +50 điểm
- **Trâu vàng nhỏ**: +500 điểm, hiếm
- **Nón lá**: shield 1 lần va chạm
- **Cây gậy trúc**: +50% tốc độ trong 5s
- **Đôi giày Mông**: nhảy cao gấp đôi trong 5s
- **Bát phở**: hồi 1 mạng (nếu có hệ thống mạng)

---

## 6. Game Modes

### 6.1. Story Mode (chế độ kể chuyện)
- 5 vùng đất: **Mù Cang Chải → Sa Pa → Hà Giang → Hoàng Su Phì → Y Tý**
- Mỗi vùng 8-10 màn, độ khó tăng dần
- Boss cuối vùng: thi đấu với "huyền thoại bản địa"

### 6.2. Endless Mode (vô tận)
- Ruộng bậc thang vô hạn, độ khó tăng theo thời gian
- Bảng xếp hạng toàn cầu

### 6.3. Daily Challenge
- 1 màn đặc biệt mỗi ngày, cùng seed cho mọi người chơi
- Reward: skin / item

### 6.4. Versus Mode (online PvP — phase 2)
- 2-4 người chơi cùng 1 đường đua, ai về đích trước thắng
- Có item ném (như Mario Kart): ném bùn, thả trâu cản đường

---

## 7. Hệ Thống Tiến Độ

### 7.1. Currency
- **Thóc** (soft currency): kiếm trong game
- **Vàng** (premium): mua bằng tiền thật / IAP

### 7.2. Unlock
- **Nhân vật**: 8-12 nhân vật (em bé Mông, cô gái Thái, ông già Dao, du khách Tây ba lô...)
- **Skin nhân vật**: trang phục theo mùa, lễ hội
- **Trail effect**: vệt chạy (mưa, lửa, hoa...)
- **Map theme**: mùa lúa chín, mùa nước đổ, mùa đông tuyết

### 7.3. Achievement
- "Lần đầu về đích"
- "Combo 50 hit"
- "Không ngã 1 lần trong 10 màn liên tiếp"
- "Nhặt đủ 1000 hạt thóc"
- ...

---

## 8. Tech Stack ĐÃ CHỐT

> Quyết định cuối cùng: ưu tiên performance + native + Subway Surfers style → **Unity**.
> Unity compile sang IL2CPP = native ARM code, là chuẩn công nghiệp cho 3D runner mobile (Subway Surfers, Temple Run, Crossy Road đều dùng Unity).

- **Engine**: **Unity 2022 LTS** (LTS để ổn định, không update minor liên tục)
- **Language**: C# (Unity standard)
- **Scripting backend**: IL2CPP (bắt buộc cho iOS, dùng cho cả Android)
- **Render pipeline**: URP (Universal Render Pipeline) — tối ưu mobile
- **Input**: Unity Input System package (touch + swipe)
- **UI**: UI Toolkit (mới) hoặc uGUI (cũ, ổn định hơn) → MVP dùng **uGUI**
- **Audio**: Unity built-in + 1 plugin nén âm thanh (nếu cần)
- **Build target**:
  - Android: API 24+ (Android 7.0), ARM64
  - iOS: iOS 13+, ARM64
- **Source control**: Git + Unity Smart Merge + Git LFS cho asset binary
- **CI** (sau MVP): GitHub Actions với Unity build action

**Tools làm art (DIY):**
- 3D model low-poly: Blender (free)
- Texture: Krita / Photoshop / Figma
- Audio: Audacity + freesound.org / soundly

**Tham khảo chi tiết** → `docs/TECH_STACK.md`

---

## 9. Lộ Trình Phát Triển (Roadmap)

### Phase 0: Prototype ✅ DONE
- [x] Setup Unity 2022 LTS + URP + Input System
- [x] Tier system (3 bậc thang Y-axis) thay thế lane system
- [x] SwipeUp/Down đổi bậc, SwipeLeft/Right dodge ngang
- [x] Track generation chunk-based với object pooling
- [x] Score + combo system, HUD, GameOver UI
- [x] Editor setup scripts (Build Phase 0 ▶, Build Phase 1 ▶)
- [x] **Mục tiêu**: chơi được, cảm nhận được "vibe" ✅

### Phase 1: MVP Core ✅ DONE (code)
- [x] Tier system hoàn chỉnh với IsTierChanging collision guard
- [x] 3 obstacle (Buffalo, BambooFence, MudPit) — placeholder visuals
- [x] 2 items (Rice +10đ, GoldBuffalo +200đ)
- [x] ObjectPool cho obstacles, items, chunks
- [x] ScoreSystem (distance + items × combo multiplier)
- [x] AudioManager + AudioDatabase scaffold
- [x] UI flow: MainMenu → Gameplay → GameOver
- [x] Local save (PlayerPrefs) cho high score
- [ ] **Chờ làm trong Unity**: chạy "Build Phase 1 ▶", gán audio clips, test trên máy thật
- [ ] **Mục tiêu**: playtest với 10-20 người, thu feedback

### Phase 2: Polish & Launch Soft (4-6 tuần)
- [ ] Art chính thức (thuê illustrator hoặc tự vẽ)
- [ ] 5 vùng × 8 màn = 40 màn
- [ ] Endless Mode + leaderboard
- [ ] Daily Challenge
- [ ] Hệ thống nhân vật + skin
- [ ] Đóng gói Capacitor → APK Android testflight
- [ ] **Mục tiêu**: launch web + Android, thu user thật

### Phase 3: Live ops (liên tục)
- [ ] Versus Mode online
- [ ] Sự kiện mùa: Tết, mùa lúa chín, Trung Thu
- [ ] iOS App Store
- [ ] Marketing: video ngắn TikTok/Reels theo trend gốc

---

## 10. Monetization (nếu muốn kiếm tiền)

- **Rewarded ad**: xem video để hồi sinh / nhân đôi điểm
- **Banner ad**: ở menu chính (không trong gameplay)
- **IAP**: gói "không quảng cáo" (~50k VND), gói skin, gói thóc
- **Battle Pass mùa**: 79k VND / mùa, unlock skin + reward
- **KHÔNG dùng pay-to-win** — giữ fair play

---

## 11. Marketing Hooks

- Tận dụng trend gốc: dùng video cuộc thi thật làm teaser
- Mời KOLs vùng cao chơi thử (em bé Mông, các travel vlogger)
- Hashtag: `#dauruongbacthang` `#mucangchaichallenge`
- Cho phép share replay video kèm leaderboard
- Collab với du lịch địa phương (mã giảm giá tour Mù Cang Chải khi đạt mốc)

---

## 12. Quyết Định Đã Chốt

| Câu hỏi | Quyết định |
|---------|-----------|
| Platform | **Native iOS + Android** (Unity → IL2CPP) |
| Style đồ họa | **3D low-poly, Subway Surfers-style** (3-lane runner) |không .chỉ là style đồ hoạ giống Subway Surfers. còn không phải 3 lane.
| Mode chơi | **Single-player only** (MVP) | đúng
| Monetization | **Free-to-play, gắn ads sau MVP** | đúng
| Art | **Tự làm (DIY)** — Blender + Krita | đúng
| Tên game | **Đua Ruộng** | đúng

**Hệ quả của các quyết định này:**
- Bỏ tất cả backend / online / leaderboard ra khỏi MVP
- Bỏ IAP / ads SDK ra khỏi MVP (focus gameplay trước)
- Lock vào Unity 2022 LTS + URP + uGUI (xem mục 8)
- 3-lane swipe controls thay vì tap-only (Scheme B trong mục 3)

---

## 13. Bước Tiếp Theo

→ Confirm những câu hỏi mục 12, rồi tao bắt đầu Phase 0: setup project + làm prototype tap-to-jump trong vòng vài giờ.
