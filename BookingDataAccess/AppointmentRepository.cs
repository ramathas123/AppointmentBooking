using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BookingDataAccess
{
    public interface IAppointmentRepository
    {
        Task AddAppointment(DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, string createdBy);
        Task DeleteAppointment(DateTime appointmentDate, TimeSpan startTime, string modifiedBy);
        Task<List<DateTime>> FindBookedTimeSlotsAsync(DateTime date);
        Task<bool> IsSlotAvailable(DateTime dateTime);
    }

    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly string _connectionString;

        public AppointmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddAppointment(DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, string createdBy)
        {
            if (!await IsSlotAvailable(appointmentDate + startTime))
            {
                Console.WriteLine("This time slot is already booked.");
                return;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("INSERT INTO Appointments (AppointmentDate, StartTime, EndTime, CreatedBy, CreatedDate) VALUES (@Date, @StartTime, @EndTime, @CreatedBy, @CreatedDate)", connection);
                command.Parameters.AddWithValue("@Date", appointmentDate.Date);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", endTime);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);

                connection.Open();
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Appointment added.");
            }
        }

        public async Task DeleteAppointment(DateTime appointmentDate, TimeSpan startTime, string modifiedBy)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("UPDATE Appointments SET Status = 0, ModifiedBy = @ModifiedBy, ModifiedDate = @ModifiedDate WHERE AppointmentDate = @Date AND StartTime = @StartTime AND Status = 1", connection);
                command.Parameters.AddWithValue("@Date", appointmentDate.Date);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@ModifiedBy", modifiedBy);
                command.Parameters.AddWithValue("@ModifiedDate", DateTime.UtcNow);

                connection.Open();
                int affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    Console.WriteLine("No active appointment found to delete.");
                }
                else
                {
                    Console.WriteLine("Appointment deleted successfully.");
                }
            }
        }

        public async Task<List<DateTime>> FindBookedTimeSlotsAsync(DateTime date)
        {
            List<DateTime> bookedSlots = new List<DateTime>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "SELECT StartTime FROM Appointments WHERE AppointmentDate = @Date AND Status = 1";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Date", date.Date);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        bookedSlots.Add(date.Date + (TimeSpan)reader["StartTime"]);
                    }
                }
            }

            return bookedSlots;
        }

        public async Task<bool> IsSlotAvailable(DateTime dateTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(1) FROM Appointments WHERE AppointmentDate = @Date AND StartTime = @StartTime AND Status = 1";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Date", dateTime.Date);
                command.Parameters.AddWithValue("@StartTime", dateTime.TimeOfDay);

                connection.Open();
                int count = (int)await command.ExecuteScalarAsync();
                return count == 0;
            }
        }
    }
}
