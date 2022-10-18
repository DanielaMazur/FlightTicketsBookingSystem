using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace AuthService.Middleware
{
     public class MaxConcurrentRequestsMiddleware
     {
          private int _concurrentRequestsCount;

          private readonly RequestDelegate _next;
          private readonly MaxConcurrentRequestsOptions _options;

          public MaxConcurrentRequestsMiddleware(RequestDelegate next,
               IOptions<MaxConcurrentRequestsOptions> options)
          {
               _concurrentRequestsCount = 0;

               _next = next ?? throw new ArgumentNullException(nameof(next));
               _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
          }

          public async Task Invoke(HttpContext context)
          {
               if (CheckLimitExceeded())
               {
                    IHttpResponseFeature responseFeature = context.Features.Get<IHttpResponseFeature>();

                    responseFeature.StatusCode = StatusCodes.Status429TooManyRequests;
                    responseFeature.ReasonPhrase = "Concurrent request limit exceeded.";
               }
               else
               {
                    await _next(context);

                    Interlocked.Decrement(ref _concurrentRequestsCount);
               }
          }

          private bool CheckLimitExceeded()
          {
               bool limitExceeded;

               int initialConcurrentRequestsCount, incrementedConcurrentRequestsCount;
               do
               {
                    limitExceeded = true;

                    initialConcurrentRequestsCount = _concurrentRequestsCount;
                    if (initialConcurrentRequestsCount >= _options.Limit)
                    {
                         break;
                    }

                    limitExceeded = false;
                    incrementedConcurrentRequestsCount = initialConcurrentRequestsCount + 1;
               }
               while (initialConcurrentRequestsCount != Interlocked.CompareExchange(
                           ref _concurrentRequestsCount, incrementedConcurrentRequestsCount, initialConcurrentRequestsCount));

               return limitExceeded;
          }
     }
}
