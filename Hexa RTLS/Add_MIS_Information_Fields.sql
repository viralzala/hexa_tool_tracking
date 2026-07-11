-- SQL Script to add MIS Information fields to tAssetTag table
-- Execute this script manually in SQL Server Management Studio
-- Date: 2026-07-11

-- Add new fields for MIS Information tab
ALTER TABLE tAssetTag
ADD
    WorkOrderNumber NVARCHAR(25) NULL,
    MISNumber NVARCHAR(25) NULL,
    PartNumber NVARCHAR(25) NULL,
    PartName NVARCHAR(25) NULL,
    LaunchDate DATETIME NULL,
    EndDate DATETIME NULL;

-- Note: The following fields already exist in the table:
-- - BLETAGNo (Nullable<int>)
-- - BatteryLevel (Nullable<int>)
-- - PlantName (string, MaxLength 25)
-- - Program (string, MaxLength 25)
-- - Module (string, MaxLength 25)
-- - BuildingName (string, MaxLength 25)
-- - CurrentLocation (string, MaxLength 25)

-- Verify the changes
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tAssetTag'
    AND COLUMN_NAME IN (
        'WorkOrderNumber',
        'MISNumber',
        'PartNumber',
        'PartName',
        'LaunchDate',
        'EndDate',
        'BLETAGNo',
        'BatteryLevel',
        'PlantName',
        'Program',
        'Module',
        'BuildingName',
        'CurrentLocation'
    )
ORDER BY COLUMN_NAME;