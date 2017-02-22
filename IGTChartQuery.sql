SELECT * FROM(
SELECT CASE
WHEN CAST(DTIn AS TIME) > CAST('08:00:00 PM' AS TIME) AND CAST(DTIn AS TIME) < CAST('11:59:59 PM' AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST('12:00:00 AM' AS TIME) AND CAST(DTIn AS TIME) < CAST('7:59:59 AM' AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END as RANGE,
COUNT(Stat) as TotalInput,
SUM(case Stat when '0' then 1 else 0 end) as JamClear,
SUM(case Stat when '1' then 1 else 0 end) as OKLot,
SUM(case Stat when '2' then 1 else 0 end) as AQLLot, 
SUM(case Stat when '4' then 1 else 0 end) as RJ4Lot, 
SUM(case Stat when '6' then 1 else 0 end) as RJ6Lot, 
SUM(case Stat when '8' then 1 else 0 end) as RJ8Lot, 
SUM(case when Stat='4' OR Stat='6' OR Stat='8' then 1 else 0 end) as AllRJLot 
FROM TblInOut
group by CASE
WHEN CAST(DTIn AS TIME) > CAST('08:00:00 PM' AS TIME) AND CAST(DTIn AS TIME) < CAST('11:59:59 PM' AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST('12:00:00 AM' AS TIME) AND CAST(DTIn AS TIME) < CAST('7:59:59 AM' AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END)Y WHERE RANGE BETWEEN '2016-04-10' AND '2016-04-12'