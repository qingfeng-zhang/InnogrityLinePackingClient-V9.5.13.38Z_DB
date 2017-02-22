
DECLARE @para1 Datetime; --LastIN
--DECLARE @para2 Datetime; --LastOut
--DECLARE @para3 varchar(30); --StationNo
--DECLARE @para4 varchar(30); --Status
--DECLARE @para5 varchar(30); --Reason

SET @para1 = GETDATE();
--SET @para3 = '2';
--SET @para4 = '222'; --NOT USED


UPDATE TblInOut 
SET  Reason = @para5, 
Stat = @para4,
DTOut = @para1
output inserted.FL
WHERE Reason = 0  AND DTOut is NULL AND Stat = 0 AND STnNo = @para3 AND FL <> 'FFFFFFFFFF'
