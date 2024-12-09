﻿// <auto-generated />
using System;
using Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("Domain.Buildings.Building", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_by_user_id");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_date_time_utc");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("INTEGER")
                        .HasColumnName("is_default");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<int>("NumberOfFloors")
                        .HasColumnType("INTEGER")
                        .HasColumnName("number_of_floors");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("TEXT")
                        .HasColumnName("updated_by_user_id");

                    b.Property<DateTime?>("UpdatedDateTimeUtc")
                        .HasColumnType("TEXT")
                        .HasColumnName("updated_date_time_utc");

                    b.HasKey("Id")
                        .HasName("pk_buildings");

                    b.HasIndex("CreatedByUserId")
                        .HasDatabaseName("ix_buildings_created_by_user_id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_buildings_name");

                    b.HasIndex("UpdatedByUserId")
                        .HasDatabaseName("ix_buildings_updated_by_user_id");

                    b.ToTable("buildings", "dbo");

                    b.HasData(
                        new
                        {
                            Id = new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                            CreatedByUserId = new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                            CreatedDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            IsDefault = true,
                            Name = "Joe's Building",
                            NumberOfFloors = 10
                        });
                });

            modelBuilder.Entity("Domain.Elevators.Elevator", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("TEXT")
                        .HasColumnName("building_id");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_by_user_id");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_date_time_utc");

                    b.Property<int>("CurrentFloor")
                        .HasColumnType("INTEGER")
                        .HasColumnName("current_floor");

                    b.Property<int>("DestinationFloor")
                        .HasColumnType("INTEGER")
                        .HasColumnName("destination_floor");

                    b.Property<string>("DestinationFloors")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("destination_floors");

                    b.Property<int>("DoorStatus")
                        .HasColumnType("INTEGER")
                        .HasColumnName("door_status");

                    b.Property<string>("ElevatorDirection")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("elevator_direction");

                    b.Property<string>("ElevatorStatus")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("elevator_status");

                    b.Property<string>("ElevatorType")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("elevator_type");

                    b.Property<int>("FloorsPerSecond")
                        .HasColumnType("INTEGER")
                        .HasColumnName("floors_per_second");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER")
                        .HasColumnName("number");

                    b.Property<int>("PersonCapacity")
                        .HasColumnType("INTEGER")
                        .HasColumnName("person_capacity");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("TEXT")
                        .HasColumnName("updated_by_user_id");

                    b.Property<DateTime?>("UpdatedDateTimeUtc")
                        .HasColumnType("TEXT")
                        .HasColumnName("updated_date_time_utc");

                    b.HasKey("Id")
                        .HasName("pk_elevators");

                    b.HasIndex("CreatedByUserId")
                        .HasDatabaseName("ix_elevators_created_by_user_id");

                    b.HasIndex("UpdatedByUserId")
                        .HasDatabaseName("ix_elevators_updated_by_user_id");

                    b.HasIndex("BuildingId", "Number")
                        .IsUnique()
                        .HasDatabaseName("ix_elevators_building_id_number");

                    b.ToTable("elevators", "dbo");

                    b.HasData(
                        new
                        {
                            Id = new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                            BuildingId = new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                            CreatedByUserId = new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                            CreatedDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            CurrentFloor = 1,
                            DestinationFloor = 0,
                            DestinationFloors = "",
                            DoorStatus = 0,
                            ElevatorDirection = "None",
                            ElevatorStatus = "Active",
                            ElevatorType = "HighSpeed",
                            FloorsPerSecond = 5,
                            Number = 1,
                            PersonCapacity = 10
                        },
                        new
                        {
                            Id = new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                            BuildingId = new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                            CreatedByUserId = new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                            CreatedDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            CurrentFloor = 1,
                            DestinationFloor = 0,
                            DestinationFloors = "",
                            DoorStatus = 0,
                            ElevatorDirection = "None",
                            ElevatorStatus = "Active",
                            ElevatorType = "Passenger",
                            FloorsPerSecond = 1,
                            Number = 2,
                            PersonCapacity = 10
                        },
                        new
                        {
                            Id = new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                            BuildingId = new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                            CreatedByUserId = new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                            CreatedDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            CurrentFloor = 1,
                            DestinationFloor = 0,
                            DestinationFloors = "",
                            DoorStatus = 0,
                            ElevatorDirection = "None",
                            ElevatorStatus = "Active",
                            ElevatorType = "Passenger",
                            FloorsPerSecond = 1,
                            Number = 3,
                            PersonCapacity = 10
                        },
                        new
                        {
                            Id = new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                            BuildingId = new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                            CreatedByUserId = new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                            CreatedDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            CurrentFloor = 1,
                            DestinationFloor = 0,
                            DestinationFloors = "",
                            DoorStatus = 0,
                            ElevatorDirection = "None",
                            ElevatorStatus = "Maintenance",
                            ElevatorType = "Passenger",
                            FloorsPerSecond = 1,
                            Number = 4,
                            PersonCapacity = 10
                        },
                        new
                        {
                            Id = new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                            BuildingId = new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                            CreatedByUserId = new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                            CreatedDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            CurrentFloor = 1,
                            DestinationFloor = 0,
                            DestinationFloors = "",
                            DoorStatus = 0,
                            ElevatorDirection = "None",
                            ElevatorStatus = "OutOfService",
                            ElevatorType = "Passenger",
                            FloorsPerSecond = 1,
                            Number = 5,
                            PersonCapacity = 10
                        });
                });

            modelBuilder.Entity("Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("first_name");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("last_name");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("password_hash");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.ToTable("users", "dbo");

                    b.HasData(
                        new
                        {
                            Id = new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                            Email = "admin@building.com",
                            FirstName = "Admin",
                            LastName = "Joe",
                            PasswordHash = "55BC042899399B562DD4A363FD250A9014C045B900716FCDC074861EB69C344A-B44367BE2D0B037E31AEEE2649199100"
                        });
                });

            modelBuilder.Entity("Domain.Buildings.Building", b =>
                {
                    b.HasOne("Domain.Users.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_buildings_users_created_by_user_id");

                    b.HasOne("Domain.Users.User", "UpdatedByUser")
                        .WithMany()
                        .HasForeignKey("UpdatedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_buildings_users_updated_by_user_id");

                    b.Navigation("CreatedByUser");

                    b.Navigation("UpdatedByUser");
                });

            modelBuilder.Entity("Domain.Elevators.Elevator", b =>
                {
                    b.HasOne("Domain.Buildings.Building", "Building")
                        .WithMany()
                        .HasForeignKey("BuildingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_elevators_buildings_building_id");

                    b.HasOne("Domain.Users.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedByUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_elevators_users_created_by_user_id");

                    b.HasOne("Domain.Users.User", "UpdatedByUser")
                        .WithMany()
                        .HasForeignKey("UpdatedByUserId")
                        .HasConstraintName("fk_elevators_users_updated_by_user_id");

                    b.Navigation("Building");

                    b.Navigation("CreatedByUser");

                    b.Navigation("UpdatedByUser");
                });
#pragma warning restore 612, 618
        }
    }
}
