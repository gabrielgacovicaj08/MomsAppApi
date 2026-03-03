USE [master]
GO

/****** Object:  Database [EmpMang]    Script Date: 2/27/2026 4:28:15 PM ******/
CREATE DATABASE [EmpMang]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'EmpMang', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\EmpMang.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'EmpMang_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\EmpMang_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [EmpMang] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [EmpMang].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [EmpMang] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [EmpMang] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [EmpMang] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [EmpMang] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [EmpMang] SET ARITHABORT OFF 
GO
ALTER DATABASE [EmpMang] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [EmpMang] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [EmpMang] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [EmpMang] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [EmpMang] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [EmpMang] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [EmpMang] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [EmpMang] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [EmpMang] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [EmpMang] SET  DISABLE_BROKER 
GO
ALTER DATABASE [EmpMang] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [EmpMang] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [EmpMang] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [EmpMang] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [EmpMang] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [EmpMang] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [EmpMang] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [EmpMang] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [EmpMang] SET  MULTI_USER 
GO
ALTER DATABASE [EmpMang] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [EmpMang] SET DB_CHAINING OFF 
GO
ALTER DATABASE [EmpMang] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [EmpMang] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [EmpMang] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [EmpMang] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [EmpMang] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [EmpMang] SET QUERY_STORE = ON
GO
ALTER DATABASE [EmpMang] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO

USE [EmpMang]
GO

/****** Object:  Table [dbo].[Employees]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employees](
	[employee_id] [int] IDENTITY(1,1) NOT NULL,
	[first_name] [nvarchar](100) NOT NULL,
	[last_name] [nvarchar](100) NOT NULL,
	[phone] [nvarchar](20) NULL,
	[email] [nvarchar](250) NOT NULL,
	[role] [nvarchar](20) NOT NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Structures]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Structures](
	[structure_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](max) NOT NULL,
	[address_line] [nvarchar](max) NOT NULL,
	[city] [nvarchar](100) NOT NULL,
	[zip] [nvarchar](15) NOT NULL,
	[client_name] [nvarchar](150) NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[structure_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Assignments]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Assignments](
	[assignment_id] [int] IDENTITY(1,1) NOT NULL,
	[work_date] [date] NOT NULL,
	[employee_id] [int] NOT NULL,
	[structure_id] [int] NOT NULL,
	[shift_start] [time](0) NULL,
	[shift_end] [time](0) NULL,
	[status] [nvarchar](20) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_assignments] PRIMARY KEY CLUSTERED 
(
	[assignment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_assignments_workdate_employee_structure_shift] UNIQUE NONCLUSTERED 
(
	[work_date] ASC,
	[employee_id] ASC,
	[structure_id] ASC,
	[shift_start] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[WorkLogs]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkLogs](
	[log_id] [int] IDENTITY(1,1) NOT NULL,
	[assignment_id] [int] NOT NULL,
	[started_at] [datetime2](7) NULL,
	[ended_at] [datetime2](7) NULL,
	[minutes_worked] [int] NULL,
	[notes] [nvarchar](max) NULL,
	[issues_flagged] [bit] NOT NULL,
	[submitted_at] [datetime2](7) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_work_logs] PRIMARY KEY CLUSTERED 
(
	[log_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_work_logs_assignment] UNIQUE NONCLUSTERED 
(
	[assignment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  View [dbo].[vw_DailyAssignments]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_DailyAssignments]
AS
SELECT
    a.assignment_id,
    a.work_date,

    a.employee_id,
    e.first_name + ' ' + e.last_name AS employee_name,

    a.structure_id,
    s.name AS structure_name,

    a.shift_start,
    a.shift_end,

    a.status,

    wl.log_id,
    wl.submitted_at,
    wl.issues_flagged

FROM dbo.Assignments a
JOIN dbo.Employees e 
    ON e.employee_id = a.employee_id
JOIN dbo.structures s 
    ON s.structure_id = a.structure_id
LEFT JOIN dbo.WorkLogs wl 
    ON wl.assignment_id = a.assignment_id;
GO

/****** Object:  View [dbo].[vw_LogsView]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vw_LogsView]
AS 
SELECT 
	wl.assignment_id,
	a.work_date,
	wl.started_at,
	wl.ended_at,
	wl.notes,
	wl.submitted_at,
	
	e.first_name,
	e.last_name,
	s.name AS structure_name
FROM dbo.WorkLogs wl
INNER JOIN Assignments a ON wl.assignment_id = a.assignment_id
INNER JOIN Employees e ON a.employee_id = e.employee_id
INNER JOIN Structures s ON a.structure_id = s.structure_id
GO

/****** Object:  Table [dbo].[UserAccounts]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAccounts](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[employee_id] [int] NOT NULL,
	[email] [nvarchar](255) NOT NULL,
	[password_hash] [nvarchar](255) NOT NULL,
	[last_login_at] [datetime2](7) NULL,
	[is_locked] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NOT NULL,
	[refresh_token] [nvarchar](max) NULL,
	[refresh_token_expiry_time] [datetime2](7) NULL,
	[Role] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_UserAccounts] PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_UserAccounts_email] UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_UserAccounts_employee] UNIQUE NONCLUSTERED 
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Index [UX_Assignments_Employee_WorkDate]    Script Date: 2/27/2026 4:28:15 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Assignments_Employee_WorkDate] ON [dbo].[Assignments]
(
	[employee_id] ASC,
	[work_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [UX_WorkLogs_Assignment]    Script Date: 2/27/2026 4:28:15 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_WorkLogs_Assignment] ON [dbo].[WorkLogs]
(
	[assignment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Assignments] ADD  CONSTRAINT [DF_assignments_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[Assignments] ADD  CONSTRAINT [DF_assignments_updated_at]  DEFAULT (sysdatetime()) FOR [updated_at]
GO

ALTER TABLE [dbo].[Employees] ADD  DEFAULT ((1)) FOR [is_active]
GO

ALTER TABLE [dbo].[Employees] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[Employees] ADD  DEFAULT (sysdatetime()) FOR [updated_at]
GO

ALTER TABLE [dbo].[Structures] ADD  DEFAULT ((1)) FOR [is_active]
GO

ALTER TABLE [dbo].[Structures] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[Structures] ADD  DEFAULT (sysdatetime()) FOR [updated_at]
GO

ALTER TABLE [dbo].[UserAccounts] ADD  CONSTRAINT [DF_UserAccounts_is_locked]  DEFAULT ((0)) FOR [is_locked]
GO

ALTER TABLE [dbo].[UserAccounts] ADD  CONSTRAINT [DF_UserAccounts_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[UserAccounts] ADD  CONSTRAINT [DF_UserAccounts_updated_at]  DEFAULT (sysdatetime()) FOR [updated_at]
GO

ALTER TABLE [dbo].[WorkLogs] ADD  CONSTRAINT [DF_work_logs_issues_flagged]  DEFAULT ((0)) FOR [issues_flagged]
GO

ALTER TABLE [dbo].[WorkLogs] ADD  CONSTRAINT [DF_work_logs_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[WorkLogs] ADD  CONSTRAINT [DF_work_logs_updated_at]  DEFAULT (sysdatetime()) FOR [updated_at]
GO

ALTER TABLE [dbo].[Assignments]  WITH CHECK ADD  CONSTRAINT [FK_assignments_employee] FOREIGN KEY([employee_id])
REFERENCES [dbo].[Employees] ([employee_id])
GO
ALTER TABLE [dbo].[Assignments] CHECK CONSTRAINT [FK_assignments_employee]
GO

ALTER TABLE [dbo].[Assignments]  WITH CHECK ADD  CONSTRAINT [FK_assignments_structure] FOREIGN KEY([structure_id])
REFERENCES [dbo].[Structures] ([structure_id])
GO
ALTER TABLE [dbo].[Assignments] CHECK CONSTRAINT [FK_assignments_structure]
GO

ALTER TABLE [dbo].[UserAccounts]  WITH CHECK ADD  CONSTRAINT [FK_UserAccounts_employee] FOREIGN KEY([employee_id])
REFERENCES [dbo].[Employees] ([employee_id])
GO
ALTER TABLE [dbo].[UserAccounts] CHECK CONSTRAINT [FK_UserAccounts_employee]
GO

ALTER TABLE [dbo].[WorkLogs]  WITH CHECK ADD  CONSTRAINT [FK_work_logs_assignment] FOREIGN KEY([assignment_id])
REFERENCES [dbo].[Assignments] ([assignment_id])
GO
ALTER TABLE [dbo].[WorkLogs] CHECK CONSTRAINT [FK_work_logs_assignment]
GO

ALTER TABLE [dbo].[Assignments]  WITH CHECK ADD  CONSTRAINT [CK_assignments_shift_times] CHECK  (([shift_start] IS NULL OR [shift_end] IS NULL OR [shift_end]>[shift_start]))
GO
ALTER TABLE [dbo].[Assignments] CHECK CONSTRAINT [CK_assignments_shift_times]
GO

ALTER TABLE [dbo].[Assignments]  WITH CHECK ADD  CONSTRAINT [CK_assignments_status] CHECK  (([status]='CANCELLED' OR [status]='MISSED' OR [status]='COMPLETED' OR [status]='IN_PROGRESS' OR [status]='SCHEDULED'))
GO
ALTER TABLE [dbo].[Assignments] CHECK CONSTRAINT [CK_assignments_status]
GO

ALTER TABLE [dbo].[Employees]  WITH CHECK ADD CHECK  (([role]='SUPERVISOR' OR [role]='WORKER' OR [role]='ADMIN'))
GO

ALTER TABLE [dbo].[Employees]  WITH CHECK ADD  CONSTRAINT [CK_Employees_Email_NotEmpty] CHECK  ((len(ltrim(rtrim([email])))>(0)))
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [CK_Employees_Email_NotEmpty]
GO

ALTER TABLE [dbo].[Employees]  WITH CHECK ADD  CONSTRAINT [CK_Employees_FirstName_NotEmpty] CHECK  ((len(ltrim(rtrim([first_name])))>(0)))
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [CK_Employees_FirstName_NotEmpty]
GO

ALTER TABLE [dbo].[Employees]  WITH CHECK ADD  CONSTRAINT [CK_Employees_LastName_NotEmpty] CHECK  ((len(ltrim(rtrim([last_name])))>(0)))
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [CK_Employees_LastName_NotEmpty]
GO

ALTER TABLE [dbo].[WorkLogs]  WITH CHECK ADD  CONSTRAINT [CK_work_logs_time_order] CHECK  (([started_at] IS NULL OR [ended_at] IS NULL OR [ended_at]>=[started_at]))
GO
ALTER TABLE [dbo].[WorkLogs] CHECK CONSTRAINT [CK_work_logs_time_order]
GO

/****** Object:  StoredProcedure [dbo].[AvailableWorkersPerDay]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- AvailableWorkersPerDay '2026-02-25'

CREATE PROCEDURE [dbo].[AvailableWorkersPerDay]
	@work_date DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.*
	FROM dbo.Employees e
	WHERE e.is_active = 1
	AND NOT EXISTS (
		SELECT 1
		FROM dbo.Assignments a
		WHERE a.employee_id = e.employee_id 
		AND a.work_date = @work_date );
END
GO

/****** Object:  StoredProcedure [dbo].[CreateAssignment]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateAssignment] 
	@work_date DATE,
	@employee_id INT,
	@structure_id INT,
	@shift_start TIME(0) NULL,
	@shift_end TIME(0) NULL

AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Assignments (work_date, employee_id, structure_id, shift_start, shift_end, status)
	VALUES (@work_date, @employee_id, @structure_id, @shift_start, @shift_end, 'SCHEDULED')
END
GO

/****** Object:  StoredProcedure [dbo].[CreateEmployee]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[CreateEmployee] 
	@first_name NVARCHAR(100),
	@last_name NVARCHAR(100),
	@phone NVARCHAR(20),
	@email NVARCHAR(150),
	@role NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;

	IF @first_name IS NULL OR LTRIM(RTRIM(@first_name)) = ''
  BEGIN
    RAISERROR('First name is required.', 16, 1);
    RETURN;
  END

  IF @last_name IS NULL OR LTRIM(RTRIM(@last_name)) = ''
  BEGIN
    RAISERROR('Last name is required.', 16, 1);
    RETURN;
  END

  IF @email IS NULL OR LTRIM(RTRIM(@email)) = ''
  BEGIN
    RAISERROR('Email is required.', 16, 1);
    RETURN;
  END

	IF EXISTS (SELECT 1 FROM Employees WHERE email = @email)
	BEGIN
		RAISERROR('Employee with this email already exists', 16, 1);
		RETURN;
	END

	INSERT INTO Employees (first_name, last_name, phone, email, role)
		VALUES (@first_name, @last_name, @phone, @email, @role)

		SELECT SCOPE_IDENTITY() AS employee_id;


	END;
	
GO

/****** Object:  StoredProcedure [dbo].[CreateStructure]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateStructure] 
	@name NVARCHAR(150),
	@address_line NVARCHAR(250),
	@city NVARCHAR(50),
	@zip NVARCHAR(7),
	@client_name NVARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Structures (name, address_line, city, zip, client_name)
		VALUES (@name, @address_line, @city, @zip, @client_name)
END
GO

/****** Object:  StoredProcedure [dbo].[CreateUserAccount]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateUserAccount] 
	@employee_id INT,
	@email NVARCHAR(150),
	@password_hash NVARCHAR(MAX),
	@role NVARCHAR(15)
AS 
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT 1 FROM UserAccounts WHERE email = @email)
	BEGIN
		RAISERROR('Uesr with this email already exists', 16, 1);
		RETURN;
	END;

	INSERT INTO UserAccounts(employee_id, email, password_hash, role)
		VALUES (@employee_id, @email, @password_hash, @role)

		SELECT SCOPE_IDENTITY() AS user_id;

END;
GO

/****** Object:  StoredProcedure [dbo].[CreateWorkLog]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateWorkLog]
	@assignment_id INT,
	@started_at DATETIME2(7),
	@ended_at DATETIME2(7),
	@notes NVARCHAR(MAX) NULL
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	 BEGIN TRAN;

     IF NOT EXISTS (SELECT 1 FROM dbo.Assignments WHERE assignment_id = @assignment_id)
     BEGIN
        ROLLBACK;
        RAISERROR('There is no Assignment correlated to this ID', 16, 1);
        RETURN;
    END

    -- 3) Prevent multiple logs for the same assignment (if desired)
    IF EXISTS (SELECT 1 FROM dbo.WorkLogs WHERE assignment_id = @assignment_id)
    BEGIN
        ROLLBACK;
        RAISERROR('Work log already exists for this assignment.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.WorkLogs (assignment_id, started_at, ended_at, notes, submitted_at)
    VALUES (@assignment_id, @started_at, @ended_at, @notes, SYSDATETIME());

    -- 4) If ended_at is provided, mark assignment completed
    IF (@ended_at IS NOT NULL)
    BEGIN
        UPDATE dbo.Assignments
        SET status = 'COMPLETED',
            updated_at = SYSDATETIME()
        WHERE assignment_id = @assignment_id;
    END

    COMMIT;
END
GO

/****** Object:  StoredProcedure [dbo].[DeleteEmployee]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteEmployee]
    @employee_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE employee_id = @employee_id)
    BEGIN
        RAISERROR('Employee not found.', 16, 1);
        RETURN;
    END

    BEGIN TRAN;

    UPDATE dbo.Employees 
    SET is_active = 0,
        updated_at = SYSDATETIME()
    WHERE employee_id = @employee_id;

     -- Cancel only assignments that haven't been completed yet (and are today or future)
    UPDATE dbo.Assignments
    SET status = 'CANCELLED',
        updated_at = SYSDATETIME()
    WHERE employee_id = @employee_id
      AND work_date >= CAST(GETDATE() AS date)
      AND status IN ('SCHEDULED');

    COMMIT;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

/****** Object:  StoredProcedure [dbo].[DeleteStructure]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteStructure]
    @structure_id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Structures
        WHERE structure_id = @structure_id
    )
    BEGIN
        RAISERROR('Structure not found.', 16, 1);
        RETURN;
    END


    -- 3. Delete
    UPDATE Structures 
    SET is_active = 0,
        updated_at = SYSDATETIME()
    WHERE structure_id = @structure_id
END
GO

/****** Object:  StoredProcedure [dbo].[GetAllAssignmentsByDay]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllAssignmentsByDay]
	@work_date DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.assignment_id, a.work_date, a.shift_start, a.shift_end, e.first_name, e.last_name, s.name AS HotelName, status
	FROM Assignments a
	INNER JOIN Employees e 
	ON a.employee_id = e.employee_id
	INNER JOIN Structures s
	ON a.structure_id = s.structure_id
	WHERE work_date = @work_date



END;
GO

/****** Object:  StoredProcedure [dbo].[GetAllEmployees]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllEmployees]
AS
BEGIN
	SET NOCOUNT ON;
SELECT 
	employee_id,
	first_name,
	last_name,
	phone,
	email,
	role,
	is_active
FROM Employees
ORDER BY last_name, first_name 
END;
GO

/****** Object:  StoredProcedure [dbo].[GetAllStructures]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllStructures]
AS
BEGIN
	SET NOCOUNT ON;
SELECT
	structure_id,
	name,
	address_line,
	city,
	zip,
	client_name,
	is_active
FROM dbo.Structures
ORDER BY name
END;
GO

/****** Object:  StoredProcedure [dbo].[GetAssignmentsByEmpId]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAssignmentsByEmpId]
	@employee_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.assignment_id, a.work_date, a.shift_start, a.shift_end, e.first_name, e.last_name, s.name AS HotelName, status
	FROM Assignments a
	INNER JOIN Employees e 
	ON a.employee_id = e.employee_id
	INNER JOIN Structures s
	ON a.structure_id = s.structure_id
	WHERE e.employee_id = @employee_id



END;
GO

/****** Object:  StoredProcedure [dbo].[GetEmployeeById]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- GetEmployeeById 2008

CREATE PROCEDURE [dbo].[GetEmployeeById]
	@employee_id INT

AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
	first_name,
	last_name,
	email,
	phone,
	role,
	is_active

	FROM Employees e
	WHERE e.employee_id = @employee_id
END;
GO

/****** Object:  StoredProcedure [dbo].[GetStructureById]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetStructureById]
	@structure_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		name,
		address_line,
		city,
		zip,
		client_name
	FROM dbo.Structures
	WHERE structure_id = @structure_id
END
GO

/****** Object:  StoredProcedure [dbo].[UpdateEmployee]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[UpdateEmployee]
    @employee_id INT,
    @first_name  NVARCHAR(100) = NULL,
    @last_name   NVARCHAR(100) = NULL,
    @phone      NVARCHAR(20)  = NULL,
    @email      NVARCHAR(255) = NULL,
    @role       NVARCHAR(50)  = NULL,
    @is_active   BIT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Employees
    SET
        first_name = COALESCE(@first_name, first_name),
        last_name  = COALESCE(@last_name,  last_name),
        phone     = COALESCE(@phone,     phone),
        email     = COALESCE(@email,     email),
        role      = COALESCE(@role,      role),
        is_active  = COALESCE(@is_active,  is_active)
    WHERE employee_id = @employee_id;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

/****** Object:  StoredProcedure [dbo].[UpdateStructure]    Script Date: 2/27/2026 4:28:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateStructure]
	@structure_id INT,
	@name NVARCHAR(150) = NULL,
	@address_line NVARCHAR(250) = NULL,
	@city NVARCHAR(50) = NULL,
	@zip NVARCHAR(7) = NULL,
	@client_name NVARCHAR(150) = NULL,
	@is_active BIT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Structures 
	SET
		name = COALESCE(@name, name),
		address_line = COALESCE(@address_line, address_line),
		city = COALESCE(@city, city),
		zip = COALESCE(@zip, zip),
		client_name = COALESCE(@client_name, client_name),
		is_active = @is_active,
		updated_at = SYSDATETIME()
	WHERE structure_id = @structure_id
END
GO

USE [master]
GO

ALTER DATABASE [EmpMang] SET  READ_WRITE 
GO
