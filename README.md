# Connector

Game puzzle nối điểm (connect-the-dots / flow) làm bằng **Unity 6** (6000.0.70f1) với **URP 2D**, target **Android**.

Người chơi kéo chuột nối các điểm cùng màu trên lưới thành đường, phủ kín bàn để thắng. Game chia theo **Stage** (chủ đề/màu) và mỗi stage có **50 Level**, mở khóa tuần tự.

---

## Trạng thái dự án

| Phần | Trạng thái |
|------|-----------|
| Menu chính, chọn stage/level, mở khóa | ✅ Đã code |
| Lưu tiến độ (PlayerPrefs) | ✅ Đã code |
| Dữ liệu level (ScriptableObject) | ✅ Đã code |
| Quản lý âm thanh | ✅ Đã code |
| **Gameplay (dựng lưới, vẽ đường, kiểm tra thắng)** | ✅ Đã code — `GamePlayManager.cs` + `Node.cs` |
| Công cụ tạo level (`LevelGenerator/`) | 🛠️ Có sẵn, chạy trong Editor |

---

## Cấu trúc thư mục

```
Assets/
├── Common/                  # Tài nguyên dùng chung
│   ├── Prefabs/
│   │   ├── Levels/                  # Các LevelData.asset (Level130, Level140, …)
│   │   └── DefaultLevel.asset       # Level mặc định (fallback)
│   └── Scripts/
│       ├── LevelData.cs             # SO: 1 level = danh sách Edge   (namespace Connect.common)
│       └── LevelList.cs             # SO: List<LevelData> Levels     (namespace Connect.common)
├── Project/
│   ├── Scenes/
│   │   ├── MainMenu.unity           # Menu + chọn stage/level
│   │   └── GamePlay.unity           # Màn chơi
│   ├── Prefabs/                     # Level.prefab, Stage.prefab (UI button), Node, Board…
│   └── Scripts/
│       ├── GameManager.cs           # Singleton: tiến độ, load level, đổi scene
│       ├── MainMenuManager.cs       # Điều khiển panel menu
│       ├── StageButtonManager.cs    # Nút chọn stage
│       ├── LevelButton.cs           # Nút chọn level (khóa/mở)
│       ├── SoundManager.cs          # Singleton: phát SFX
│       ├── GamePlayManager.cs       # Dựng board + node, xử lý input, kiểm tra thắng
│       └── Node.cs                  # 1 điểm/ô lưới: màu, cạnh nối, logic nối/gỡ đường
├── Settings/                # Cấu hình URP 2D
└── Resources/, Editor/, ExternalAsset/, LevelGenerator/, TextMesh Pro/
```

---

## Kiến trúc & luồng hoạt động

### Singleton

- **`GameManager`** (`Connect.Core`) — `DontDestroyOnLoad`, tồn tại xuyên scene.
  - Giữ `CurrentStage`, `CurrentLevel`, `StageName`.
  - `Init()` nạp tất cả level từ `LevelList.Levels` vào `Dictionary<string, LevelData>` theo `LevelName`.
- **`SoundManager`** — `DontDestroyOnLoad`, phát SFX qua `PlaySound(AudioClip)`.
- **`MainMenuManager`** (`instance`) — sống trong scene MainMenu, điều khiển panel.
- **`GamePlayManager`** (`Instance`) — sống trong scene GamePlay, dựng màn chơi.

### Dữ liệu level (ScriptableObject)

- **`LevelData`** = `LevelName` + `List<Edge>`.
- **`Edge`** = `List<Vector2Int> Points`; `StartPoint` = Points[0], `EndPoint` = Points cuối.
- **`LevelList`** = `List<LevelData> Levels` — toàn bộ level của game.

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

## Gameplay (scene GamePlay)

### Dựng màn — `GamePlayManager.Awake`

Kích cỡ lưới = **`CurrentStage + 4`** (stage 1 → 5×5, stage 3 → 7×7…).

1. **`SpawnBoard()`** — tạo board nền + các ô `_bgCellPrefab`, chỉnh `Camera.orthographicSize` và vị trí cho khớp cỡ lưới.
2. **`SpawnNodes()`** — instantiate 1 `Node` mỗi ô:
   - `GetColorID(i,j)` dò `CurrentLevelData.Edges`; nếu ô là `StartPoint`/`EndPoint` của edge thứ `colorId` → node đó là **điểm màu** (bật `_point`, tô `NodeColors[colorId]`); không thì node trống.
   - Nối cạnh: mỗi node biết 4 hàng xóm (up/down/left/right) qua `SetEdge`.

> ⚠️ **Board và Node phải cùng công thức `CurrentStage + 4`.** Nếu lệch (vd Node dùng `CurrentLevel`) → lưới node nhỏ hơn board, điểm màu bị cắt/thiếu.

### Input — `GamePlayManager.Update`

- Chuột trái xuống trên 1 node `IsClickable` → chọn `startNode`, bật `_clickHighlight`.
- Rê chuột sang node kề khác màu-cuối hợp lệ → `startNode.UpdateInput(tempNode)` nối đường, rồi `CheckWin()`.

### Logic nối đường — `Node`

- **`ConnectedEdges`** (Node→GameObject cạnh): các node có thể nối tới + sprite cạnh tương ứng.
- **`ConnectedNodes`**: các node đang thực sự nối.
- `UpdateInput` xử lý: nối mới, gỡ khi nối lại, cắt bớt khi node đã đủ 2 cạnh, chặn tạo ô vuông (box) / bậc 3 (`IsDegreeThree`), lan màu theo đường (`AddEdge` gán `colorId`).
- **`IsWin`**: điểm cuối (`_point` bật) cần đúng 1 cạnh; ô thường cần đúng 2 cạnh.
- **`SolveHighLight`**: đường đã nối 2 đầu cùng màu → bật `_highLight`.

### Thắng — `CheckWin`

Gọi `SolveHighLight` mọi node; nếu **tất cả** node `IsWin` → `GameManager.UnlockLevel()`, hiện `_winText`, khóa input.

---

## Hằng số / quy ước

- Cỡ lưới = `CurrentStage + 4`. 50 level mỗi stage; tối đa 7 stage (stage 8 → reset về menu).
- Tên GameObject của `LevelButton` phải kết thúc bằng `_<số level>` (vd `Level_5`).
- Scene: `"MainMenu"`, `"GamePlay"` — phải khớp tên file & Build Settings (chú ý chữ **P** hoa trong `GamePlay`).
- `NodeColors` (list trong `GamePlayManager` Inspector) phải để **alpha = 255**, không thì điểm màu tàng hình.
- Level panel trong scene MainMenu nên để **inactive** khi lưu, tránh `LevelButton.OnEnable` chạy trước `MainMenuManager.Awake` (null ref).

---

## Ghi chú kỹ thuật

- Namespace hiện tại: `Connect.Core` (script logic), `Connect.common` (LevelData/LevelList — chữ thường). Bản tham chiếu gốc dùng `Connect.Common` (chữ hoa) — cần đồng bộ nếu tích hợp thêm `LevelGenerator`.
- `MainMenuManager` dùng singleton tên `instance` (thường); các manager khác dùng `Instance` (hoa).
