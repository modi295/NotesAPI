namespace NotesAPI.DTO
{
 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
    using NotesAPI.Models;

    public class Notes
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [ForeignKey("Email")]
    public virtual User? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string NoteTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public byte[]? DisplayPicture { get; set; }
    public byte[]? NotesAttachment { get; set; }
    public byte[]? PreviewUpload { get; set; }

    [Required]
    [MaxLength(100)]
    public string NotesType { get; set; } = string.Empty;

    [Required]
    public int NumberOfPages { get; set; }

    [Required]
    public string NotesDescription { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? UniversityInformation { get; set; }

    [Required]
    [MaxLength(200)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CourseInformation { get; set; }

    [MaxLength(100)]
    public string? CourseCode { get; set; }

    [MaxLength(100)]
    public string? ProfessorLecturer { get; set; }

    [Required]
    [MaxLength(20)]
    public string SellFor { get; set; } = string.Empty;

    [Required]
    public float SellPrice { get; set; } = 0;

    [MaxLength(350)]
    public string? DisplayPictureP { get; set; }

    [MaxLength(350)]
    public string? NotesAttachmentP { get; set; }

    [MaxLength(350)]
    public string? PreviewUploadP { get; set; }

    [MaxLength(200)]
    public string? Remark { get; set; }

    [MaxLength(3)]
    public string? StatusFlag { get; set; }

    [MaxLength(3)]
    public string? PublishFlag { get; set; }
}

}