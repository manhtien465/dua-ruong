# CLAUDE.md - AI Playbook cho Đua Ruộng

> File này là kim chỉ nam cho AI assistant khi làm việc trên dự án. Đọc trước khi code.

---

## 1. Project Snapshot

- **Tên game**: Đua Ruộng
- **Genre**: Endless runner 3D, 3-lane swipe (Subway Surfers-style)
- **Theme**: Đua leo ruộng bậc thang Việt Nam (Mù Cang Chải, Sa Pa, Hà Giang...)
- **Platform**: Native iOS + Android
- **Engine**: Unity 2022 LTS + C# (IL2CPP backend cho native performance)
- **Mode**: Single-player only (MVP)
- **Monetization**: F2P, gắn ads sau MVP

## 2. Mục Tiêu MVP

> Định nghĩa "Done" của MVP: chơi được trên điện thoại thật, fps ≥ 60 trên máy tầm trung (Snapdragon 7-series / iPhone 11), 1 vòng chơi 60-90s tạo cảm giác "thèm chơi tiếp".

**Trong MVP:**
- 1 nhân vật chính (skin Mông cơ bản)
- 1 theme map (Mù Cang Chải mùa lúa chín)
- Endless mode duy nhất
- 3 chướng ngại + 2 item
- Tap to jump, swipe lên/xuống/trái/phải
- Hệ thống điểm + combo multiplier
- Local save (PlayerPrefs / JSON)
- Audio cơ bản (1 BGM + 5 SFX)

**KHÔNG trong MVP:**
- Online / leaderboard / PvP
- IAP, ads (gắn sau)
- Multiple characters / skins
- Story mode, daily challenge
- Cloud save, achievements

→ Nếu một feature không có trong list "Trong MVP", phải hỏi user trước khi code.

## 3. Quy Tắc Vàng (Golden Rules)

1. **Performance first**: mỗi feature phải đo profiler trên thiết bị thật. Target: 60fps, < 200MB RAM, < 100MB build size.
2. **Pool everything**: chướng ngại, item, particle, audio source — KHÔNG `Instantiate`/`Destroy` trong gameplay loop.
3. **No magic numbers**: giá trị tuning (tốc độ, jump force, score) phải nằm trong ScriptableObject hoặc config asset.
4. **Single source of truth**: state game ở 1 nơi (GameManager), UI chỉ subscribe và render.
5. **Mobile-first input**: test bằng touch, không phải bàn phím. Editor input chỉ là tiện ích dev.
6. **Atlas & compress**: texture phải vào atlas, audio dùng Vorbis (BGM) / ADPCM (SFX), model giữ < 1k tris.
7. **Không over-engineer**: MVP cần playable, không cần "đẹp về mặt kiến trúc". DI framework, ECS, custom event bus → KHÔNG dùng cho đến khi đo được pain.

## 4. Workflow Mặc Định

1. **Trước khi code**: đọc `docs/ARCHITECTURE.md` + `docs/CODING_STANDARDS.md`
2. **Trước khi thêm feature ngoài MVP**: hỏi user
3. **Trước khi commit**: chạy `unity-test` (khi đã setup), check console không lỗi/warning mới
4. **Khi gặp performance issue**: profile trước, đoán sau
5. **Khi user nói "tao muốn X"**: suy nghĩ X thuộc MVP hay phase sau, đề xuất rõ trước khi code

## 5. Ngôn Ngữ

- **Code (variable, class, comment)**: tiếng Anh
- **Game text (UI, dialog, achievement names)**: tiếng Việt
- **Doc (.md)**: tiếng Việt là chính, mix Anh cho thuật ngữ kỹ thuật
- **Commit message**: tiếng Anh, conventional commits (`feat:`, `fix:`, `perf:`...)
- **Chat với user**: tiếng Việt, register "tao/mày", ngắn gọn

## 6. Cấu Trúc Repo (Khi Đã Setup Unity)

```
dua-ruong/
├── PLAN.md                     # plan chi tiết game design
├── CLAUDE.md                   # file này
├── README.md                   # how to build & run
├── docs/
│   ├── ARCHITECTURE.md         # cấu trúc code Unity
│   ├── CODING_STANDARDS.md     # C# style guide
│   ├── TECH_STACK.md           # tools & versions
│   └── GAME_DESIGN.md          # mechanics, balance, skills
├── DuaRuong/                   # Unity project root
│   ├── Assets/
│   │   ├── _Project/           # tất cả code & asset của game (prefix _ để lên đầu)
│   │   │   ├── Scripts/
│   │   │   ├── Prefabs/
│   │   │   ├── ScriptableObjects/
│   │   │   ├── Art/
│   │   │   ├── Audio/
│   │   │   ├── Scenes/
│   │   │   └── UI/
│   │   ├── Plugins/            # 3rd-party
│   │   └── Settings/           # URP, Input, etc.
│   ├── Packages/
│   └── ProjectSettings/
└── .gitignore
```

## 7. Tham Chiếu Nhanh

- **Game design chi tiết**: `PLAN.md`
- **Code architecture**: `docs/ARCHITECTURE.md`
- **Code style**: `docs/CODING_STANDARDS.md`
- **Tech stack & versions**: `docs/TECH_STACK.md`
- **Mechanics & balance**: `docs/GAME_DESIGN.md`

## 8. Nguyên Tắc Tương Tác Với User

- User là solo dev, làm art tự, ưu tiên MVP. Không đề xuất feature ngoài MVP trừ khi user hỏi.
- Trả lời ngắn, có recommendation rõ. Đừng list 10 option khi chỉ cần 2-3.
- Khi unsure giữa A và B, đưa ra cả 2 + tradeoff + đề xuất chọn cái nào, để user quyết.
- Không tự ý chạy build / commit / push khi chưa được yêu cầu.
