﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SimpleObjectStore.Models;

#nullable disable

namespace SimpleObjectStore.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.9");

            modelBuilder.Entity("SimpleObjectStore.Models.AllowedHost", b =>
                {
                    b.Property<string>("Hostname")
                        .HasMaxLength(2048)
                        .HasColumnType("TEXT");

                    b.HasKey("Hostname");

                    b.ToTable("AllowedHosts");
                });

            modelBuilder.Entity("SimpleObjectStore.Models.ApiKey", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.Property<bool>("AccessTimeLimited")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("ValidUntil")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("SimpleObjectStore.Models.Bucket", b =>
                {
                    b.Property<string>("BucketId")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("DirectoryName")
                        .IsRequired()
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<DateTimeOffset>("LastAccess")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<bool>("Private")
                        .HasColumnType("INTEGER");

                    b.HasKey("BucketId");

                    b.HasIndex("DirectoryName")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Buckets");
                });

            modelBuilder.Entity("SimpleObjectStore.Models.BucketFile", b =>
                {
                    b.Property<string>("StorageFileId")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<long>("AccessCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BucketId")
                        .IsRequired()
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileSizeMB")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("LastAccess")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Private")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StoredFileName")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.HasKey("StorageFileId");

                    b.HasIndex("BucketId");

                    b.ToTable("BucketFiles");
                });

            modelBuilder.Entity("SimpleObjectStore.Models.BucketFile", b =>
                {
                    b.HasOne("SimpleObjectStore.Models.Bucket", "Bucket")
                        .WithMany("Files")
                        .HasForeignKey("BucketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bucket");
                });

            modelBuilder.Entity("SimpleObjectStore.Models.Bucket", b =>
                {
                    b.Navigation("Files");
                });
#pragma warning restore 612, 618
        }
    }
}
