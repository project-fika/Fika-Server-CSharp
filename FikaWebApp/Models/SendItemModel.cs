namespace FikaWebApp.Models;

public record SendItemModel
{
    public string? ItemName { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public int Amount { get; set; } = 1;
    public string Message { get; set; } = string.Empty;
    public bool FoundInRaid { get; set; } = true;

    public bool UseDate { get; set; }
    public DateTime? Date { get; set; }
}