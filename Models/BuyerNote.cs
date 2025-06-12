namespace NotesAPI.Models;

public partial class BuyerNote  : AuditableEntity
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public int NoteId { get; set; }

    public string NoteTitle { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string SellFor { get; set; } = null!;

    public double SellPrice { get; set; }

    public string? PurchaseEmail { get; set; }

    public string? BuyerEmail { get; set; }

    public string? ApproveFlag { get; set; }

    public virtual User EmailNavigation { get; set; } = null!;

    public virtual Note Note { get; set; } = null!;
}
