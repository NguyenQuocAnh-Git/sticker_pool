# Sticker Pool — Bộ bàn giao

Word puzzle casual cho mobile: chọn 1 trong 3 từ, mọi sticker cùng tính chất bay khỏi hũ, clear sạch hũ là thắng.

## Bắt đầu từ đâu

1. Mở `prototype/index.html` bằng trình duyệt (Chrome/Safari, nên mở ở chế độ điện thoại) để chơi thử 10 level.
2. Đọc `docs/GDD.md`: toàn bộ thiết kế, luật, hệ thống tag, level, vật lý, animation, âm thanh và việc còn mở.

## Thư mục

| Đường dẫn | Nội dung |
|---|---|
| `prototype/index.html` | Bản chơi thử, một file duy nhất. Code gồm dữ liệu sticker, sinh level, vật lý, animation và âm thanh |
| `docs/GDD.md` | Tài liệu thiết kế |
| `stickers/svg/` | 49 sticker dạng SVG (64×64, đã kèm texture chất liệu, chưa có viền trắng) |
| `stickers/png/` | 49 sticker PNG 300×300 nền trong suốt, đã có viền trắng |
| `stickers/sticker_sheet.png` | Ảnh tổng hợp toàn bộ sticker kèm tag |
| `data/stickers.json` | Mỗi sticker: màu, nhóm, nơi chốn, từ vui nhộn, chất liệu, hitbox |
| `data/tags.json` | Các phân vùng và từ, mã màu |
| `data/levels.json` | 10 level: số sticker, số màu, từ được mở, từ vui nhộn, số lượt |
| `audio/` | Âm thanh xuất từ prototype (WAV mono 44,1 kHz) |

## Ghi chú

- Prototype dùng font Google (Baloo 2, Be Vietnam Pro); không có mạng thì tự dùng font hệ thống.
- Bản playtest mở sẵn cả 10 level.
- Âm thanh chỉ phát sau lần chạm đầu tiên (quy định của trình duyệt).
