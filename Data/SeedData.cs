using HotelDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Data;

public static class SeedData
{
    private static readonly Random _random = new();

    public static async Task Initialize(IServiceProvider serviceProvider, IWebHostEnvironment env)
    {
        Console.WriteLine($"WebRootPath: {env.WebRootPath}");

        using var context = new HotelBookingContext(
            serviceProvider.GetRequiredService<DbContextOptions<HotelBookingContext>>()
        );

        if (!context.Hotels.Any())
        {
            var uploadsDirectory = Path.Combine(env.WebRootPath ?? string.Empty, "uploads");
            if (!Directory.Exists(uploadsDirectory))
                Directory.CreateDirectory(uploadsDirectory);

            var seedImagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SeedImages");
            if (!Directory.Exists(seedImagesDirectory))
                throw new DirectoryNotFoundException(
                    $"Seed images directory not found: {seedImagesDirectory}"
                );

            var hotelFolders = Directory.GetDirectories(seedImagesDirectory);
            foreach (var hotelFolder in hotelFolders)
                await SeedHotelAsync(context, hotelFolder, uploadsDirectory);

            await context.SaveChangesAsync();
        }

        if (!context.Users.Any())
        {
            await SeedAdminUserAsync(context);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedHotelAsync(
        HotelBookingContext context,
        string hotelFolderPath,
        string uploadsDirectory
    )
    {
        var folderName = Path.GetFileName(hotelFolderPath);
        var folderParts = folderName.Split('_');

        if (folderParts.Length != 3)
            throw new FormatException(
                $"Invalid folder name format: {folderName}. Expected format: 'hotelname_location_starcount'."
            );

        var hotelName = folderParts[0];
        var hotelLocation = folderParts[1];
        var starCount = folderParts[2];

        var hotelUploadsPath = Path.Combine(uploadsDirectory, hotelName);
        if (!Directory.Exists(hotelUploadsPath))
            Directory.CreateDirectory(hotelUploadsPath);

        var hotelImagePath = Path.Combine(hotelFolderPath, "HotelImage.jpg");
        if (!File.Exists(hotelImagePath))
            throw new FileNotFoundException($"Hotel image file not found: {hotelImagePath}");

        var hotelImageBytes = await File.ReadAllBytesAsync(hotelImagePath);
        var hotelImageUrl = $"https://localhost:5001/uploads/{hotelName}/HotelImage.jpg";
        await File.WriteAllBytesAsync(
            Path.Combine(hotelUploadsPath, "HotelImage.jpg"),
            hotelImageBytes
        );

        var numberOfRatings = _random.Next(100, 1001);
        var totalRating = 0;

        for (var i = 0; i < numberOfRatings; i++)
            totalRating += _random.Next(6, 11);

        var averageRating = Math.Round((double)totalRating / numberOfRatings, 1);

        var hotel = new Hotel
        {
            Name = hotelName,
            Location = hotelLocation,
            PictureUrl = hotelImageUrl,
            Rooms = new List<Room>(),
            Rating = averageRating,
            NumberOfRatings = numberOfRatings,
            StarCount = int.Parse(starCount)
        };

        var roomsFolderPath = Path.Combine(hotelFolderPath, "Rooms");

        if (!Directory.Exists(roomsFolderPath))
            throw new DirectoryNotFoundException($"Rooms folder not found: {roomsFolderPath}");

        foreach (var roomFolderPath in Directory.GetDirectories(roomsFolderPath))
        {
            var roomFolderName = Path.GetFileName(roomFolderPath);
            var roomFolderParts = roomFolderName.Split('_');

            if (roomFolderParts.Length != 2)
                throw new FormatException(
                    $"Invalid room folder name format: {roomFolderName}. Expected format: 'roomtype_maxnumberofguests'."
                );

            var roomType = roomFolderParts[0];
            var maxNumberOfGuests = int.Parse(roomFolderParts[1]);

            var roomUploadsPath = Path.Combine(hotelUploadsPath, "Rooms", roomType);
            if (!Directory.Exists(roomUploadsPath))
                Directory.CreateDirectory(roomUploadsPath);

            var roomPictures = new List<RoomPicture>();
            foreach (var imagePath in Directory.GetFiles(roomFolderPath))
            {
                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                var imageUrl =
                    $"https://localhost:5001/uploads/{hotelName}/Rooms/{roomType}/{Path.GetFileName(imagePath)}";
                await File.WriteAllBytesAsync(
                    Path.Combine(roomUploadsPath, Path.GetFileName(imagePath)),
                    imageBytes
                );

                roomPictures.Add(new RoomPicture { Url = imageUrl });
            }

            var room = new Room
            {
                Type = roomType,
                Price =
                    roomType == "Deluxe"
                        ? 150
                        : roomType == "Standard"
                            ? 100
                            : 200,
                Pictures = roomPictures,
                MaxNumberOfGuests = maxNumberOfGuests
            };

            hotel.Rooms.Add(room);
        }

        context.Hotels.Add(hotel);
    }

    private static async Task SeedAdminUserAsync(HotelBookingContext context)
    {
        // For testing purposes only
        var adminUser = new User
        {
            Email = "test@test.com",
            Username = "test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
            VerificationCode = null,
            IsVerified = true,
            RegistrationTimestamp = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
