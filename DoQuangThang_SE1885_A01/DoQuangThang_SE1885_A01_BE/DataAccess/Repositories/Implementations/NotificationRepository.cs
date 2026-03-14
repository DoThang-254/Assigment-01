using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly FunewsManagementContext _context;
        public NotificationRepository(FunewsManagementContext context)
        {
            _context = context;
        }
        public async Task AddNotification(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Notification> GetAllNotifications()
        {
            return _context.Notifications;
        }
    }
}
