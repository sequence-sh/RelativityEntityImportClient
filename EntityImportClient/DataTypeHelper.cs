using System;
using ReductechEntityImport;

namespace Reductech.Sequence.Connectors.EntityImportClient
{

public static class DataTypeHelper
{
    public static Type Map(this StartImportCommand.Types.DataField.Types.DataType dataType)
    {
        switch (dataType)
        {
            case StartImportCommand.Types.DataField.Types.DataType.String: return typeof(string);
            case StartImportCommand.Types.DataField.Types.DataType.Int: return typeof(int);
            case StartImportCommand.Types.DataField.Types.DataType.Double: return typeof(double);
            case StartImportCommand.Types.DataField.Types.DataType.Bool: return typeof(bool);
            case StartImportCommand.Types.DataField.Types.DataType.Date: return typeof(DateTime);
            default: throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
        }
    }
}

}
