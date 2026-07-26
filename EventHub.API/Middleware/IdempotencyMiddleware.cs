using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Collections.Concurrent;

namespace EventHub.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cache)
    {
        if (context.Request.Method != HttpMethods.Post && context.Request.Method != HttpMethods.Put)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
        {
            await _next(context);
            return;
        }

        string cacheKey = $"idempotency_{key}";
        
        
        if (cache.Exists(cacheKey))
        {
            var cachedResponse = cache.Get<string>(cacheKey);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse!);
            return;
        }

        
        var semaphore = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        var originalBodyStream = context.Response.Body; 

        try
        {
            
            if (cache.Exists(cacheKey))
            {
                var cachedResponse = cache.Get<string>(cacheKey);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(cachedResponse!);
                return;
            }

            
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            
            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                cache.Set(cacheKey, responseBody, TimeSpan.FromMinutes(15));
            }

    
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            semaphore.Release();
        }
    }
}