namespace SmartRAGCommerce.Application.DTOs.SmartRAG;

public class DocumentUploadRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/plain";
    public string Content { get; set; } = string.Empty;
}