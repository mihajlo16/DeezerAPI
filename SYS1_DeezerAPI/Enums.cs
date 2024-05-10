using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI
{
    public enum LogLevel
    {
        Trace = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        FatalError = 4,
    }

    public enum StatusCode
    {
        Ok = 0,
        NotFound = 1,
        BadRequest = 2,
        InternalError = 3
    }
}
