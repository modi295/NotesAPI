
namespace NotesAPI.Models;

public partial class DownloadNote  : AuditableEntity
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public int NoteId { get; set; }

    public string NoteTitle { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string SellFor { get; set; } = null!;

    public double SellPrice { get; set; }

    public string? PurchaseTypeFlag { get; set; }

    public string? PurchaseEmail { get; set; }

    public string? BuyerEmail { get; set; }

    public string? Rating { get; set; }

    public string? Comment { get; set; }

    public string? ReportRemark { get; set; }

    public virtual User EmailNavigation { get; set; } = null!;

    public virtual Note Note { get; set; } = null!;
}
