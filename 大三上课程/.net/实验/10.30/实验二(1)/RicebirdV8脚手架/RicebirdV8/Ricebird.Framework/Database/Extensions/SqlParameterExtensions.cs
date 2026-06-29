namespace Microsoft.Data.SqlClient;
public static class SqlParameterExtensions
{
    /// <summary>
    /// 该函数仅供调试用，正常情况下，绝对不要使用他！
    /// </summary>
    /// <param name="para"></param>
    /// <returns></returns>
    public static string ToDeclareSql(this SqlParameter para)
    {
        StringBuilder sqlBuilder = new StringBuilder();
        switch (para.SqlDbType)
        {
            case System.Data.SqlDbType.Bit:
                string sqlValue = para.SqlValue?.ToString() ?? string.Empty;
                sqlBuilder.AppendLine($"DECLARE {para.ParameterName} {para.SqlDbType.ToString().ToLower()};");
                sqlBuilder.AppendLine($"SET {para.ParameterName} = {(sqlValue.Equals("TRUE", StringComparison.CurrentCultureIgnoreCase) ? 1 : 0)};");
                break;
            case System.Data.SqlDbType.Binary:
            case System.Data.SqlDbType.Float:
            case System.Data.SqlDbType.BigInt:
            case System.Data.SqlDbType.Money:
            case System.Data.SqlDbType.Int:
            case System.Data.SqlDbType.Decimal:
            case System.Data.SqlDbType.Real:
            case System.Data.SqlDbType.SmallInt:
            case System.Data.SqlDbType.SmallMoney:
            case System.Data.SqlDbType.Timestamp:
            case System.Data.SqlDbType.TinyInt:
            case System.Data.SqlDbType.VarBinary:
            case System.Data.SqlDbType.Variant:
                sqlBuilder.AppendLine($"DECLARE {para.ParameterName} {para.SqlDbType.ToString().ToLower()};");
                sqlBuilder.AppendLine($"SET {para.ParameterName} = {para.SqlValue};");
                break;
            case System.Data.SqlDbType.VarChar:
            case System.Data.SqlDbType.NVarChar:
                sqlBuilder.AppendLine($"DECLARE {para.ParameterName} {para.SqlDbType.ToString().ToLower()}(MAX);");
                sqlBuilder.AppendLine($"SET {para.ParameterName} = '{para.SqlValue}';");
                break;
            case System.Data.SqlDbType.SmallDateTime:
            case System.Data.SqlDbType.DateTime:
            case System.Data.SqlDbType.Char:
            case System.Data.SqlDbType.NChar:
            case System.Data.SqlDbType.NText:
            case System.Data.SqlDbType.UniqueIdentifier:
            case System.Data.SqlDbType.Text:
            case System.Data.SqlDbType.Xml:
            case System.Data.SqlDbType.Udt:
            case System.Data.SqlDbType.Date:
            case System.Data.SqlDbType.Time:
            case System.Data.SqlDbType.DateTime2:
            case System.Data.SqlDbType.DateTimeOffset:
                sqlBuilder.AppendLine($"DECLARE {para.ParameterName} {para.SqlDbType.ToString().ToLower()};");
                sqlBuilder.AppendLine($"SET {para.ParameterName} = '{para.SqlValue}';");
                break;
            case System.Data.SqlDbType.Structured:
            case System.Data.SqlDbType.Image:
            default:
                break;
        }
        return sqlBuilder.ToString();
    }

    public static string ToDeclareSql(this IEnumerable<SqlParameter> parameters)
    {
        StringBuilder sqlBuilder = new StringBuilder();
        foreach (SqlParameter parameter in parameters)
        {
            sqlBuilder.Append(parameter.ToDeclareSql());
        }

        return sqlBuilder.ToString();
    }
}

