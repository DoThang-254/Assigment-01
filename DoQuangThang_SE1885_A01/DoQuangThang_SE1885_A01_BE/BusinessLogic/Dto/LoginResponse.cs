namespace BusinessLogic.Dto
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }  
        public string RefreshToken { get; set; }
        public int AccountId { get; set; }
        public string? Email { get; set; }
        public int? Role { get; set; }
    }
}
