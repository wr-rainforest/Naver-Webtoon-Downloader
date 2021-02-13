using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database
{
    public class WebtoonDbContext : DbContext
    {
        public static string WebtoonDatabaseFilePath { get; set; }

        public DbSet<Webtoon> Webtoons { get; set; }

        public DbSet<Episode> Episodes { get; set; }

        public DbSet<Image> Images { get; set; }

        public WebtoonDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source = {WebtoonDatabaseFilePath}; Mode=ReadWriteCreate; Cache=Shared;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Webtoon>()
                .HasKey(w => w.ID);

            modelBuilder.Entity<Episode>()
                .HasKey(e => new { e.WebtoonID, e.No });

            modelBuilder.Entity<Image>()
                .HasKey(i => new { i.WebtoonID, i.EpisodeNo, i.Index });

            modelBuilder.Entity<Episode>()
                        .HasOne(e => e.Webtoon)
                        .WithMany(w => w.Episodes)
                        .HasForeignKey(e => e.WebtoonID);

            modelBuilder.Entity<Image>()
                        .HasOne(i => i.Webtoon)
                        .WithMany()
                        .HasForeignKey(i => i.WebtoonID);

            modelBuilder.Entity<Image>()
                        .HasOne(i => i.Episode)
                        .WithMany(e => e.Images)
                        .HasForeignKey(i => new { i.WebtoonID, i.EpisodeNo });

            modelBuilder.Entity<Episode>()
                        .HasIndex(e => e.WebtoonID);

            modelBuilder.Entity<Image>()
                        .HasIndex(i => i.WebtoonID);
            modelBuilder.Entity<Image>()
                        .HasIndex(i => i.EpisodeNo);
        }
    }
}
