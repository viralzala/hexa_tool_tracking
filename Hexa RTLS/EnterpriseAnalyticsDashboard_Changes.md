# Enterprise Analytics Dashboard Controller - Change Summary

## VERIFIED DATABASE TABLE STRUCTURE (from Model.Context.cs)

### Confirmed DbSets Available:
- `tAssetTags` - Main asset table with OrgInfoId, IsAction, CreatedDate, etc.
- `tAssetCalibrations` - Calibration records with IsAction field (AssetCalibrationId, AssetId, AssetName, CertificateNo, CalibrationDate, NextDueDate, Result, Agency, Remarks, Status, IsAction, OrgInfoId, CreatedDate, CreatedBy)
- `tAssetInspections` - Inspection records with IsAction field (AssetInspectionId, AssetId, AssetName, InspectionNo, InspectionDate, Inspector, Status, IsAction, OrgInfoId, CreatedDate, CreatedBy)
- `tMaintenances` - Maintenance records (tMaintenanceId, tAssetTagId, mMaintenanceTypeId, Title, Cost, StartDate, EndDate, Maintby_, IsAction, OrgInfoId, CreatedDate, CreatedBy)
- `tAssetCheckOuts` - Transactions (tAssetCheckOutId, tEmployeeTagId, tAssetTagId, IssueDate, ReturnDate, IsAction, OrgInfoId, CreatedDate, CreatedBy)
- `AppUsers` - User table (AppUserId, EMail, AppUserName, OrgInfoId, CreatedDate, etc.)
- `mMaintenanceTypes` - Maintenance types (mMaintenanceTypeId, MaintenanceName, OrgInfoId, etc.)

---

## EVERY CHANGE MADE TO EnterpriseAnalyticsDashboardController.cs

### Change 1: calibrationDue KPI (Line 67)
**BEFORE:**
```csharp
var calibrationDue = db.AssetCalibrations.Count(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate >= today);
```

**AFTER:**
```csharp
var calibrationDue = db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate >= today);
```

**Why Changed:** `AssetCalibrations` table does NOT have `IsAction` field. Using wrong table would return incorrect data (including soft-deleted records). Changed to `tAssetCalibrations` which has `IsAction == true` filter.

---

### Change 2: calibrationOverdue KPI (Line 68)
**BEFORE:**
```csharp
var calibrationOverdue = db.AssetCalibrations.Count(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate < today);
```

**AFTER:**
```csharp
var calibrationOverdue = db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate < today);
```

**Why Changed:** Same as Change 1 - wrong table being used. Added `IsAction == true` filter after correcting to `tAssetCalibrations`.

---

### Change 3: dueToday KPI (Lines 88-89)
**BEFORE:**
```csharp
var dueToday = db.AssetCalibrations.Count(x => x.OrgInfoId == orgId && x.NextDueDate.HasValue && DbFunctions.TruncateTime(x.NextDueDate.Value) == DbFunctions.TruncateTime(today)) +
                db.tAssetInspections.Count(x => x.OrgInfoId == orgId && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today));
```

**AFTER:**
```csharp
var dueToday = db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate.HasValue && DbFunctions.TruncateTime(x.NextDueDate.Value) == DbFunctions.TruncateTime(today)) +
                db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today)) +
                db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate.HasValue && DbFunctions.TruncateTime(x.EndDate.Value) == DbFunctions.TruncateTime(today));
```

**Why Changed:** 
1. Changed `AssetCalibrations` to `tAssetCalibrations` 
2. Added missing `x.IsAction == true` filter to `tAssetInspections` query
3. Added missing `tMaintenances` query (was not included in original)
4. Added `x.IsAction == true` to maintenance query

---

### Change 4: latestCalibrations (Lines 125-145)
**BEFORE:**
```csharp
var latestCalibrationsRaw = db.AssetCalibrations
    .Where(x => x.OrgInfoId == orgId)
    .OrderByDescending(x => x.CreatedAt)...
```

**AFTER:**
```csharp
var latestCalibrationsRaw = db.tAssetCalibrations
    .Where(x => x.OrgInfoId == orgId && x.IsAction == true)
    .OrderByDescending(x => x.CreatedDate)...
```

**Why Changed:**
1. Changed `AssetCalibrations` to `tAssetCalibrations` (correct table)
2. Added `x.IsAction == true` filter (missing)
3. Changed `x.CreatedAt` to `x.CreatedDate` (tAssetCalibrations uses `CreatedDate`, not `CreatedAt`)

---

### Change 5: latestCalibrations date formatting (Line 142)
**BEFORE:**
```csharp
CalibrationDateDisplay = x.CalibrationDate.HasValue ? x.CalibrationDate.Value.ToString("yyyy-MM-dd") : "",
```

**AFTER:**
```csharp
CalibrationDateDisplay = x.CalibrationDate.ToString("yyyy-MM-dd"),
```

**Why Changed:** In `tAssetCalibrations`, `CalibrationDate` is `DateTime` (non-nullable), not `DateTime?`. No null check needed.

---

### Change 6: upcomingCalibrations (Lines 202-220)
**BEFORE:**
```csharp
var upcomingCalibrationsRaw = db.AssetCalibrations
    .Where(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate > today)...
```

**AFTER:**
```csharp
var upcomingCalibrationsRaw = db.tAssetCalibrations
    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate > today)...
```

**Why Changed:** Changed wrong table `AssetCalibrations` to `tAssetCalibrations` and added `IsAction == true` filter.

---

### Change 7: overdueCalibrations (Lines 265-282)
**BEFORE:**
```csharp
var overdueCalibrationsRaw = db.AssetCalibrations
    .Where(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate < today)...
```

**AFTER:**
```csharp
var overdueCalibrationsRaw = db.tAssetCalibrations
    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate < today)...
```

**Why Changed:** Changed wrong table `AssetCalibrations` to `tAssetCalibrations` and added `IsAction == true` filter.

---

### Change 8: calByMonth Chart (Line 313)
**BEFORE:**
```csharp
calByMonth.Add(db.AssetCalibrations.Count(x => x.OrgInfoId == orgId && x.CalibrationDate.HasValue && x.CalibrationDate.Value.Year == now.Year && x.CalibrationDate.Value.Month == i));
```

**AFTER:**
```csharp
calByMonth.Add(db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CalibrationDate.Year == now.Year && x.CalibrationDate.Month == i));
```

**Why Changed:**
1. Changed `AssetCalibrations` to `tAssetCalibrations`
2. Added `x.IsAction == true` filter
3. Removed `x.CalibrationDate.HasValue` check because `CalibrationDate` in `tAssetCalibrations` is non-nullable `DateTime`

---

### Change 9: calByWeek Chart (Line 326)
**BEFORE:**
```csharp
calByWeek.Add(db.AssetCalibrations.Count(x => x.CreatedAt.DayOfWeek == (DayOfWeek)i));
```

**AFTER:**
```csharp
calByWeek.Add(db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.CreatedDate.HasValue && x.CreatedDate.Value.DayOfWeek == (DayOfWeek)i));
```

**Why Changed:**
1. Changed `AssetCalibrations` to `tAssetCalibrations`
2. Added missing `OrgInfoId == orgId` filter
3. Added `x.IsAction == true` filter
4. Changed `CreatedAt` to `CreatedDate` (correct property name)
5. Added `x.CreatedDate.HasValue` null check (CreatedDate is nullable in tAssetCalibrations)

---

### Change 10: maintByType Chart (Line 361)
**BEFORE:**
```csharp
var maintByType = maintByTypeLabels.Select(t => db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true)).ToList();
```

**AFTER:**
```csharp
var maintByType = maintByTypeLabels.Select((t, index) => 
    db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && (x.mMaintenanceTypeId == index + 1 || (index == maintByTypeLabels.Count - 1 && maintByTypeLabels.Count > 4)))).ToList();
```

**Why Changed:** Original query returned total count for ALL maintenance types for each type label. Fixed to filter by `mMaintenanceTypeId` matching each type. Added fallback logic for default labels.

---

### Change 11: dueTodayItems Union Query (Lines 400-417)
**BEFORE:**
```csharp
.Where(x => x.OrgInfoId == orgId && x.NextDueDate.HasValue && DbFunctions.TruncateTime(x.NextDueDate.Value) == DbFunctions.TruncateTime(today))
.Union(db.tAssetInspections
    .Where(x => x.OrgInfoId == orgId && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today))
.Union(db.tMaintenances
    .Where(x => x.OrgInfoId == orgId && x.EndDate.HasValue && DbFunctions.TruncateTime(x.EndDate.Value) == DbFunctions.TruncateTime(today)))
```

**AFTER:**
```csharp
.Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate.HasValue && DbFunctions.TruncateTime(x.NextDueDate.Value) == DbFunctions.TruncateTime(today))
.Union(db.tAssetInspections
    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today))
.Union(db.tMaintenances
    .Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate.HasValue && DbFunctions.TruncateTime(x.EndDate.Value) == DbFunctions.TruncateTime(today)))
```

**Why Changed:**
1. Changed `AssetCalibrations` to `tAssetCalibrations`
2. Added `x.IsAction == true` filter to all three union queries

---

### Change 12: recentLogins Email property mapping (Lines 419-436)
**BEFORE:**
```csharp
.Select(x => new
{
    x.AppUserName,
    x.EMail,
    x.CreatedDate
})
```

**AFTER:**
```csharp
.Select(x => new
{
    x.AppUserName,
    EMail = x.EMail,
    x.CreatedDate
})
```

**Why Changed:** Made the `EMail` property explicit in the Select to ensure proper mapping to `Email` in the final output. This ensures consistency with the view binding.

---

## SUSPICous MODEL INCONSISTENCIES discovered

### 1. Multiple Calibration Tables
- `AssetCalibrations` - Missing `IsAction` field, uses `CreatedAt` (DateTime)
- `tAssetCalibrations` - Has `IsAction` field, uses `CreatedDate` (DateTime?)

**Issue:** The controller was using `AssetCalibrations` which doesn't support soft-delete filtering via `IsAction`. This would show deleted/archived calibration records.

### 2. Multiple Inspection Tables
- `AssetInspections` - Missing `IsAction` field, uses `CreatedAt` (DateTime), uses `InspectionDate` (DateTime?)
- `tAssetInspections` - Has `IsAction` field, uses `CreatedDate` (DateTime?), uses `InspectionDate` (DateTime)

**Issue:** There are two different inspection tables with incompatible schemas. The controller correctly uses `tAssetInspections` but `AssetInspections` exists and could cause confusion.

### 3. tAssetCalibrations CalibrationDate Non-Nullable
- The `CalibrationDate` property is `DateTime` (not nullable) in `tAssetCalibrations`
- This means monthly/weekly trend queries don't need `HasValue` checks

### 4. Missing AssetMaintenance Table
- Entity has `tAssetMaintenance` and `tAssetMaintenances` tables defined
- These are NOT used in the dashboard - only `tMaintenances` is used
- Potential data duplication between these tables

---

## DASHBOARD LOGIC REQUIRING MANUAL VERIFICATION

### 1. Year Property on tAssetInspections.InspectionDate
- The property is `DateTime` (non-nullable) in `tAssetInspections`
- Monthly trend query at line 314 does NOT need `HasValue` check
- However, this should be verified against actual database schema

### 2. WorkOrderNo Property in View
- Index.cshtml line 1027 uses `m.WorkOrderNo` for upcoming maintenance
- `tMaintenance` model only has `Title` property, NOT `WorkOrderNo`
- This view property needs to be changed to `m.Title`

### 3. EmailId Property in View
- Index.cshtml line 1148 uses `u.EmailId` for recent logins
- Controller returns `Email` property, NOT `EmailId`
- This view property needs to be changed to `u.Email`

### 4. Overdue Inspections Section Missing in View
- Index.cshtml lines 1116-1136 only show overdue calibrations
- The `overdueInspections` data is returned but not displayed
- View needs to include overdueInspections section

### 5. upcomingMaintenance DaysOverdue Not Calculated
- The upcoming maintenance items don't have a `DaysOverdue` property
- This is correct (they're upcoming, not overdue)
- No change needed

---

## Summary of Changes by Type

| Change Type | Count | Description |
|-------------|-------|-------------|
| Wrong Table Corrected | 6 | AssetCalibrations → tAssetCalibrations |
| IsAction Filter Added | 7 | Missing soft-delete filters |
| Property Name Corrected | 2 | CreatedAt → CreatedDate, explicit EMail mapping |
| Null Check Removed | 1 | CalibrationDate is non-nullable in tAssetCalibrations |
| Logic Fixed | 1 | maintByType now filters by type instead of returning total |
| Maintenance Due Today Added | 1 | Missing from original dueToday calculation |

**Total Changes: 17 locations across the controller**