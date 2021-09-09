using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using kCura.Relativity.ImportAPI;
using ReductechEntityImport;

namespace Reductech.EDR.Connectors.EntityImportClient
{

class ReductechImportImplementation : Reductech_Entity_Import.Reductech_Entity_ImportBase
{
    private StartImportCommand _command = null;

    /// <inheritdoc />
    public override async Task<StartImportReply> StartImport(
        StartImportCommand request,
        ServerCallContext context)
    {
        await Task.CompletedTask;

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
        Debugger.Launch();

        if (_command is null)
            return new ImportDataReply() { Success = false, Message = "Import was not started" };

        var importApi = new ImportAPI(
            _command.RelativityUsername,
            _command.RelativityPassword,
            _command.RelativityWebAPIUrl
        );

        var job = importApi.NewNativeDocumentImportJob();

        JobHelpers.SetSettings(job.Settings, _command);
        JobHelpers.SetJobMessages(job);
        JobHelpers.SetExtraMessages(job);

        //const bool streamRows = true;

        //if (streamRows)
        {
            var dataReader = new AsyncDataReader(
                _command.DataFields.Select(x => x.Name).ToArray(),
                _command.DataFields.Select(x => x.DataType.Map()).ToArray(),
                requestStream
            );

            job.SourceData.SourceData = dataReader;
        }
        //else
        {
            //var dataTable = new DataTable();

            //dataTable.Columns.AddRange(
            //    _command.DataFields.Select(x => new DataColumn(x.Name, Map(x.DataType))).ToArray()
            //);

            //var streamList = await requestStream.ToListAsync();

            //foreach (var row in streamList)
            //{
            //    var values = row.Values.Select(x => x.GetValue()).ToArray();

            //    dataTable.Rows.Add(values);
            //}

            //job.SourceData.SourceData = dataTable.CreateDataReader();
        }

        // Wait for the job to complete.
        try
        {
            job.Execute();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new ImportDataReply() { Success = true, Message = "Success" };
    }
}

}
