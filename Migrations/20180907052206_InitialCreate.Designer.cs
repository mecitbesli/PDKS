﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PDKS;

namespace PDKS.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20180907052206_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("PDKS.AdminAuthorization", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Authority")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("Customize")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("Requests")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("AdminAuthorizationSet");
                });

            modelBuilder.Entity("PDKS.StandartWorkHours", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BreakTime");

                    b.Property<DateTime>("EndHr");

                    b.Property<DateTime>("StartHr");

                    b.HasKey("Id");

                    b.ToTable("StandartWorkhoursSet");
                });

            modelBuilder.Entity("PDKS.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DaysOff")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(15);

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Password")
                        .IsRequired();

                    b.Property<int>("Role")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(2);

                    b.Property<int>("StandartWorkHoursId");

                    b.Property<string>("Username")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("StandartWorkHoursId");

                    b.ToTable("UserSet");
                });

            modelBuilder.Entity("PDKS.UserWorked", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Excuse");

                    b.Property<int>("ReqApproved")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(2);

                    b.Property<int>("Request");

                    b.Property<int>("UserId");

                    b.Property<int>("WorkedTime");

                    b.Property<DateTime>("date");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserWorkedSet");
                });

            modelBuilder.Entity("PDKS.AdminAuthorization", b =>
                {
                    b.HasOne("PDKS.User", "User")
                        .WithOne("AdminAuthorization")
                        .HasForeignKey("PDKS.AdminAuthorization", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PDKS.User", b =>
                {
                    b.HasOne("PDKS.StandartWorkHours", "StandartWorkHours")
                        .WithMany("Users")
                        .HasForeignKey("StandartWorkHoursId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PDKS.UserWorked", b =>
                {
                    b.HasOne("PDKS.User", "User")
                        .WithMany("UserWorkedDays")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
