using System.ComponentModel.DataAnnotations;

namespace DoQuangThang_SE1885_A01_FE.Models.Accounts
{
    public class AccountsDto
    {
        public short AccountId { get; set; }

        [Required]
        [StringLength(150)]
        public string? AccountName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string? AccountEmail { get; set; }

        [Range(0, int.MaxValue)]
        public int? AccountRole { get; set; }
    }
}
