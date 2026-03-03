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
