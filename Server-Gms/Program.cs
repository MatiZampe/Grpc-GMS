﻿using Country;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Gms
{
    internal class Program

    {
        const int Port = 50051;
        static void Main(string[] args)
        {

            Server server = null;

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("GMS_DB_CONNECTION") ?? throw new Exception("GMS_DB_CONNECTION environment variable not set");

                server = new Server()
                {
                    Services = { CountryService.BindService(new CountryServiceImpl(connectionString)) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }// credentials) }
                };

                server.Start();
                Console.WriteLine("The server is listening on the port : " + Port);
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine("The server failed to start : " + e.Message);
                throw;
            }
            finally
            {
                if (server != null)
                    server.ShutdownAsync().Wait();
            }


        }
    }
}
