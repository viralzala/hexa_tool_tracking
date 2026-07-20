-- Migration for Asset Information tab changes
-- 1. Add FK column to mIteamTypeMaster for cascading dropdowns (Asset Category -> Asset Sub Category 1)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.mIteamTypeMaster') AND name = 'mGroupMasterId')
BEGIN
    ALTER TABLE dbo.mIteamTypeMaster ADD mGroupMasterId INT NULL
    PRINT 'Added mGroupMasterId to mIteamTypeMaster'
END

-- 2. Add new columns to tAssetTag
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tAssetTag') AND name = 'ToolId')
BEGIN
    ALTER TABLE dbo.tAssetTag ADD ToolId NVARCHAR(100) NULL
    PRINT 'Added ToolId to tAssetTag'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tAssetTag') AND name = 'ToolName')
BEGIN
    ALTER TABLE dbo.tAssetTag ADD ToolName NVARCHAR(200) NULL
    PRINT 'Added ToolName to tAssetTag'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tAssetTag') AND name = 'AssignedLocation')
BEGIN
    ALTER TABLE dbo.tAssetTag ADD AssignedLocation NVARCHAR(200) NULL
    PRINT 'Added AssignedLocation to tAssetTag'
END

PRINT 'Migration completed successfully.'
