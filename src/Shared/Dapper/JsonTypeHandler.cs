using System.Data;
using System.Text.Json;
using Dapper;

namespace Shared.Dapper;

public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = value == null
            ? DBNull.Value
            : JsonSerializer.Serialize(value);
    }

    public override T? Parse(object value)
    {
        if (value is DBNull)
        {
            return default;
        }

        string? jsonToString = value as string;

        if (string.IsNullOrEmpty(jsonToString))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(jsonToString);
    }
}