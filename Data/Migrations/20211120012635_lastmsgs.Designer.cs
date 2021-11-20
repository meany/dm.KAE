﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using dm.KAE.Data;

namespace dm.KAE.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20211120012635_lastmsgs")]
    partial class lastmsgs
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("dm.KAE.Data.Models.Holder", b =>
                {
                    b.Property<int>("HolderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastBlockNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("HolderId");

                    b.HasIndex("Value");

                    b.ToTable("Holders");
                });

            modelBuilder.Entity("dm.KAE.Data.Models.LastMessage", b =>
                {
                    b.Property<int>("LastMessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<long>("MessageId")
                        .HasColumnType("bigint");

                    b.HasKey("LastMessageId");

                    b.HasIndex("ChatId");

                    b.ToTable("LastMessages");
                });

            modelBuilder.Entity("dm.KAE.Data.Models.Price", b =>
                {
                    b.Property<int>("PriceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Group")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("MarketCapUSD")
                        .HasColumnType("int");

                    b.Property<int>("MarketCapUSDChange")
                        .HasColumnType("int");

                    b.Property<decimal>("MarketCapUSDChangePct")
                        .HasColumnType("decimal(12,8)");

                    b.Property<decimal>("PriceBTC")
                        .HasColumnType("decimal(16,8)");

                    b.Property<int>("PriceBTCChange")
                        .HasColumnType("int");

                    b.Property<decimal>("PriceBTCChangePct")
                        .HasColumnType("decimal(12,8)");

                    b.Property<decimal>("PriceETH")
                        .HasColumnType("decimal(16,8)");

                    b.Property<int>("PriceETHChange")
                        .HasColumnType("int");

                    b.Property<decimal>("PriceETHChangePct")
                        .HasColumnType("decimal(12,8)");

                    b.Property<decimal>("PriceUSD")
                        .HasColumnType("decimal(11,6)");

                    b.Property<int>("PriceUSDChange")
                        .HasColumnType("int");

                    b.Property<decimal>("PriceUSDChangePct")
                        .HasColumnType("decimal(12,8)");

                    b.Property<int>("Source")
                        .HasColumnType("int");

                    b.Property<int>("VolumeUSD")
                        .HasColumnType("int");

                    b.Property<int>("VolumeUSDChange")
                        .HasColumnType("int");

                    b.Property<decimal>("VolumeUSDChangePct")
                        .HasColumnType("decimal(12,8)");

                    b.HasKey("PriceId");

                    b.HasIndex("Date");

                    b.HasIndex("Group");

                    b.ToTable("Prices");
                });

            modelBuilder.Entity("dm.KAE.Data.Models.Request", b =>
                {
                    b.Property<int>("RequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("Response")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("User")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RequestId");

                    b.HasIndex("Date");

                    b.HasIndex("Response", "Type");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("dm.KAE.Data.Models.Stat", b =>
                {
                    b.Property<int>("StatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Circulation")
                        .HasColumnType("decimal(25,18)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Group")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Supply")
                        .HasColumnType("decimal(25,18)");

                    b.Property<int>("Transactions")
                        .HasColumnType("int");

                    b.HasKey("StatId");

                    b.HasIndex("Date");

                    b.ToTable("Stats");
                });
#pragma warning restore 612, 618
        }
    }
}
