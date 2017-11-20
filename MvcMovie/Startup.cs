using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
// 引入这两个类
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            //"Data Source={your sql server host address};Initial Catalog=TestNetCoreEF;user id={your username};password={your password};"
            //var connection = @"Server=(localdb)\mssqllocaldb;Database=MvcMovie;Trusted_Connection=True;";
            var connection = @"Data Source=.;Database=MvcMovie;User ID=sa;Password=123456;";
            services.AddDbContext<MvcMovieContext>(options => options.UseSqlServer(connection));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseWebSockets();

            // 配置 webSocket 中间件
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<ChatWebSocketMiddleware>();

            // 接受WebSOCKET 请求
            // app.Use(async (context, next) =>
            // {
            //     if (context.Request.Path == "/ws")
            //     {
            //         if (context.WebSockets.IsWebSocketRequest)
            //         {
            //             WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            //             await Echo(context, webSocket);
                        
            //         }
            //         else
            //         {
            //             context.Response.StatusCode = 400;
            //         }
            //     }
            //     else
            //     {
            //         await next();
            //     }

            // });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        //发送接收消息
        // private async Task Echo(HttpContext context, WebSocket webSocket)
        //     {
        //     var buffer = new byte[1024 * 4];
        //     WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //     while (!result.CloseStatus.HasValue)
        //     {
        //         await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

        //         result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //     }
        //     await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        //     }
    }
}
