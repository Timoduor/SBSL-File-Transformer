using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace SbslFileTransformer.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context, ILogger<RequestResponseLoggingMiddleware> logger, IConfiguration config)
        {
            bool logRequestResponse = config.GetValue<bool>("CommonSettings:LogRequestResponseDetails");
            bool logRequestBody = config.GetValue<bool>("CommonSettings:LogRequestBody");
            bool logResponseBody = config.GetValue<bool>("CommonSettings:LogResponseBody");

            if (logRequestResponse)
            {
                await LogRequest(context, logger, logRequestBody);
                await LogResponse(context, logger, logResponseBody);
            }
        }

        private async Task LogResponse(HttpContext context, ILogger<RequestResponseLoggingMiddleware> logger, bool logRequestResponseBody)
        {
            var originalBodyStream = context.Response.Body;
            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            //handle request before
            await _next(context);
            //handle response after

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            logger.LogInformation($"Http Response Information:{Environment.NewLine}" +
                                   $"Schema:{context.Request.Scheme} " + $"{Environment.NewLine}" +
                                   $"Host: {context.Request.Host} " + $"{Environment.NewLine}" +
                                   $"Path: {context.Request.Path} " + $"{Environment.NewLine}" +
                                   $"StatusCode: {context.Response.StatusCode} " + $"{Environment.NewLine}" +
                                   $"QueryString: {context.Request.QueryString} " + $"{Environment.NewLine}" + $"{Environment.NewLine}" +
                                   (logRequestResponseBody ? $"Response Body: {text}" : ""));
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task LogRequest(HttpContext context, ILogger<RequestResponseLoggingMiddleware> logger, bool logRequestResponseBody)
        {
            context.Request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            logger.LogInformation($"Http Request Information:{Environment.NewLine}" +
                                   $"Schema:{context.Request.Scheme} " + $"{Environment.NewLine}" +
                                   $"Host: {context.Request.Host} " + $"{Environment.NewLine}" +
                                   $"Path: {context.Request.Path} " + $"{Environment.NewLine}" +
                                   $"QueryString: {context.Request.QueryString} " + $"{Environment.NewLine}" + $"{Environment.NewLine}" +
                                   (logRequestResponseBody ? $"Request Body: {ReadStreamInChunks(requestStream)}" : ""));
            context.Request.Body.Position = 0;
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                    0,
                    readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);
            return textWriter.ToString();
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static void UseRequestResponseLoggingMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
