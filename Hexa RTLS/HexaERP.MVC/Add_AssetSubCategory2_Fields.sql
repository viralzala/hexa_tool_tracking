-- Migration for Asset Sub Category 2 master and Add Asset page updates
-- 1. Create mAssetSubCategory2 table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.mAssetSubCategory2') AND type = 'U')
BEGIN
    CREATE TABLE dbo.mAssetSubCategory2 (
        AssetSubCategory2Id INT IDENTITY(1,1) NOT NULL,
        AssetSubCategoryId INT NOT NULL,
        AssetSubCategory2Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy NVARCHAR(100) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_mAssetSubCategory2 PRIMARY KEY CLUSTERED (AssetSubCategory2Id),
        CONSTRAINT FK_mAssetSubCategory2_mIteamTypeMaster FOREIGN KEY (AssetSubCategoryId)
            REFERENCES dbo.mIteamTypeMaster(mIteamTypeMasterId)
    )
    PRINT 'Created mAssetSubCategory2 table'
END

-- 2. Add AssetSubCategory2Id to tAssetTag
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.tAssetTag') AND name = 'AssetSubCategory2Id')
BEGIN
    ALTER TABLE dbo.tAssetTag ADD AssetSubCategory2Id INT NULL
    PRINT 'Added AssetSubCategory2Id to tAssetTag'
END

PRINT 'Migration completed successfully.'
