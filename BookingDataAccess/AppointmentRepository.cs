using System.Data.SqlClient;

namespace BookingDataAccess
{
    public interface IAppointmentRepository
    {

        Task AddAppointment(DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime);
        Task DeleteAppointment(DateTime appointmentDate, TimeSpan startTime);
        Task<List<DateTime>> FindBookedTimeSlotsAsync(DateTime date);
        Task<bool> IsSlotAvailable(DateTime dateTime);
    }
    public class AppointmentRepository: IAppointmentRepository
    {
        private readonly string _connectionString;

        public AppointmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddAppointment(DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
        {
            DateTime potentialStartTime = appointmentDate + startTime;
            if (! (await IsSlotAvailable(potentialStartTime)))
            {
                Console.WriteLine("this time slot is already booked.");
                return;
            }
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("INSERT INTO Appointments (AppointmentDate, StartTime, EndTime) VALUES (@Date, @StartTime, @EndTime)", connection);
                command.Parameters.AddWithValue("@Date", appointmentDate.Date);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", endTime);

                connection.Open();
                await command.ExecuteNonQueryAsync();
            }
            Console.WriteLine("Appointment added.");
        }

        public async Task DeleteAppointment(DateTime appointmentDate, TimeSpan startTime)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Appointments WHERE AppointmentDate = @Date AND StartTime = @StartTime";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Date", appointmentDate.Date);
                command.Parameters.AddWithValue("@StartTime", startTime);

                connection.Open();
                int affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    Console.WriteLine("No appointment found to delete.");
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
                string sql = "SELECT StartTime FROM Appointments WHERE AppointmentDate = @Date";
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
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(1) FROM Appointments WHERE AppointmentDate = @Date AND StartTime = @StartTime";
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