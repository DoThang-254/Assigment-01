namespace Presentation.ViewModels.Auth
{
    public class CreateAccountRequest
    {
        public string? AccountName { get; set; }

        public string? AccountEmail { get; set; }

        public string? AccountPassword { get; set; }

        public int? AccountRole { get; set; }

    }
}
