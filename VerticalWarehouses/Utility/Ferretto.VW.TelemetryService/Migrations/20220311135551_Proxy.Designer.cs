﻿// <auto-generated />
using System;
using Ferretto.VW.TelemetryService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ferretto.VW.TelemetryService.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20220311135551_Proxy")]
    partial class Proxy
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.19");

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.ErrorLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AdditionalText")
                        .HasColumnType("TEXT");

                    b.Property<int>("BayNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Code")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DetailCode")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InverterIndex")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MachineId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("OccurrenceDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("ResolutionDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MachineId");

                    b.ToTable("ErrorLogs");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.IOLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BayNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Input")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MachineId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Output")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MachineId");

                    b.ToTable("IOLogs");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.Machine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModelName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RawDatabaseContent")
                        .HasColumnType("BLOB");

                    b.Property<string>("SerialNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Machines");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.MissionLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Bay")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CellId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Destination")
                        .HasColumnType("TEXT");

                    b.Property<int>("Direction")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EjectLoadUnit")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("LoadUnitHeight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoadUnitId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MachineId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MissionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MissionType")
                        .HasColumnType("TEXT");

                    b.Property<int?>("NetWeight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Priority")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Stage")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<int>("Step")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StopReason")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.Property<int?>("WmsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("MachineId");

                    b.ToTable("MissionLogs");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.Proxy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordSalt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.Property<string>("User")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Proxys");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.ScreenShot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BayNumber")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Image")
                        .HasColumnType("BLOB");

                    b.Property<int>("MachineId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("ViewName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MachineId");

                    b.ToTable("ScreenShots");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.ServicingInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("InstallationDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsHandOver")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LastServiceDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("NextServiceDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("ServiceStatusId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.Property<int?>("TotalMissions")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ServicingInfos");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.ErrorLog", b =>
                {
                    b.HasOne("Ferretto.VW.TelemetryService.Data.Machine", "Machine")
                        .WithMany()
                        .HasForeignKey("MachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.IOLog", b =>
                {
                    b.HasOne("Ferretto.VW.TelemetryService.Data.Machine", "Machine")
                        .WithMany()
                        .HasForeignKey("MachineId");
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.MissionLog", b =>
                {
                    b.HasOne("Ferretto.VW.TelemetryService.Data.Machine", "Machine")
                        .WithMany()
                        .HasForeignKey("MachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Ferretto.VW.TelemetryService.Data.ScreenShot", b =>
                {
                    b.HasOne("Ferretto.VW.TelemetryService.Data.Machine", "Machine")
                        .WithMany()
                        .HasForeignKey("MachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
