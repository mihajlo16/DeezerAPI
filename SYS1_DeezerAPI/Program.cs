
using SYS1_DeezerAPI.AppLogic;
using SYS1_DeezerAPI.Utils;

namespace SYS1_DeezerAPI
{
    public static class Program
    {
        public async static Task Main()
        {
            try
            {
                var _task = Task.Run(() => WebAPI.StartAsync());

                Console.WriteLine("###### Press any key to stop application! #####");
                await Console.In.ReadLineAsync();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.FatalError, e.Message);
            }
        }
    }
}