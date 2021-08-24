using System;
using ReductechRelativityImport;

namespace ImportClient
{

public static class FieldValueHelper
{
    public static object GetValue(this ImportObject.Types.FieldValue fieldValue)
    {
        switch (fieldValue.TestOneofCase)
        {
            case ImportObject.Types.FieldValue.TestOneofOneofCase.StringValue:
                return fieldValue.StringValue;
            case ImportObject.Types.FieldValue.TestOneofOneofCase.IntValue:
                return fieldValue.IntValue;
            case ImportObject.Types.FieldValue.TestOneofOneofCase.DoubleValue:
                return fieldValue.DoubleValue;
            case ImportObject.Types.FieldValue.TestOneofOneofCase.BoolValue:
                return fieldValue.BoolValue;
            case ImportObject.Types.FieldValue.TestOneofOneofCase.DateValue:
                return fieldValue.DateValue;
            default: return DBNull.Value;
        }
    }
}

}
