using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        public void CopyMachineDatabaseToServer()
        {
            const int NUMBER_OF_RETRIES = 5;

            for (var i = 0; i < NUMBER_OF_RETRIES; i++)
            {
                this.Logger.LogInformation($"Try: #{i + 1}. Copying machine database to server...");

                if (this.ExecuteBackupScript() == true)
                {
                    break;
                }
            }
        }

        private bool ExecuteBackupScript()
        {
            var success = false;

            // The location of cmd file is written hardcoded. TODO: use a different strategy?
            // See the Installer/Scripts location (Installer project) to retrieve the file
            // Note about batch file (remote_backup.cmd):
            //  - use the IP of server (default: 192.168.137.1)
            //  - fergrp_2012 /user:wmsadmin are the credential for accessing the server
            //  - use of xcopy command (it retrieves an error code, if it fails)
            var backupScript = "f:\\database\\remote_backup.cmd";

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
                            success = true;
                            switch (process.ExitCode)
                            {
                                case 0:
                                    this.Logger.LogInformation($"Database Backup executed.");
                                    break;

                                case 4:
                                    this.Logger.LogInformation($"Database Backup error: invalid file name.");
                                    break;

                                case 1:
                                    this.Logger.LogInformation($"Database Backup error: file does not exist.");
                                    break;

                                case 2:
                                    this.Logger.LogInformation($"Database Backup error: CTRL+C pressed to terminate copying.");
                                    break;

                                case 5:
                                    this.Logger.LogInformation($"Database Backup error: disk write error.");
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex.Message);
                    }
                }
                else
                {
                    this.Logger.LogDebug($"Database Backup error: file {backupScript} empty.");
                    success = true;
                }
            }
            else
            {
                this.Logger.LogDebug($"Database Backup error: file {backupScript} not found.");
                success = true;
            }

            return success;
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
