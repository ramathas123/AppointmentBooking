﻿using BookingDataAccess;

namespace BookingManager
{
    public interface IAppointmentBookingManager
    {
        Task AddAppointment(DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime);
        Task DeleteAppointment(DateTime appointmentDate, TimeSpan startTime);
        Task KeepTimeSlot(TimeSpan time);
        Task<DateTime?> FindFreeTimeSlot(DateTime date);
        bool IsSecondDayOfThirdWeek(DateTime date);
    }
    public class AppointmentBookingManager: IAppointmentBookingManager
    {
        private readonly IAppointmentRepository _repository;

        public AppointmentBookingManager(IAppointmentRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAppointment(DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
        {
            if (IsTimeWithinBusinessHours(startTime))
            {
                await _repository.AddAppointment(appointmentDate, startTime, endTime);
            }
            else
            {
                Console.WriteLine("The acceptable time is between 9AM and 5PM");
            }            
        }

        public async Task DeleteAppointment(DateTime appointmentDate, TimeSpan startTime)
        {
            await _repository.DeleteAppointment(appointmentDate, startTime);
        }
        public async Task KeepTimeSlot(TimeSpan time)
        {
            DateTime date = DateTime.Today;
            bool slotKept = false;

            while (!slotKept)
            {
                if (!IsTimeReserved(date, time))
                {
                    DateTime potentialStartTime = date + time;
                    if (await _repository.IsSlotAvailable(potentialStartTime))
                    {
                        await _repository.AddAppointment(date, time, time.Add(TimeSpan.FromMinutes(30)));
                        Console.WriteLine($"Time slot kept: {potentialStartTime}");
                        slotKept = true;
                    }
                }
                date = date.AddDays(1);  // Move to the next day
            }
        }
        public async Task<DateTime?> FindFreeTimeSlot(DateTime date)
        {
            var startTime = new TimeSpan(9, 0, 0); // Start of work day
            var endTime = new TimeSpan(17, 0, 0); // End of work day
            TimeSpan interval = TimeSpan.FromMinutes(30); // Duration of each slot

            List<DateTime> bookedSlots = await _repository.FindBookedTimeSlotsAsync(date);

            for (var time = startTime; time < endTime; time += interval)
            {
                var potentialStartTime = date.Date + time;
                if (!IsTimeReserved(date, time) && !bookedSlots.Contains(potentialStartTime))
                {
                    return potentialStartTime;
                }
            }

            return null; // No available slot found
        }

        private bool IsTimeReserved(DateTime date, TimeSpan time)
        {
            return time.Hours == 16 && IsSecondDayOfThirdWeek(date);
        }

        public bool IsSecondDayOfThirdWeek(DateTime date)
        {
            var firstOfMonth = new DateTime(date.Year, date.Month, 1);
            var dayOfWeek = (int)firstOfMonth.DayOfWeek;
            var secondDayOfThirdWeek = firstOfMonth.AddDays(((2 - dayOfWeek + 7) % 7) + 14);

            return date.Date == secondDayOfThirdWeek.Date;
        }

        private bool IsTimeWithinBusinessHours(TimeSpan timeToCheck)
        {
            TimeSpan startTime = new TimeSpan(9, 0, 0); // 9 AM
            TimeSpan endTime = new TimeSpan(16, 30, 0);  // 5 PM

            // Check if the time is between 9 AM and 5 PM
            return (timeToCheck >= startTime && timeToCheck <= endTime);
        }
    }
}