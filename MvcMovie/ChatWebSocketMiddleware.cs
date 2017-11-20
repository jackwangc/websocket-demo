using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MvcMovie
{
    public class ChatWebSocketMiddleware
    {
        //连接池
        private static ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        //处理 httprequest 请求的委托
        private readonly RequestDelegate _next;

        // 委托实例化
        public ChatWebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //Invoke 异步方法
        public async Task Invoke(HttpContext context)
        {
            // 保证请求合法性，委托调用方法
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            // 传播有关取消操作的通知
            CancellationToken ct = context.RequestAborted;
            // 将请求转化为 websocket 连接
            WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            // 生成 websocket连接的 guid(全局唯一标识符)
            var socketId = Guid.NewGuid().ToString();
            // 添加到 sockets 连接池中
            _sockets.TryAdd(socketId, currentSocket);
            
            while (true)
            {
                // 获取是否已请求取消此标记
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // 接受websocket 传入消息
                var response = await ReceiveStringAsync(currentSocket, ct);
                // 判断传入消息为空时 连接是否已经中断
                if(string.IsNullOrEmpty(response))
                {
                    if(currentSocket.State != WebSocketState.Open)
                    {
                        // break 直接跳出循环
                        break;
                    }

                    // 结束本次循环
                    continue;
                }

                foreach (var socket in _sockets)
                {
                    // socket 连接没有打开时 取消该项操作
                    if(socket.Value.State != WebSocketState.Open)
                    {
                        continue;
                    }
                    // 广播信息
                    await SendStringAsync(socket.Value, response, ct);
                }
            }

            // 从连接池移除关闭的 socket 连接
            WebSocket dummy;
            _sockets.TryRemove(socketId, out dummy);

            // 关闭连接
            await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            currentSocket.Dispose();
        }

        // 信息广播方法
        private static Task SendStringAsync(WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        // 信息接收方法
        private static async Task<string> ReceiveStringAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                // Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}