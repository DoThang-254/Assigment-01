using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        public string Token { get; set; }      
        public string JwtId { get; set; }      

        public bool IsUsed { get; set; }      
        public bool IsRevoked { get; set; }    // Đã bị thu hồi chưa? (Logout)

        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; } 

        // 3. Refresh Token Rotation (Chuỗi tin cậy)
        // Khi token này được dùng (IsUsed=true), nó sinh ra token mới nào?
        public string? ReplacedByToken { get; set; }

        // 4. Quản lý thiết bị (Active Sessions)
        // Để hiển thị: "Đăng nhập từ iPhone 14 - Chrome - 192.168.1.1"
        [MaxLength(50)]
        public string? CreatedByIp { get; set; }

        [MaxLength(256)]
        public string? UserAgent { get; set; }

        // Khóa ngoại trỏ về bảng Account
        [ForeignKey(nameof(SystemAccount))]
        public short AccountId { get; set; }
        public virtual SystemAccount SystemAccount { get; set; }
    }
}
