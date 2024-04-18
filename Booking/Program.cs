using BookingDataAccess;
using BookingManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        // Build a service collection
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        var manager = serviceProvider.GetRequiredService<IAppointmentBookingManager>();


        Console.WriteLine("Enter command (ADD, DELETE, FIND, KEEP) or 'EXIT' to quit:");
        string input;

        while ((input = Console.ReadLine()) != null && input.ToUpper() != "EXIT")
        {
            string[] parts = input.Trim().Split();
            if (parts.Length == 0) continue;

            string command = parts[0].ToUpper();
            switch (command)
            {
                case "ADD":
                    await HandleAddCommand(parts, manager);
                    break;
                case "DELETE":
                    await HandleDeleteCommand(parts, manager);
                    break;
                case "FIND":
                    await HandleFindCommand(parts, manager);
                    break;
                case "KEEP":
                    await HandleKeepCommand(parts, manager);
                    break;
                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }
        Console.WriteLine("Exiting program.");
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Setup configuration sources
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Register services with DI system
        var connectionString = configuration.GetConnectionString("master");

        services.AddSingleton<IAppointmentRepository, AppointmentRepository>(provider =>
            new AppointmentRepository(connectionString));
        services.AddTransient<IAppointmentBookingManager, AppointmentBookingManager>();

    }

    private static async Task HandleAddCommand(string[] args, IAppointmentBookingManager manager)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Insufficient arguments for ADD.");
            return;
        }
        DateTime? date = args[1].ToDate();
        if (date == null)
        {
            Console.WriteLine("Invalid Date.");
            return;
        }
        TimeSpan? timeSpan = args[2].ToTimeSpan();
        if (timeSpan == null)
        {
            Console.WriteLine("Invalid Time");
            return;
        }
        await manager.AddAppointment(date.Value, timeSpan.Value, timeSpan.Value.Add(TimeSpan.FromMinutes(30)));
        
    }

    private static async Task HandleDeleteCommand(string[] args, IAppointmentBookingManager manager)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Insufficient arguments for DELETE.");
            return;
        }
        DateTime? date = args[1].ToDate();
        if (date == null)
        {
            Console.WriteLine("Invalid Date.");
            return;
        }
        TimeSpan? time = args[2].ToTimeSpan();
        if (time == null)
        {
            Console.WriteLine("Invalid Time.");
            return;
        }
        await manager.DeleteAppointment(date.Value, time.Value);        
    }

    private static async Task HandleFindCommand(string[] args, IAppointmentBookingManager manager)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Insufficient arguments for FIND.");
            return;
        }
        DateTime? date = args[1].ToDate();
        if (date == null)
        {
            Console.WriteLine("Invalid Date.");
            return;
        }
        var freeTime = await manager.FindFreeTimeSlot(date.Value);
        if(freeTime == null)
        {
            Console.WriteLine("Free time slot not found.");
        }
        Console.WriteLine($"{freeTime} is Free time slot");
    }

    private static async Task HandleKeepCommand(string[] args, IAppointmentBookingManager manager)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Insufficient arguments for KEEP.");
            return;
        }
        TimeSpan? time = args[1].ToTimeSpan();
        if (time == null)
        {
            Console.WriteLine("Invalid Time.");
            return;
        }
        await manager.KeepTimeSlot(time.Value);
        Console.WriteLine("Time slot kept.");
    }
}
