using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        public void CopyMachineDatabaseToServer(string server, string username, string password, string database, string serialNumber)
        {
            const int NUMBER_OF_RETRIES = 2;

            for (var i = 0; i < NUMBER_OF_RETRIES; i++)
            {
                this.Logger.LogInformation($"Try: #{i + 1}. Copying machine database to server...");

                if (this.ExecuteBackupScript(server, username, password, database, serialNumber) == true)
                {
                    break;
                }
                if (i == NUMBER_OF_RETRIES - 1)
                {
                    throw new ApplicationException(server);
                }
            }
        }

        private bool ExecuteBackupScript(string server, string username, string password, string database, string serialNumber)
        {
            var success = false;
            var error = string.Empty;

            database = database.Replace('/', '\\');

            // The location of cmd file is written hardcoded. TODO: use a different strategy?
            // See the Installer/Scripts location (Installer project) to retrieve the file
            // Note about batch file (remote_backup.cmd):
            //  - use the IP of server (default: 192.168.137.1)
            //  - fergrp_2012 /user:wmsadmin are the credential for accessing the server
            //  - use of xcopy command (it retrieves an error code, if it fails)
            string backupScript = @"F:\\database\\remote_backup.cmd";

            try
            {
                // Create remote_backup.cmd for execute script
                if (File.Exists(backupScript))
                {
                    File.Delete(backupScript);
                }

                // Create a new file
                using (StreamWriter sw = File.CreateText(backupScript))
                {
                    sw.WriteLine("net use z: /delete");
                    sw.WriteLine($"net use z: \\\\{server} {password} /user:{username} /persistent:no");
                    sw.WriteLine($"if not exist z:\\{serialNumber} (md z:\\{serialNumber})");
                    var DbName = database.Split('\\').Last();
                    sw.WriteLine($@"xcopy ""{database}"" z:\{serialNumber}\{DbName}* /y");
                    sw.WriteLine("net use z: /delete");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.ToString());
            }

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
                                    success = true;
                                    this.Logger.LogInformation($"Database Backup executed.");
                                    break;

                                case 4:
                                    error = "Database Backup error: invalid file name.";
                                    break;

                                case 1:
                                    error = "Database Backup error: file does not exist.";
                                    break;

                                case 2:
                                    error = "Database Backup error.";
                                    break;

                                case 5:
                                    error = "Database Backup error: disk write error.";
                                    break;

                                default:
                                    error = $"Error code {process.ExitCode}.";
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

            if (File.Exists(backupScript))
            {
                // Delete remote_backup.cmd
                File.Delete(backupScript);
            }

            if (!string.IsNullOrEmpty(error))
            {
                this.Logger.LogError(error);
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
