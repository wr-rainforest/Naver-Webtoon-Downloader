using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database.Model
{
    public class WebtoonContext : DbContext
    {
        public DbSet<Webtoon> Webtoons { get; set; }

        public DbSet<Episode> Episodes { get; set; }

        public DbSet<Image> Images { get; set; }

        public WebtoonContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source = AppData\\webtoon.sqlite; Mode=ReadWriteCreate; Cache=Shared;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Episode>()
                        .HasOne(e => e.Webtoon)
                        .WithMany(w => w.Episodes)
                        .HasForeignKey(e => e.TitleId);

            modelBuilder.Entity<Image>()
                        .HasOne(i => i.Webtoon)
                        .WithMany()
                        .HasForeignKey(i => i.TitleId);

            modelBuilder.Entity<Image>()
                        .HasOne(i => i.Episode)
                        .WithMany(e => e.Images)
                        .HasForeignKey(i => i.EpisodeNo);
        }
    }
}
