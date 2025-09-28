using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStartup
{
    public static class IPCNotice
    {
        private const string IPCChannelName = "AutoStartup/Background/IPC";

        private static CancellationTokenSource cancellationTokenSource;

        private static NamedPipeServerStream pipeServer;

        public static event Action OnActivateRequested;

        // 发送激活信号到运行中的实例
        public static void SendActivationSignal()
        {
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", IPCChannelName, PipeDirection.InOut);
                // 尝试连接，超时5秒
                pipeClient.Connect(5000);

                // 发送激活消息
                string message = "ACTIVATE";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                pipeClient.Write(messageBytes, 0, messageBytes.Length);

                // 读取响应
                byte[] buffer = new byte[1024];
                int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch { }
        }

        // 启动管道服务器
        public static void StartPipeServer()
        {
            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    using (pipeServer = new NamedPipeServerStream(
                        IPCChannelName,
                        PipeDirection.InOut,
                        10,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous))
                    {
                        try
                        {
                            await pipeServer.WaitForConnectionAsync(cancellationTokenSource.Token);
                            await ProcessPipeConnection(pipeServer);
                        }
                        catch
                        { }
                    }
                }
            }, cancellationTokenSource.Token);
        }

        // 停止管道服务器
        public static void StopPipeServer()
        {
            cancellationTokenSource?.Cancel();
            pipeServer?.Dispose();
        }

        // 处理管道消息
        private static void HandlePipeMessage(string message)
        {
            switch (message)
            {
                case "ACTIVATE":
                    OnActivateRequested?.Invoke();
                    break;
            }
        }

        // 处理管道连接
        private static async Task ProcessPipeConnection(NamedPipeServerStream server)
        {
            try
            {
                // 读取消息
                byte[] buffer = new byte[1024];
                int bytesRead = await server.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    HandlePipeMessage(message);
                }

                // 发送响应
                string response = "ACK";
                byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
                await server.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"管道处理错误: {ex.Message}");
            }
        }
    }
}