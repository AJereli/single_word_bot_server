using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace SigneWordBotAspCore
{
    
    
    public partial class SwDbContext : DbContext
    {
        
        public static readonly Microsoft.Extensions.Logging.LoggerFactory _myLoggerFactory = 
            new LoggerFactory(new[] { 
                new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() 
            });
        public SwDbContext()
        {
        }

        public SwDbContext(DbContextOptions<SwDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BasketAccessEnum> BasketAccessEnum { get; set; }
        public virtual DbSet<CredentialsModel> Credentials { get; set; }
        public virtual DbSet<Note> Note { get; set; }
        public virtual DbSet<PasswordsBasketModel> PasswordsBasket { get; set; }
        public virtual DbSet<UserModel> User { get; set; }
        public virtual DbSet<UserBasket> UserBasket { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(_myLoggerFactory);

            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=signle_word_db;User Id=username;Password=;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<BasketAccessEnum>(entity =>
            {
                entity.ToTable("basket_access_enum");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(512);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<CredentialsModel>(entity =>
            {
                entity.ToTable("credentials");

                entity.HasIndex(e => e.Login)
                    .HasName("idx_credentials_login");

                entity.HasIndex(e => e.Name)
                    .HasName("idx_credentials_name");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BasketPassId).HasColumnName("basket_pass_id");

                entity.Property(e => e.Login)
                    .IsRequired()
                    .HasColumnName("login")
                    .HasMaxLength(1024);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(1024);

                entity.Property(e => e.UnitPassword)
                    .IsRequired()
                    .HasColumnName("unit_password");

                entity.HasOne(d => d.BasketModelPass)
                    .WithMany(p => p.Credentials)
                    .HasForeignKey(d => d.BasketPassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_credentials_basket_pass");
            });

            modelBuilder.Entity<Note>(entity =>
            {
                entity.ToTable("note");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Details).HasColumnName("details");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.UserModel)
                    .WithMany(p => p.Note)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_note_basket_access_enum");
            });

            modelBuilder.Entity<PasswordsBasketModel>(entity =>
            {
                entity.ToTable("passwords_basket");

                entity.HasIndex(e => e.Name)
                    .HasName("idx_passwords_basket_name");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BasketPass)
                    .HasColumnName("basket_pass")
                    .HasMaxLength(1024);

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(512);
            });

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.Id)
                    .HasName("idx_table");

                entity.HasIndex(e => e.TgId)
                    .HasName("idx_tg_id")
                    .IsUnique();

                entity.HasIndex(e => e.TgUsername)
                    .HasName("idx_tg_username");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(256);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(512);

                entity.Property(e => e.SecondName)
                    .HasColumnName("second_name")
                    .HasMaxLength(256);

                entity.Property(e => e.TgId)
                    .HasColumnName("tg_id")
                    .ForNpgsqlHasComment("Uniq telegram ID of the user");

                entity.Property(e => e.TgUsername)
                    .HasColumnName("tg_username")
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<UserBasket>(entity =>
            {
                entity.ToTable("user_basket");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccessTypeId).HasColumnName("access_type_id");

                entity.Property(e => e.BasketId).HasColumnName("basket_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.AccessType)
                    .WithMany(p => p.UserBasket)
                    .HasForeignKey(d => d.AccessTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_basket_access_enum");

                entity.HasOne(d => d.BasketModel)
                    .WithMany(p => p.UserBasket)
                    .HasForeignKey(d => d.BasketId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_basket_credentials");

                entity.HasOne(d => d.UserModel)
                    .WithMany(p => p.UserBasket)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_basket_user");
            });
        }
    }
}
