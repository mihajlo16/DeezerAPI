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
                var context = await _listener.GetContextAsync();
                _=HandleRequestAsync(context);
            }     
        }

        private async static Task HandleRequestAsync(object? state)
        {
            HttpListenerContext? context = null;
            try
            {
                context = (HttpListenerContext)state!;

                var rawUrl = context.Request.RawUrl;
                Logger.Log(LogLevel.Info, $"Processing request: {rawUrl}");

                NameValueCollection queryParams = context.Request.QueryString;
                TrackQueryParameters trackParams = new(queryParams);

                if (Misc.AreAllPropertiesNull(trackParams))
                    await ReturnResponseAsync(StatusCode.BadRequest, "Invalid query parameters!", context, rawUrl);
                else
                {
                    string queryKey = trackParams.ToString().Replace(' ', '_').ToLower();

                   var cacheResult = await TrackCache.GetOrCreateAsync(queryKey, entry => DeezerClient.SearchTracks(trackParams));

                    //if (cacheResult != null)
                    //{
                    //    results = cacheResult;
                    //    Logger.Log(LogLevel.Trace, $"Data found in cache. (Query: {queryKey})");
                    //}
                    //else
                    //{
                    //    results = DeezerClient.SearchTracks(trackParams);
                    //    TrackCache.WriteToCache(queryKey, results);
                    //    Logger.Log(LogLevel.Trace, $"Writing data to cache. (Query: {queryKey})");
                    //}

                    if (cacheResult.Count == 0) await ReturnResponseAsync(StatusCode.NotFound, $"No tracks with query parameters: ", context, queryKey);
                    else await ReturnResponseAsync(StatusCode.Ok, cacheResult, context, queryKey);
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.Message);
                await ReturnResponseAsync(StatusCode.InternalError, e.Message, context);
            }

        }

        private async static Task ReturnResponseAsync(StatusCode status, object content, HttpListenerContext? context, string? query = null)
        {
            if (context != null)
            {
                try
                {
                    switch (status)
                    {
                        case StatusCode.Ok:
                            var tracks = (List<Track>)content;
                            string jsonResponse = JsonConvert.SerializeObject(tracks);
                            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                            context.Response.ContentType = "application/json";
                            context.Response.ContentLength64 = buffer.Length;
                            context.Response.StatusCode = 200;
                            context.Response.StatusDescription = "OK";
                            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            context.Response.OutputStream.Close();

                            Logger.Log(LogLevel.Info, $"Returned {tracks.Count} tracks to client. (query: {query})");
                            break;
                        case StatusCode.NotFound:
                            context.Response.StatusCode = 404;
                            context.Response.StatusDescription = "Not Found";

                            await TextResponseAsync((string)content + query, context);

                            Logger.Log(LogLevel.Info, $"Returned NotFound response to client. (query: {query})");
                            break;
                        case StatusCode.BadRequest:
                            context.Response.StatusCode = 400;
                            context.Response.StatusDescription = "Bad Request";

                            Logger.Log(LogLevel.Info, $"Returned BadRequest response to client. (request: {query})");
                            await TextResponseAsync((string)content, context);

                            break;
                        case StatusCode.InternalError:
                            context.Response.StatusCode = 500;
                            context.Response.StatusDescription = "Internal Server Error";

                            await TextResponseAsync((string)content, context);

                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message);
                }
            }
        }

        private async static Task TextResponseAsync(string message, HttpListenerContext? context)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            context!.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}
