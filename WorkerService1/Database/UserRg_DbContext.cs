using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WorkerService1.Database
{
    public partial class UserRg_DbContext : DbContext
    {
        public UserRg_DbContext()
        {
        }

        public UserRg_DbContext(DbContextOptions<UserRg_DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblGender> TblGenders { get; set; } = null!;
        public virtual DbSet<TblSetEvent> TblSetEvents { get; set; } = null!;
        public virtual DbSet<TblUserRegsitration> TblUserRegsitrations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblGender>(entity =>
            {
                entity.HasKey(e => e.GenderId);

                entity.ToTable("tblGender");

                entity.Property(e => e.Gender).HasMaxLength(20);
            });

            modelBuilder.Entity<TblSetEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.ToTable("tblSetEvents");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Time).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TblSetEvents)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tblSetEvents_tblUserRegsitration");
            });

            modelBuilder.Entity<TblUserRegsitration>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("tblUserRegsitration");

                entity.Property(e => e.ChannelId).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
