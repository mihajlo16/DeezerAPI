using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SYS1_DeezerAPI.Models;
using SYS1_DeezerAPI.Models.Dtos;
using SYS1_DeezerAPI.Services;
using SYS1_DeezerAPI.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.AppLogic
{
    public static class WebAPI
    {
        private static readonly HttpListener _listener = new();
        private static readonly string _url = "http://localhost";
        private static readonly int _port = 5000;

        private static bool _active;

        static WebAPI()
        {
            _listener.Prefixes.Add($"{_url}:{_port}/");
        }

        public async static Task StartAsync()
        {
            _active = true;
            _listener.Start();

            Logger.Log(LogLevel.Info, $"WebAPI started listening at port {_port}");

            while (_active)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    HandleRequestAsync(context);
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    await Logger.Log(LogLevel.Info, "Listener was stopped. Press ENTER to exit.");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public static void Stop()
        {
            _active = false;
            _listener.Stop();
        }

        private async static Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var rawUrl = context.Request.RawUrl;
                Logger.Log(LogLevel.Info, $"Processing request: {rawUrl}");

                NameValueCollection queryParams = context.Request.QueryString;
                TrackQueryParameters trackParams = new(queryParams);

                if (Misc.AreAllPropertiesNull(trackParams))
                {
                    await ReturnResponseAsync(StatusCode.BadRequest, "Invalid query parameters!", context, rawUrl);
                    return;
                }
                string queryKey = trackParams.ToString().Replace(' ', '_').ToLower();

                var cacheResult = await TrackCache.GetOrCreateAsync(queryKey, entry => DeezerClient.SearchTracks(trackParams));

                if (cacheResult.Count == 0)
                {
                    await ReturnResponseAsync(StatusCode.NotFound, $"No tracks with query parameters: {queryKey}", context, queryKey);
                }
                else
                {
                    await ReturnResponseAsync(StatusCode.Ok, cacheResult, context, queryKey);
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.Message);
                await ReturnResponseAsync(StatusCode.InternalError, e.Message, context);
            }
        }

        private static async Task ReturnResponseAsync(StatusCode status, object content, HttpListenerContext context, string? query = null)
        {
            if (context == null) return;

            string jsonResponse;

            if (status == StatusCode.Ok)
            {
                context.Response.ContentType = "application/json";
                jsonResponse = JsonConvert.SerializeObject(content);
            }
            else
            {
                context.Response.ContentType = "text/plain";
                jsonResponse = (string)content;
            }

            var response = Encoding.UTF8.GetBytes(jsonResponse).AsMemory();
            context.Response.StatusCode = (int)status;
            context.Response.StatusDescription = status.ToString();
            context.Response.ContentLength64 = response.Length;

            await context.Response.OutputStream.WriteAsync(response, CancellationToken.None);

            Logger.Log(LogLevel.Info, $"Returned {status} response to client. (query: {query})");
            context.Response.OutputStream.Close();
        }
    }

}
