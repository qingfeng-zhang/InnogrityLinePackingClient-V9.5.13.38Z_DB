SELECT * FROM(
SELECT CASE
WHEN CAST(DTIn AS TIME) > CAST(@Para3 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para4 AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST(@Para5 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para6 AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END as RANGE,
--COUNT(Stat) as TotalInput,
0 as JamClear,
0 as OKLot,
0 as AQLLot, 
0 as AllRJLot,
0 as RJ4Lot, 
0 as RJ6Lot, 
0 as RJ8Lot,
(SUM(case Stat when '1' then 1 else 0 end)*100.0/(COUNT(Stat)-SUM(case Stat when '0' then 1 else 0 end)-SUM(case Stat when '2' then 1 else 0 end))) as Yield
FROM TblInOut
group by CASE
WHEN CAST(DTIn AS TIME) > CAST(@Para3 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para4 AS TIME) THEN CAST(DTIn as date)
WHEN CAST(DTIn AS TIME) > CAST(@Para5 AS TIME) AND CAST(DTIn AS TIME) < CAST(@Para6 AS TIME) THEN CAST(DATEADD(DAY,-1, DTIn) as date)
ELSE '1999-01-01'
END
)Y WHERE RANGE BETWEEN @Para1 AND @Para2 
ORDER BY RANGE ASC
