# Sticker Pool (Unity)

Bản chuyển đổi từ prototype HTML (`Assets/sticker-pool-handoff`) sang Unity 6 (URP 2D), mobile dọc 1080×1920.
Scene chơi được: `Assets/StickerPool/Scenes/Game.unity`.

## Kiến trúc MVC

| Tầng | Thư mục | Vai trò |
|---|---|---|
| Model | `Scripts/Model` | C# thuần (không phụ thuộc Unity): RNG `Mulberry32`, `LevelGenerator`, `OfferGenerator`, `GreedySolver`, `BudgetCalculator`, `GameSession` |
| Data | `Scripts/Data` + `Data/` | ScriptableObject: `StickerData`, `TagData`, `TagCategoryData`, `LevelData`, `GameDatabase`, `GameConfig` |
| View | `Scripts/View` | MonoBehaviour chỉ hiển thị và bắn sự kiện: `StickerView`, `JarView`, `TapeView`, `HudView`, `RollCallView`, `ResultPanelView`, `SfxPlayer`... |
| Controller | `Scripts/Controller` | `GameController` (nối Model với View), `ChooseSequence` (animation chọn từ bằng PrimeTween), `SaveService`, `StickerObjectPool` |

Model là bản port bit-exact của logic prototype. Test đối chiếu với chính JavaScript của prototype nằm ở
`Tests/EditMode` (`ParityGolden.json` sinh bằng Node từ `prototype/index.html`). Chạy: Window ▸ General ▸ Test Runner ▸ EditMode.

## Chỉnh tay (không cần code)

- **Cảm giác animation, thời lượng, dim, âm lượng/pitch:** `Data/GameConfig.asset` (có sẵn AnimationCurve cho easing).
- **Vật lý:** gravity ở Project Settings ▸ Physics 2D (đang -26); `GameController` đặt fixed timestep 1/60 và 60 FPS khi chạy. Ma sát/độ nảy: `Data/Physics/*.physicsMaterial2D`.
  Collider và vị trí hình của từng sticker: mở prefab `Prefabs/Stickers/Sticker_<id>.prefab` (đổi `CircleCollider2D` sang Capsule cho vật dài).
- **Level:** `Data/Levels/Level_XX.asset` (số sticker, số màu, từ được mở, từ vui nhộn, lượt dư, seed, hint).
  Thêm level = tạo asset (Create ▸ Sticker Pool ▸ Level) rồi thêm vào `GameDatabase.levels`. Thêm một ô dot vào HUD nếu vượt 10 level.
- **Sticker/tag mới:** Create ▸ Sticker Pool ▸ Sticker / Tag, thêm vào `GameDatabase`. Không đổi thứ tự sticker đã có
  (sinh level duyệt theo thứ tự này).
- **Màu băng keo:** gắn theo từ, không theo vị trí. Mỗi `Data/Tags/Tag_*.asset` có `Tape Color`: từ màu dùng đúng màu của nó
  (đỏ, cam, vàng, xanh lá, xanh dương, nâu), các từ khác dùng một màu pastel theo phân vùng (nhóm hồng, nơi chốn mint,
  vui nhộn tím nhạt, chất liệu xám xanh). Chữ trên băng tự đổi trắng/đen theo độ sáng của nền (`TapeView`).
- **Hiệu ứng băng keo xuất hiện:** `TapeView` (prefab `TapeButton`, mục Entrance: thời lượng, độ trượt, độ nghiêng, độ dẹt khi dính)
  và `GameConfig` (mục Tape entrance: độ trễ lúc vào màn, nhịp giữa 3 băng). Chạy mỗi khi có bộ từ mới: vào màn, sau mỗi lượt, khi đổi từ.
- **Băng keo giữ nguyên giữa các lượt:** `GameSession.Slots` là từ trên 3 băng. Sau mỗi lượt chỉ băng vừa chọn (hoặc băng hết sticker để quét)
  bị bóc ra và thay từ mới (`OfferGenerator.Refill`); hai băng còn lại đứng yên. Từ mới luôn là từ quét nhiều nhất trong bộ gợi ý
  tươi, trừ khi băng giữ lại đã quét được bằng đó, nên luôn còn một nước đi lớn. "Đổi từ" vẫn thay cả 3 băng.
- **Âm thanh băng keo dán:** `Audio/Common/tape_stick.wav`, gán ở `GameConfig.tapeStickClip`; mỗi băng trong một đợt cao hơn băng trước
  (`tapeStickSemitones`), `tapeSoundOffset` căn tiếng "tack" vào lúc băng chạm xuống. `tapeDisappearDuration`: thời gian băng bị bóc ra.
- **Mở khóa level:** `GameConfig.unlockAllLevels` (đang bật cho playtest).
- **Bố cục UI:** mọi thứ nằm sẵn trong scene `Game` và prefab `Prefabs/UI`: `Canvas_Hud` (top bar, thanh tiến độ, bong bóng gợi ý,
  băng keo, nút Đổi từ, bàn tay hướng dẫn, banner vào màn), `Canvas_RollCall`, `Canvas_Menu` (Main Menu + Level Map),
  `Canvas_Overlay` (Settings, Confirm, thẻ thắng/thua), `Canvas_Transition` (màn giấy chuyển cảnh).
- **Theme "bàn thủ công ấm áp":** `Data/UiTheme.asset` giữ bảng màu, vùng chạm tối thiểu (128 px ≈ 46 pt), cỡ chữ tối thiểu (32 px),
  độ nảy nút, thời lượng popup/chuyển màn. Các thành phần gắn `UiThemeTint` đổi màu ngay trong Editor khi sửa asset này
  (camera, tấm che miệng hũ, nền menu/map, giấy, mực chữ...). Sprite panel/icon ở `Art/UI/Theme`, âm UI ở `Audio/Common`
  (`ui_click`, `popup_open`, `popup_close`, `star`, `whoosh`, gán trong `GameConfig` mục Interface sounds).
- **Nút và công tắc:** prefab `UiPillButton`, `UiIconButton`, `UiToggle`. Thành phần `UiButton` lo hiệu ứng nhấn, tiếng click,
  haptic nhẹ và nới vùng chạm lên tối thiểu của theme.
- **Luồng màn hình:** `ScreenFlow` (Menu / Map / Game) chạy tấm giấy chuyển cảnh `TransitionView`; `GameController` nối các view.
  Settings có Sound, Haptics, Reduce motion, Language; Restart và Về menu luôn hỏi xác nhận (`ConfirmPopupView`).
- **Reduce motion:** bật trong Settings (mặc định theo cài đặt iOS). `UiMotion.Factor` rút các animation còn 35%, gồm cả chuỗi điểm danh.
- **Haptics (iOS):** `Plugins/iOS/StickerPoolNative.mm` (chỉ build iOS) + `Scripts/View/Haptics.cs`. Editor và Android không rung.
  Cần build trên Mac/Xcode để thử trên máy thật.
- **Đổi từ (Swap words) đang tắt tạm:** `GameConfig.swapWordsEnabled` = false ẩn nút, chặn tính năng và hạ băng keo xuống chỗ trống.
  Bật lại là nút và lượt đổi từ hoạt động như cũ.
- **Khóa level:** `GameConfig.unlockAllLevels` (đang bật). Tắt đi thì Level Map khóa các màn chưa mở và hiện ổ khóa.
- **Level Map:** sẵn 20 nút; chỉnh nhịp lượn, khoảng cách ở `LevelMapView`. Khi game có ít hơn 20 màn sẽ hiện biển "Sắp có thêm màn mới".
- **Ngôn ngữ:** Window ▸ Asset Management ▸ Localization Tables (bảng `UI`, `Tags`, `Stickers`, `Hints`; English mặc định + Vietnamese).
  Nút ngôn ngữ trong Settings đổi ngôn ngữ và lưu lại.

## Lưu ý kỹ thuật

- **Không dùng SpriteMask + `SpriteRenderer.maskInteraction`** cho sticker: trong URP 2D (Unity 6000.3) nhiều sprite bị mask
  dùng chung một texture (mọi sticker hiện cùng một hình). Phần sticker rơi từ trên xuống được che bằng tấm `MouthCover`
  (màu nền) trong prefab `Jar`.
- Sticker được pool theo id (`StickerObjectPool`): chơi lại level không tạo object mới.
- Thứ tự sorting: mat 0 · sticker trong hũ 9/10 · dim 20 · cover 30 · Canvas_Hud 100 · sticker đang bay 149/150 ·
  Canvas_RollCall 200 · confetti 260 · Canvas_Overlay 300.
- Camera `CameraFitWidth` giữ nguyên bề rộng 5,625 đơn vị (hũ 5 đơn vị = 960 px của canvas 1080×1920); Canvas ở chế độ Expand.
- Font: Baloo 2 + Be Vietnam Pro (SIL OFL, xem `Art/Fonts/OFL-*.txt`), TMP font asset dạng Dynamic đã nạp sẵn ký tự tiếng Việt.
- PlayerPrefs: `stickerpool.sound`, `stickerpool.haptics`, `stickerpool.reducedmotion`, `stickerpool.tutorial`,
  `stickerpool.language`, `stickerpool.cleared` (cũ, tính 1 sao), `stickerpool.stars` (`chỉ số:sao`).
- Play mode trong Editor chỉ chạy khi cửa sổ có focus (Project Settings ▸ Player ▸ Run In Background đang tắt).
