
using System.Collections.Concurrent;

namespace SmartRAGCommerce.Application.Services.Analytics;

public sealed class AnalyticsConversationStore
{
    private const int MaxMessagesPerSession = 20;

    private readonly ConcurrentDictionary<string, SessionState> _sessions = new();

    public IReadOnlyList<AnalyticsConversationMessage> GetHistory(
        string sessionId)
    {
        var session = _sessions.GetOrAdd(
            sessionId,
            _ => new SessionState());

        lock (session.SyncRoot)
        {
            return session.Messages.ToList();
        }
    }

    public void AddExchange(
        string sessionId,
        string question,
        string answer,
        string? resultData = null)
    {
        var session = _sessions.GetOrAdd(
            sessionId,
            _ => new SessionState());

        lock (session.SyncRoot)
        {
            session.Messages.Add(
                new AnalyticsConversationMessage("user", question));

            var assistantContent = answer;

            if (!string.IsNullOrWhiteSpace(resultData))
            {
                assistantContent +=
                    "\nPREVIOUS SQL RESULT DATA:\n" + resultData;
            }

            session.Messages.Add(
                new AnalyticsConversationMessage(
                    "assistant",
                    assistantContent));

            if (session.Messages.Count > MaxMessagesPerSession)
            {
                session.Messages.RemoveRange(
                    0,
                    session.Messages.Count - MaxMessagesPerSession);
            }
        }
    }

    private sealed class SessionState
    {
        public object SyncRoot { get; } = new();

        public List<AnalyticsConversationMessage> Messages { get; } = new();
    }
}

public sealed record AnalyticsConversationMessage(
    string Role,
    string Content);
