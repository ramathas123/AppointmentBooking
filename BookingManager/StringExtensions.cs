using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManager
{
    public static class StringExtensions
    {
        //Convert DD/MM format string to Date
        public static DateTime? ToDate(this string dateString, string year = null)
        {
            try
            {
                year = year ?? DateTime.Now.Year.ToString(); // if the year is not provided, use current year
                dateString += "/" + year; // Append the current year to match the required format

                return DateTime.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return null; // Return null if parsing fails
            }
        }

        //Convert hh:mm format string to TimeSpan
        public static TimeSpan? ToTimeSpan(this string timeString)
        {
            try
            {
                return TimeSpan.ParseExact(timeString, "hh\\:mm", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {                
                return null; 
            }
        }
    }
}
