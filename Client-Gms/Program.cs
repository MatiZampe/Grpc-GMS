using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_Gms
{
    internal class Program
    {
        static async void Main(string[] args)
        {

            Channel channel = new Channel("localhost", 50051, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("The client connected successfully");
            });

            channel.ShutdownAsync().Wait();
            Console.ReadKey();

        }
    }
}
