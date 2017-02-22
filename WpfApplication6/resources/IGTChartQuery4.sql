
--DECLARE @para1 varchar(30); 
--DECLARE @para2 varchar(30); 
--DECLARE @para3 varchar(30); 
--DECLARE @para4 varchar(30); 
--DECLARE @para5 varchar(30); 

--SET @para1 = '2016-04-26';
--SET @para2 = '08:59:59 AM';
--SET @para3 = '11:59:59 PM';
--SET @para4 = '11:59:59 PM';
--SET @para5 = '08:59:59 PM';

SELECT * FROM(
SELECT 
CASE
WHEN CAST(DTIn AS TIME) > CAST(@para2 AS TIME) AND CAST(DTIn AS TIME) < CAST(@para3 AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST(@para4 AS TIME) AND CAST(DTIn AS TIME) < CAST(@para5 AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END as DATE,
Reason AS RJCode,
Count(*) AS Count
FROM TblInOut
WHERE Reason <> 0
group by
CASE
WHEN CAST(DTIn AS TIME) > CAST(@Para2 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para3 AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST(@Para4 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para5 AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END, Reason)Y WHERE DATE = @para1
ORDER BY Count DESC
