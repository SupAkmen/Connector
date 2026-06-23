# Connector

Game puzzle nối điểm (connect-the-dots / flow) làm bằng **Unity** với **URP 2D**.

Người chơi vẽ các đường nối (Edge) giữa các điểm trên lưới để hoàn thành từng level. Game chia theo **Stage** (chủ đề/màu) và mỗi stage có **50 Level**, mở khóa tuần tự.

---

## Trạng thái dự án

| Phần | Trạng thái |
|------|-----------|
| Menu chính, chọn stage/level, mở khóa | ✅ Đã code |
| Lưu tiến độ (PlayerPrefs) | ✅ Đã code |
| Dữ liệu level (ScriptableObject) | ✅ Đã code |
| Quản lý âm thanh | ✅ Đã code |
| **Gameplay (vẽ đường, kiểm tra thắng)** | ⚠️ **Chưa code** — `GameplayManager.cs` và `Node.cs` còn rỗng |

---

## Cấu trúc thư mục

```
Assets/
├── Common/                  # Tài nguyên dùng chung
│   ├── Prefabs/
│   │   ├── Levels/Levels.asset      # LevelList: danh sách tất cả level
│   │   └── DefaultLevel.asset       # Level mặc định (fallback)
│   └── Scripts/
│       ├── LevelData.cs             # SO: 1 level = danh sách Edge
│       └── LevelList.cs             # SO: tập hợp tất cả LevelData
├── Project/
│   ├── Scenes/
│   │   ├── MainMenu.unity           # Menu + chọn stage/level
│   │   └── GamePlay.unity           # Màn chơi
│   ├── Prefabs/                     # Level.prefab, Stage.prefab (UI button)
│   └── Scripts/
│       ├── GameManager.cs           # Singleton: tiến độ, load level, đổi scene
│       ├── MainMenuManager.cs       # Điều khiển panel menu
│       ├── StageButtonManager.cs    # Nút chọn stage
│       ├── LevelButton.cs           # Nút chọn level (khóa/mở)
│       ├── SoundManager.cs          # Singleton: phát SFX
│       ├── GameplayManager.cs       # ⚠️ rỗng — logic chơi
│       └── Node.cs                  # ⚠️ rỗng — điểm trên lưới
├── Settings/                # Cấu hình URP 2D
└── Resources/, Editor/, ExternalAsset/, LevelGenerator/, TextMesh Pro/
```

---

## Kiến trúc & luồng hoạt động

### Singleton

- **`GameManager`** (`Connect.Core`) — `DontDestroyOnLoad`, tồn tại xuyên scene.
  - Giữ `CurrentStage`, `CurrentLevel`, `StageName`.
  - `Init()` nạp tất cả level từ `LevelList` vào `Dictionary<string, LevelData>` theo `LevelName`.
- **`SoundManager`** — phát hiệu ứng âm thanh qua `PlaySound(AudioClip)`.

### Dữ liệu level (ScriptableObject)

- **`LevelData`** = `LevelName` + `List<Edge>`.
- **`Edge`** = `List<Vector2Int> Points`; có `StartPoint` (điểm đầu) và `EndPoint` (điểm cuối).
- **`LevelList`** = `List<LevelData>` — toàn bộ level của game.

### Quy ước đặt tên level

Key tra cứu ghép từ stage + level:

```
"Level" + CurrentStage + CurrentLevel   →  ví dụ "Level23" = Stage 2, Level 3
```

Dùng cho cả `PlayerPrefs` (lưu mở khóa) lẫn `Dictionary` (tra `LevelData`).

### Tiến độ & mở khóa

- **`IsLevelUnlock(level)`** — level 1 luôn mở; còn lại đọc `PlayerPrefs` (1 = mở, 0 = khóa).
- **`UnlockLevel()`** — tăng `CurrentLevel`; quá 50 → sang stage kế; quá stage 7 → quay về menu chính. Lưu trạng thái mở vào `PlayerPrefs`.
- **`GetLevel()`** — trả `LevelData` theo key hiện tại, fallback `DefaultLevel`.

### Luồng UI menu

```
TitlePanel ──ClickedPlay──▶ StagePanel ──ClickedStage──▶ LevelPanel ──Clicked level──▶ GamePlay scene
     ▲                          │                            │
     └──── BackToTitle ─────────┘         BackToStage ◀───────┘
```

- `StageButtonManager` đặt stage hiện tại rồi gọi `MainMenuManager.ClickedStage(name, color)`.
- `MainMenuManager.ClickedStage` lưu màu chủ đề + bắn event `LevelOpened`.
- Mỗi `LevelButton` lắng nghe `LevelOpened`: đọc số level từ tên GameObject (`..._<số>`), hỏi `IsLevelUnlock`, tô màu mở/khóa. Click chỉ vào được khi đã mở.

---

## Hằng số / quy ước

- 50 level mỗi stage; tối đa 7 stage (stage 8 → reset về menu).
- Tên GameObject của `LevelButton` phải kết thúc bằng `_<số level>` (vd `Level_5`).
- Scene: `"MainMenu"`, `"Gameplay"` — phải khớp Build Settings.

---

## Cần làm tiếp

1. **`Node.cs`** — biểu diễn 1 điểm/ô trên lưới (vị trí `Vector2Int`, màu, trạng thái nối).
2. **`GameplayManager.cs`** — dựng lưới từ `GameManager.Instance.GetLevel()`, xử lý kéo/vẽ đường theo `Edge`, kiểm tra thắng, gọi `UnlockLevel()` + `SoundManager.PlaySound`.
3. Kết nối thư mục **`LevelGenerator/`** (công cụ tạo level) với `LevelList`.
