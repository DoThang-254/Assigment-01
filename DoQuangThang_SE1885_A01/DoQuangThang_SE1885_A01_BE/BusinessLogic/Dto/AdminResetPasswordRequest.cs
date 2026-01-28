using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Dto
{
    public class AdminResetPasswordRequest
    {
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        public int TargetAccountId { get; set; }
        public string NewPassword { get; set; }
    }
}
