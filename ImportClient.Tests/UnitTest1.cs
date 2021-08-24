using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using ReductechRelativityImport;

namespace ImportClient.Tests
{


internal class FakeStreamReader<T> : IAsyncStreamReader<T>
{
    public readonly IEnumerator<T> _enumerator;
    public FakeStreamReader(IEnumerator<T> enumerator) {
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
public class UnitTest1
{
}

}
