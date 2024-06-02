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
using System.Net.Http;
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
        private static readonly SemaphoreSlim _semaphore = new(Environment.ProcessorCount);

        private static readonly CancellationTokenSource _cts = new();
        private static Task _apiTask = null!;

        static WebAPI()
        {
            _listener.Prefixes.Add($"{_url}:{_port}/");
        }

        public static void Start()
        {
            try
            {
                _listener.Start();
                LoggerAsync.Log(LogLevel.Info, $"WebAPI started listening at port {_port}");

                _apiTask = Task.Run(() => Listen(), _cts.Token);
            }
            catch (HttpListenerException) when (_cts.Token.IsCancellationRequested)
            {
                LoggerAsync.Log(LogLevel.Info, "Listener was stopped. Press ENTER to exit.");
                _cts.Cancel();
            }
            catch (Exception ex)
            {
                LoggerAsync.Log(LogLevel.Error, $"Unexpected error: {ex.Message}");
            }

        }

        private static async Task Listen()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context), _cts.Token);
            }
        }

        public async static Task StopAsync()
        {
            LoggerAsync.Dispose();
            _cts.Cancel();
            _listener.Stop();
            await _apiTask;
        }

        private async static Task HandleRequestAsync(HttpListenerContext context)
        {
            await _semaphore.WaitAsync(_cts.Token);
            try
            {
                var rawUrl = context.Request.RawUrl;
                LoggerAsync.Log(LogLevel.Info, $"Processing request: {rawUrl}");

                NameValueCollection queryParams = context.Request.QueryString;
                TrackQueryParameters trackParams = new(queryParams);

                if (Misc.AreAllPropertiesNull(trackParams))
                {
                    await ReturnResponseAsync(StatusCode.BadRequest, "Invalid query parameters!", context, rawUrl);
                    return;
                }
                string queryKey = trackParams.ToString().Replace(' ', '_').ToLower();

                var cacheResult = await TrackCache.GetOrCreateAsync(queryKey, entry => DeezerClient.SearchTracks(trackParams, _cts.Token));

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
                LoggerAsync.Log(LogLevel.Error, e.Message);
                await ReturnResponseAsync(StatusCode.InternalError, e.Message, context);
            }
            finally
            {
                _semaphore.Release();
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

            await context.Response.OutputStream.WriteAsync(response, _cts.Token);

            LoggerAsync.Log(LogLevel.Info, $"Returned {status} response to client. (query: {query})");
            context.Response.OutputStream.Close();
        }
    }
}
