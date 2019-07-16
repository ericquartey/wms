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

        private bool disposed = false;

        private SQLiteConnection logConnection;

        #endregion

        #region Constructors

        public SQLiteCustomTarget()
        {
        }

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
            if (this.logConnection == null)
            {
                this.ConfigureDatabase();
            }

            // Insert a record
            var InsertSql = @"INSERT INTO LogEntries (TimeStamp, Message, LoggerName, Level, Exception) values (@timestamp, @message, @loggername, @level, @exception)";
            var InsertCom = new SQLiteCommand(InsertSql, this.logConnection);
            InsertCom.Parameters.Add(new SQLiteParameter("@timestamp") { Value = DateTime.Now });
            InsertCom.Parameters.Add(new SQLiteParameter("@loggername") { Value = logEvent.LoggerName });
            InsertCom.Parameters.Add(new SQLiteParameter("@level") { Value = logEvent.Level });
            InsertCom.Parameters.Add(new SQLiteParameter("@message") { Value = logEvent.Message });
            InsertCom.Parameters.Add(new SQLiteParameter("@exception") { Value = logEvent.Exception });

            try
            {
                InsertCom.ExecuteNonQuery();
            }
            catch (Exception)
            {
                //TODO fix db migration issue
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
                var checkCommand = new SQLiteCommand(sqlQuery, this.logConnection);
                checkResult = checkCommand.ExecuteScalar();
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
                    var createTableCommand = new SQLiteCommand(sqlCommand, this.logConnection);
                    createTableCommand.ExecuteNonQuery();
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
