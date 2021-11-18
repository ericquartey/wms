using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class LogoutSettingsProvider : BaseProvider, ILogoutSettingsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public LogoutSettingsProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IMissionsDataProvider missionsDataProvider) : base(eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        private void CheckLogoutSettings()
        {
            lock (this.dataContext)
            {
                if (this.dataContext.LogoutSettings != null &&
                                !this.dataContext.LogoutSettings.Any())
                {
                    this.dataContext.LogoutSettings.Add(new LogoutSettings());
                    this.dataContext.LogoutSettings.Add(new LogoutSettings());
                    this.dataContext.LogoutSettings.Add(new LogoutSettings());

                    this.dataContext.SaveChanges();
                }
            }
        }

        public IEnumerable<LogoutSettings> GetAllLogoutSettings()
        {
            lock (this.dataContext)
            {
                var result = this.dataContext.LogoutSettings.AsNoTracking();
                return result;
            }
        }

        public void AddOrModifyLogoutSettings(LogoutSettings logoutSettings)
        {
            lock (this.dataContext)
            {
                if (!this.dataContext.LogoutSettings.Any(s => s.Id == logoutSettings.Id))
                {
                    this.AddLogoutSettings(logoutSettings);
                }
                else
                {
                    this.ModifyLogoutSettings(logoutSettings);
                }
            }
        }

        public void AddLogoutSettings(LogoutSettings logoutSettings)
        {
            lock (this.dataContext)
            {
                logoutSettings.RemainingTime = logoutSettings.Timeout;
                this.dataContext.LogoutSettings.Add(logoutSettings);
                this.dataContext.SaveChanges();
            }
        }

        public void RemoveLogoutSettingsById(int id)
        {
            lock (this.dataContext)
            {
                var removeElement = this.dataContext.LogoutSettings.Single(s => s.Id == id);
                this.dataContext.LogoutSettings.Remove(removeElement);
                this.dataContext.SaveChanges();
            }
        }

        public void ModifyLogoutSettings(LogoutSettings newLogoutSettings)
        {
            lock (this.dataContext)
            {
                if (newLogoutSettings != null)
                {
                    var logoutSettings = this.dataContext.LogoutSettings.Single(s => s.Id == newLogoutSettings.Id);
                    logoutSettings.IsActive = newLogoutSettings.IsActive;
                    logoutSettings.Timeout = newLogoutSettings.Timeout;
                    logoutSettings.BeginTime = newLogoutSettings.BeginTime;
                    logoutSettings.EndTime = newLogoutSettings.EndTime;
                    logoutSettings.RemainingTime = logoutSettings.Timeout;
                    this.dataContext.LogoutSettings.Update(logoutSettings);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void ResetRemainingTime(int id)
        {
            lock (this.dataContext)
            {
                var logoutSettings = this.dataContext.LogoutSettings.Single(s => s.Id == id);
                logoutSettings.RemainingTime = logoutSettings.Timeout;
                this.dataContext.LogoutSettings.Update(logoutSettings);
                this.dataContext.SaveChanges();
            }
        }

        private void UpdateRemainingTime(int id, double time)
        {
            lock (this.dataContext)
            {
                var logoutSettings = this.dataContext.LogoutSettings.Single(s => s.Id == id);
                logoutSettings.RemainingTime = time;
                this.dataContext.LogoutSettings.Update(logoutSettings);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateStatus(double minutes)
        {
            var anyActiveMission = this.missionsDataProvider.GetAllActiveMissions().Any();

            var logoutSettings = this.GetAllLogoutSettings();

            if (logoutSettings != null && logoutSettings.Any())
            {
                var active = logoutSettings.Where(s => s.IsActive).ToList();
                var resetElements = logoutSettings.Where(s => s.RemainingTime != s.Timeout).ToList();

                if (active != null && active.Any() && !anyActiveMission)
                {
                    foreach (var logout in active)
                    {
                        if(resetElements.Any(s => s.Id == logout.Id))
                        {
                            var itemToRemove = resetElements.Single(s => s.Id == logout.Id);
                            resetElements.Remove(itemToRemove);
                        }

                        var actualTime = DateTime.Now.TimeOfDay;

                        var timeCondition = logout.BeginTime > logout.EndTime ? actualTime >= logout.BeginTime || actualTime <= logout.EndTime : actualTime >= logout.BeginTime && actualTime <= logout.EndTime;
                        if (timeCondition)
                        {
                            if (logout.RemainingTime - minutes == 0)
                            {
                                this.UpdateRemainingTime(logout.Id, logout.Timeout);

                                var notificationMessage = new NotificationMessage(
                                                           new LogoutMessageData(),
                                                           "Execute Logout Command",
                                                           MessageActor.Any,
                                                           MessageActor.DeviceManager,
                                                           MessageType.Logout,
                                                           BayNumber.All,
                                                           BayNumber.All,
                                                           MessageStatus.NotSpecified);
                                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
                            }
                            else
                            {
                                this.UpdateRemainingTime(logout.Id, logout.RemainingTime - minutes);
                            }
                        }
                        else if (logout.RemainingTime != logout.Timeout)
                        {
                            this.ResetRemainingTime(logout.Id);
                        }
                    }
                }

                if (resetElements != null && resetElements.Any())
                {
                    foreach (var resetElement in resetElements)
                    {
                        if (resetElement.RemainingTime != resetElement.Timeout)
                        {
                            this.ResetRemainingTime(resetElement.Id);
                        }
                    }
                }
            }
            else
            {
                this.CheckLogoutSettings();
            }
        }

        #endregion
    }
}
