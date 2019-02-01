using Microsoft.EntityFrameworkCore;

namespace MAS_DataLayer
{
   
    public class StatusLog
    {
        public int StatusLogId { get; set; }

        public string LogMessage { get; set; }
    }

}
