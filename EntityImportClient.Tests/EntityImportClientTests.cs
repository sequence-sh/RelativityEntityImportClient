using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using kCura.Relativity.DataReaderClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ReductechEntityImport;

namespace Reductech.Sequence.Connectors.EntityImportClient.Tests
{

internal class FakeStreamReader<T> : IAsyncStreamReader<T>
{
    public readonly IEnumerator<T> _enumerator;

    public FakeStreamReader(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
    }

    /// <inheritdoc />
    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return _enumerator.MoveNext();
    }

    /// <inheritdoc />
    public T Current => _enumerator.Current;
}

[TestClass]
public class AsyncDataReaderTests
{
    [TestMethod]
    public void TestDataReading()
    {
        var columns = new[] { "StringColumn", "IntColumn" };
        var types   = new[] { typeof(string), typeof(int) };

        var objects = new List<ImportObject>()
        {
            new()
            {
                Values =
                {
                    new ImportObject.Types.FieldValue() { StringValue = "abc" },
                    new ImportObject.Types.FieldValue() { IntValue    = 123 },
                }
            },
            new()
            {
                Values =
                {
                    new ImportObject.Types.FieldValue() { StringValue = "def" },
                    new ImportObject.Types.FieldValue() { IntValue    = 456 },
                }
            }
        };

        var streamReader = new FakeStreamReader<ImportObject>(objects.GetEnumerator());

        var dataReader = new AsyncDataReader(columns, types, streamReader);

        var dataTable = new DataTable();

        dataTable.Columns.AddRange(
            columns.Zip(types, (s, type) => new DataColumn(s, type)).ToArray()
        );

        LoadAllRows(dataTable, dataReader);

        dataTable.Rows.Count.Should().Be(2);
    }

    private static void LoadAllRows(DataTable table, IDataReader dataReader)
    {
        while (dataReader.Read())
        {
            dataReader.FieldCount.Should().Be(table.Columns.Count);
            var objects = new List<object>();

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                var name   = dataReader.GetName(i);
                column.ColumnName.ToLowerInvariant().Should().Be(name);

                var value1 = dataReader[i];
                var value2 = dataReader[name];
                value1.Should().Be(value2);

                objects.Add(value1);
            }

            table.Rows.Add(objects.ToArray());
        }

        dataReader.Dispose();
    }
}

[TestClass]
public class HelperTests
{
    [DataTestMethod]
    public void TestSetSettings()
    {
        var settings = (Settings)Activator.CreateInstance(typeof(Settings), true);

        var command = new StartImportCommand()
        {
            WorkspaceArtifactId = 12345,
            ControlNumberField  = "ControlNum",
            FilePathField       = "File Path",
            FolderPathField     = "Folder Path",
            RelativityPassword  = "Pass",
            RelativityUsername  = "User",
            RelativityWebAPIUrl = "abcd"
        };

        JobHelpers.SetSettings(settings, command);

        settings.ApplicationName.Should().Be("Reductech Import");
        settings.CaseArtifactId.Should().Be(12345);
    }

    [TestMethod]
    public void SetJobMessages()
    {
        var mockRepo = new MockRepository(MockBehavior.Strict);

        var notifierMock  = mockRepo.Create<IImportNotifier>();
        var errorListener = new ErrorListener();

        notifierMock.SetupAdd(
            m => m.OnComplete += It.IsAny<IImportNotifier.OnCompleteEventHandler>()
        );

        notifierMock.SetupAdd(
            m => m.OnProcessProgress += It.IsAny<IImportNotifier.OnProcessProgressEventHandler>()
        );

        notifierMock.SetupAdd(
            m => m.OnProgress += It.IsAny<IImportNotifier.OnProgressEventHandler>()
        );

        notifierMock.SetupAdd(
            m => m.OnFatalException += It.IsAny<IImportNotifier.OnFatalExceptionEventHandler>()
        );

        JobHelpers.SetJobMessages(notifierMock.Object, errorListener);

        mockRepo.VerifyAll();
    }

    [DataTestMethod]
    [DataRow(StartImportCommand.Types.DataField.Types.DataType.String, typeof(string))]
    [DataRow(StartImportCommand.Types.DataField.Types.DataType.Int,    typeof(int))]
    [DataRow(StartImportCommand.Types.DataField.Types.DataType.Double, typeof(double))]
    [DataRow(StartImportCommand.Types.DataField.Types.DataType.Bool,   typeof(bool))]
    [DataRow(StartImportCommand.Types.DataField.Types.DataType.Date,   typeof(DateTime))]
    public void TestMap(
        StartImportCommand.Types.DataField.Types.DataType dataType,
        Type expectedType)
    {
        var actual = dataType.Map();

        actual.Should().Be(expectedType);
    }

    [TestMethod]
    public void TestFieldValueHelper()
    {
        Test(new ImportObject.Types.FieldValue() { StringValue = "foo" }, "foo");
        Test(new ImportObject.Types.FieldValue() { IntValue    = 123 },   123);
        Test(new ImportObject.Types.FieldValue() { DoubleValue = 12.3 },  12.3);
        Test(new ImportObject.Types.FieldValue() { BoolValue   = true },  true);

        Test(
            new ImportObject.Types.FieldValue() { DateValue = DateTime.Now.ToShortTimeString() },
            DateTime.Now.ToShortTimeString()
        );

        void Test(
            ImportObject.Types.FieldValue fieldValue,
            object expectedObject)
        {
            var actual = fieldValue.GetValue();

            actual.Should().Be(expectedObject);
        }
    }
}

[TestClass]
public class UnitTest1 { }

}
