using System.Text;
using Microsoft.EntityFrameworkCore;
using SmartRAGCommerce.Application.Interfaces.Analytics;
using SmartRAGCommerce.Infrastructure.Persistence.Context;

namespace SmartRAGCommerce.Infrastructure.Analytics;

public class DatabaseSchemaProvider : IDatabaseSchemaProvider
{
    private readonly ApplicationDbContext _context;

    public DatabaseSchemaProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<string> GetSchemaDescriptionAsync()
    {
        var schema = new StringBuilder();

        foreach (var entity in _context.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();

            if (tableName == null)
                continue;

            schema.AppendLine($"TABLE: {tableName}");
            schema.AppendLine("COLUMNS:");

            foreach (var property in entity.GetProperties())
            {
                var columnType = property.ClrType.Name;
                schema.AppendLine($"- {property.Name} ({columnType})");
            }

            var primaryKey = entity.FindPrimaryKey();

            if (primaryKey != null)
            {
                schema.AppendLine(
                    $"PRIMARY KEY: {string.Join(", ", primaryKey.Properties.Select(p => p.Name))}");
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var principalTable = foreignKey.PrincipalEntityType.GetTableName();

                schema.AppendLine(
                    $"FOREIGN KEY: {string.Join(", ", foreignKey.Properties.Select(p => p.Name))} " +
                    $"REFERENCES {principalTable} " +
                    $"({string.Join(", ", foreignKey.PrincipalKey.Properties.Select(p => p.Name))})");
            }

            schema.AppendLine();
        }

        return Task.FromResult(schema.ToString());
    }
}