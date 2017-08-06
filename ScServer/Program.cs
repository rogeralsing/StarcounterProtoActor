using System.Linq;
using System.Threading.Tasks;
using Messages;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Remote;
using Starcounter;
using static Proto.Actor;
using ProtosReflection = Messages.ProtosReflection;

namespace ScServer
{
    internal class Program
    {
        private static void Main()
        {
            Log.SetLoggerFactory(new LoggerFactory()
                .AddDebug(LogLevel.Debug));

            //register the our known messages
            Serialization.RegisterFileDescriptor(ProtosReflection.Descriptor);

            //start the Proto.Actor remote server on 127.0.0.1:12000
            Remote.Start("127.0.0.1", 12000);

            //define an actor of type HelloActor
            var props = FromProducer(() => new HelloActor(Log.CreateLogger("HelloActor")))
                .WithDispatcher(new StarcounterDispatcher()); //Schedule this actor onto the Starcounter scheduler

            //create an instance of our definition, name the actor HelloActor, this is the name we can use to reach it remote
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
        private readonly ILogger _logger;

        public HelloActor(ILogger logger)
        {
            _logger = logger;
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    _logger.LogDebug("Hello Actor started");
                    break;
                case HelloRequest _:
                    Db.Transact(() =>
                    {
                        _logger.LogDebug($"Hello Actor got request from {context.Sender.ToShortString()}");

                        //get or create an instance of HelloState
                        var state = Db.SQL<HelloState>("SELECT s from ScServer.HelloState s").FirstOrDefault() ??
                                    new HelloState();

                        //increate hello count
                        state.HelloCount++;

                        //respond back to the sender of the current message
                        context.Respond(new HelloResponse
                        {
                            Message = $"You have said hello {state.HelloCount} times!"
                        });
                    });
                    break;
            }
            return Done;
        }
    }
}