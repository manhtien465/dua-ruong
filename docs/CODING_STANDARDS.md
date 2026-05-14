# Coding Standards - Đua Ruộng

> C# style guide cho Unity. Bắt buộc tuân thủ. Vi phạm = revert hoặc fix trước khi merge.

---

## 1. Naming Conventions

| Loại | Quy ước | Ví dụ |
|------|---------|-------|
| Class / struct | PascalCase | `PlayerController`, `ScoreSystem` |
| Method | PascalCase | `Jump()`, `AddScore()` |
| Public field / property | PascalCase | `public int MaxLives` |
| Private field | `_camelCase` | `private int _currentLane` |
| Serialized private field | `_camelCase` + `[SerializeField]` | `[SerializeField] private float _jumpForce;` |
| Const / static readonly | `UPPER_SNAKE` | `const int MAX_LANES = 3;` |
| Local variable | camelCase | `var nextChunk = pool.Get();` |
| Parameter | camelCase | `void Move(int laneIndex)` |
| Event | PascalCase, prefix `On` | `event Action OnPlayerHit;` |
| Interface | `I` prefix | `IPoolable`, `ISaveable` |
| ScriptableObject asset | PascalCase, suffix `Config` / `Data` | `PlayerConfig.asset` |

**Đặt tên có nghĩa:** không dùng `mgr`, `ctrl`, `tmp`, `data1`. Tên class/method phải đọc lên hiểu ngay.

---

## 2. File Organization

- **1 class = 1 file**, tên file = tên class
- File path phản ánh namespace (nếu dùng namespace)
- Thứ tự trong file:
  1. `using` directives
  2. `namespace` (optional cho MVP)
  3. Constants
  4. Serialized fields (`[SerializeField]`)
  5. Public properties
  6. Private fields
  7. Events
  8. Unity callbacks (`Awake → OnEnable → Start → Update → OnDisable → OnDestroy`)
  9. Public methods
  10. Private methods

---

## 3. Unity-Specific Patterns

### 3.1. Cache references trong `Awake()`

```csharp
// ❌ BAD
void Update() {
    transform.position += Camera.main.transform.forward * Time.deltaTime;
}

// ✅ GOOD
private Transform _cameraTransform;
void Awake() {
    _cameraTransform = Camera.main.transform;
}
void Update() {
    transform.position += _cameraTransform.forward * Time.deltaTime;
}
```

### 3.2. Dùng `[SerializeField] private` thay vì `public`

```csharp
// ❌ BAD — phơi bày field cho code khác sửa
public float jumpForce = 10f;

// ✅ GOOD — chỉ Inspector thấy, code khác không sửa được
[SerializeField] private float _jumpForce = 10f;
```

### 3.3. Tránh `null` check Unity object bằng `==`

Unity override `==` cho `UnityEngine.Object`, nhưng tốn performance:

```csharp
// ❌ BAD trong hot path
if (target == null) return;

// ✅ GOOD — check 1 lần rồi cache
private bool _hasTarget;
void Awake() { _hasTarget = target != null; }
```

### 3.4. KHÔNG dùng coroutines trong gameplay loop

Coroutines tạo garbage. Dùng:
- `Update()` + counter cho timing đơn giản
- `async UniTask` (nếu add UniTask package) cho async phức tạp

### 3.5. KHÔNG `string` hóa event/animation

```csharp
// ❌ BAD
animator.SetTrigger("Jump");

// ✅ GOOD
private static readonly int JumpHash = Animator.StringToHash("Jump");
animator.SetTrigger(JumpHash);
```

---

## 4. Performance Rules (Hot Path)

`Update()`, `FixedUpdate()`, và `OnTriggerXxx` chạy mỗi frame → tránh allocate:

- ❌ KHÔNG `new` trong hot path (kể cả `new Vector3()` thì OK vì struct, nhưng `new List<>()` thì KHÔNG)
- ❌ KHÔNG `foreach` trên `IEnumerable` (allocate enumerator); dùng `for` với `List<T>`
- ❌ KHÔNG `string` concat / `string.Format` mỗi frame; dùng `StringBuilder` cached
- ❌ KHÔNG LINQ trong hot path
- ❌ KHÔNG `GetComponent<T>()` mỗi frame; cache reference trong `Awake`
- ✅ Dùng `OnTriggerEnter` cho collision phát 1 lần, không dùng `OnTriggerStay`

---

## 5. ScriptableObject Pattern

```csharp
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "DuaRuong/Configs/Player")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float _runSpeed = 10f;
    [SerializeField] private float _laneChangeSpeed = 15f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _jumpDuration = 0.5f;

    public float RunSpeed => _runSpeed;
    public float LaneChangeSpeed => _laneChangeSpeed;
    public float JumpForce => _jumpForce;
    public float JumpDuration => _jumpDuration;
}
```

- Tên `[CreateAssetMenu]` luôn bắt đầu với `DuaRuong/`
- Field private + property readonly → không sửa runtime accidentally
- Một SO = một concept, không gộp 50 field vào 1 SO

---

## 6. Event Pattern

```csharp
// ✅ Dùng C# Action / event, không dùng UnityEvent runtime
public static class GameEvents
{
    public static event Action<int> OnScoreChanged;
    public static event Action OnPlayerDied;

    public static void RaiseScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
}

// Subscribe
void OnEnable() => GameEvents.OnScoreChanged += HandleScore;
void OnDisable() => GameEvents.OnScoreChanged -= HandleScore;  // ← BẮT BUỘC unsubscribe
```

---

## 7. Comments

- Default: **không viết comment**. Tên hàm/biến tốt = comment thừa.
- Chỉ viết comment khi:
  - Workaround cho Unity bug → ghi rõ link issue
  - Hằng số magic mà không thể đặt tên rõ
  - Logic không obvious (vd: tại sao lerp factor là 0.93 thay vì 0.95)
- KHÔNG viết comment "// TODO" mà không có ngày + người làm
- KHÔNG viết XML doc cho method private

---

## 8. Git Conventions

### Commit message
```
<type>(<scope>): <subject>

<body — optional, why + what>
```

Type: `feat`, `fix`, `perf`, `refactor`, `art`, `audio`, `chore`, `docs`

Ví dụ:
```
feat(player): add lane change with swipe input
perf(track): pool obstacle prefabs to reduce GC
fix(score): combo not resetting on hit
art(player): replace placeholder with rigged Mong character model
```

### Branch
- `main` — production-ready, build được luôn
- `dev` — integration branch
- `feat/<short-name>` — feature branches
- `fix/<short-name>` — bug fix branches

### KHÔNG commit
- `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`
- File `.csproj`, `.sln` (Unity tự generate)
- `UserSettings/`
- File `.meta` của file đã ignore

---

## 9. Code Review Checklist

Trước khi merge, self-check:

- [ ] Không có `Debug.Log` thừa
- [ ] Không có `GameObject.Find` / `FindObjectOfType` trong hot path
- [ ] Tất cả event subscribe trong `OnEnable` đều unsubscribe trong `OnDisable`
- [ ] Magic numbers đã được đưa vào ScriptableObject hoặc const
- [ ] Prefab spawn lặp lại đã dùng ObjectPool
- [ ] Không có warning trong Unity console
- [ ] Test trên device thật (không chỉ Editor)
- [ ] Build vẫn ra (Android Player Settings không bị break)

---

## 10. Khi Conflict Với Quy Tắc

Nếu một quy tắc cản trở giải quyết vấn đề, **comment rõ ràng** lý do bypass và **discuss với team** (hoặc với chính mình ở commit message). KHÔNG silent override.
