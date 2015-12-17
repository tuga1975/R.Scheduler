USE [enter_db_name_here]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[RSCHED_AUDIT_HISTORY]') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE [dbo].[RSCHED_AUDIT_HISTORY]
GO

CREATE TABLE [dbo].[RSCHED_AUDIT_HISTORY](
	[TIME_STAMP] [datetime2](7) NOT NULL,
	[ACTION] [varchar](200) NOT NULL,
	[FIRE_INSTANCE_ID] [nchar](200) NOT NULL,
	[JOB_NAME] [varchar](200) NULL,
	[JOB_GROUP] [varchar](200) NULL,
	[JOB_TYPE] [varchar](200) NULL,
	[TRIGGER_NAME] [varchar](200) NULL,
	[TRIGGER_GROUP] [varchar](200) NULL,
	[FIRE_TIME_UTC] [datetimeoffset](7) NULL,
	[SCHEDULED_FIRE_TIME_UTC] [datetimeoffset](7) NULL,
	[JOB_RUN_TIME] [bigint] NULL,
	[PARAMS] [nvarchar](max) NULL,
	[REFIRE_COUNT] [int] NULL,
	[RECOVERING] [bit] NULL,
	[RESULT] [nvarchar](max) NULL,
	[EXECUTION_EXCEPTION] [nvarchar](max) NULL,
	CONSTRAINT [PK_RSCHED_AUDIT_HISTORY] PRIMARY KEY CLUSTERED 
	(
		[ACTION] ASC,
		[FIRE_INSTANCE_ID] ASC
	)
)
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[RSCHED_JOB_ID_KEY_MAP]') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE [dbo].[RSCHED_JOB_ID_KEY_MAP]
GO

CREATE TABLE [dbo].[RSCHED_JOB_ID_KEY_MAP](
	[ID] [uniqueidentifier] NOT NULL,
	[JOB_NAME] [nvarchar](150) NOT NULL,
	[JOB_GROUP] [nvarchar](150) NOT NULL,
        CONSTRAINT [PK_RSCHED_JOB_ID_KEY_MAP] PRIMARY KEY CLUSTERED 
 	(
		[ID] ASC
	)
)
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[RSCHED_TRIGGER_ID_KEY_MAP]') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE [dbo].[RSCHED_TRIGGER_ID_KEY_MAP]
GO

CREATE TABLE [dbo].[RSCHED_TRIGGER_ID_KEY_MAP](
	[ID] [uniqueidentifier] NOT NULL,
	[TRIGGER_NAME] [nvarchar](150) NOT NULL,
	[TRIGGER_GROUP] [nvarchar](150) NOT NULL,
	CONSTRAINT [PK_RSCHED_TRIGGER_ID_KEY_MAP] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)
)
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[RSCHED_CALENDAR_ID_KEY_MAP]') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE [dbo].[RSCHED_CALENDAR_ID_KEY_MAP]
GO

CREATE TABLE [dbo].[RSCHED_CALENDAR_ID_KEY_MAP](
	[ID] [uniqueidentifier] NOT NULL,
	[CALENDAR_NAME] [nvarchar](150) NOT NULL
	CONSTRAINT [PK_RSCHED_CALENDAR_ID_KEY_MAP] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)
)
GO