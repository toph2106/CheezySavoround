## Luồng chạy chính của Game
Game được vận hành theo một vòng lặp cốt lõi dựa trên Máy trạng thái nhằm tối ưu trải nghiệm người chơi:
1. **Lobby/Menu State:** Người chơi bắt đầu tại màn hình chính. Tại đây, hệ thống cung cấp các tính năng Meta-game như: Điểm danh nhận thưởng hàng ngày, Cửa hàng (Mua Skin, mua Booster Dao/Nĩa), và Hệ thống Thành tựu.
2. **Gameplay State:** Khi vào màn chơi, người chơi thực hiện thao tác xếp các đĩa Pizza. Mục tiêu là ghép đủ 6 lát Pizza cùng loại trên một đĩa để "nổ" đĩa và ghi điểm. Hoàn thành số lượng mục tiêu do Level yêu cầu để qua màn.
3. **Progression State:** Sau khi thắng/thua, người chơi nhận Vàng (Gold). Vàng được lưu trữ và dùng để tái đầu tư vào Cửa hàng (mua Booster giúp qua các màn khó hơn) hoặc sưu tầm Skin mới, tạo động lực chơi tiếp.

## Cấu trúc và Cấu hình Dữ liệu (JSON & ScriptableObject)
Dự án áp dụng mô hình phân tách dữ liệu để dễ dàng mở rộng:
- **Dữ liệu tĩnh (Static Data):** Các cấu hình về Level, Chỉ số Thành tựu, Giá tiền Cửa hàng được thiết kế dưới dạng **ScriptableObject** của Unity. Điều này giúp Game Designer dễ dàng tinh chỉnh chỉ số ngay trên Inspector mà không cần chạm vào code.
- **Dữ liệu động (Dynamic Data - JSON):** Toàn bộ tiến trình của người chơi (Vàng, Skin đã sở hữu, Level hiện tại, Tiến độ thành tựu...) được serialize thành định dạng **JSON** thông qua lớp `UserData`. Dữ liệu JSON này sau đó được hệ thống `SaveSystem` mã hóa (AES Encryption) và lưu xuống file `save.dat` ở bộ nhớ máy (`Application.persistentDataPath`). Khi khởi động, game sẽ đọc file, giải mã và parse lại từ JSON thành Object để sử dụng.
![System Flow Diagram](Docs/CheezySavoround.png)