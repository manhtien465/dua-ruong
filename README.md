# Đua Ruộng

Mobile endless runner 3D lấy cảm hứng từ cuộc thi đua leo ruộng bậc thang Việt Nam.

> Vuốt qua các bậc ruộng vàng rực Mù Cang Chải, né trâu cản đường, nhặt thóc vàng. Đua xem ai về đích nhanh nhất.

---

## Quick Info

- **Engine**: Unity 2022.3 LTS (C#, IL2CPP)
- **Platform**: iOS + Android
- **Style**: 3D low-poly, 3-lane swipe runner (Subway Surfers-inspired)
- **Genre**: Endless runner, single-player
- **Status**: 🚧 Tiền MVP — đang setup

## Docs

| File | Nội dung |
|------|----------|
| [PLAN.md](./PLAN.md) | Plan tổng thể: ý tưởng, scope, roadmap |
| [CLAUDE.md](./CLAUDE.md) | Playbook cho AI assistant — đọc đầu tiên |
| [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) | Cấu trúc code Unity |
| [docs/CODING_STANDARDS.md](./docs/CODING_STANDARDS.md) | C# style guide |
| [docs/TECH_STACK.md](./docs/TECH_STACK.md) | Versions & tools |
| [docs/GAME_DESIGN.md](./docs/GAME_DESIGN.md) | Mechanics chi tiết |

## Setup (sẽ làm sau khi tạo Unity project)

1. Cài Unity Hub → cài Unity 2022.3 LTS với Android + iOS Build Support
2. Cài Git LFS: `git lfs install`
3. Clone repo: `git clone <url>`
4. Mở Unity project tại `DuaRuong/`
5. File → Build Settings → Switch Platform sang Android hoặc iOS
6. Press Play

## Build

### Android
```
Unity → File → Build Settings → Android → Build → output .aab
```

### iOS
```
Unity → File → Build Settings → iOS → Build → mở Xcode project → Archive
```

## Roadmap Tóm Tắt

- [x] Plan & docs
- [ ] Phase 0: Unity project skeleton + prototype tap/swipe
- [ ] Phase 1: MVP gameplay loop (60-90s playable run)
- [ ] Phase 2: Polish, art final, soft launch
- [ ] Phase 3: Live ops, ads, các mode thêm

Chi tiết tại [PLAN.md](./PLAN.md).
