# Sticker Pool: Kế hoạch cải tiến UI/UX (iOS dọc, casual mobile)

Ngày lập: 2026-10-13 · Phạm vi đã chốt với chủ dự án: **mức lớn** (thêm Main Menu, bản đồ level, Pause/Settings, transition), hướng nhìn **bàn thủ công ấm áp**, **iPhone mọi kích cỡ** (SE 16:9 đến Pro Max, có notch/Dynamic Island, chỉ chơi dọc).

> Cơ sở đánh giá: đo trực tiếp scene `Game` (kích thước RectTransform, cỡ chữ, canvas scaler) và đọc code View. Chưa chụp được ảnh play-mode có sticker; bước P0 sẽ chụp bộ ảnh baseline ở nhiều tỉ lệ để đối chiếu trước/sau.

---

## 1. Kết luận: UI/UX hiện tại đã hợp thị trường chưa?

**Chưa.** Phần *gameplay feel* (sticker bay, đánh số, băng keo dán, âm thanh theo từ) đã ở mức tốt. Phần *UI bao quanh* vẫn là bố cục của prototype web, chưa được thiết kế cho ngón cái và chưa có lớp "sản phẩm".

### Điểm mạnh nên giữ
- Băng keo washi là điểm nhận diện duy nhất của game, có animation vào/dính/bóc và press feedback.
- Chuỗi chọn từ → hàng điểm danh → bay vào băng keo đã đủ nhịp và đủ "đã tay".
- Safe area, đa ngôn ngữ (EN/VI), Canvas scale theo bề rộng đã đúng nền tảng.

### Vấn đề, xếp theo mức nghiêm trọng
Quy đổi: canvas 1080 px ≈ 390 pt trên iPhone, tức **1 pt ≈ 2,77 px**. Apple HIG yêu cầu vùng chạm ≥ 44 pt ≈ **122 px**.

| # | Vấn đề | Số đo hiện tại | Hệ quả |
|---|---|---|---|
| 1 | Nút công cụ (Sound, Swap, Restart, EN) và 10 chấm level quá nhỏ | cao 76 px ≈ **27 pt** | Bấm trượt, vi phạm HIG |
| 2 | Chữ quá nhỏ | LEVEL 26 px ≈ 9,4 pt · MOVES LEFT 28 px ≈ 10 pt · nhãn category băng keo 26 px ≈ 9,4 pt · gợi ý/Info 38 px ≈ 13,7 pt | Khó đọc với nhóm 40-50+ tuổi |
| 3 | Toàn bộ điều khiển phụ nằm sát đỉnh | ToolsRow ở y = -352 px từ đỉnh | Ngoài vùng ngón cái, phải đổi tay khi cầm một tay |
| 4 | Restart nằm cạnh Swap words, không xác nhận | Hai nút liền kề, cùng cỡ, cùng kiểu | Dễ bấm nhầm, mất tiến trình |
| 5 | HUD là dải 5 hàng chữ + pill xếp chồng | Title, moves, level, stickers-left, tools | Dày đặc, không có thứ bậc, trái tinh thần "thư giãn" |
| 6 | Chỉ băng keo có phản hồi khi chạm | `TapeView` có press scale; nút HUD, nút result, level dot không có | Cảm giác "chết" |
| 7 | Không có haptic, không có tiếng UI click | `grep Haptic` rỗng | Thiếu một nửa cảm giác "đã tay" trên iOS |
| 8 | Chọn level bằng 10 chấm trong HUD | `LevelRow` | Là công cụ dev, không phải UX cho người chơi |
| 9 | Chưa có Main Menu, Pause/Settings, bản đồ level, transition màn | Scene mở thẳng vào màn chơi | Thiếu lớp sản phẩm, khó giữ chân |
| 10 | Hướng dẫn chỉ là một dòng chữ xám nhỏ | `Info` 38 px | Người mới không biết "chạm vào băng keo" |
| 11 | Nền navy tối + mat xanh lá | Camera bg `#1C252C` | Lạnh, lệch so với đối tượng nữ 25-50 muốn thư giãn |
| 12 | Màn thắng/thua: nút 96 px ≈ 35 pt, thẻ phẳng | `Next/Again/Retry` 330×96 | Dưới chuẩn HIG, chưa có khoảnh khắc ăn mừng |

---

## 2. Định hướng thiết kế

### 2.1 Concept: "Bàn thủ công ấm áp"
Người chơi ngồi ở bàn làm scrapbook. Giữ cutting mat + washi tape + sticker, nhưng đổi bối cảnh sang ấm, sáng, mềm.

- **Nền:** giấy kem / gỗ sáng có hạt nhẹ, vignette ấm. Thay camera background navy.
- **Panel:** giấy ghi chú bo tròn, viền trắng như sticker, bóng đổ mềm, vài mảnh washi trang trí ở góc.
- **Hũ:** giữ cutting mat nhưng đổi sang xanh sage dịu, lưới mờ hơn để sticker nổi bật.
- **Nguyên tắc:** ít chữ, nhiều hình; mỗi màn chỉ có một hành động chính (chọn băng keo).

### 2.2 Design tokens (một `UiTheme` ScriptableObject, chỉnh tay được)
| Token | Giá trị đề xuất |
|---|---|
| Nền chính | kem `#F6EBDD` · nền phụ gỗ `#E8D3B5` |
| Mực chữ | nâu đậm `#3B2F2A` (tương phản ≥ 7:1 trên kem) |
| Nhấn chính (CTA) | vàng mật `#F2B544`, chữ nâu đậm |
| Nhấn phụ | san hô `#EE7B6C` (cảnh báo, hết lượt) · sage `#7FB59A` (thành công) |
| Panel | trắng ngà `#FFFDF8`, viền sticker trắng 6 px, bóng `#3B2F2A` 18% |
| Bán kính góc | 28 px (nút), 44 px (panel) |
| Cỡ chữ (px @1080) | Display 120 · H1 84 · H2 56 · Body 42 (≈15 pt) · Caption tối thiểu 32 (≈11,5 pt) |
| Vùng chạm tối thiểu | **128 px (≈46 pt)**, khoảng cách giữa hai vùng chạm ≥ 16 px |
| Font | giữ Baloo 2 (tiêu đề, số) + Be Vietnam Pro (nội dung); đã nạp ký tự tiếng Việt |

Tất cả giá trị nằm trong một asset để chỉnh bằng tay, đúng quy tắc dự án: không tạo UI lúc chạy, không hardcode.

---

## 3. Bố cục màn chơi mới (canvas 1080 × 2336 trên iPhone 15, mở rộng theo tỉ lệ)

```
┌──────────────────────────────┐ ← safe area top
│ [⚙]      LEVEL 5  ★★☆     [ 5 ]│  Top bar 140 px: Settings · tên level + sao · Moves
│                    ──────────  │  Thanh tiến độ sticker còn lại (dưới top bar)
│        ┌──────────────┐        │
│        │              │        │
│        │     HŨ       │        │  Hũ rộng 984 px, canh giữa dọc theo không gian còn lại
│        │  (cutting    │        │
│        │    mat)      │        │
│        └──────────────┘        │
│    "Chọn một màu trên băng"    │  Bong bóng gợi ý, cỡ chữ Body 42, có mũi tên chỉ xuống
│                                │
│  [Băng 1] [Băng 2] [Băng 3]    │  Cao ≥ 170 px, ngay vùng ngón cái
│         [ ⟳ Đổi từ (1) ]       │  Nút phụ 128 px, nằm giữa dưới băng keo
└──────────────────────────────┘ ← safe area bottom (home indicator)
```

Thay đổi chính:
1. **Top bar gọn:** một hàng duy nhất. Bỏ tiêu đề "Sticker Pool" khỏi màn chơi (đã có ở Main Menu), bỏ 10 chấm level, bỏ Sound/EN/Restart khỏi HUD.
2. **Settings (⚙) mở popup:** Sound, Music (nếu có), Ngôn ngữ, Rung (haptics), Chơi lại màn (có xác nhận), Về menu.
3. **Số lượt còn:** giữ số lớn nhưng đặt trong một pill, dùng thêm các "chấm lượt" (pips) khi ≤ 8 lượt. Còn ≤ 1 lượt: pill chuyển san hô và nhịp đập nhẹ.
4. **Sticker còn lại:** thành thanh tiến độ mỏng dưới top bar kèm số (thay dòng chữ "8 stickers left in the jar").
5. **Thumb zone:** ba băng keo và nút "Đổi từ" ở nửa dưới. Đổi từ có icon, badge số lần còn lại, trạng thái xám khi hết.
6. **Gợi ý:** thành bong bóng ghi chú ngay trên băng keo. Level 1 có thêm **bàn tay chỉ** nhấp nháy lên băng keo đầu tiên cho tới khi người chơi chạm lần đầu.
7. **Co giãn:** vùng hũ lấy không gian còn lại giữa top bar và cụm băng keo. Với iPhone SE (tỉ lệ 16:9) thu nhỏ khoảng cách, không thu nhỏ vùng chạm.

---

## 4. Các màn hình mới

| Màn | Nội dung | Ghi chú |
|---|---|---|
| **Main Menu** | Logo "Sticker Pool", nút **Chơi** lớn (ghi "Tiếp tục: Level N"), nút Level map, nút Settings. Vài sticker trôi nhẹ làm nền | Một `UiScreen` trong scene, không tạo lúc chạy |
| **Level Map** | Đường dán băng keo uốn lượn, mỗi level là một sticker tròn có số và sao. Level đã xong có sao, level hiện tại nảy nhẹ, cuộn dọc | Thay 10 chấm. Cờ `unlockAllLevels` giữ cho debug; bản phát hành khóa theo tiến độ |
| **Pause/Settings** | Popup giấy: Sound, Haptics, Language, Restart (xác nhận), Home | Một popup dùng chung cho Menu và màn chơi |
| **Level Intro** | Banner "Level 5" trượt vào, hũ thả sticker như hiện tại, gợi ý hiện sau đó | Ngắn ≤ 0,8 s, bỏ qua được bằng chạm |
| **Win** | Thẻ giấy có washi trên đỉnh, 3 sao dạng sticker dán từng cái (kèm tiếng + haptic), confetti, dòng "Dùng 5/7 lượt". Nút **Level tiếp** to, nằm dưới thẻ trong vùng ngón cái | Nút phụ "Chơi lại" nhỏ hơn, dạng text button |
| **Lose** | Giọng mềm ("Gần được rồi!"), hiện số sticker còn lại, nút **Thử lại** to. Không dùng màu đỏ gắt | Phù hợp tinh thần thư giãn |

---

## 5. Motion và juice (thời lượng nằm trong `GameConfig`/`UiTheme`)

| Tình huống | Hiệu ứng | Thời lượng |
|---|---|---|
| Nút bấm (mọi nút) | Nhấn: scale 0,94 + tối nhẹ; thả: `OutBack` về 1; có tiếng click + haptic nhẹ | 80 ms / 180 ms |
| Mở popup | Nền mờ 0→55%, card scale 0,85→1 `OutBack` | 300 ms |
| Đóng popup | Scale 1→0,92 + fade | 180 ms |
| Chuyển màn (Menu ↔ Map ↔ Game) | Cuộn giấy / bóc sticker che màn, rồi mở ra | 450-600 ms |
| Level Intro | Banner trượt từ trên, hũ thả sticker | ≤ 800 ms |
| Moves giảm | Số lật xuống, pill nảy; còn 1 lượt: đập chậm màu san hô | 300 ms |
| Sao thắng | Dán từng sao: scale 2→1 + xoay nhẹ, trễ 140 ms mỗi sao | 350 ms mỗi sao |
| Băng keo | Giữ nguyên hiệu ứng hiện có | không đổi |
| Reduced motion | Đọc cài đặt iOS (qua plugin) hoặc toggle trong Settings, rút còn 35% | theo GDD |

**Haptics (iOS):** chạm băng keo = impact nhẹ; mỗi sticker bay vào băng = tick rất nhẹ (giới hạn tần suất); thắng = notification success; hết lượt = soft. Cần plugin native nhỏ (`UIImpactFeedbackGenerator`, ~30 dòng Objective-C trong `Plugins/iOS`), có tùy chọn tắt trong Settings. `Handheld.Vibrate` trên iOS chỉ có một kiểu rung thô nên không dùng.

**Âm thanh UI:** click, mở/đóng popup, sao dán, chuyển màn. Giữ phong cách hiện tại (âm theo từ + pop).

---

## 6. Nguồn art (đã chọn phương án lai, vì bạn để tôi quyết)

| Loại | Cách làm | Lý do |
|---|---|---|
| Panel, nút, pill, thanh tiến độ | Sinh sprite **9-slice** bằng script editor tạm (xóa sau khi dùng, theo quy ước dự án) | Đồng nhất tuyệt đối với tokens, nhẹ, không phụ thuộc bên ngoài |
| Icon (gear, loa, shuffle, restart, home, khóa, sao) | Bộ icon mã nguồn mở rasterize ra PNG trắng 256 px, tô màu trong Unity (kiểm tra giấy phép khi làm; ưu tiên MIT/Apache) | Đồng bộ nét, tránh sai lệch phong cách của icon sinh AI |
| Nền giấy/gỗ, backdrop Menu và Map, washi trang trí | Sinh bằng Unity AI (`AssetGeneration`), bạn duyệt từng ảnh trước khi dùng | Đây là phần cần "mắt nghệ thuật" nhất |
| Sticker gameplay | Giữ nguyên 49 sticker hiện có | Không thuộc phạm vi |

Sprite đóng gói vào SpriteAtlas riêng cho UI ở giai đoạn cuối.

---

## 7. Kiến trúc kỹ thuật

Tuân thủ quyết định đã khóa: MVC, View không chứa logic, không tạo UI lúc chạy, mọi thứ là prefab/đối tượng đặt sẵn trong scene, timing nằm trong asset.

- `UiTheme` (ScriptableObject): màu, cỡ chữ, bán kính, thời lượng.
- `UiButton` (View): press scale, âm click, haptic, vùng chạm mở rộng (kiểm tra `rect` ≥ 128 px ở Editor, cảnh báo khi nhỏ hơn).
- `UiScreen` + `ScreenRouter`: các màn là `CanvasGroup` đặt sẵn; router điều khiển chuyển màn và chồng popup, có back (nút quay lại của hệ thống không bắt buộc trên iOS).
- `HapticService`: bọc plugin native, bản Editor là no-op.
- `LevelMapView`, `MainMenuView`, `SettingsPopupView`, `ConfirmPopupView`, `LevelIntroView`: View mỏng, sự kiện bắn ra cho `GameController`.
- `SaveService`: thêm cờ haptics, music, reduced motion, level hiện tại, sao theo level.
- `LocalizedLabel`: thêm khóa mới vào bảng `UI` (EN + VI).
- Tách `Canvas_Hud` thành Top bar và Bottom bar neo riêng theo cạnh trên/dưới của safe area, thay vì một khối `Hud` neo tuyệt đối.
- Tests EditMode giữ nguyên (không đụng Model).

---

## 8. Lộ trình (mỗi phase đều chạy được và quay lại được)

| Phase | Nội dung | Kết quả kiểm chứng | Cỡ |
|---|---|---|---|
| **P0 Nền tảng** | `UiTheme`, `UiButton`, `HapticService`, router khung, chụp ảnh baseline ở 5 tỉ lệ | Có ảnh trước/sau; script kiểm tra vùng chạm | M |
| **P1 Theme và art** | Palette ấm, nền giấy, 9-slice, icon, đổi camera bg, hũ sage | Ảnh màn chơi ở theme mới | M |
| **P2 HUD màn chơi** | Top bar, thanh tiến độ, thumb-zone, nút Đổi từ, bong bóng gợi ý, bàn tay chỉ level 1 | Mọi vùng chạm ≥ 128 px, chữ ≥ 32 px | L |
| **P3 Popup** | Settings, Confirm Restart, Win, Lose mới | Restart cần xác nhận; sao dán từng cái | M |
| **P4 Menu và Map** | Main Menu, Level Map, Level Intro, transition | Luồng Menu → Map → Game → Result → Map | L |
| **P5 Juice** | Haptics native, tiếng UI, reduced motion, tinh chỉnh timing | Test trên máy thật | M |
| **P6 QA** | Ma trận thiết bị, hiệu năng, localization VI dài chữ | Checklist bên dưới đạt | S |

### Ma trận kiểm thử
iPhone SE 3 (16:9, không notch) · iPhone 13 mini (5,4″) · iPhone 15 (Dynamic Island) · iPhone 15 Pro Max · tỉ lệ 19,5:9 dài nhất. Kiểm tra cả EN và VI, chữ dài nhất ("Quần áo", "Thủy tinh", "Bầu trời") không tràn băng keo.

### Tiêu chí nghiệm thu
- Mọi phần tử bấm được ≥ 128 px, cách nhau ≥ 16 px; chữ ≥ 32 px.
- Không có điều khiển nào ngoài vùng ngón cái trừ ⚙.
- Mọi nút đều có phản hồi nhìn, nghe, rung.
- Restart và Về menu đều có xác nhận.
- Tương phản chữ/nền ≥ 4,5:1 (chữ thường), ≥ 3:1 (chữ lớn).
- Không giảm 60 FPS trên máy mục tiêu; không thêm GC alloc mỗi frame ở HUD.

---

## 9. Rủi ro và giả định
- **Chưa test được trên thiết bị iOS và chưa có haptic native thật:** cần máy Mac và Xcode để build; Editor chỉ mô phỏng. Giả định bạn có đường build iOS ở giai đoạn P5.
- **Art sinh AI có thể lệch phong cách:** chỉ dùng cho nền và backdrop, có bước duyệt của bạn.
- **Bản đồ level mới 10 level:** map nên thiết kế sẵn cho 20 level (GDD) để không phải làm lại.
- **Ảnh baseline play-mode:** Editor mất focus thì không tick (đã ghi trong memory), nên baseline dùng cách chụp đồng bộ đã biết.
- Không đổi Model, vật lý, sinh level, tuân thủ các phần đã khóa.
