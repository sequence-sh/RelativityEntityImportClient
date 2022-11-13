using System;
using Grpc.Core;
using SequenceEntityImport;

namespace Sequence.Connectors.EntityImportClient
{

public class Program
{
    const int Port = 30051;

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Entity Import Client");

        var server = new Server
        {
            Services =
            {
                Sequence_Entity_Import.BindService(new SequenceImportImplementation())
            },
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };

        server.Start();

        Console.ReadLine();
    }
}

}
