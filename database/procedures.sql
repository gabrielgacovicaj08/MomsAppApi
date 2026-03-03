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

CREATE PROCEDURE [dbo].[UpdateAssignment]
    @assignment_id INT,
    @work_date DATE,
    @employee_id INT,
    @structure_id INT,
    @shift_start TIME(0) = NULL,
    @shift_end TIME(0) = NULL,
    @status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Assignments
    SET
        work_date = @work_date,
        employee_id = @employee_id,
        structure_id = @structure_id,
        shift_start = @shift_start,
        shift_end = @shift_end,
        status = UPPER(@status),
        updated_at = SYSDATETIME()
    WHERE assignment_id = @assignment_id;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
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
