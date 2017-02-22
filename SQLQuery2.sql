SELECT FORMAT(CAST(DTIn AS DATETIME),'hh:mm:ss tt') as DATE, 
COUNT(Stat) as TotalInput,
SUM(case Stat when '0' then 1 else 0 end) as JamClear,
SUM(case Stat when '1' then 1 else 0 end) as OKLot,
SUM(case Stat when '2' then 1 else 0 end) as AQLLot, 
SUM(case Stat when '4' then 1 else 0 end) as RJ4Lot, 
SUM(case Stat when '6' then 1 else 0 end) as RJ6Lot, 
SUM(case Stat when '8' then 1 else 0 end) as RJ8Lot, 
SUM(case when Stat='4' OR Stat='6' OR Stat='8' then 1 else 0 end) as AllRJLot 
FROM TblInOut
group by FORMAT(CAST(DTIn AS DATETIME),'hh:mm:ss tt')
