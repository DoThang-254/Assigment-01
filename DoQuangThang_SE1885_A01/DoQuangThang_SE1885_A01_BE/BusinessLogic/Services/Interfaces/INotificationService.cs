using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface INotificationService
    {
        Task AddNotification(Notification notification);

        Task<List<Notification>> GetAllNotifications();
    }
}
