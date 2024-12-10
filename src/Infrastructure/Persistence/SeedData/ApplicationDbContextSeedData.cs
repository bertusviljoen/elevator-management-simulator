using Domain.Buildings;
using Domain.Elevators;
using Domain.Users;
using Infrastructure.Authentication;
using Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Persistence.SeedData;

public static class ApplicationDbContextSeedData
{

    public static List<User> GetSeedUsers()
    {
        //Administrator user to manage the application
        return new List<User>
        {
            new()
            {
                Id = Guid.Parse("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                FirstName = "Admin",
                LastName = "Joe",
                Email = "admin@building.com",
                PasswordHash =
                    "55BC042899399B562DD4A363FD250A9014C045B900716FCDC074861EB69C344A-B44367BE2D0B037E31AEEE2649199100", //Admin123
            }
        };
    }
    
    public static List<Building> GetSeedBuildings()
    {
        return new List<Building>
        {
            new()
            {
                Id = Guid.Parse("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                Name = "Joe's Building",
                NumberOfFloors = 10,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id
            }
        };
    }

    public static List<Elevator> GetSeedElevators()
    {
        return new List<Elevator>
        {
            new()
            {
                Id = Guid.Parse("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                CurrentFloor = 1,
                Number = 1,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Active,
                ElevatorType = ElevatorType.HighSpeed,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                FloorsPerSecond = 5,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                CurrentFloor = 1,
                Number = 2,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Active,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                CurrentFloor = 1,
                Number = 3,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Active,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("b8557436-6472-4ad7-b111-09c8a023c463"),
                CurrentFloor = 1,
                Number = 4,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Maintenance,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("bbfbdffa-f7cd-4241-a222-85a733098782"),
                CurrentFloor = 1,
                Number = 5,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.OutOfService,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            }
        };
    }

    public static void SeedData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(GetSeedUsers());
        modelBuilder.Entity<Building>().HasData(GetSeedBuildings());
        modelBuilder.Entity<Elevator>().HasData(GetSeedElevators());
    }
}
