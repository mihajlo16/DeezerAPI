using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Utils
{
    public class LogMessage
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
