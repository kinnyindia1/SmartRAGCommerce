
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.Extensions.Configuration;
using SmartRAGCommerce.Application.Interfaces.Analytics;

namespace SmartRAGCommerce.Infrastructure.Analytics;

public class SqlQueryExecutor : IAnalyticsQueryExecutor
{
    private const int CommandTimeoutSeconds = 30;
    private const int MaxRows = 500;

    private readonly string _connectionString;

    public SqlQueryExecutor(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AnalyticsConnection")
            ?? throw new InvalidOperationException(
                "AnalyticsConnection is not configured.");
    }

    public async Task<List<Dictionary<string, object?>>> ExecuteReadOnlyAsync(
        string sql)
    {
        ValidateSql(sql);

        var results = new List<Dictionary<string, object?>>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = sql;
        command.CommandTimeout = CommandTimeoutSeconds;
        command.CommandType = CommandType.Text;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(
                StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);

                // Prevent duplicate column names from overwriting values.
                if (row.ContainsKey(columnName))
                    columnName = $"{columnName}_{i}";

                row[columnName] = reader.IsDBNull(i)
                    ? null
                    : reader.GetValue(i);
            }

            results.Add(row);

            if (results.Count >= MaxRows)
                break;
        }

        return results;
    }

    private static void ValidateSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new InvalidOperationException(
                "SQL query cannot be empty.");
        }

        var parser = new TSql160Parser(true);

        using var reader = new StringReader(sql);

        var fragment = parser.Parse(
            reader,
            out IList<ParseError> errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Invalid SQL syntax.");
        }

        // Allow exactly one SQL batch containing exactly one SELECT.
        if (fragment is not TSqlScript script ||
            script.Batches.Count != 1 ||
            script.Batches[0].Statements.Count != 1 ||
            script.Batches[0].Statements[0] is not SelectStatement)
        {
            throw new InvalidOperationException(
                "Only a single SELECT statement is allowed.");
        }

        // Block SELECT INTO.
        var visitor = new ReadOnlySelectVisitor();

        fragment.Accept(visitor);

        if (visitor.HasSelectInto)
        {
            throw new InvalidOperationException(
                "SELECT INTO is not allowed.");
        }
    }

    private sealed class ReadOnlySelectVisitor : TSqlFragmentVisitor
    {
        public bool HasSelectInto { get; private set; }

        public override void ExplicitVisit(SelectStatement node)
        {
            if (node.Into != null)
            {
                HasSelectInto = true;
            }

            base.ExplicitVisit(node);
        }
    }
}
