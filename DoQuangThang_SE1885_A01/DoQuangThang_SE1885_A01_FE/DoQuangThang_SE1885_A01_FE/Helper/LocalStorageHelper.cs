using System.Security.Cryptography; // Cần thêm thư viện này
using System.Text;

namespace DoQuangThang_SE1885_A01_FE.Helper // Lưu ý namespace của bạn là Helper (số ít)
{
    public static class LocalStorageHelper
    {
        private static readonly string _folderPath = Path.Combine(Directory.GetCurrentDirectory(), "LocalData");

        // Hàm helper để tạo tên file an toàn từ Key (URL)
        private static string GetSafeFilename(string key)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(key);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // Chuyển byte thành chuỗi Hex (ví dụ: "a3f5...")
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower() + ".dat";
            }
        }

        public static async Task SaveDataAsync(string key, byte[] content)
        {
            if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);

            // Dùng hàm GetSafeFilename để lấy tên file đã mã hóa
            string fileName = GetSafeFilename(key);
            string filePath = Path.Combine(_folderPath, fileName);

            await File.WriteAllBytesAsync(filePath, content);
        }

        public static async Task<byte[]> GetDataAsync(string key)
        {
            // Dùng hàm GetSafeFilename để tìm đúng file
            string fileName = GetSafeFilename(key);
            string filePath = Path.Combine(_folderPath, fileName);

            if (!File.Exists(filePath)) return null;

            return await File.ReadAllBytesAsync(filePath);
        }
    }
}