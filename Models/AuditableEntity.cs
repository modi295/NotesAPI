namespace NotesAPI.Models
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? AddedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}