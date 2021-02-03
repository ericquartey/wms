using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Ferretto.VW.MAS.Utils.Events;

namespace Ferretto.VW.MAS.InternalTiming
{
    internal sealed class InternalTimingService : BackgroundService
    {
        #region Fields

        private const int DefaultTimeOutPeriodMilliseconds = 1000 * 2; /*15 * 60 * 1000;*/  // 15 minutes

        private const int MinTimeOutPeriodMillisconds = 60 * 1000;  // 1 minute

        private readonly IDataLayerService dataLayerService;

        private readonly ILogger<InternalTimingService> logger;

        private readonly NotificationEvent notificationEvent;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Constructors

        public InternalTimingService(
            IEventAggregator eventAggregator,
            IDataLayerService dataLayerService,
            ILogger<InternalTimingService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataLayerService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            if (this.dataLayerService.IsReady)
            {
                this.OnDataLayerReady(null);
            }
            else
            {
                this.notificationEvent.Subscribe(
                    this.OnDataLayerReady,
                    ThreadOption.PublisherThread,
                    false,
                    m => m.Type is CommonUtils.Messages.Enumerations.MessageType.DataLayerReady);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            this.cancellationTokenSource?.Cancel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        private void ExecuteBackupScript()
        {
            //var backupScript = "f:\\database\\remote_backup.cmd";
            var backupScript = "C:\\BackUpDB\\remote_backup.cmd";  // Use of xcopy ... command inside the batch file
            var info = new FileInfo(backupScript);
            if (info.Exists)
            {
                var script = File.ReadAllText(backupScript);
                if (!string.IsNullOrEmpty(script))
                {
                    var process = new Process();
                    process.StartInfo.FileName = backupScript;
                    process.StartInfo.ErrorDialog = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.ErrorDialog = false;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    try
                    {
                        process.Start();
                        var thread = new Thread(new ParameterizedThreadStart(this.ReadStandardError));
                        thread.Start(process.StandardError);
                        process.WaitForExit();
                        thread.Join();

                        if (process.HasExited)
                        {
                            switch (process.ExitCode)
                            {
                                case 0:
                                    this.logger.LogInformation($"Database Backup executed");
                                    break;

                                case 4:
                                    this.logger.LogInformation($"Database Backup error: invalid file name");
                                    break;

                                case 1:
                                    this.logger.LogInformation($"Database Backup error: file does not exist");
                                    break;

                                case 2:
                                    this.logger.LogInformation($"Database Backup error: CTRL+C pressed to terminate copying");
                                    break;

                                case 5:
                                    this.logger.LogInformation($"Database Backup error: disk write error");
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex.Message);
                    }
                }
                else
                {
                    this.logger.LogDebug($"file {backupScript} empty");
                }
            }
            else
            {
                this.logger.LogDebug($"file {backupScript} not found");
            }
        }

        private async Task ExecutePollingAsync()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = this.cancellationTokenSource.Token;

            try
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var machineVolatileDataProvider = scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                    var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();

                    var timeIntervalMilliseconds = DefaultTimeOutPeriodMilliseconds;

                    do
                    {
                        try
                        {
                            if (!machineVolatileDataProvider.IsDeviceManagerBusy)
                            {
                                if (/*machineVolatileDataProvider.EnableLocalDbSavingOnServer*/ machineProvider.IsDbSaveOnServer())
                                {
                                    this.logger.LogDebug("Attempting to send a back database file to server.");

                                    this.ExecuteBackupScript();
                                }

                                timeIntervalMilliseconds = DefaultTimeOutPeriodMilliseconds;
                            }
                            else
                            {
                                timeIntervalMilliseconds = MinTimeOutPeriodMillisconds;
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogWarning("Unable to send backup database file: '{details}'.", ex.Message);
                        }
                        finally
                        {
                            this.logger.LogDebug($"Pausing service for {timeIntervalMilliseconds / 1000} s.");

                            await Task.Delay(
                                timeIntervalMilliseconds,
                                this.cancellationTokenSource.Token);
                        }
                    }
                    while (!cancellationToken.IsCancellationRequested);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
            {
                this.logger.LogTrace("Stopping service.");
                return;
            }
        }

        private void OnDataLayerReady(NotificationMessage message)
        {
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource = null;

            Task.Run(this.ExecutePollingAsync);
        }

        private void ReadStandardError(object obj)
        {
            if (obj is StreamReader inputStream)
            {
                try
                {
                    var msgError = "";
                    while (!inputStream.EndOfStream)
                    {
                        msgError += ((char)inputStream.Read());
                    }
                }
                catch
                {
                    // do nothing
                };
            }
        }

        #endregion
    }
}
