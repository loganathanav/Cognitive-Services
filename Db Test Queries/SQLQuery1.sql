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

-- truncate table [dbo].[ZetronTrnFrameTags]

-- select * from [dbo].[ZetronTrnMediaDetails]
-- select * from [dbo].[ZetronMstIncidents]


-- select * from [dbo].[ZetronTrnFrames]

--Update [dbo].[ZetronMstIncidents]
--SET Status=4

