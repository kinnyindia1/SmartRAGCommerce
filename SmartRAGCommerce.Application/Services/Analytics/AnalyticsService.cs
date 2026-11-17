
using System.Text.Json;
using SmartRAGCommerce.Application.DTOs.Analytics;
using SmartRAGCommerce.Application.Interfaces.AI;
using SmartRAGCommerce.Application.Interfaces.Analytics;

namespace SmartRAGCommerce.Application.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IOllamaService _ollamaService;
    private readonly IDatabaseSchemaProvider _schemaProvider;
    private readonly IAnalyticsQueryExecutor _queryExecutor;
    private readonly ISmartRagQueryService _smartRagQueryService;
    private readonly AnalyticsConversationStore _conversationStore;

    public AnalyticsService(
        IOllamaService ollamaService,
        IDatabaseSchemaProvider schemaProvider,
        IAnalyticsQueryExecutor queryExecutor,
        ISmartRagQueryService smartRagQueryService,
        AnalyticsConversationStore conversationStore)
    {
        _ollamaService = ollamaService;
        _schemaProvider = schemaProvider;
        _queryExecutor = queryExecutor;
        _smartRagQueryService = smartRagQueryService;
        _conversationStore = conversationStore;
    }

    public async Task<AnalyticsQueryResponseDto> AskAsync(
        AnalyticsQueryRequestDto request)
    {
        var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
            ? Guid.NewGuid().ToString()
            : request.SessionId;

        var history = _conversationStore.GetHistory(sessionId);

        var conversationContext = string.Join(
            "\n\n",
            history.Select(x =>
                $"{x.Role.ToUpperInvariant()}: {x.Content}"));

        var contextualQuestion = string.IsNullOrWhiteSpace(conversationContext)
            ? request.Question
            : $"""
              PREVIOUS CONVERSATION:
              {conversationContext}

              CURRENT QUESTION:
              {request.Question}
              """;

        // 1. Classify question.
        var questionType = await ClassifyQuestionAsync(
            request.Question,
            conversationContext);

        Console.WriteLine(
            $"========== QUESTION ROUTE: {questionType} ==========");

        string documentContext = string.Empty;

        // 2. Retrieve documents for DOCUMENT and HYBRID.
        if (questionType is "DOCUMENT" or "HYBRID")
        {
            documentContext =
                await _smartRagQueryService.GetContextAsync(
                    contextualQuestion);
        }

        // 3. DOCUMENT route.
        if (questionType == "DOCUMENT")
        {
            var documentAnswer = string.IsNullOrWhiteSpace(documentContext)
                ? "I couldn't find a relevant answer in the indexed documents."
                : documentContext;

            _conversationStore.AddExchange(
                sessionId,
                request.Question,
                documentAnswer);

            return new AnalyticsQueryResponseDto
            {
                SessionId = sessionId,
                Question = request.Question,
                Answer = documentAnswer,
                Data = new(),
                RowCount = 0
            };
        }

        // 4. Get database schema.
        var schema = await _schemaProvider.GetSchemaDescriptionAsync();

        var prompt = $"""
        You are an expert Microsoft SQL Server analytics assistant.

        DATABASE SCHEMA:
        {schema}

        PREVIOUS CONVERSATION:
        {conversationContext}

        DOCUMENT CONTEXT:
        {documentContext}

        CURRENT QUESTION:
        {request.Question}

        CONVERSATION RULES:
        1. Resolve follow-up words such as it, its, them and those
           using the previous conversation.
        2. Use previous SQL result data to identify the exact
           product, customer or entity being discussed.
        3. If the previous result ranked products by revenue and
           the user asks for "its monthly revenue", use the
           highest-ranked product unless another product was selected.
        4. Retain previous entity filters in follow-up questions.

        SQL RULES:
        1. Generate exactly one valid SQL Server SELECT statement.
        2. Return raw SQL only, without markdown.
        3. Use only actual tables and columns in the schema.
        4. Use document context only to identify relevant entities.
        5. SQL Server is the source of truth for business metrics.
        6. Never generate INSERT, UPDATE, DELETE, DROP, ALTER or EXEC.
        7. Never generate SELECT INTO.
        8. Use appropriate JOIN conditions.
        9. Avoid SELECT *.
        10. Use TOP (100) for detail queries without a specified limit.
        11. Never use LIMIT or OFFSET.
        12. Use OrderItems.UnitPrice * OrderItems.Quantity
            for historical sales revenue.
        13. Include ALL order statuses by default.
        14. Add Orders.Status = 'Completed' only when the user
            explicitly requests completed orders or completed revenue.
        15. Do not invent database values.
        16. For product-specific revenue, filter by the correct product.
        17. For monthly product revenue, retain the product filter
            from the previous conversation.
        18. Do not remove existing relevant entity filters.
        19. Generate SQL Server (T-SQL) syntax only.
        20. For top N queries, use SELECT TOP (N) immediately after SELECT.
        21. NEVER append TOP N after ORDER BY.
        22. Do not use LIMIT.
        23. Every SQL query must end with a semicolon.
        24. Return only one complete SQL SELECT statement.
        25. Do not include Markdown code fences or explanations.

        Generate SQL now.
        """;

        // 5. Generate SQL.
        var sql = await _ollamaService.GenerateResponseAsync(prompt);

        sql = CleanSql(sql);

        Console.WriteLine("========== GENERATED SQL ==========");
        Console.WriteLine(sql);
        Console.WriteLine("===================================");

        // 6. Execute read-only SQL.
        var data = await _queryExecutor.ExecuteReadOnlyAsync(sql);

        var resultJson = JsonSerializer.Serialize(data);

        // 7. Build response without another Ollama call.
        string answer;

        if (data.Count == 0)
        {
            answer = "No matching database records were found.";
        }
        else if (questionType == "HYBRID")
        {
            answer = $"""
            Document information:
            {documentContext}

            Database results:
            Found {data.Count} matching record(s).
            See the structured results for the verified database values.
            """;
        }
        else
        {
            answer =
                $"Found {data.Count} matching record(s). " +
                "See the structured results for the database values.";
        }

        // 8. Save conversation and SQL results.
        _conversationStore.AddExchange(
            sessionId,
            request.Question,
            answer,
            resultJson);

        return new AnalyticsQueryResponseDto
        {
            SessionId = sessionId,
            Question = request.Question,
            Answer = answer,
            Data = data,
            RowCount = data.Count
        };
    }

    private async Task<string> ClassifyQuestionAsync(
        string question,
        string conversationContext)
    {
        var prompt = $"""
        Classify the user's question into exactly ONE category.

        DOCUMENT:
        Requires uploaded documents, manuals, policies,
        specifications or unstructured knowledge only.

        DATABASE:
        Requires SQL Server records, calculations, totals,
        counts, comparisons, customers, products, orders,
        payments or revenue only.

        HYBRID:
        Requires BOTH document knowledge and SQL Server records.

        PREVIOUS CONVERSATION:
        {conversationContext}

        CURRENT QUESTION:
        {question}

        Interpret follow-up questions using the previous conversation.

        Examples:

        What is the internal project code?
        DOCUMENT

        What is my total revenue?
        DATABASE

        Which products in my documentation generated the
        highest revenue?
        HYBRID

        What is the monthly revenue of that product?
        DATABASE

        Return exactly one word:
        DOCUMENT
        DATABASE
        or
        HYBRID
        """;

        var result = await _ollamaService.GenerateResponseAsync(prompt);

        var firstLine = result.Trim()
            .Replace("\"", "")
            .Replace("`", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.Trim().ToUpperInvariant();

        return firstLine switch
        {
            "DOCUMENT" => "DOCUMENT",
            "HYBRID" => "HYBRID",
            "DATABASE" => "DATABASE",
            _ => "DATABASE"
        };
    }

    private static string CleanSql(string sql)
    {
        sql = sql.Trim();

        if (sql.StartsWith("```"))
        {
            var firstNewLine = sql.IndexOf('\n');

            if (firstNewLine >= 0)
                sql = sql[(firstNewLine + 1)..];

            var lastFence = sql.LastIndexOf("```");

            if (lastFence >= 0)
                sql = sql[..lastFence];
        }

        return sql.Trim();
    }
}
