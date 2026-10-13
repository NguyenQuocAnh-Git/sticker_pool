# Sticker Pool — Tài liệu thiết kế (GDD)

Phiên bản prototype: 13/10/2026 · 10 level chơi được · 49 sticker

## 1. Tổng quan

**Ý tưởng:** Trong hũ có một đống sticker. Mỗi lượt, người chơi chọn 1 trong 3 từ. Mọi sticker mang tính chất đó (cùng màu, cùng nhóm, cùng nơi chốn…) sẽ bay khỏi hũ. Clear sạch hũ trong số lượt cho phép là thắng.

**Thể loại:** Casual Puzzle, lai giữa hai nhánh:
- Dọn đống vật thể có giới hạn lượt (gần Tile Explorer, Zen Match về cảm giác chơi).
- Phân loại / liên tưởng (gần NYT Connections, Word Associations Solitaire).

Kỹ năng cốt lõi là phân loại và liên tưởng, không phải đánh vần. Khi lên store nên đặt ở mục Puzzle, gắn thêm tag Word.

**Điểm khác biệt:** các game cùng dòng bắt người chơi tự gom từng món vào nhóm. Game này làm ngược lại: người chơi chọn *danh mục*, và cả loạt sticker tự bay đi. Mỗi lượt vì thế là một cú "quét" đã tay.

**Người chơi mục tiêu:** người chơi casual puzzle trưởng thành, phần lớn là nữ 25–50+. Họ chơi phiên ngắn, muốn thư giãn nhưng vẫn "dùng não nhẹ".

**Định hướng cảm xúc:** thư giãn, không áp lực. Thỏa mãn khi một lượt clear được nhiều sticker. Thỉnh thoảng bật cười vì một lựa chọn hài hước.

## 2. Luật chơi và vòng lặp chính

1. Vào màn: sticker rơi từ trên xuống hũ, va chạm và xếp thành đống.
2. Dưới hũ có 3 "băng keo washi", mỗi băng là một từ, kèm nhãn phân vùng (MÀU, NHÓM, NƠI CHỐN, VUI NHỘN).
3. Người chơi chọn một từ:
   - Tất cả sticker khớp bay lên, xếp hàng giữa hũ (tối đa 6 cái một hàng), và được **đánh số ngay khi từng cái vừa tới hàng**. Tiêu đề phía trên đếm theo, ví dụ "Biết bay · 7".
   - Sau một nhịp ngắn, cả hàng bay vào băng keo vừa chọn. Mỗi sticker chạm băng keo phát một tiếng "pop".
   - Phần sticker còn lại trong hũ rơi xuống lấp chỗ trống.
4. Lặp lại tới khi hũ sạch (thắng) hoặc hết lượt (thua, chơi lại).
5. Chạm vào một sticker trong hũ: hiện tên sticker và hất nó lên nhẹ. Đây chỉ là tương tác cho vui, không tốn lượt.
6. **Đổi từ:** mỗi màn được đổi bộ 3 từ 1 lần.
7. **Sao:** dư ≥2 lượt = 3 sao, dư 1 lượt = 2 sao, vừa đủ = 1 sao.

## 3. Màn hình và UI

Bố cục dọc, một cột, tối đa 500px:
- **HUD:** tên game, các chấm level (bản playtest mở sẵn cả 10 level), số "lượt còn" cỡ lớn (đổi màu đỏ khi còn ≤1).
- **Hàng công cụ:** số sticker còn trong hũ · Tiếng: Bật/Tắt · Đổi từ (n) · Chơi lại.
- **Hũ:** hình vuông, nền thảm cắt giấy xanh có lưới và thước đo.
- **Dòng gợi ý** dưới hũ (hint của từng level, tên sticker khi chạm).
- **3 băng keo washi** màu hồng / xanh bạc hà / vàng, xoay nghiêng nhẹ, mép răng cưa.

Thứ tự 3 băng keo được xáo ngẫu nhiên mỗi lượt.

## 4. Sticker

### 4.1 Phong cách art

- Hình phẳng, **một màu chính** mỗi sticker, chi tiết phụ (mắt, bánh xe, cuống) chỉ dùng nét đen `#2b2b2b`.
- Viền trắng dày như sticker thật, cộng bóng đổ nhẹ.
- Kích thước gốc 64×64 (SVG viewBox). Trong game, hình chiếm 84% ô sticker.
- **6 màu**, chọn để khác hẳn nhau:

| Màu | Hex |
|---|---|
| Đỏ | `#e5383b` |
| Cam | `#f48c06` |
| Vàng | `#ffc300` |
| Xanh lá | `#38b000` |
| Xanh dương | `#1e88e5` |
| Nâu | `#8d5a3b` |

Đã loại: tím (dễ lẫn với xanh dương), hồng, trắng, đen, xám.

### 4.2 Texture chất liệu

Chất liệu phải nhìn ra được, nên mỗi chất liệu có một lớp phủ đơn giản lên màu chính:

| Chất liệu | Cách thể hiện |
|---|---|
| Gỗ | Vân gỗ gợn sóng màu tối |
| Kim loại | Vệt bóng chéo như phản sáng |
| Vải | Đường chỉ khâu nét đứt bên trong + vân dệt mờ |
| Thủy tinh | Trong suốt (độ đục 42%) + viền màu + 2 vệt sáng |
| Cao su | Điểm bóng tròn + tối dần ở mép |

Sticker không có chất liệu thì để phẳng.

### 4.3 Danh sách 49 sticker

| Tên | Màu | Nhóm | Nơi chốn | Vui nhộn | Chất liệu | File |
|---|---|---|---|---|---|---|
| Táo | Đỏ | Đồ ăn | – | – | – | `tao` |
| Dâu tây | Đỏ | Đồ ăn | – | – | – | `dau-tay` |
| Ô tô | Đỏ | Xe cộ | – | Kêu to | Kim loại | `o-to` |
| Bóng bay | Đỏ | Đồ chơi | Bầu trời | Biết bay | Cao su | `bong-bay` |
| Bọ rùa | Đỏ | Con vật | Khu vườn | Biết bay | – | `bo-rua` |
| Cua | Đỏ | Con vật | Biển | – | – | `cua` |
| Trống | Đỏ | Đồ chơi | – | Kêu to | Gỗ | `trong` |
| Khăn len | Đỏ | Quần áo | – | – | Vải | `khan-len` |
| Quả cam | Cam | Đồ ăn | – | – | – | `qua-cam` |
| Cà rốt | Cam | Đồ ăn | Khu vườn | – | – | `ca-rot` |
| Bóng rổ | Cam | Đồ chơi | – | – | Cao su | `bong-ro` |
| Cáo | Cam | Con vật | – | – | – | `cao` |
| Cá hề | Cam | Con vật | Biển | – | – | `ca-he` |
| Chiếc dép | Cam | Quần áo | Biển | Biết bay | Cao su | `chiec-dep` |
| Cái còi | Cam | Đồ chơi | – | Kêu to | – | `cai-coi` |
| Tất | Cam | Quần áo | – | – | Vải | `tat` |
| Chuối | Vàng | Đồ ăn | – | – | – | `chuoi` |
| Ngôi sao | Vàng | – | Bầu trời | – | – | `ngoi-sao` |
| Mặt trời | Vàng | – | Bầu trời | – | – | `mat-troi` |
| Mặt trăng | Vàng | – | Bầu trời | – | – | `mat-trang` |
| Chìa khóa | Vàng | Đồ dùng | – | – | Kim loại | `chia-khoa` |
| Vịt con | Vàng | Con vật | Khu vườn | – | – | `vit-con` |
| Kèn | Vàng | Đồ chơi | – | Kêu to | Kim loại | `ken` |
| Bóng đèn | Vàng | Đồ dùng | – | – | Thủy tinh | `bong-den` |
| Con ong | Vàng | Con vật | Khu vườn | Biết bay | – | `con-ong` |
| Ếch | Xanh lá | Con vật | Khu vườn | – | – | `ech` |
| Dưa leo | Xanh lá | Đồ ăn | Nhà bếp | – | – | `dua-leo` |
| Quả lê | Xanh lá | Đồ ăn | – | – | – | `qua-le` |
| Ủng | Xanh lá | Quần áo | Khu vườn | – | Cao su | `ung` |
| Con rùa | Xanh lá | Con vật | Biển | – | – | `con-rua` |
| Xe buýt | Xanh lá | Xe cộ | – | Kêu to | Kim loại | `xe-buyt` |
| Con vẹt | Xanh lá | Con vật | Bầu trời | Biết bay, Kêu to | – | `con-vet` |
| Cá | Xanh dương | Con vật | Biển | – | – | `ca` |
| Áo thun | Xanh dương | Quần áo | – | – | Vải | `ao-thun` |
| Thuyền | Xanh dương | Xe cộ | Biển | – | – | `thuyen` |
| Chim | Xanh dương | Con vật | Bầu trời | Biết bay | – | `chim` |
| Cốc thủy tinh | Xanh dương | Đồ dùng | Nhà bếp | – | Thủy tinh | `coc-thuy-tinh` |
| Bướm | Xanh dương | Con vật | Khu vườn | Biết bay | – | `buom` |
| Máy bay | Xanh dương | Xe cộ | Bầu trời | Biết bay, Kêu to | Kim loại | `may-bay` |
| Ấm trà | Xanh dương | Đồ dùng | Nhà bếp | – | – | `am-tra` |
| Gấu bông | Nâu | Đồ chơi | – | – | Vải | `gau-bong` |
| Ghế gỗ | Nâu | Đồ dùng | – | – | Gỗ | `ghe-go` |
| Khúc gỗ | Nâu | – | Khu vườn | – | Gỗ | `khuc-go` |
| Đàn guitar | Nâu | Đồ chơi | – | – | Gỗ | `dan-guitar` |
| Bánh quy | Nâu | Đồ ăn | Nhà bếp | – | – | `banh-quy` |
| Bánh mì | Nâu | Đồ ăn | Nhà bếp | – | – | `banh-mi` |
| Đồng hồ báo thức | Nâu | Đồ dùng | – | Kêu to | – | `dong-ho-bao-thuc` |
| Con chó | Nâu | Con vật | – | Kêu to | – | `con-cho` |
| Muôi gỗ | Nâu | Đồ dùng | Nhà bếp | – | Gỗ | `muoi-go` |

Số sticker mang mỗi tag: Con vật 13 · Đồ ăn 9 · Kêu to 9 · Vàng 9 · Nâu 9 · Đỏ 8 · Biết bay 8 · Khu vườn 8 · Cam 8 · Xanh dương 8 · Đồ chơi 7 · Bầu trời 7 · Đồ dùng 7 · Xanh lá 7 · Biển 6 · Nhà bếp 6 · Kim loại 5 · Gỗ 5 · Quần áo 5 · Xe cộ 4 · Cao su 4 · Vải 4 · Thủy tinh 2

Ảnh tổng hợp: `stickers/sticker_sheet.png`.

## 5. Hệ thống tag

### 5.1 Các phân vùng

| Phân vùng | Các từ | Mở ở level |
|---|---|---|
| Màu | Đỏ, Cam, Vàng, Xanh lá, Xanh dương, Nâu | 1 |
| Nhóm (đợt 1) | Đồ ăn, Con vật, Đồ chơi | 4 |
| Nơi chốn | Nhà bếp, Biển, Bầu trời, Khu vườn | 7 |
| Nhóm (đợt 2) | Quần áo, Xe cộ, Đồ dùng | 9 |
| Chất liệu | Gỗ, Vải (lv 11) · Kim loại, Thủy tinh (lv 12) | 11–12 |
| Vui nhộn | Mỗi từ xuất hiện ở một level đặc biệt | 6, 10, 14, 17, 20 |

### 5.2 Quy tắc gắn tag

- Mỗi sticker có **đúng 1 màu**.
- Mỗi sticker có **tối đa 1 nhóm**, 1 nơi chốn, 1 chất liệu. Riêng các vật trên trời (sao, mặt trời, mặt trăng) không thuộc nhóm nào.
- Từ vui nhộn có thể nhiều hơn 1.
- Chỉ gắn tag mà ai nhìn vào cũng đồng ý. Tag nào gây tranh cãi thì bỏ.
- Mục tiêu: mỗi sticker mang khoảng 3–4 tính chất để có nhiều lời giải.

### 5.3 Từ vui nhộn

Cái buồn cười nằm ở chỗ **từ thì bình thường, nhưng nhóm sticker bay ra lại bất ngờ**. Mỗi từ cần ít nhất một sticker "lạc loài":

| Từ | Sticker bình thường | Sticker bất ngờ | Trạng thái |
|---|---|---|---|
| Biết bay | chim, ong, bướm, máy bay, bóng bay | **chiếc dép** (khi mẹ ném) | Đã làm (lv 6) |
| Kêu to | trống, kèn, còi, ô tô, con chó | **đồng hồ báo thức**, **con vẹt** | Đã làm (lv 10) |
| Có mùi | cá, hành, sầu riêng | **chiếc tất** | Kế hoạch (lv 14) |
| Hay bị mất | chìa khóa, điều khiển TV, kính | **chiếc tất** (lại là nó) | Kế hoạch (lv 17) |
| Dính tay | mật ong, kẹo, băng keo, kẹo cao su | | Kế hoạch (lv 20) |
| Có chân | ghế, bàn, con gà, con mèo | **cái quần** | Kế hoạch (lv 20) |

Chiếc tất xuất hiện ở nhiều từ vui nhộn nên có tiềm năng thành mascot.

### 5.4 Đã cân nhắc và loại bỏ

- **Hình dạng (Tròn, Dài):** không rõ ràng khi chọn.
- **Cao su** làm từ chọn được: khó nhận ra. Vẫn giữ làm texture.
- **"Lăn được":** gần như là "Tròn" đổi tên, thay bằng "Có chân".
- **"Trong bếp" kiểu cũ, "Nhạc cụ", "Trái cây/Rau củ" tách riêng:** gộp vào Nhóm và Nơi chốn cho gọn.

## 6. Level design

### 6.1 Nguyên tắc độ khó

Ưu tiên cảm giác thư giãn: mỗi lượt phải quét được nhiều sticker.
- **Trung bình 4–5 sticker mỗi lượt** từ level 4 trở đi. Nếu xuống dưới 3, game bắt đầu cảm giác lặt vặt.
- **Luôn có lượt dư:** +2 cho level 1–10, +1 cho level 11–20. Người chơi casual gần như không thua, chỉ khác nhau số sao.
- **Mỗi level chỉ dùng 3–5 màu**, không dùng đủ 6, để mỗi màu quét được nhiều.
- **Level mở cơ chế mới thì không tăng số sticker.** Sau mỗi lần mở cơ chế là một level nghỉ.
- **Số sticker tăng tối đa 2 mỗi level**, dừng ở 28 (giới hạn đọc được trên màn hình điện thoại).

**Núm điều chỉnh độ khó**, xoay theo thứ tự từ nhẹ tới mạnh:
1. Số lượt dư.
2. Số màu / số nhóm trong level (ít giá trị khác nhau thì mỗi lần quét lớn hơn).
3. Từ "mồi" trong 3 lựa chọn (quét 1–2 sticker, đặt cạnh từ quét 6).
4. Số sticker.

### 6.2 10 level đã làm

| Lv | Sticker | Số màu | Từ vui nhộn | Lời giải (máy) | Lượt cho phép | Gợi ý trong game |
|---|---|---|---|---|---|---|
| 1 | 8 | 3 | – | 3 | 5 | Chọn một màu trên băng keo. Mọi sticker màu đó sẽ bay khỏi hũ. |
| 2 | 10 | 3 | – | 3 | 5 | Mỗi lần chơi lại, hũ và các lựa chọn sẽ được xáo khác đi. |
| 3 | 12 | 4 | – | 4 | 6 | Thêm một màu. Chọn màu có nhiều sticker nhất để quét thật nhiều. |
| 4 | 12 | 4 | – | 2 | 4 | Mở khóa nhóm: đồ ăn, con vật, đồ chơi. |
| 5 | 14 | 4 | – | 3 | 5 | Một sticker vừa có màu vừa có nhóm. Từ nào quét được nhiều nhất? |
| 6 | 14 | 4 | Biết bay | 4 | 6 | Màn vui: thứ gì biết bay? Coi chừng có món bất ngờ. |
| 7 | 16 | 4 | – | 4 | 6 | Mở khóa nơi chốn: nhà bếp, biển, bầu trời, khu vườn. |
| 8 | 16 | 5 | – | 4 | 6 | Màn nghỉ. Thư giãn và quét thật nhiều nhé. |
| 9 | 18 | 5 | – | 5 | 7 | Mở khóa thêm nhóm: quần áo, xe cộ, đồ dùng. |
| 10 | 18 | 5 | Kêu to | 5 | 7 | Màn vui: thứ gì kêu to? |

Màn 4 dễ hơn màn 3 là cố ý: lần đầu thấy "Con vật" quét 8 sticker một lúc là khoảnh khắc người chơi hiểu cơ chế nhóm.

### 6.3 Kế hoạch 20 level

| Lv | Sticker | Mở mới |
|---|---|---|
| 1–3 | 8, 10, 12 | Màu |
| 4–5 | 12, 14 | Nhóm đợt 1 |
| 6 ★ | 14 | Biết bay |
| 7–8 | 16, 16 | Nơi chốn; level 8 là màn nghỉ |
| 9 | 18 | Nhóm đợt 2 |
| 10 ★ | 18 | Kêu to |
| 11 | 18 | Gỗ, Vải + 23 sticker mới |
| 12 | 20 | Kim loại, Thủy tinh |
| 13 | 20 | Màn nghỉ |
| 14 ★ | 22 | Có mùi |
| 15–16 | 22, 24 | |
| 17 ★ | 24 | Hay bị mất |
| 18 | 26 | |
| 19 | 24 | Màn nghỉ |
| 20 ★ | 28 | Dính tay + Có chân |

★ = màn vui nhộn. Kho sticker cần tăng từ 49 lên khoảng 72 ở level 11 (khoảng 12 sticker mỗi màu) để người chơi có cảm giác "mở hộp sticker mới".

### 6.4 Sinh level và xáo mỗi lần chơi

- Mỗi level có một `seed` cố định quyết định **bộ sticker** (cùng sticker mỗi lần chơi lại).
- Mỗi lần bắt đầu / chơi lại, một `run seed` ngẫu nhiên xáo **thứ tự rơi, vị trí, góc xoay và bộ 3 từ**. Hũ trông khác đi, và đường đi tới lời giải cũng khác.
- Cách chọn bộ sticker: chọn `k` màu (ở màn vui nhộn, ưu tiên các màu có nhiều sticker mang từ vui nhộn), rồi ưu tiên sticker mang nhiều tag đang mở trong level.

### 6.5 Thuật toán đưa ra 3 từ

1. Lấy các từ đang mở của level còn khớp ít nhất 1 sticker trong hũ. Nếu có từ 3 từ khớp ≥2 sticker trở lên thì chỉ dùng những từ này.
2. Luôn có **1 từ quét nhiều nhất** (hoặc kém nhất 1 sticker).
3. Ở màn vui nhộn, từ vui nhộn luôn được đưa ra khi nó còn khớp ≥2 sticker.
4. Phần còn lại chọn ngẫu nhiên, rồi xáo thứ tự hiển thị.
5. Bộ từ xác định theo (seed level, sticker còn lại, số lần đổi từ, run seed): cùng trạng thái thì cùng bộ từ.

### 6.6 Số lượt

- **Lời giải của máy:** giải tham lam, mỗi lượt chọn từ quét nhiều nhất trong 3 từ được đưa ra.
- **Số lượt của level** = trung vị lời giải máy trên 41 lần xáo mẫu + lượt dư. Số này cố định cho mọi lần chơi.
- Mỗi lần chơi chỉ dùng những lần xáo mà máy giải được trong số lượt đó (trung bình 1–2 lần thử). Màn nào cũng chắc chắn thắng được.

## 7. Vật lý

Solver Verlet tự viết, không dùng thư viện:
- Thế giới 500×500 đơn vị, khớp hũ vuông. Ba bức tường: đáy, trái, phải (kéo dài lên trên miệng hũ).
- Trọng lực 0,18 mỗi substep · 2 substep mỗi frame (60 fps) · 5 vòng giải va chạm · cản không khí 0,5% · ma sát đáy 8% · vận tốc tối đa 28.
- **Hitbox hình tròn đo theo đúng hình vẽ** của từng sticker: bán kính = trung bình của (bán kính diện tích tương đương) và (bán kính quán tính); tâm đặt ở trọng tâm phần hình. Giá trị trong `data/stickers.json` (`hitbox`, đơn vị SVG 64×64).
- Quy đổi: bán kính thế giới = bán kính SVG × (100 × 0,84 / 64) + 2.
- Sticker xoay theo quãng đường lăn (góc += Δx / bán kính).
- **Spawn theo lưới so le** (5 rồi 4 sticker mỗi hàng, hàng cách nhau 80) phía trên miệng hũ, nên không bao giờ chồng nhau lúc xuất hiện.
- Màn đông nhất (28 sticker) chỉ đầy khoảng nửa hũ.

Hạn chế đã biết: vật dài (chuối, cà rốt, chìa khóa) dùng hitbox tròn nên đầu hình có thể đè nhẹ lên sticker bên cạnh. Có thể nâng cấp lên hitbox dạng viên thuốc.

## 8. Animation và nhịp

| Pha | Thời lượng |
|---|---|
| Sticker bay lên hàng | 470 ms mỗi cái, cái sau trễ 75 ms |
| Đánh số | Ngay khi từng sticker tới hàng (90% thời lượng bay), kèm nảy 200 ms |
| Giữ hàng sau khi đếm xong | 480 ms |
| Bay vào băng keo | 480 ms mỗi cái, cái sau trễ 45 ms |
| Băng keo nảy khi nhận | 260 ms |

Một lượt 8 sticker từ lúc chọn tới lúc đếm xong khoảng 1 giây. Hũ tối lại 62% trong lúc điểm danh để hàng sticker nổi bật. Có hỗ trợ `prefers-reduced-motion` (rút còn 35% thời lượng).

## 9. Âm thanh

Toàn bộ âm thanh trong prototype được tạo bằng Web Audio (không cần file). Thư mục `audio/` là bản xuất WAV của từng âm (mono, 44,1 kHz) để tham khảo hoặc làm brief cho sound designer.

**Khi sticker tới hàng điểm danh**, tiếng phát theo từ đã chọn. Cao độ tăng dần theo số đếm theo thang ngũ cung (0, 2, 4, 7, 9, 12, 14, 16, 19 nửa cung), dừng tăng từ sticker thứ 9:

| Từ | Âm | File |
|---|---|---|
| Màu (mọi màu) | Chuông mộc | `tag_color.wav` |
| Đồ ăn | Giòn rụm 3 nhịp | `tag_food.wav` |
| Con vật | "Chíp" hai nốt | `tag_animal.wav` |
| Đồ chơi | Tiếng kêu chút chít | `tag_toy.wav` |
| Quần áo, Vải | Sột soạt vải | `tag_cloth.wav` |
| Xe cộ | Còi xe nhỏ | `tag_vehicle.wav` |
| Đồ dùng, Gỗ | Gõ gỗ "cốc" | `tag_wood.wav` |
| Nhà bếp, Kim loại | Tiếng "tinh" kim loại | `tag_metal.wav` |
| Biển | Bong bóng nước | `tag_water.wav` |
| Bầu trời | Lấp lánh | `tag_sky.wav` |
| Khu vườn | Lá xào xạc | `tag_leaf.wav` |
| Biết bay | Gió "vút" | `tag_fly.wav` |
| Kêu to | Còi hơi | `tag_loud.wav` |
| Thủy tinh | Chạm cốc "keng" | `tag_glass.wav` |
| Cao su | "Boing" | `tag_rubber.wav` |

File `*_count5.wav` là cùng âm đó ở số đếm thứ 5 (cao hơn), để nghe sự tăng cao độ.

**Khi sticker chạm băng keo:** `pop.wav`, tiếng "pop" ASMR ngắn, cao độ ngẫu nhiên ±15% mỗi lần.
**Thắng màn:** `win.wav`, 4 nốt đi lên.

Bản game thật nên thay bằng âm thanh do sound designer làm theo đúng danh sách này, đặc biệt tiếng giấy sticker và tiếng pop.

## 10. Việc còn mở

1. Vẽ thêm khoảng 23 sticker cho level 11–20 và các từ vui nhộn còn lại (sầu riêng, hành, điều khiển TV, kính, mật ong, kẹo, băng keo, bàn, con gà, con mèo, cái quần…).
2. Làm level 11–20 theo bảng 6.3.
3. Cân nhắc cơ chế "pool đầy dần" (vài sticker mới rơi vào hũ mỗi lượt) nếu cần thêm độ căng cho các level sau.
4. Hitbox dạng viên thuốc cho vật dài.
5. Tùy chọn chạm để bỏ qua bước điểm danh.
6. Meta game, booster (thêm lượt, đổi từ), kiếm tiền bằng quảng cáo + booster.
7. Kiểm tra lại các tag dễ tranh cãi: trống có tính là gỗ, vịt con có ở khu vườn, khúc gỗ ở khu vườn.

## 11. Game tham khảo

- **Word Associations Solitaire** (TidalAurum): gần nhất về cơ chế (danh mục, thẻ bị che, giới hạn lượt). https://apps.apple.com/us/app/-/id6755318680
- **Categories: Words Associations:** gom từ theo chủ đề. https://apps.apple.com/app/id6746159736
- **Tile Explorer – Triple Match** (Oakever Games): pool nhiều lớp, booster, nhịp độ khó. https://play.google.com/store/apps/details?id=com.oakever.tiletrip
- **Zen Match** (Good Job Games): hiệu ứng clear, cảm giác thư giãn, cách chèn quảng cáo. https://apps.apple.com/us/app/-/id1518273799
- **NYT Games – Connections:** từ bẫy và khoảnh khắc "aha". https://play.google.com/store/apps/details?id=com.nytimes.crossword
