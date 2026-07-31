-- Migration script to add mGroupMasterId column to mAssetSubCategory2 table
-- This column stores the Asset Category ID (foreign key to mGroupMaster table)
-- It allows saving both AssetCategoryId and AssetSubCategory1Id for Asset Sub Category 2

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'mAssetSubCategory2' 
    AND COLUMN_NAME = 'mGroupMasterId'
)
BEGIN
    ALTER TABLE mAssetSubCategory2 ADD mGroupMasterId INT NULL;
    
    -- Update existing records to set mGroupMasterId based on the related mIteamTypeMaster record
    UPDATE sc2 
    SET sc2.mGroupMasterId = itm.mGroupMasterId
    FROM mAssetSubCategory2 sc2
    INNER JOIN mIteamTypeMaster itm ON sc2.AssetSubCategoryId = itm.mIteamTypeMasterId
    WHERE itm.mGroupMasterId IS NOT NULL;
    
    PRINT 'Column mGroupMasterId added to mAssetSubCategory2 table successfully';
END
ELSE
BEGIN
    PRINT 'Column mGroupMasterId already exists in mAssetSubCategory2 table';
END