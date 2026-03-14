// --- CẤU HÌNH ---
const API_URL = "https://localhost:7066"; // Đổi thành port của Backend API
const HUB_URL = `${API_URL}/hubs/notifications`;

// 1. Khởi tạo kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect() // Tự động kết nối lại nếu mất mạng
    .build();

// 2. Lắng nghe sự kiện từ Server (Real-time)
connection.on("ReceiveNewArticle", (data) => {
    // data bao gồm { msg: "...", date: "..." }

    // A. Hiển thị Toast thông báo góc màn hình
    console.log("Nhận thông báo mới:", data);
    showToast(data.msg);

    // B. Thêm vào danh sách dropdown
    addNotificationToUi(data.msg, data.date);  

    // C. Cập nhật số lượng chưa đọc
    updateBadgeCount();
});

// 3. Bắt đầu kết nối
connection.start()
    .then(() => {
        console.log("SignalR Connected!");
        // Sau khi kết nối xong, tải 10 thông báo cũ từ API (Lịch sử)
        loadRecentNotifications();
    })
    .catch(err => console.error("SignalR Connection Error: ", err));

// --- CÁC HÀM XỬ LÝ GIAO DIỆN ---

// Hàm tải lịch sử 10 thông báo cũ
function loadRecentNotifications() {
    fetch(`${API_URL}/api/Notification/recent`)
        .then(response => response.json())
        .then(data => {
            console.log("Lịch sử thông báo:", data);

            // Xóa nội dung cũ nếu cần
            const list = document.getElementById("notification-list");
            list.innerHTML = "";

            if (data && data.length > 0) {
                data.forEach(item => {
                    // Gọi hàm để thêm từng thông báo vào <ul>
                    // item.title và item.createdAt tùy thuộc vào cấu trúc JSON của API bạn
                    addNotificationToUi(item.title || item.content, item.createdAt, false);
                });

                // Cập nhật số lượng thông báo dựa trên số item lấy được
                updateBadgeCount(data.length);            }
        })
        .catch(err => console.error("Không tải được lịch sử thông báo", err));
}

// Hàm thêm item vào danh sách
function addNotificationToUi(msg, date) {
    const list = document.getElementById("notification-list");

    const li = document.createElement("li");
    li.style.padding = "10px";
    li.style.borderBottom = "1px solid #eee";
    li.innerHTML = `
        <div style="font-weight: bold; font-size: 14px;">${msg}</div>
        <div style="font-size: 11px; color: gray;">${new Date(date).toLocaleString()}</div>
    `;

    // Thêm vào đầu danh sách
    list.prepend(li);

    // Giới hạn chỉ giữ 10 cái trên giao diện
    if (list.children.length > 10) {
        list.removeChild(list.lastChild);
    }
}

// Hàm cập nhật số trên Badge đỏ
function updateBadgeCount() {
    const badge = document.getElementById("notification-count");
    let currentCount = parseInt(badge.innerText) || 0;

    badge.innerText = currentCount + 1;
    badge.style.display = "block"; // Hiện badge
}

// Hàm hiển thị Toast
function showToast(message) {
    const container = document.getElementById("toast-container");
    const toast = document.createElement("div");

    toast.innerText = message;
    toast.style.background = "#333";
    toast.style.color = "#fff";
    toast.style.padding = "12px 20px";
    toast.style.marginTop = "10px";
    toast.style.borderRadius = "5px";
    toast.style.boxShadow = "0 2px 10px rgba(0,0,0,0.2)";
    toast.style.animation = "fadeIn 0.5s";

    container.appendChild(toast);

    // Tự biến mất sau 5s
    setTimeout(() => {
        toast.style.opacity = "0";
        setTimeout(() => toast.remove(), 500); // Đợi hiệu ứng mờ dần
    }, 5000);
}

// Hàm Toggle danh sách (được gọi từ onclick trong HTML)
function toggleNotificationList() {
    const list = document.getElementById("notification-list");
    const badge = document.getElementById("notification-count");

    if (list.style.display === "none") {
        list.style.display = "block";
        // Reset badge về 0 khi đã xem
        badge.innerText = "0";
        badge.style.display = "none";
    } else {
        list.style.display = "none";
    }
}

// Click ra ngoài thì đóng dropdown
document.addEventListener('click', function (event) {
    const container = document.getElementById('notification-area');
    const list = document.getElementById("notification-list");
    if (!container.contains(event.target) && list.style.display === "block") {
        list.style.display = "none";
    }
});