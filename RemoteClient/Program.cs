using System;
using Messages;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Remote;

namespace RemoteClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.SetLoggerFactory(new LoggerFactory()
                .AddConsole(LogLevel.Debug));

            Serialization.RegisterFileDescriptor(Messages.ProtosReflection.Descriptor);

            Remote.Start("127.0.0.1", 0);
            var pid = new PID("127.0.0.1:12000", "HelloActor");
            var res = pid.RequestAsync<HelloResponse>(new HelloRequest()).Result;
            Console.WriteLine(res.Message);
            Console.ReadLine();
        }
    }
}