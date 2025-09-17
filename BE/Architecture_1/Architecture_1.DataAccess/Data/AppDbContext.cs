using System;
using System.Collections.Generic;
using Architecture_1.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AssigneeFacilityMajorAssignment> AssigneeFacilityMajorAssignments { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<FacilityItem> FacilityItems { get; set; }

    public virtual DbSet<FacilityItemAssignment> FacilityItemAssignments { get; set; }

    public virtual DbSet<FacilityMajor> FacilityMajors { get; set; }

    public virtual DbSet<FacilityMajorType> FacilityMajorTypes { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<JobType> JobTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportType> ReportTypes { get; set; }

    public virtual DbSet<RequestStatus> RequestStatuses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceAvailability> ServiceAvailabilities { get; set; }

    public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<TaskRequest> TaskRequests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=HAOHA\\SQLEXPRESS;Database=Architecture_1_DB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3213E83F299777D1");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Email, "UQ__Account__AB6E61647BAEDA8E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName).HasColumnName("fullName");
            entity.Property(e => e.IsDeactivated).HasColumnName("isDeactivated");
            entity.Property(e => e.JobTypeId).HasColumnName("jobTypeId");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("roleId");

            entity.HasOne(d => d.JobType).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account__jobType__06CD04F7");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account__roleId__05D8E0BE");
        });

        modelBuilder.Entity<AssigneeFacilityMajorAssignment>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.FacilityMajorId }).HasName("PK__Assignee__8D45E5ABF20EB3C4");

            entity.ToTable("AssigneeFacilityMajorAssignment");

            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.FacilityMajorId).HasColumnName("facilityMajorId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsHead).HasColumnName("isHead");
            entity.Property(e => e.WorkDescription).HasColumnName("workDescription");

            entity.HasOne(d => d.Account).WithMany(p => p.AssigneeFacilityMajorAssignments)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AssigneeF__accou__07C12930");

            entity.HasOne(d => d.FacilityMajor).WithMany(p => p.AssigneeFacilityMajorAssignments)
                .HasForeignKey(d => d.FacilityMajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AssigneeF__facil__08B54D69");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B669134BC");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.ToTable("Chat");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.From).HasColumnName("from");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.SenderName)
                .HasColumnType("text")
                .HasColumnName("senderName");
            entity.Property(e => e.To).HasColumnName("to");

            entity.HasOne(d => d.FromNavigation).WithMany(p => p.ChatFromNavigations)
                .HasForeignKey(d => d.From)
                .HasConstraintName("FK_Chat_Accounts");

            entity.HasOne(d => d.ToNavigation).WithMany(p => p.ChatToNavigations)
                .HasForeignKey(d => d.To)
                .HasConstraintName("FK_Chat_Accounts1");
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facility__3213E83FCDD66FD7");

            entity.ToTable("Facility");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsDeactivated).HasColumnName("isDeactivated");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<FacilityItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facility__3213E83F6655D534");

            entity.ToTable("FacilityItem", tb => tb.HasTrigger("trg_UpdateUpdatedAt_FacilityItem"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<FacilityItemAssignment>(entity =>
        {
            entity.HasKey(e => new { e.FacilityItemId, e.FacilityMajorId }).HasName("PK__Facility__16B4D1FEF6375315");

            entity.ToTable("FacilityItemAssignment");

            entity.Property(e => e.FacilityItemId).HasColumnName("facilityItemId");
            entity.Property(e => e.FacilityMajorId).HasColumnName("facilityMajorId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.ItemCount).HasColumnName("itemCount");

            entity.HasOne(d => d.FacilityItem).WithMany(p => p.FacilityItemAssignments)
                .HasForeignKey(d => d.FacilityItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FacilityI__facil__09A971A2");

            entity.HasOne(d => d.FacilityMajor).WithMany(p => p.FacilityItemAssignments)
                .HasForeignKey(d => d.FacilityMajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FacilityI__facil__0A9D95DB");
        });

        modelBuilder.Entity<FacilityMajor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facility__3213E83F3303B373");

            entity.ToTable("FacilityMajor");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CloseScheduleDate).HasColumnName("closeScheduleDate");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FacilityId).HasColumnName("facilityId");
            entity.Property(e => e.FacilityMajorTypeId).HasColumnName("facilityMajorTypeId");
            entity.Property(e => e.IsDeactivated).HasColumnName("isDeactivated");
            entity.Property(e => e.IsOpen).HasColumnName("isOpen");
            entity.Property(e => e.MainDescription).HasColumnName("mainDescription");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.OpenScheduleDate).HasColumnName("openScheduleDate");
            entity.Property(e => e.WorkShiftsDescription).HasColumnName("workShiftsDescription");

            entity.HasOne(d => d.Facility).WithMany(p => p.FacilityMajors)
                .HasForeignKey(d => d.FacilityId)
                .HasConstraintName("FK__FacilityM__facil__0C85DE4D");

            entity.HasOne(d => d.FacilityMajorType).WithMany(p => p.FacilityMajors)
                .HasForeignKey(d => d.FacilityMajorTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FacilityM__facil__0B91BA14");
        });

        modelBuilder.Entity<FacilityMajorType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facility__3213E83FF60DF66D");

            entity.ToTable("FacilityMajorType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3213E83FD2B9D833");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FacilityMajorId).HasColumnName("facilityMajorId");
            entity.Property(e => e.IsDeactivated).HasColumnName("isDeactivated");
            entity.Property(e => e.Rate).HasColumnName("rate");

            entity.HasOne(d => d.Account).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__accoun__0D7A0286");

            entity.HasOne(d => d.FacilityMajor).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.FacilityMajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__facili__0E6E26BF");
        });

        modelBuilder.Entity<JobType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobType__3213E83F5A212130");

            entity.ToTable("JobType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED9B767271");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Products__Catego__2DE6D218");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Report__3213E83FB0787CEB");

            entity.ToTable("Report");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FacilityMajorId).HasColumnName("facilityMajorId");
            entity.Property(e => e.IsResolved).HasColumnName("isResolved");
            entity.Property(e => e.ReportTypeId).HasColumnName("reportTypeId");

            entity.HasOne(d => d.Account).WithMany(p => p.Reports)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Report__accountI__0F624AF8");

            entity.HasOne(d => d.FacilityMajor).WithMany(p => p.Reports)
                .HasForeignKey(d => d.FacilityMajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Report__facility__10566F31");

            entity.HasOne(d => d.ReportType).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ReportTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Report__reportTy__114A936A");
        });

        modelBuilder.Entity<ReportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReportTy__3213E83F60900A83");

            entity.ToTable("ReportType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RequestStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RequestS__3213E83FAD35F8CC");

            entity.ToTable("RequestStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3213E83FB4521A35");

            entity.ToTable("Role");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Service__3213E83FE282D9AB");

            entity.ToTable("Service");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CloseScheduleDate).HasColumnName("closeScheduleDate");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FacilityMajorId).HasColumnName("facilityMajorId");
            entity.Property(e => e.IsDeactivated).HasColumnName("isDeactivated");
            entity.Property(e => e.IsInitRequestDescriptionRequired).HasColumnName("isInitRequestDescriptionRequired");
            entity.Property(e => e.IsOpen).HasColumnName("isOpen");
            entity.Property(e => e.MainDescription).HasColumnName("mainDescription");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.OpenScheduleDate).HasColumnName("openScheduleDate");
            entity.Property(e => e.RequestInitHintDescription).HasColumnName("requestInitHintDescription");
            entity.Property(e => e.ServiceTypeId).HasColumnName("serviceTypeId");
            entity.Property(e => e.WorkShiftsDescription).HasColumnName("workShiftsDescription");

            entity.HasOne(d => d.FacilityMajor).WithMany(p => p.Services)
                .HasForeignKey(d => d.FacilityMajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Service__facilit__123EB7A3");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.Services)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Service__service__1332DBDC");
        });

        modelBuilder.Entity<ServiceAvailability>(entity =>
        {
            entity.HasKey(e => new { e.ServiceId, e.DayOfWeek, e.StartRequestableTime, e.EndRequestableTime }).HasName("PK__ServiceA__4638B5ABD55BF615");

            entity.ToTable("ServiceAvailability", tb => tb.HasTrigger("ValidateTimeOverlap"));

            entity.Property(e => e.ServiceId).HasColumnName("serviceId");
            entity.Property(e => e.DayOfWeek).HasColumnName("dayOfWeek");
            entity.Property(e => e.StartRequestableTime).HasColumnName("startRequestableTime");
            entity.Property(e => e.EndRequestableTime).HasColumnName("endRequestableTime");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceAvailabilities)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceAv__servi__14270015");
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceR__3213E83F5662BAE7");

            entity.ToTable("ServiceRequest", tb => tb.HasTrigger("trg_UpdateUpdatedAt_ServiceRequest"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedAssigneeId).HasColumnName("assignedAssigneeId");
            entity.Property(e => e.CancelReason).HasColumnName("cancelReason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DateRequest).HasColumnName("dateRequest");
            entity.Property(e => e.IsCancelAutomatically).HasColumnName("isCancelAutomatically");
            entity.Property(e => e.IsSeen)
                .HasDefaultValue(true)
                .HasColumnName("isSeen");
            entity.Property(e => e.ProgressNote).HasColumnName("progressNote");
            entity.Property(e => e.RequestInitDescription).HasColumnName("requestInitDescription");
            entity.Property(e => e.RequestResultDescription).HasColumnName("requestResultDescription");
            entity.Property(e => e.RequestStatusId).HasColumnName("requestStatusId");
            entity.Property(e => e.RequesterId).HasColumnName("requesterId");
            entity.Property(e => e.ServiceId).HasColumnName("serviceId");
            entity.Property(e => e.TimeRequest).HasColumnName("timeRequest");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.AssignedAssignee).WithMany(p => p.ServiceRequestAssignedAssignees)
                .HasForeignKey(d => d.AssignedAssigneeId)
                .HasConstraintName("FK__ServiceRe__assig__0D7A0286");

            entity.HasOne(d => d.RequestStatus).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.RequestStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__reque__0C85DE4D");

            entity.HasOne(d => d.Requester).WithMany(p => p.ServiceRequestRequesters)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__reque__0B91BA14");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__servi__0A9D95DB");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceT__3213E83FA2E49505");

            entity.ToTable("ServiceType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TaskRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskRequ__3213E83FE2D6C42A");

            entity.ToTable("TaskRequest", tb => tb.HasTrigger("trg_UpdateUpdatedAt_TaskRequest"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CancelReason).HasColumnName("cancelReason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FacilityMajorId).HasColumnName("facilityMajorId");
            entity.Property(e => e.RequestStatusId).HasColumnName("requestStatusId");
            entity.Property(e => e.RequesterId).HasColumnName("requesterId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.FacilityMajor).WithMany(p => p.TaskRequests)
                .HasForeignKey(d => d.FacilityMajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskReque__facil__19DFD96B");

            entity.HasOne(d => d.RequestStatus).WithMany(p => p.TaskRequests)
                .HasForeignKey(d => d.RequestStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskReque__reque__1AD3FDA4");

            entity.HasOne(d => d.Requester).WithMany(p => p.TaskRequests)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskReque__reque__18EBB532");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
