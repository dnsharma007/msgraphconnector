// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// <ArticleDbContextSnippet>
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Graph.Models;

namespace articlesFreshServiceConnector.Data
{

    public class ArticleDbContext : DbContext
    {
        public ArticleDbContext(DbContextOptions<ArticleDbContext> options)
            : base(options)
        {
        }
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<SolutionCategory>? SolutionCategory => Set<SolutionCategory>();
        public DbSet<SolutionFolder>? SolutionFolder => Set<SolutionFolder>();
        public DbSet<FolderArticle>? FolderArticles => Set<FolderArticle>();
        public void EnsureDatabase()
        {
            if (Database.EnsureCreated() || !Articles.Any())
            {
                // File was just created (or is empty),
                // seed with data from CSV file
                // var parts = CsvDataLoader.LoadPartsFromCsv("ArticleParts.csv");
                //Parts.AddRange(parts);

                //SaveChanges();
            }
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=Articles.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           /* modelBuilder.Entity<SolutionFolder>().Property(p => p.ManageByGroupIds)
     .HasConversion(
         v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
         v => JsonSerializer.Deserialize<Json>(v, JsonSerializerOptions.Default));*/

          modelBuilder.Entity<SolutionFolder>().Property(p => p.ApprovalSettings)
     .HasConversion(
         v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
         v => JsonSerializer.Deserialize<Json>(v, JsonSerializerOptions.Default));

            modelBuilder.Entity<SolutionFolder>()
            .HasOne(p => p.SolutionCategory)
            .WithMany(c => c.SolutionFolders)
            .HasForeignKey(p => p.CategoryId);
            //.HasNoKey(p=>p.ApprovalSettings);

 /* 
            modelBuilder.Entity<ApprovalSettings>()
            .HasOne(a => a.SolutionFolder)
            .WithOne(y => y.ApprovalSettings)
            .HasForeignKey<ApprovalSettings>(a => a.FolderId);*/

            modelBuilder.Entity<FolderArticle>()
                .HasOne(sp => sp.SolutionFolder)
                .WithMany(p => p.FolderArticles)
                .HasForeignKey(sp => sp.FolderId);
            // EF Core can't store lists, so add a converter for the Articles
            // property to serialize as a JSON string on save to DB
            modelBuilder.Entity<Article>()
                .Property(ap => ap.Attachments)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptions.Default)
                );
           modelBuilder.Entity<FolderArticle>()
                .Property(e => e.Keywords)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            modelBuilder.Entity<FolderArticle>()
        .Property<bool>("IsDeleted")
        .IsRequired()
        .HasDefaultValue(false);
        }
      
    }
    // </ArticleDbContextSnippet>
}