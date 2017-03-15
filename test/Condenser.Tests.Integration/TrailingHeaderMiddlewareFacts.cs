﻿using CondenserDotNet.Client;
using CondenserDotNet.Middleware.TrailingHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Condenser.Tests.Integration
{
    public class TrailingHeaderMiddlewareFacts
    {
        [Fact]
        public async Task HeaderIsAdded()
        {
            var port = ServiceManagerConfig.GetNextAvailablePort();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{port}")
                .UseStartup<Startup>()
                .Build();
            host.Start();

            try
            {
                var client = new HttpClient();
                var result = await client.GetAsync($"http://localhost:{port}");
                var content = await result.Content.ReadAsStringAsync();
                Assert.Equal("Hello", content);
                Assert.Equal("FunnyHeader", result.Headers.Trailer.First());
            }
            finally
            {
                host.Dispose();
            }
        }

        public class Startup
        {
            public void Configure(IApplicationBuilder app)
            {
                app.UseMiddleware<TrailingHeadersMiddleware>();
                app.Use(async (context, next) =>
                {
                    var trailingHeader = context.Features.Get<ITrailingHeadersFeature>();
                    var headerValue = "";
                    trailingHeader.RegisterHeader("FunnyHeader", () => headerValue);

                    await context.Response.WriteAsync("Hello");

                    headerValue = "DownUnder";
                    return;
                });
            }
        }
    }
}
