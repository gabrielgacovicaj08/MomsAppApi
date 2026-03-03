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
