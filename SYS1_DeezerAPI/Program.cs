

using SYS1_DeezerAPI.AppLogic;
using SYS1_DeezerAPI.Utils;

namespace SYS1_DeezerAPI
{
    public static class Program
    {
        public static void Main()
        {
            try
            {
                Thread apiThread = new(a => WebAPI.Start());
                apiThread.Start();

                Console.WriteLine("###### Press any key to stop application! #####");
                Console.Read();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.FatalError, e.Message);
            }
        }
    }
}