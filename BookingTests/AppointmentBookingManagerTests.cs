using BookingDataAccess;
using BookingManager;
using Moq;

namespace BookingTests
{
    [TestFixture]
    public class AppointmentBookingManagerTests
    {
        private Mock<IAppointmentRepository> _mockRepository;
        private AppointmentBookingManager _manager;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IAppointmentRepository>();
            _manager = new AppointmentBookingManager(_mockRepository.Object);
        }

        [Test]
        public void IsSecondDayOfThirdWeek_ShouldReturnTrue_WhenDateIsSecondDayOfThirdWeek()
        {
            // Arrange
            // April 2024 started on a Monday, so the second day of the third week is Tuesday, April 16th
            var date = new DateTime(2024, 04, 16);

            // Act
            var result = _manager.IsSecondDayOfThirdWeek(date);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsSecondDayOfThirdWeek_ShouldReturnFalse_WhenDateIsNotSecondDayOfThirdWeek()
        {
            // Arrange            
            var date = new DateTime(2024, 4, 17); // Not the second day of the third week

            // Act
            var result = _manager.IsSecondDayOfThirdWeek(date);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task AddAppointment_WithinBusinessHours_AddsSuccessfully()
        {
            // Arrange
            var date = new DateTime(2023, 4, 15); // Any business day
            var startTime = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = startTime.Add(TimeSpan.FromMinutes(30)); // 10:30 AM

            _mockRepository.Setup(repo => repo.AddAppointment(date, startTime, endTime, It.IsAny<string>()))
                           .Returns(Task.CompletedTask);

            // Act
            await _manager.AddAppointment(date, startTime, endTime);

            // Assert
            _mockRepository.Verify(repo => repo.AddAppointment(date, startTime, endTime, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task AddAppointment_OutsideBusinessHours_ReturnsFalse()
        {
            // Arrange
            var date = new DateTime(2023, 4, 15);
            var startTime = new TimeSpan(8, 0, 0); // 8 AM, before business hours
            var endTime = startTime.Add(TimeSpan.FromMinutes(30));

            // Act
             await _manager.AddAppointment(date, startTime, endTime);

            // Assert
            _mockRepository.Verify(repo => repo.AddAppointment(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.Never);
        }
    }
}