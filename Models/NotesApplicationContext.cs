using Microsoft.EntityFrameworkCore;

namespace NotesAPI.Models;

public partial class NotesApplicationContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public NotesApplicationContext()
    {
    }

    public NotesApplicationContext(DbContextOptions<NotesApplicationContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        var currentUser = _httpContextAccessor?.HttpContext?.User?.FindFirst("fullName")?.Value;
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.Entity is not AuditableEntity auditableEntity || auditableEntity == null) continue;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.AddedBy = currentUser;

                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = currentUser;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = currentUser;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    public virtual DbSet<BuyerNote> BuyerNotes { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<DownloadNote> DownloadNotes { get; set; }

    public virtual DbSet<Lookup> Lookups { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<SequelizeMetum> SequelizeMeta { get; set; }

    public virtual DbSet<SoldNote> SoldNotes { get; set; }

    public virtual DbSet<Support> Supports { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<Student> Students { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuyerNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BuyerNotes_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.ApproveFlag)
                .HasMaxLength(1)
                .HasColumnName("approveFlag");
            entity.Property(e => e.BuyerEmail)
                .HasMaxLength(100)
                .HasColumnName("buyerEmail");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.NoteId).HasColumnName("noteId");
            entity.Property(e => e.NoteTitle)
                .HasMaxLength(100)
                .HasColumnName("noteTitle");
            entity.Property(e => e.PurchaseEmail)
                .HasMaxLength(100)
                .HasColumnName("purchaseEmail");
            entity.Property(e => e.SellFor)
                .HasMaxLength(20)
                .HasColumnName("sellFor");
            entity.Property(e => e.SellPrice)
                .HasDefaultValueSql("'0'::double precision")
                .HasColumnName("sellPrice");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");

            entity.HasOne(d => d.EmailNavigation).WithMany(p => p.BuyerNotes)
                .HasPrincipalKey(p => p.Email)
                .HasForeignKey(d => d.Email)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BuyerNotes_email_fkey");

            entity.HasOne(d => d.Note).WithMany(p => p.BuyerNotes)
                .HasForeignKey(d => d.NoteId)
                .HasConstraintName("fk_buyernotes_noteId");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Contacts_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .HasColumnName("fullName");
            entity.Property(e => e.Subject)
                .HasMaxLength(100)
                .HasColumnName("subject");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");
        });

        modelBuilder.Entity<DownloadNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DownloadNotes_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.BuyerEmail)
                .HasMaxLength(100)
                .HasColumnName("buyerEmail");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.Comment)
                .HasMaxLength(40)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.NoteId).HasColumnName("noteId");
            entity.Property(e => e.NoteTitle)
                .HasMaxLength(100)
                .HasColumnName("noteTitle");
            entity.Property(e => e.PurchaseEmail)
                .HasMaxLength(100)
                .HasColumnName("purchaseEmail");
            entity.Property(e => e.PurchaseTypeFlag).HasMaxLength(5);
            entity.Property(e => e.Rating)
                .HasMaxLength(1)
                .HasColumnName("rating");
            entity.Property(e => e.ReportRemark).HasMaxLength(60);
            entity.Property(e => e.SellFor)
                .HasMaxLength(20)
                .HasColumnName("sellFor");
            entity.Property(e => e.SellPrice)
                .HasDefaultValueSql("'0'::double precision")
                .HasColumnName("sellPrice");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");

            entity.HasOne(d => d.EmailNavigation).WithMany(p => p.DownloadNotes)
                .HasPrincipalKey(p => p.Email)
                .HasForeignKey(d => d.Email)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DownloadNotes_email_fkey");

            entity.HasOne(d => d.Note).WithMany(p => p.DownloadNotes)
                .HasForeignKey(d => d.NoteId)
                .HasConstraintName("fk_downloadnotes_noteId");
        });

        modelBuilder.Entity<Lookup>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("Lookups_pkey");

            entity.Property(e => e.TypeId)
                .HasMaxLength(6)
                .HasColumnName("typeId");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.TypeCode)
                .HasMaxLength(6)
                .HasColumnName("typeCode");
            entity.Property(e => e.TypeName)
                .HasMaxLength(40)
                .HasColumnName("typeName");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Notes_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.Country)
                .HasMaxLength(200)
                .HasColumnName("country");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(100)
                .HasColumnName("courseCode");
            entity.Property(e => e.CourseInformation)
                .HasMaxLength(100)
                .HasColumnName("courseInformation");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.DisplayPicture).HasColumnName("displayPicture");
            entity.Property(e => e.DisplayPictureP)
                .HasMaxLength(350)
                .HasColumnName("displayPictureP");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.NoteTitle)
                .HasMaxLength(100)
                .HasColumnName("noteTitle");
            entity.Property(e => e.NotesAttachment).HasColumnName("notesAttachment");
            entity.Property(e => e.NotesAttachmentP)
                .HasMaxLength(350)
                .HasColumnName("notesAttachmentP");
            entity.Property(e => e.NotesDescription).HasColumnName("notesDescription");
            entity.Property(e => e.NotesType)
                .HasMaxLength(100)
                .HasColumnName("notesType");
            entity.Property(e => e.NumberOfPages).HasColumnName("numberOfPages");
            entity.Property(e => e.PreviewUpload).HasColumnName("previewUpload");
            entity.Property(e => e.PreviewUploadP)
                .HasMaxLength(350)
                .HasColumnName("previewUploadP");
            entity.Property(e => e.ProfessorLecturer)
                .HasMaxLength(100)
                .HasColumnName("professorLecturer");
            entity.Property(e => e.PublishFlag)
                .HasMaxLength(3)
                .HasColumnName("publishFlag");
            entity.Property(e => e.Remark)
                .HasMaxLength(200)
                .HasColumnName("remark");
            entity.Property(e => e.SellFor)
                .HasMaxLength(20)
                .HasColumnName("sellFor");
            entity.Property(e => e.SellPrice)
                .HasDefaultValueSql("'0'::double precision")
                .HasColumnName("sellPrice");
            entity.Property(e => e.StatusFlag)
                .HasMaxLength(3)
                .HasColumnName("statusFlag");
            entity.Property(e => e.UniversityInformation)
                .HasMaxLength(200)
                .HasColumnName("universityInformation");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");

            entity.HasOne(d => d.EmailNavigation).WithMany(p => p.Notes)
                .HasPrincipalKey(p => p.Email)
                .HasForeignKey(d => d.Email)
                .HasConstraintName("fk_notes_user_email");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OTPs_pkey");

            entity.ToTable("OTPs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(70)
                .HasColumnName("email");
            entity.Property(e => e.ExpiredAt).HasColumnName("expiredAt");
            entity.Property(e => e.Otp1)
                .HasMaxLength(50)
                .HasColumnName("otp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<SequelizeMetum>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("SequelizeMeta_pkey");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SoldNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SoldNotes_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.BuyerEmail)
                .HasMaxLength(100)
                .HasColumnName("buyerEmail");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.NoteId).HasColumnName("noteId");
            entity.Property(e => e.NoteTitle)
                .HasMaxLength(100)
                .HasColumnName("noteTitle");
            entity.Property(e => e.PurchaseEmail)
                .HasMaxLength(100)
                .HasColumnName("purchaseEmail");
            entity.Property(e => e.SellFor)
                .HasMaxLength(20)
                .HasColumnName("sellFor");
            entity.Property(e => e.SellPrice)
                .HasDefaultValueSql("'0'::double precision")
                .HasColumnName("sellPrice");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");

            entity.HasOne(d => d.EmailNavigation).WithMany(p => p.SoldNotes)
                .HasPrincipalKey(p => p.Email)
                .HasForeignKey(d => d.Email)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SoldNotes_email_fkey");

            entity.HasOne(d => d.Note).WithMany(p => p.SoldNotes)
                .HasForeignKey(d => d.NoteId)
                .HasConstraintName("fk_soldnotes_noteId");
        });

        modelBuilder.Entity<Support>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Supports_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.EmailAddress)
                .HasMaxLength(50)
                .HasColumnName("emailAddress");
            entity.Property(e => e.FacebookUrl)
                .HasMaxLength(120)
                .HasColumnName("facebookUrl");
            entity.Property(e => e.LinkedinUrl)
                .HasMaxLength(120)
                .HasColumnName("linkedinUrl");
            entity.Property(e => e.NoteImage)
                .HasMaxLength(350)
                .HasColumnName("noteImage");
            entity.Property(e => e.ProfilePicture)
                .HasMaxLength(350)
                .HasColumnName("profilePicture");
            entity.Property(e => e.SupportEmail)
                .HasMaxLength(60)
                .HasColumnName("supportEmail");
            entity.Property(e => e.SupportPhone)
                .HasMaxLength(10)
                .HasColumnName("supportPhone");
            entity.Property(e => e.TwitterUrl)
                .HasMaxLength(120)
                .HasColumnName("twitterUrl");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active)
                .HasMaxLength(1)
                .HasColumnName("active");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("addedBy");
            entity.Property(e => e.Address1)
                .HasMaxLength(100)
                .HasColumnName("address1");
            entity.Property(e => e.Address2)
                .HasMaxLength(100)
                .HasColumnName("address2");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.College)
                .HasMaxLength(100)
                .HasColumnName("college");
            entity.Property(e => e.Country)
                .HasMaxLength(30)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("firstName");
            entity.Property(e => e.Gender)
                .HasMaxLength(8)
                .HasColumnName("gender");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("lastName");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.PhoneNumberCode)
                .HasMaxLength(5)
                .HasColumnName("phoneNumberCode");
            entity.Property(e => e.ProfilePicture).HasColumnName("profilePicture");
            entity.Property(e => e.Remark)
                .HasMaxLength(200)
                .HasColumnName("remark");
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .HasDefaultValueSql("'User'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .HasColumnName("state");
            entity.Property(e => e.University)
                .HasMaxLength(100)
                .HasColumnName("university");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updatedBy");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(50)
                .HasColumnName("zipCode");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
