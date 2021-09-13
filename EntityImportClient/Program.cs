using System;
using Grpc.Core;
using ReductechEntityImport;

namespace Reductech.EDR.Connectors.EntityImportClient
{

public class Program
{
    const int Port = 30051;

    public static void Main(string[] args)
    {
        var server = new Server
        {
            Services =
            {
                Reductech_Entity_Import.BindService(new ReductechImportImplementation())
            },
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };

        server.Start();

        Console.ReadLine();
    }
}

}
