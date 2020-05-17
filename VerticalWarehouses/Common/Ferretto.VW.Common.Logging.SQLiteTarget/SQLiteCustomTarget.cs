using System;
using System.Data.SQLite;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Ferretto.VW.Common.Logging.SQLiteTarget
{
    [Target("SQLite")]
    public sealed class SQLiteCustomTarget : TargetWithLayout
    {
        #region Fields

        private bool disposed;

        private SQLiteConnection logConnection;

        #endregion

        #region Properties

        [RequiredParameter]
        public string DbName { get; set; }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.logConnection?.Shutdown();
                this.logConnection?.Close();
                this.logConnection?.Dispose();
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (this.logConnection is null)
            {
                this.ConfigureDatabase();
            }

            // Insert a record
            var insertSql = @"INSERT INTO LogEntries (TimeStamp, Message, LoggerName, Level, Exception) values (@timestamp, @message, @loggername, @level, @exception)";
            using (var insertCom = new SQLiteCommand(insertSql, this.logConnection))
            {
                insertCom.Parameters.Add(new SQLiteParameter("@timestamp") { Value = DateTime.Now });
                insertCom.Parameters.Add(new SQLiteParameter("@loggername") { Value = logEvent.LoggerName });
                insertCom.Parameters.Add(new SQLiteParameter("@level") { Value = logEvent.Level });
                insertCom.Parameters.Add(new SQLiteParameter("@message") { Value = logEvent.Message });
                insertCom.Parameters.Add(new SQLiteParameter("@exception") { Value = logEvent.Exception });

                try
                {
                    insertCom.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // TODO fix db migration issue
                }
            }
        }

        private void ConfigureDatabase()
        {
            this.logConnection = new SQLiteConnection($"Data Source={this.DbName};Version=3");
            this.logConnection.Open();

            var sqlQuery = @"SELECT name FROM sqlite_master WHERE type='table' AND name='LogEntries'";
            object checkResult = null;
            try
            {
                using (var checkCommand = new SQLiteCommand(sqlQuery, this.logConnection))
                {
                    checkResult = checkCommand.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                this.logConnection.Close();
                this.logConnection = null;
            }

            if (checkResult == null)
            {
                var sqlCommand =
                    @"CREATE TABLE 'LogEntries' ( 'Exception' TEXT NULL, 'Level' TEXT NULL, 'LogEntryID' INTEGER NOT NULL CONSTRAINT 'PK_LogEntries' PRIMARY KEY AUTOINCREMENT, 'LoggerName' TEXT NULL, 'Message' TEXT NULL, 'TimeStamp' TEXT NOT NULL)";
                try
                {
                    using (var createTableCommand = new SQLiteCommand(sqlCommand, this.logConnection))
                    {
                        createTableCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    this.logConnection.Close();
                    this.logConnection = null;
                }
            }
        }

        #endregion
    }
}
