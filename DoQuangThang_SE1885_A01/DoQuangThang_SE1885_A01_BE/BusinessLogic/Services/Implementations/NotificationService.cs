using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task AddNotification(Notification notification)
        {
            if(notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }
            await _notificationRepository.AddNotification(notification);
        }

        public async Task<List<Notification>> GetAllNotifications()
        {
            return await _notificationRepository.GetAllNotifications().ToListAsync();
        }
    }
}
