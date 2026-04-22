using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntuitERP.Services
{
    public class SessionTimeoutService
    {
        private DateTime _lastActivity;
        private Timer _timer;
        private readonly SystemSettingsService _settingsService;
        private readonly UserContext _userContext;
        private int _timeoutMinutes = 30;
        public event EventHandler SessionExpired;

        public SessionTimeoutService(SystemSettingsService settingsService, UserContext userContext)
        {
            _settingsService = settingsService;
            _userContext = userContext;
            _lastActivity = DateTime.Now;
        }

        public async Task StartMonitoringAsync()
        {
            _timeoutMinutes = await _settingsService.GetSessionTimeoutAsync();

            if (_timeoutMinutes <= 0) return; // Timeout disabled

            _timer = new Timer(CheckTimeout, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public void RecordActivity()
        {
            _lastActivity = DateTime.Now;
        }

        private void CheckTimeout(object state)
        {
            var idle = (DateTime.Now - _lastActivity).TotalMinutes;

            if (idle >= _timeoutMinutes)
            {
                _timer?.Dispose();
                SessionExpired?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StopMonitoring()
        {
            _timer?.Dispose();
        }
    }
}

