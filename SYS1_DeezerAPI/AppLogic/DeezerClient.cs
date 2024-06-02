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
using SYS1_DeezerAPI.AppLogic;
using System.Net.Http;

namespace SYS1_DeezerAPI.Services
{
    public static class DeezerClient
    {
        private static readonly string _deezerApiUrl = "https://api.deezer.com/search";

        private static readonly HttpClient _httpClient;

        static DeezerClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_deezerApiUrl)
            };
        }

        public async static Task<List<Track>> SearchTracks(TrackQueryParameters query, CancellationToken token)
        {
            var queryDeezer = BuildQuery(query);
            var response = await _httpClient.GetAsync(queryDeezer, token);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"DeezerAPI returned response code: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync(token);

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception($"Deezer API returned empty response.");

            LoggerAsync.Log(LogLevel.Trace, $"Deezer API successfuly returned response. Query: {queryDeezer}");

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
