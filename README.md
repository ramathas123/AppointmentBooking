Instruction to RUN:
Open the solution in the visual studio
create following table in master database in you local sql server:
DataBase:
CREATE TABLE Appointments (
    Id INT PRIMARY KEY IDENTITY,
    AppointmentDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    CreatedBy VARCHAR(255),
    CreatedDate DATETIME,
    ModifiedBy VARCHAR(255),
    ModifiedDate DATETIME,
    Status BIT NOT NULL DEFAULT 1  -- 1 for active, 0 for soft deleted
);

adjust the connection string in AppSettings.json file with your username and password.

Area of improvement:
 -Exception handling should be improved by Adding try-catch blocks, specially handle SQL exceptions gracefully and log errors appropriately.
 -And the Logging can be in selerate class that can be injected to all dependant classes.
 -Repository, business class should not Write to Console,  it can refactor to have a seperate sevice to handle that. and inject that Service via DI.
 -Some Hard coded constant can be Shifted to config file , example acceptable start time and acceptable end time, date , time format.
 -The methods IsTimeReserved and IsTimeWithinBusinessHours are within the Booking manager class. Consider injecting a service to handle these time calculations, which would make testing easier and application more modular.
 - All the error message can be in Constant File instead of keeping them in classes.
 -More validation can be added around business rule.
 -Use of Entity Framework
 -Configurable parameters or keys in key vault.
 -Password encryption
 -Create a separate relational database with more entities, relationships and include normalisation principles.
 -Use of Cloud platform and services for hosting
 -Use of front-end technology for UI experience




