
using System.Diagnostics;

namespace SYS1_DeezereClient
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Starting...");

            int ok = 0, failed = 0, n = 1000;
            ParallelOptions options = new() { MaxDegreeOfParallelism = 10 };
            var query = $"?artist=eminem";


            Stopwatch stopwatch = new();
            stopwatch.Start();
            await Parallel.ForAsync(0, n, options, async (i, token) =>
            {
                HttpClient httpClient = new()
                {
                    BaseAddress = new Uri("http://localhost:5000/")
                };
                try
                {
                    var response = await httpClient.GetAsync(query, token);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"API returned response code: {response.StatusCode}");
                    }

                    var content = await response.Content.ReadAsStringAsync(token);

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        throw new Exception($"Deezer API returned empty response.");
                    }

                    Console.WriteLine($"Deezer API successfuly returned response. Query: {query}.");
                    ok++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}. Query: {query}.");
                    failed++;
                }
            });
            stopwatch.Stop();

            Console.WriteLine($"Requests sent: {n}, Query: {query}");
            Console.WriteLine($"Status: {ok} successful answers, {failed} failed.");
            Console.WriteLine($"Total time: {TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds)}");
            Console.WriteLine($"Average time per request: {stopwatch.ElapsedMilliseconds / (double)n}ms");
        }
    }
}