
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
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("###### Press ENTER to stop application #####");
                Console.ForegroundColor = default;


                var webApiTask = WebAPI.StartAsync();

                await Console.In.ReadLineAsync();

                WebAPI.Stop();
                await webApiTask;
            }
            catch (Exception e)
            {
                await Logger.Log(LogLevel.FatalError, e.Message);
            }
        }
    }
}