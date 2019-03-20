using System;
using System.Data.SQLite;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Ferretto.VW.SQLiteTarget
{
    [Target("SQLite")]
    public sealed class SQLiteCustomTarget : TargetWithLayout
    {
        #region Fields

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

        protected override void Write(LogEventInfo logEvent)
        {
            if (this.logConnection == null)
            {
                this.logConnection = new SQLiteConnection($"Data Source={this.DbName};Version=3");
                this.logConnection.Open();
            }

            // Insert a record
            var InsertSql = @"INSERT INTO LogEntries (TimeStamp, Message, LoggerName, Level, Exception) values (@timestamp, @message, @loggername, @level, @exception)";
            var InsertCom = new SQLiteCommand(InsertSql, this.logConnection);
            InsertCom.Parameters.Add(new SQLiteParameter("@timestamp") { Value = DateTime.Now });
            InsertCom.Parameters.Add(new SQLiteParameter("@loggername") { Value = logEvent.LoggerName });
            InsertCom.Parameters.Add(new SQLiteParameter("@level") { Value = logEvent.Level });
            InsertCom.Parameters.Add(new SQLiteParameter("@message") { Value = logEvent.Message });
            InsertCom.Parameters.Add(new SQLiteParameter("@exception") { Value = logEvent.Exception });
            //InsertCom.ExecuteNonQuery();
        }

        #endregion
    }
}
