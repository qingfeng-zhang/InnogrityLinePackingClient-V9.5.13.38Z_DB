SELECT * FROM(
SELECT CASE
WHEN CAST(DTIn AS TIME) > CAST(@Para3 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para4 AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST(@Para5 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para6 AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END as RANGE,
--COUNT(Stat) as TotalInput,
SUM(case Stat when '0' then 1 else 0 end) as JamClear,
0 as OKLot,
0 as AQLLot, 
0 as AllRJLot,
SUM(case Stat when '4' then 1 else 0 end) as RJ4Lot, 
SUM(case Stat when '6' then 1 else 0 end) as RJ6Lot, 
SUM(case Stat when '8' then 1 else 0 end) as RJ8Lot
FROM TblInOut
group by CASE
WHEN CAST(DTIn AS TIME) > CAST(@Para3 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para4 AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST(@Para5 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para6 AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END
)Y WHERE RANGE BETWEEN @Para1 AND @Para2 
ORDER BY RANGE ASC
