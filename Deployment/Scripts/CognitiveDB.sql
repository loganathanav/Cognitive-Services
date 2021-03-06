SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ZetronMstIncidents](
	[IncidentID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](255) NOT NULL,
	[Description] [varchar](255) NULL,
	[ReportedOn] [datetime] NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_ZetronMstIncidents] PRIMARY KEY CLUSTERED 
(
	[IncidentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ZetronTrnFrames](
	[FrameID] [int] IDENTITY(1,1) NOT NULL,
	[MediaID] [int] NOT NULL,
	[FrameTime] [datetime] NOT NULL
 CONSTRAINT [PK_ZetronTrnFrames] PRIMARY KEY CLUSTERED 
(
	[FrameID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ZetronTrnFrameTags](
	[TagID] [int] IDENTITY(1,1) NOT NULL,
	[FrameID] [int] NOT NULL,
	[Tag] [varchar](50) NOT NULL,
	[ConfidenceLevel] [decimal(9,8)] NOT NULL
 CONSTRAINT [PK_ZetronTrnFrameTags] PRIMARY KEY CLUSTERED 
(
	[TagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ZetronTrnMediaDetails]    Script Date: 9/25/2017 2:27:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ZetronTrnMediaDetails](
	[MediaID] [int] IDENTITY(1,1) NOT NULL,
	[IncidentID] [int] NOT NULL,
	[Name] [varchar](60) NOT NULL,
	[MediaURL] [varchar](max) NOT NULL,
	[MediaSummaryURL] [varchar](max) NULL,
	[MediaType] [int] NOT NULL,
	[PostedOn] [datetime] NOT NULL,
	[PostedBy] [varchar](50) NOT NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_ZetronTrnMediaDetails] PRIMARY KEY CLUSTERED 
(
	[MediaID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ZetronMstIncidentStatus] (
    [StatusID]  INT  PRIMARY KEY,
    [Status]    VARCHAR (60) NOT NULL
);
GO
SET ANSI_PADDING OFF
GO

INSERT [dbo].[ZetronMstIncidentStatus]
VALUES
(1, 'Initiated'),
(2, 'Started'),
(3, 'Processing'),
(4, 'Stopped'),
(5, 'Deactivated')
GO


CREATE TABLE [dbo].[ZetronTrnDroneLocations] (
    [LocationID]    INT            IDENTITY (1, 1) NOT NULL,
    [MediaID]       INT            NOT NULL,
    [Temperature]   DECIMAL (9, 2) NULL,
    [Humidity]      DECIMAL (9, 2) NULL,
    [WindSpeed]     DECIMAL (9, 2) NULL,
    [DewPoint]      DECIMAL (9, 2) NULL,
    [Summary]       VARCHAR (250)  NULL,
    [WindDirection] VARCHAR (250)  NULL,
    [Longitude] DECIMAL(9, 6) NULL, 
    [Latitude] DECIMAL(9, 6) NULL
);
GO

ALTER TABLE [dbo].[ZetronTrnDroneLocations]
ADD CONSTRAINT [PK_ZetronTrnDroneLocations] PRIMARY KEY CLUSTERED ([LocationID] ASC);
GO

ALTER TABLE [dbo].[ZetronTrnDroneLocations]  WITH CHECK ADD  CONSTRAINT [FK_ZetronTrnDroneLocations_ZetronTrnMediaDetails] FOREIGN KEY([MediaID])
REFERENCES [dbo].[ZetronTrnMediaDetails] ([MediaID])
GO

ALTER TABLE [dbo].[ZetronTrnDroneLocations] CHECK CONSTRAINT [FK_ZetronTrnDroneLocations_ZetronTrnMediaDetails]
GO

ALTER TABLE [dbo].[ZetronMstIncidents]  WITH CHECK ADD  CONSTRAINT [FK_ZetronMstIncidents_ZetronMstIncidentStatus] FOREIGN KEY([Status])
REFERENCES [dbo].[ZetronMstIncidentStatus] ([StatusID])
GO
ALTER TABLE [dbo].[ZetronMstIncidents] CHECK CONSTRAINT [FK_ZetronMstIncidents_ZetronMstIncidentStatus]
GO

ALTER TABLE [dbo].[ZetronTrnFrames]  WITH CHECK ADD  CONSTRAINT [FK_ZetronTrnFrames_ZetronTrnMediaDetails] FOREIGN KEY([MediaID])
REFERENCES [dbo].[ZetronTrnMediaDetails] ([MediaID])
GO
ALTER TABLE [dbo].[ZetronTrnFrames] CHECK CONSTRAINT [FK_ZetronTrnFrames_ZetronTrnMediaDetails]
GO

ALTER TABLE [dbo].[ZetronTrnFrameTags]  WITH CHECK ADD  CONSTRAINT [FK_ZetronTrnFrameTags_ZetronTrnFrames] FOREIGN KEY([FrameID])
REFERENCES [dbo].[ZetronTrnFrames] ([FrameID])
GO
ALTER TABLE [dbo].[ZetronTrnFrameTags] CHECK CONSTRAINT [FK_ZetronTrnFrameTags_ZetronTrnFrames]
GO

ALTER TABLE [dbo].[ZetronTrnMediaDetails]  WITH CHECK ADD  CONSTRAINT [FK_ZetronTrnMediaDetails_ZetronMstIncidents] FOREIGN KEY([IncidentID])
REFERENCES [dbo].[ZetronMstIncidents] ([IncidentID])
GO
ALTER TABLE [dbo].[ZetronTrnMediaDetails] CHECK CONSTRAINT [FK_ZetronTrnMediaDetails_ZetronMstIncidents]
GO

CREATE TYPE [dbo].[FrameTag] AS TABLE
(
	MediaId INT, 
	FrameTime DateTime, 
	Tag Varchar(50),
	ConfidenceLevel decimal(9,8)
)
GO


CREATE PROCEDURE AddTags(@FrameTags FrameTag READONLY)
AS
BEGIN

	DECLARE @TempFrameTags Table
	(
		MediaId INT, 
		FrameTime DateTime, 
		Tag Varchar(50),
		ConfidenceLevel decimal(9,8)
	)

	DECLARE @FrameID INT
	DECLARE @MediaID INT
	DECLARE @FrameTime DateTime

	INSERT INTO @TempFrameTags
	SELECT MediaId, FrameTime, Tag, ConfidenceLevel FROM @FrameTags

	WHILE (EXISTS(SELECT 1 FROM @TempFrameTags))
	BEGIN
		SELECT TOP 1 @MediaID=MediaId, @FrameTime=FrameTime FROM @TempFrameTags

		INSERT INTO [dbo].[ZetronTrnFrames] VALUES(@MediaID, @FrameTime)
		
		SELECT @FrameID = @@IDENTITY

		INSERT INTO [dbo].[ZetronTrnFrameTags]
		SELECT @FrameID, Tag, ConfidenceLevel  FROM @TempFrameTags WHERE FrameTime=@FrameTime

		DELETE FROM @TempFrameTags WHERE FrameTime=@FrameTime
	END
END
GO

IF object_id(N'dbo.TagCount', 'V') IS NOT NULL
	DROP VIEW dbo.TagCount
GO

CREATE VIEW dbo.TagCount AS
SELECT F.MediaID, T.Tag, Count(T.Tag) AS TagCount
FROM [dbo].[ZetronTrnFrameTags]  T WITH(NOLOCK) 
INNER JOIN [dbo].[ZetronTrnFrames] F WITH(NOLOCK) ON T.FrameID = F.FrameID
INNER JOIN [dbo].[ZetronTrnMediaDetails] M WITH(NOLOCK) ON F.MediaID = M.MediaID
INNER JOIN [dbo].[ZetronMstIncidents] I WITH(NOLOCK) ON M.IncidentID = I.IncidentID
WHERE I.[Status] < 5 AND T.ConfidenceLevel > 0.7
GROUP BY T.Tag, F.MediaID
GO

CREATE PROCEDURE GetTagCountByMediaId(@MediaID INT)
AS
BEGIN
	DECLARE @MaxFrameTime DATETIME, @FrameId INT
	SELECT @MaxFrameTime= MAX(FrameTime)  FROM [ZetronTrnFrames] WITH(NOLOCK) WHERE MediaID=@MediaID
	SELECT @FrameId=FrameId FROM [ZetronTrnFrames] WITH(NOLOCK) WHERE FrameTime=@MaxFrameTime
	
	SELECT Tag, TagCount
	INTO #t
	FROM dbo.TagCount
	WHERE MediaID = @MediaID

	SELECT ROW_NUMBER() OVER(ORDER BY TagType) Id, * FROM 
	(
	-- SELECT TOP 1 'Fire' As TagType, Tag, TagCount
	-- FROM #t
	-- WHERE Tag IN ('Small Fire', 'Medium Fire', 'Large Fire')	
	-- UNION ALL
	-- SELECT TOP 1 'Smoke' As TagType, Tag, TagCount
	-- FROM #t
	-- WHERE Tag IN ('White Smoke', 'Gray Smoke', 'Black Smoke')	
	-- UNION ALL
	
	Select 'Fire' As TagType, Tag, 1 AS TagCount 
	FROM (SELECT TOP 1 Tag, Max(ConfidenceLevel) ConfidenceLevel from [ZetronTrnFrameTags] WITH(NOLOCK) where FrameId=@FrameId and Tag IN ('Small Fire', 'Medium Fire', 'Large Fire')	
	GROUP BY Tag ORDER BY ConfidenceLevel DESC) FireState
	UNION ALL 
	Select 'Smoke' As TagType, Tag, 1 AS TagCount 
	FROM (SELECT TOP 1 Tag, Max(ConfidenceLevel) ConfidenceLevel from [ZetronTrnFrameTags] WITH(NOLOCK) where FrameId=@FrameId and Tag IN ('White Smoke', 'Gray Smoke', 'Black Smoke')	
	GROUP BY Tag ORDER BY ConfidenceLevel DESC) SmokeState
	UNION ALL
	SELECT 'Tag' As TagType, Tag, TagCount
	FROM #t
	WHERE Tag NOT IN ('Small Fire', 'Medium Fire', 'Large Fire', 'White Smoke', 'Gray Smoke', 'Black Smoke')) T	
END
GO