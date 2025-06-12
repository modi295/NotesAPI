
namespace NotesAPI.Models;

public partial class Note  : AuditableEntity
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string NoteTitle { get; set; } = null!;

    public string Category { get; set; } = null!;

    public byte[]? DisplayPicture { get; set; }

    public byte[]? NotesAttachment { get; set; }

    public string NotesType { get; set; } = null!;

    public int NumberOfPages { get; set; }

    public string NotesDescription { get; set; } = null!;

    public string? UniversityInformation { get; set; }

    public string Country { get; set; } = null!;

    public string? CourseInformation { get; set; }

    public string? CourseCode { get; set; }

    public string? ProfessorLecturer { get; set; }

    public string SellFor { get; set; } = null!;

    public double SellPrice { get; set; }

    public byte[]? PreviewUpload { get; set; }

    public string? StatusFlag { get; set; }

    public string? PublishFlag { get; set; }

    public string? DisplayPictureP { get; set; }

    public string? NotesAttachmentP { get; set; }

    public string? PreviewUploadP { get; set; }

    public string? Remark { get; set; }

    public virtual ICollection<BuyerNote> BuyerNotes { get; set; } = new List<BuyerNote>();

    public virtual ICollection<DownloadNote> DownloadNotes { get; set; } = new List<DownloadNote>();

    public virtual User EmailNavigation { get; set; } = null!;

    public virtual ICollection<SoldNote> SoldNotes { get; set; } = new List<SoldNote>();
}
