Simple clean Archituture


1. Created the project architecture – Built a .NET 9 Clean Architecture solution in Visual Studio 2022 with API, Domain, Application and Infrastructure projects.

2. Connected SQL Server 2022 – Created SmartRAGCommerceDB and configured the database connection, entities, repositories and seed data.

3. Integrated Ollama – Connected the local Ollama server using qwen2.5-coder:7b for AI-powered question understanding and SQL generation.

4. Integrated Qdrant – Configured local vector storage with the SmartRAGCommerce_Knowledge collection and all-minilm:latest embeddings (384 dimensions).

5. Integrated SmartRAG 4.0.1 – Enabled document search and configured the SmartRAG dashboard at /smartrag.

6. Implemented analytics query processing – Created a service to classify questions and route them to document search, database analytics or hybrid search.

7. Added SQL security – Implemented SQL validation using ScriptDom, read-only database access, a 30-second timeout and a 500-row limit.

8. Implemented conversation memory – Added session-based conversation history so follow-up questions can use previous questions and results.

9. Created an analytics dashboard – Built analytics.html with a question input, results table, summary metrics and charts.

10. Performed initial testing – Document retrieval, database revenue queries and hybrid queries have worked. We are currently fixing and testing SQL generation for top-product queries and verifying follow-up questions.
