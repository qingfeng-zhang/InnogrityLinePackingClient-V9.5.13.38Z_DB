
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

SELECT FL FROM TblInOut
WHERE Reason = 999 AND STnNo = @para3 AND DTIn > @para1 AND  DTIn < @para2 AND FL <> 'FFFFFFFFFF'