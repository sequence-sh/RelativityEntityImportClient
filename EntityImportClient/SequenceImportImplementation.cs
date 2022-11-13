using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using kCura.Relativity.ImportAPI;
using SequenceEntityImport;

namespace Sequence.Connectors.EntityImportClient
{

class SequenceImportImplementation : Sequence_Entity_Import.Sequence_Entity_ImportBase
{
    private StartImportCommand _command = null;

    /// <inheritdoc />
    public override async Task<StartImportReply> StartImport(
        StartImportCommand request,
        ServerCallContext context)
    {
        await Task.CompletedTask;

        Console.WriteLine("Start Import Command Received");

        if (_command is null)
        {
            _command = request;

            return new StartImportReply() { Success = true, Message = "Success" };
        }

        return new StartImportReply() { Success = false, Message = "Command was already set" };
    }

    /// <inheritdoc />
    public override async Task<ImportDataReply> ImportData(
        IAsyncStreamReader<ImportObject> requestStream,
        ServerCallContext context)
    {
        //Debugger.Launch();

        if (_command is null)
            return new ImportDataReply() { Success = false, Message = "Import was not started" };

        ImportAPI importApi;

        try
        {
            importApi = new ImportAPI(
                _command.RelativityUsername,
                _command.RelativityPassword,
                _command.RelativityWebAPIUrl
            );
        }
        catch (Exception e)
        {
            return new ImportDataReply() { Success = false, Message = e.Message };
        }

        var job           = importApi.NewNativeDocumentImportJob();
        var errorListener = new ErrorListener();

        JobHelpers.SetSettings(job.Settings, _command);
        JobHelpers.SetJobMessages(job, errorListener);
        JobHelpers.SetExtraMessages(job, errorListener);

        //const bool streamRows = true;

        var dataReader = new AsyncDataReader(
            _command.DataFields.Select(x => x.Name).ToArray(),
            _command.DataFields.Select(x => x.DataType.Map()).ToArray(),
            requestStream
        );

        job.SourceData.SourceData = dataReader;

        // Wait for the job to complete.
        try
        {
            job.Execute();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ImportDataReply() { Success = false, Message = e.Message };
        }

        if (errorListener.IsError)
        {
            Console.WriteLine("Import Failed");
            return new ImportDataReply() { Success = false, Message = errorListener.Error };
        }

        Console.WriteLine("Entities Imported");
        return new ImportDataReply() { Success = true, Message = "Success" };
    }
}

public class ErrorListener
{
    public void OnError(string message)
    {
        _errors.Add(message);
    }

    public bool IsError => _errors.Any();

    public string Error => _errors.Any() ? _errors.First() : "";

    private readonly List<string> _errors = new List<string>();
}

}
