SET QUOTED_IDENTIFIER ON

-- clean all tables
EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL'
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'

DECLARE @tablename nvarchar(max)

DECLARE cur CURSOR FOR
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT IN ('__EFMigrationsHistory')
OPEN cur

FETCH cur into @tablename
WHILE @@FETCH_STATUS = 0
BEGIN
	EXEC('DELETE FROM ' + @tablename)

	IF EXISTS (Select * from sys.identity_columns where object_name(object_id) = @tablename)
	BEGIN
		EXEC('DBCC CHECKIDENT (''' + @tablename + ''', RESEED, 0) WITH NO_INFOMSGS')
	END

    FETCH cur into @tablename
END

CLOSE cur
DEALLOCATE cur

EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
