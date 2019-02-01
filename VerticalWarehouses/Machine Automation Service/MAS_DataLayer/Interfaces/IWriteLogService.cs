using System;
using System.Collections.Generic;
using System.Text;

namespace MAS_DataLayer
{
    public interface IWriteLogService
    {
        void LogWriting(string logMessage);
    }
}
