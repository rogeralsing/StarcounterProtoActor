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
            Serialization.RegisterFileDescriptor(Messages.ProtosReflection.Descriptor);

            Log.SetLoggerFactory(new LoggerFactory()
                .AddConsole(LogLevel.Debug));

            Console.WriteLine("Starting");
            Remote.Start("127.0.0.1", 0);
            var pid = new PID("127.0.0.1:12000", "HelloActor");
            Console.WriteLine("Requesting");
            var res = pid.RequestAsync<HelloResponse>(new HelloRequest()).Result;
            Console.WriteLine("Got response");
            Console.WriteLine(res.Message);
            Console.ReadLine();
        }
    }
}