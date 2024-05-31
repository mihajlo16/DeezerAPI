using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SYS1_DeezerAPI.Models.Dtos;
using SYS1_DeezerAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SYS1_DeezerAPI.Utils;

namespace SYS1_DeezerAPI.Services
{
    public static class DeezerClient
    {

        private static readonly string _apiUrl = "https://api.deezer.com/search";

        public async static Task<List<Track>> SearchTracks(TrackQueryParameters query)
        {
            using HttpClient httpClient = new();
            httpClient.BaseAddress = new Uri(_apiUrl);

            var queryDeezer = BuildQuery(query);
            var response = await httpClient.GetAsync(queryDeezer);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"DeezerAPI returned response code: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception($"Deezer API returned empty response.");

            Logger.Log(LogLevel.Trace, $"Deezer API successfuly returned response. Query: {queryDeezer}");

            var data = JsonConvert.DeserializeObject<Response>(content);

            return data?.Data ?? [];
        }

        private static string BuildQuery(TrackQueryParameters query)
        {
            var properties = query.GetType().GetProperties();
            var stringBuilder = new StringBuilder("?q=");

            foreach (var property in properties)
            {
                var value = property.GetValue(query);
                if (value != null)
                {
                    stringBuilder.Append(property.Name)
                                 .Append(":\"")
                                 .Append(value)
                                 .Append("\",");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
