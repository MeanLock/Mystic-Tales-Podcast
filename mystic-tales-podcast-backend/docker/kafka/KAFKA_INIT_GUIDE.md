# Kafka Init Service - Docker UI Management

## 📋 Tổng quan

Service `kafka-init` được thiết kế để khởi tạo topics cho Kafka cluster một cách tự động và thông minh. Service này:

- ✅ **Tự động dừng** sau khi hoàn thành (không restart)
- 🔧 **Có thể cấu hình** thông qua biến môi trường
- 🎯 **Quản lý hoàn toàn qua Docker UI** (không cần script shell bên ngoài)
- 🧠 **Smart Mode**: Chỉ tạo/cập nhật topics khi cần thiết
- 💾 **Tối ưu hiệu suất**: Tránh ghi không cần thiết vào broker

## 🚀 Cách sử dụng

### 1. Khởi chạy kafka-init service

```bash
# Khởi động kafka-init service
docker-compose up kafka-init

# Hoặc chạy trong background
docker-compose up -d kafka-init
```

### 2. Theo dõi tiến trình

```bash
# Xem logs real-time
docker-compose logs -f kafka-init

# Xem logs đã lưu
docker-compose logs kafka-init
```

### 3. Kiểm tra trạng thái

```bash
# Kiểm tra trạng thái containers
docker-compose ps

# Kiểm tra exit code của kafka-init
docker inspect $(docker-compose ps -q kafka-init) --format='{{.State.ExitCode}}'
```

## 🔧 Cấu hình Topics

### Cách 1: Chỉnh sửa docker-compose.yml

Trong file `docker-compose.yml`, tìm section `kafka-init` và chỉnh sửa biến `TOPICS_CONFIG`:

```yaml
environment:
  TOPICS_CONFIG: |
    user-events:9:3:2:168
    order-events:12:3:2:720
    new-topic:6:3:2:72
```

### Cách 2: Override bằng environment file

Tạo file `.env` với nội dung:

```bash
TOPICS_CONFIG="user-events:9:3:2:168
order-events:12:3:2:720
custom-topic:6:3:2:24"
```

### Cách 3: Docker Compose override

```bash
# Sử dụng override file
docker-compose -f docker-compose.yml -f docker-compose.override.yml up kafka-init
```

## 📊 Format cấu hình Topics

```
topic-name:partitions:replication-factor:min-isr:retention-hours
```

**Ví dụ:**
- `user-events:9:3:2:168` = Topic "user-events", 9 partitions, replication factor 3, min ISR 2, retention 168 hours (7 days)

## 🔄 Lifecycle Management

### Exit Codes

| Code | Ý nghĩa |
|------|---------|
| 0    | ✅ Thành công hoàn toàn |
| 1    | ⚠️ Một số topics failed |
| 2    | ❌ Không tìm thấy cluster directory |
| 3    | ❌ Không tìm thấy init script |
| 124  | ⏰ Timeout (quá 10 phút) |

### Service Behaviors

- **Restart Policy**: `no` - Service không tự động restart
- **Timeout**: 10 phút tối đa, sau đó force stop
- **Cleanup**: Tự động cleanup khi exit
- **Smart Mode**: Kiểm tra cấu hình hiện tại trước khi tạo/cập nhật
- **Performance**: Chỉ ghi vào broker khi thực sự cần thiết

## 🎯 Các tình huống sử dụng

### Khởi tạo lần đầu
```bash
docker-compose up kafka-init
```

### Cập nhật topics configuration
1. Chỉnh sửa `TOPICS_CONFIG` trong docker-compose.yml
2. Chạy lại: `docker-compose up kafka-init`

### Troubleshooting
```bash
# Xóa container cũ và chạy lại
docker-compose rm -f kafka-init
docker-compose up kafka-init

# Xem chi tiết logs
docker-compose logs kafka-init | grep -E "(✅|❌|⚠️)"
```

## 🚦 Monitoring với Docker UI

Khi sử dụng Docker Desktop hoặc Portainer:

1. **Container Status**: Xem trạng thái container (running/exited)
2. **Logs**: Theo dõi real-time logs
3. **Environment**: Chỉnh sửa biến môi trường
4. **Restart**: Khởi động lại service khi cần

## 💡 Tips

- Service sẽ **tự động dừng** sau khi hoàn thành
- **Không cần** script shell bên ngoài
- **Có thể chạy bất kỳ lúc nào** để update topics
- **Smart**: Chỉ tạo/cập nhật khi cấu hình thay đổi
- **Fault-tolerant**: Tự động điều chỉnh khi broker failed
- **Performance optimized**: Tránh ghi không cần thiết vào broker

## 🚀 Smart Mode Benefits

1. **Faster execution**: Topics đã đúng cấu hình sẽ được skip
2. **Reduced broker load**: Không ghi không cần thiết
3. **Better logging**: Phân biệt rõ created/updated vs skipped
4. **Safer operations**: Chỉ thay đổi khi thực sự cần thiết

## 🔗 Related Services

- `kafka-broker-1/2/3`: Kafka brokers
- `kafka-monitor`: Cluster monitoring
- `kafka-ui`: Web interface để xem topics
