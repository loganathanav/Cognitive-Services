 -- select avg(ConfidenceLevel) from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK)

 
 --select * from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%smoke%'
 --select * from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%Fire%'
 --select AVG(ConfidenceLevel) AS LargeFire from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%Large Fire%'
 --select AVG(ConfidenceLevel) AS MediumFire from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%Medium Fire%'
 --select AVG(ConfidenceLevel) AS SmallFire from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%Small Fire%'
 
 
 
 --declare @SmokeLevel table(
 --Name VARCHAR(50),
 --Level decimal(9,8)
 --)

 --Declare @BlackSmoke decimal(9,8),@WhiteSmoke decimal(9,8),@GraySmoke decimal(9,8)
 -- select @BlackSmoke=AVG(ConfidenceLevel) from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%Black Smoke%'
 --select @WhiteSmoke=AVG(ConfidenceLevel)  from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%White Smoke%'
 --select @GraySmoke= AVG(ConfidenceLevel) from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) where tag like '%Gray Smoke%'
 --INSERT @SmokeLevel VALUES('Black Smoke',@BlackSmoke)
 --INSERT @SmokeLevel VALUES('White Smoke',@WhiteSmoke)
 --INSERT @SmokeLevel VALUES('Gray Smoke',@GraySmoke)
 --select  Name, Max(Level) AS SmokeLevel FROM @SmokeLevel GROUP BY Name Order By Max(Level) Desc

-- select * from [dbo].[ZetronMstIncidents] WITH (NOLOCK) 
-- delete from [dbo].[ZetronMstIncidents] where incidentid>36
-- truncate table [dbo].[ZetronMstIncidents]
-- select * from [dbo].[ZetronMstIncidents] WITH (NOLOCK)  WHERE IncidentId=134
-- Update [dbo].[ZetronMstIncidents] SET Status=4  WHERE IncidentId=134
-- select * from [dbo].[ZetronTrnMediaDetails] WITH(NOLOCK) WHERE IncidentId=136
-- delete from [dbo].[ZetronMstIncidents] Where IncidentId>139

--Update [dbo].[ZetronTrnMediaDetails]
--Set [Name]='ZetronDemo1.mp4'  WHERE MediaID=106

-- ALTER TABLE [dbo].[ZetronTrnMediaDetails] ADD [Name] VARCHAR(60)
-- 
--Update [dbo].[ZetronTrnMediaDetails]
--Set MediaURL=
--'https://zetron.blob.core.windows.net/demo1/ZetronDemo1.mp4'  WHERE IncidentId=110
-- truncate table dbo.ZetronTrnMediaDetails

-- select * from [dbo].[ZetronTrnFrames] with(nolock)

-- select top 10* from [dbo].[ZetronTrnFrames] with(nolock)
-- delete from  dbo.ZetronTrnFrames

-- select * from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK)
-- truncate table [dbo].[ZetronTrnFrameTags]

-- 2.50540422E-10
-- select * from [dbo].[ZetronTrnFrameTags] WITH(NOLOCK) WHERE ConfidenceLevel>0.7 Order by ConfidenceLevel DESC
-- 426

-- Update [dbo].[ZetronMstIncidents] SET Status=4

--Fire


--DECLARE @mediaId INT = 67

--SELECT Count(T.Tag) AS TagCount, T.Tag FROM [dbo].[ZetronTrnFrameTags]  T WITH(NOLOCK) INNER JOIN  [dbo].[ZetronTrnFrames] F WITH(NOLOCK) ON T.FrameID=F.FrameID
--WHERE T.ConfidenceLevel>0.7 AND  F.MediaID=@mediaId 
--GROUP BY T.Tag 
--ORDER BY TagCount DESC



--INSERT [dbo].[ZetronTrnDroneLocations] VALUES(109, 61.55, 0.7, 0.95, 51.5, 'Partially cloudy')

--Update [dbo].[ZetronTrnDroneLocations] set
--WindDirection='North East',
--Longitude=-12.424451,
--Latitude=39.658745


--select * from [dbo].[ZetronTrnDroneLocations]  with(nolock)