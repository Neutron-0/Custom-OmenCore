using System;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OmenCore.Hardware;
using Xunit;

namespace OmenCoreApp.Tests.Hardware
{
    /// <summary>
    /// Regression coverage for the v3.8.2 hang fix: HardwareWorkerClient.SendRequestAsync
    /// previously reused the same NamedPipeClientStream across every request with no
    /// serialization and no recovery after a timed-out read. A slow worker response (or
    /// any concurrent callers) could leave a response message un-consumed in the pipe
    /// buffer, so the *next* request would read a stale/misrouted reply instead of its own
    /// — permanently desyncing the connection. That manifested in the field as repeated
    /// "temperature appears frozen" warnings followed by a full Application Hang
    /// (Event ID 1002, HangType=Cross-process) on OMEN 16-xd0xxx (ProductId 8BCD), which
    /// has worker-backed CPU temperature override enabled and so exercises this path on
    /// every monitoring cycle.
    /// </summary>
    public class HardwareWorkerClientPipeTests
    {
        private static (HardwareWorkerClient Client, FieldInfo PipeField, MethodInfo SendMethod) CreateClientWithReflectionAccess()
        {
            var client = new HardwareWorkerClient();
            var pipeField = typeof(HardwareWorkerClient).GetField("_pipeClient", BindingFlags.Instance | BindingFlags.NonPublic);
            var sendMethod = typeof(HardwareWorkerClient).GetMethod("SendRequestAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            pipeField.Should().NotBeNull();
            sendMethod.Should().NotBeNull();

            return (client, pipeField!, sendMethod!);
        }

        private static async Task<(NamedPipeServerStream Server, NamedPipeClientStream Client)> CreateConnectedTestPipeAsync()
        {
            var pipeName = "OmenCoreTest_" + Guid.NewGuid().ToString("N");
            var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            var clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            var connectTask = clientStream.ConnectAsync(2000);
            await server.WaitForConnectionAsync();
            await connectTask;

            return (server, clientStream);
        }

        [Fact]
        public async Task SendRequestAsync_DisposesPipe_WhenServerNeverResponds_SoNextCallReconnectsInsteadOfReadingStaleData()
        {
            var (server, clientStream) = await CreateConnectedTestPipeAsync();
            using var serverDisposable = server;

            var (client, pipeField, sendMethod) = CreateClientWithReflectionAccess();
            pipeField.SetValue(client, clientStream);

            // Server intentionally never reads/replies — simulates a worker that's too busy
            // (GC pause, driver call) to respond inside the client's request timeout.
            var resultTask = (Task<string>)sendMethod.Invoke(client, new object[] { "GET" })!;
            var result = await resultTask;

            result.Should().BeEmpty();
            pipeField.GetValue(client).Should().BeNull(
                "a timed-out read must tear the connection down rather than leave a pipe that may still receive a late, now-unmatched response");
        }

        [Fact]
        public async Task SendRequestAsync_SerializesConcurrentCalls_SoEachCallerGetsItsOwnMatchingResponse()
        {
            var (server, clientStream) = await CreateConnectedTestPipeAsync();
            using var serverDisposable = server;

            var (client, pipeField, sendMethod) = CreateClientWithReflectionAccess();
            pipeField.SetValue(client, clientStream);

            const int requestCount = 5;

            var serverTask = Task.Run(async () =>
            {
                var buffer = new byte[256];
                for (int i = 0; i < requestCount; i++)
                {
                    var bytesRead = await server.ReadAsync(buffer, 0, buffer.Length);
                    var req = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Simulate a worker that isn't instantaneous, to widen the window for
                    // any caller interleaving the previous (unserialized) code permitted.
                    await Task.Delay(30);

                    var replyBytes = Encoding.UTF8.GetBytes($"ECHO:{req}");
                    await server.WriteAsync(replyBytes, 0, replyBytes.Length);
                    await server.FlushAsync();
                }
            });

            var calls = Enumerable.Range(0, requestCount)
                .Select(i => (Task<string>)sendMethod.Invoke(client, new object[] { $"REQ{i}" })!)
                .ToArray();

            var results = await Task.WhenAll(calls);
            await serverTask;

            for (int i = 0; i < requestCount; i++)
            {
                results[i].Should().Be($"ECHO:REQ{i}", "each caller must receive the response to its own request, never another caller's");
            }
        }
    }
}
