using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Messages;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Remote;
using Starcounter;
using static Proto.Actor;

namespace ScServer
{
    internal class Program
    {
        private static void Main()
        {
            Log.SetLoggerFactory(new LoggerFactory()
                .AddDebug(LogLevel.Debug));

            Serialization.RegisterFileDescriptor(Messages.ProtosReflection.Descriptor);


            Remote.Start("127.0.0.1", 12000);
            var props = FromProducer(() => new HelloActor());
            var helloPid = SpawnNamed(props, "HelloActor");

            //REST to Actor comunication:
            Handle.GET("/hello", () =>
            {
                //Blocking due to no async handlers in Starcounter
                var resp = helloPid.RequestAsync<HelloResponse>(new HelloRequest()).Result;
                return resp.Message;
            });
        }
    }

    [Database]
    public class HelloState
    {
        public int HelloCount { get; set; }
    }

    public class HelloActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    Trace.WriteLine("Hello Actor started");
                    break;
                case HelloRequest _:
                    return Scheduling.RunTask(() =>
                    {
                        Trace.WriteLine($"Hello Actor got request from {context.Sender.ToShortString()}");
                        Db.Transact(() =>
                        {
                            var state = Db.SQL<HelloState>("SELECT s from ScServer.HelloState s")
                                            .FirstOrDefault() ?? new HelloState();

                            state.HelloCount++;
                            context.Respond(new HelloResponse
                            {
                                Message = $"You have said hello {state.HelloCount} times!"
                            });
                        });
                    });
            }
            return Done;
        }
    }
}