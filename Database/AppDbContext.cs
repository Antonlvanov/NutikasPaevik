using Microsoft.EntityFrameworkCore;
using NutikasPaevik.Database;
using NutikasPaevik.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutikasPaevik.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Note> Notes { get; set; }
        public DbSet<Event> Events { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().ToTable("notes"); //??
            modelBuilder.Entity<Event>().ToTable("events");

            modelBuilder.Entity<Note>().Property(n => n.Title).IsRequired();
            modelBuilder.Entity<Note>().Property(n => n.Content).IsRequired();

            modelBuilder.Entity<Note>()
        .HasKey(n => n.Id);
            modelBuilder.Entity<Note>()
                .Property(n => n.SyncId)
                .IsRequired();
            modelBuilder.Entity<Note>()
                .HasIndex(n => n.SyncId)
                .IsUnique();

            modelBuilder.Entity<Event>().Property(n => n.Title).IsRequired();
            modelBuilder.Entity<Event>().Property(n => n.Description).IsRequired();

            modelBuilder.Entity<Event>()
        .HasKey(n => n.Id);
            modelBuilder.Entity<Event>()
                .Property(n => n.SyncId)
                .IsRequired();
            modelBuilder.Entity<Event>()
                .HasIndex(n => n.SyncId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
    //modelBuilder.Entity<Note>().HasData(
    //    new Note
    //    {
    //        Id = 1,
    //        UserID = 1,
    //        Title = "Aboba",
    //        Content = "Aboba Aboba",
    //        CreationTime = DateTime.Now,
    //        NoteColor = Enums.NoteColor.Yellow
    //    },
    //    new Note
    //    {
    //        Id = 2,
    //        UserID = 1,
    //        Title = "Super aboba",
    //        Content = "Aboba Aboba aksdjjkasdkans dnajdkakjsdnakj nsd kjnaskjdn akjnsd jkansjkdnakjsn dkjandkj",
    //        CreationTime = DateTime.Now,
    //        NoteColor = Enums.NoteColor.Red
    //    },
    //    new Note
    //    {
    //        Id = 3,
    //        UserID = 1,
    //        Title = "Aboba Aboba Aboba",
    //        Content = "Aboba aksdjjkasdkans dnajdkakjsdnakj nsd kjnaskjdn akjnsd jkansjkdnakjsn dkjandkj asäjdlskasdkandka ndklans ldnaks ndalns dkans",
    //        CreationTime = DateTime.Now,
    //        NoteColor = Enums.NoteColor.Blue
    //    },
    //    new Note
    //    {
    //        Id = 4,
    //        UserID = 1,
    //        Title = "Aboba",
    //        Content = "Aboba Aboba",
    //        CreationTime = DateTime.Now.AddDays(-2),
    //        NoteColor = Enums.NoteColor.Yellow
    //    },
    //    new Note
    //    {
    //        Id = 5,
    //        UserID = 1,
    //        Title = "Super aboba",
    //        Content = "Aboba Aboba aksdjjkasdkans dnajdkakjsdnakj nsd kjnaskjdn akjnsd jkansjkdnakjsn dkjandkj",
    //        CreationTime = DateTime.Now.AddDays(-1),
    //        NoteColor = Enums.NoteColor.Red
    //    }
    //);
}
