using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }      
        public string Action { get; set; }      
        public string TableName { get; set; }   
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; } 
        public string? NewValues { get; set; }  
    }
}
