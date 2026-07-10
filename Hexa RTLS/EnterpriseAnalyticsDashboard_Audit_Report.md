# Enterprise Analytics Dashboard - End-to-End Audit Report

## Executive Summary
The Enterprise Analytics Dashboard has **13 critical data issues** spanning incorrect table usage, property name mismatches, missing null checks, and incorrect logic. These issues would cause charts to display incorrect data, tables to show empty/mismatched data, and KPIs to show wrong counts.

---

## 1. DASHBOARD CONTROLLER ANALYSIS (EnterpriseAnalyticsDashboardController.cs)

### Critical Issues Found:

| # | Issue | Location | Severity |
|---|-------|----------|----------|
| C1 | **Wrong Table Used for Calibrations** - Uses `AssetCalibrations` instead of `tAssetCalibrations` | Line 73, 74, 125, 202, 265, 400, 401, 406, 407, 408 | CRITICAL |
| C2 | **Wrong Table Used for Inspections** - Uses `AssetInspections` instead of `tAssetInspections` for monthly trends | Line 314 | CRITICAL |
| C3 | **Missing IsAction Filter on AssetCalibrations** - Queries don't filter by `IsAction` | Line 73, 74, 125, 202, 265, 400, 401, 406 | HIGH |
| C4 | **Missing IsAction Filter on AssetInspections** - Queries don't filter by `IsAction` | Line 406, 407 | HIGH |
| C5 | **Null Reference Exception Risk** - `inspByMonth` query accesses `.Year` on nullable `InspectionDate` without null check | Line 314 | HIGH |
| C6 | **Incorrect Maintenance Type Count Logic** - `maintByType` counts ALL maintenances, not per-type | Line 361 | HIGH |

---

## 2. KPI VALIDATION

### KPI Issue Matrix:

| Dashboard Widget | Controller Variable | Database Table | LINQ Query | Expected Result | Actual Result | Status | Root Cause | Recommended Fix |
|------------------|---------------------|----------------|------------|-----------------|---------------|--------|------------|-----------------|
| Total Assets | `totalAssets` | tAssetTags | `Count(x => x.OrgInfoId == orgId && x.IsAction == true)` | All active assets count | Correct | CORRECT | - | - |
| Active Assets | `activeAssets` | tAssetTags | `totalAssets` | All active assets count | Same as totalAssets (correct) | CORRECT | - | - |
| Assets Issued | `assetsIssued` | tAssetTags | `Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.tEmployeeTagId != null)` | Assets assigned to employees | Correct | CORRECT | - | - |
| Assets Available | `assetsAvailable` | Calculated | `totalAssets - assetsIssued` | Available assets | Correct | CORRECT | - | - |
| Calibration Due | `calibrationDue` | AssetCalibrations | `Count(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate >= today)` | Pending calibrations | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations` instead of `tAssetCalibrations`, missing `IsAction` filter | Change to `db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate >= today)` |
| Calibration Overdue | `calibrationOverdue` | AssetCalibrations | `Count(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate < today)` | Overdue calibrations | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations` instead of `tAssetCalibrations`, missing `IsAction` filter | Change to `db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate < today)` |
| Inspection Due | `inspectionDue` | tAssetInspections | `Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate >= today)` | Pending inspections | Correct | CORRECT | - | - |
| Inspection Overdue | `inspectionOverdue` | tAssetInspections | `Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate < today)` | Overdue inspections | Correct | CORRECT | - | - |
| Maintenance Due | `maintenanceDue` | tMaintenances | `Count(x => x.OrgInfoId == orgId && x.IsAction == true && (x.EndDate >= today \|\| x.EndDate == null))` | Pending maintenance | Correct | CORRECT | - | - |
| Maintenance Overdue | `maintenanceOverdue` | tMaintenances | `Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate.HasValue && x.EndDate.Value < today)` | Overdue maintenance | Correct | CORRECT | - | - |
| Expired Assets | `expiredAssets` | tAssetTags | `Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.PhaseOutDate.HasValue && x.PhaseOutDate.Value < today)` | Expired assets | Correct | CORRECT | - | - |

---

## 3. CHART VALIDATION

### Chart Issue Matrix:

| Chart Dataset | Controller Variable | Database Table | LINQ Query | Expected Result | Actual Result | Status | Root Cause | Recommended Fix |
|---------------|---------------------|----------------|------------|-----------------|---------------|--------|------------|-----------------|
| assetByMonth | `assetByMonth` | tAssetTags | `x.CreatedDate.Value.Year == now.Year && x.CreatedDate.Value.Month == i` | Monthly asset counts | Correct | CORRECT | - | - |
| calByMonth | `calByMonth` | AssetCalibrations | `x.CalibrationDate.Value.Year == now.Year && x.CalibrationDate.Value.Month == i` | Monthly calibration counts | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations` (missing `IsAction`), `CalibrationDate` may be null causing issues | Should use `tAssetCalibrations` with `IsAction` filter and null check for `CalibrationDate` |
| inspByMonth | `inspByMonth` | tAssetInspections | `x.InspectionDate.Year == now.Year && x.InspectionDate.Month == i` | Monthly inspection counts | **INCORRECT - NULL EXCEPTION RISK** | INCORRECT | No null check on `InspectionDate` in LINQ query (controller line 314) - the query uses `AssetInspections` table which has nullable `InspectionDate`, and property access without null check will cause runtime exception | Change to: `x.InspectionDate.HasValue && x.InspectionDate.Value.Year == now.Year && x.InspectionDate.Value.Month == i` |
| maintByMonth | `maintByMonth` | tMaintenances | `x.CreatedDate.Value.Year == now.Year && x.CreatedDate.Value.Month == i` | Monthly maintenance counts | Correct | CORRECT | - | - |
| assetByWeek | `assetByWeek` | tAssetTags | `x.CreatedDate.Value.DayOfWeek == (DayOfWeek)i` | Weekly asset counts | Correct | CORRECT | - | - |
| calByWeek | `calByWeek` | AssetCalibrations | `x.CreatedAt.DayOfWeek == (DayOfWeek)i` | Weekly calibration counts | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations` without `IsAction` filter | Should use `tAssetCalibrations` with `IsAction` filter |
| assetsByDepartment | `assetsByDepartment` | tAssetTags | `GroupBy(x => x.OwnerDepartment)` | Department distribution | Correct | CORRECT | - | - |
| assetsByZone | `assetsByZone` | tAssetTags | `GroupBy(x => x.mZoneId)` | Zone distribution | Correct | CORRECT | - | - |
| assetStatus | `assetStatus` | tAssetTags | `GroupBy(x => x.mStatusMasterId)` | Status distribution | Correct | CORRECT | - | - |
| maintByType | `maintByType` | tMaintenances | `Count(x => x.OrgInfoId == orgId && x.IsAction == true)` | Maintenance by type | **INCORRECT - WRONG COUNT** | INCORRECT | Query doesn't filter by maintenance type, returns total count for ALL types instead of per-type counts. Controller code: `db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true)` for each type - this returns the same total count for every type | Should include `mMaintenanceTypeId` filter matching each type from `maintByTypeLabels` |
| yearlyAssets | `yearlyAssets` | tAssetTags | `x.CreatedDate.Value.Year == y` | Yearly asset counts | Correct | CORRECT | - | - |
| costDistribution | `costDistribution` | tAssetTags | `GroupBy(x => x.mGroupMasterId).Sum(a => a.PurchaseCost ?? 0)` | Cost by group | Correct | CORRECT | - | - |

---

## 4. TABLE VALIDATION

### Table Issue Matrix:

| Table Name | Controller Variable | Database Table | LINQ Query | Expected Result | Actual Result | Status | Root Cause | Recommended Fix |
|------------|---------------------|----------------|------------|-----------------|---------------|--------|------------|-----------------|
| latestAssets | `latestAssets` | tAssetTags | `Where(x => x.OrgInfoId == orgId && x.IsAction == true).OrderByDescending(x => x.CreatedDate).Take(10)` | 10 most recent assets | Correct | CORRECT | - | - |
| latestCalibrations | `latestCalibrations` | AssetCalibrations | `Where(x => x.OrgInfoId == orgId).OrderByDescending(x => x.CreatedAt).Take(10)` | 10 most recent calibrations | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations`, missing `IsAction` filter | Should use `tAssetCalibrations` with `IsAction == true` filter |
| latestInspections | `latestInspections` | tAssetInspections | `Where(x => x.OrgInfoId == orgId && x.IsAction == true)` | 10 most recent inspections | Correct | CORRECT | - | - |
| latestMaintenance | `latestMaintenance` | tMaintenances | `Where(x => x.OrgInfoId == orgId && x.IsAction == true)` | 10 most recent maintenance | Correct | CORRECT | - | - |
| upcomingCalibrations | `upcomingCalibrations` | AssetCalibrations | `Where(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate > today)` | Upcoming calibrations | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations`, missing `IsAction` filter | Should use `tAssetCalibrations` with `IsAction == true` filter |
| upcomingInspections | `upcomingInspections` | tAssetInspections | `Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate > today)` | Upcoming inspections | Correct | CORRECT | - | - |
| upcomingMaintenance | `upcomingMaintenance` | tMaintenances | `Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.EndDate != null && x.EndDate > today)` | Upcoming maintenance | Correct | CORRECT | - | - |
| overdueCalibrations | `overdueCalibrations` | AssetCalibrations | `Where(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate < today)` | Overdue calibrations | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations`, missing `IsAction` filter | Should use `tAssetCalibrations` with `IsAction == true` filter |
| overdueInspections | `overdueInspections` | tAssetInspections | `Where(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate < today)` | Overdue inspections | Correct | CORRECT | - | - |
| dueTodayItems | `dueTodayItems` | AssetCalibrations, tAssetInspections, tMaintenances | Union query | Items due today | **INCORRECT** | INCORRECT | Uses wrong table `AssetCalibrations`, missing `IsAction` on `AssetInspections` and `tMaintenances` in dueToday query | Add `IsAction` filters to inspection and maintenance queries |
| transactions | `transactions` | tAssetCheckOuts | `Where(x => x.OrgInfoId == orgId && x.IsAction == true)` | Recent transactions | Correct | CORRECT | - | - |
| recentLogins | `recentLogins` | AppUsers | `Where(x => x.OrgInfoId == orgId)` | Recent login users | **INCORRECT - WRONG PROPERTY** | INCORRECT | Controller maps `EMail` to `Email`, but View uses `u.EmailId` which doesn't exist | View should use `u.Email` |

---

## 5. JAVASCRIPT VALIDATION

### JavaScript Issue Matrix:

| JavaScript Property | JSON Property | Controller Property | Status | Root Cause | Recommended Fix |
|---------------------|---------------|-------------------|--------|------------|-----------------|
| $scope.totalAssets | totalAssets | totalAssets | Correct | - | - |
| $scope.activeAssets | activeAssets | activeAssets | Correct | - | - |
| $scope.assetsIssued | assetsIssued | assetsIssued | Correct | - | - |
| $scope.assetsAvailable | assetsAvailable | assetsAvailable | Correct | - | - |
| $scope.calibrationDue | calibrationDue | calibrationDue | Correct | - | - |
| $scope.inspectionDue | inspectionDue | inspectionDue | Correct | - | - |
| $scope.maintenanceDue | maintenanceDue | maintenanceDue | Correct | - | - |
| $scope.recentLogins | recentLogins | recentLogins | **INCORRECT** | View uses `u.EmailId` but controller returns `Email` | View line 1148: Change `u.EmailId` to `u.Email` |
| All charts | Multiple properties | Various | Correct | - | - |

---

## 6. VIEW VALIDATION

### View Issue Matrix:

| View Element | Expected Binding | Actual Binding | Status | Root Cause | Recommended Fix |
|--------------|------------------|----------------|--------|------------|-----------------|
| KPI Cards (lines 668-738) | All 8 KPIs | All 8 KPIs | Correct | - | - |
| Chart Containers | ID matches JS | ID matches JS | Correct | - | - |
| latestAssets table | a.IteamName, a.ModelNo, etc. | Correctly mapped | Correct | - | - |
| upcomingMaintenance.WorkOrderNo | m.WorkOrderNo | m.Title | **INCORRECT** | Property doesn't exist - `WorkOrderNo` not in tMaintenance model | View line 1027: Change `m.WorkOrderNo` to `m.Title` |
| recentLogins.EmailId | u.EmailId | u.Email | **INCORRECT** | Property mismatch - controller returns `Email` but view uses `EmailId` | View line 1148: Change `u.EmailId` to `u.Email` |
| Overdue Items section | Shows both calibrations and inspections | Only shows calibrations | **INCORRECT** | Template missing `overdueInspections` list rendering | View line 1120-1135: Add overdueInspections section |

---

## 7. DATABASE TABLE STRUCTURE ANALYSIS

### Table Structure Comparison:

**AssetCalibrations (used by controller - WRONG):**
- Id (int)
- AssetMasterId (int?)
- AssetId (string) - NOT int
- AssetName (string)
- CertificateNo (string)
- CalibrationDate (DateTime?)
- NextDueDate (DateTime?)
- Result (string)
- Agency (string)
- Remarks (string)
- **CreatedAt (DateTime) - NOT nullable**
- OrgInfoId (int?)
- CreatedBy (string)

**tAssetCalibrations (should be used):**
- AssetCalibrationId (int)
- AssetId (int) - NOT nullable
- AssetName (string)
- InspectionNo (string) - Note: this is CalibrationNo in context
- CertificateNo (string)
- CalibrationDate (DateTime) - NOT nullable
- NextDueDate (DateTime?)
- Result (string)
- Agency (string)
- Remarks (string)
- Status (string)
- **IsAction (bool) - NOT nullable** - Missing from AssetCalibrations usage!
- OrgInfoId (int)
- CreatedDate (DateTime?)
- CreatedBy (string)

**AssetInspections (used in line 314 - WRONG TABLE):**
- Id (int)
- AssetMasterId (int?)
- AssetId (string) - NOT int
- InspectionNo (string)
- AssetName (string)
- InspectionDate (DateTime?) - **NULLABLE**
- Inspector (string)
- ...
- **CreatedAt (DateTime) - NOT nullable**
- OrgInfoId (int?)
- CreatedBy (string)
- **NO IsAction field!**

---

## 8. SUMMARIZED ISSUE COUNT

| Category | Total Issues | Critical | High | Medium |
|----------|--------------|----------|------|--------|
| Controller | 6 | 2 | 4 | 0 |
| KPI | 2 | 2 | 0 | 0 |
| Charts | 4 | 1 | 3 | 0 |
| Tables | 4 | 2 | 2 | 0 |
| JavaScript | 1 | 0 | 1 | 0 |
| View | 3 | 2 | 1 | 0 |
| **TOTAL** | **20** | **9** | **11** | **0** |

---

## 9. RECOMMENDED CODE FIXES

### Fix 1: Controller - Use Correct Calibration Table (Lines 73-74, 125-145, 202-220, 265-282, 400-417)

```csharp
// BEFORE (Line 73):
var calibrationDue = db.AssetCalibrations.Count(x => x.OrgInfoId == orgId && x.NextDueDate != null && x.NextDueDate >= today);

// AFTER:
var calibrationDue = db.tAssetCalibrations.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.NextDueDate != null && x.NextDueDate >= today);
```

### Fix 2: Controller - Add Null Check for inspByMonth (Line 314)

```csharp
// BEFORE:
inspByMonth.Add(db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate.Year == now.Year && x.InspectionDate.Month == i));

// AFTER:
inspByMonth.Add(db.tAssetInspections.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.InspectionDate.HasValue && x.InspectionDate.Value.Year == now.Year && x.InspectionDate.Value.Month == i));
```

### Fix 3: Controller - Fix maintByType Query (Line 361)

```csharp
// BEFORE:
var maintByType = maintByTypeLabels.Select(t => db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true)).ToList();

// AFTER:
var maintByType = maintByTypeLabels.Select((t, index) => 
    db.tMaintenances.Count(x => x.OrgInfoId == orgId && x.IsAction == true && x.mMaintenanceTypeId == (index + 1))).ToList();
```

### Fix 4: Controller - Fix dueTodayItems (Lines 404-407)

```csharp
// BEFORE:
.Where(x => x.OrgInfoId == orgId && x.InspectionDate > today && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today))

// AFTER:
.Where(x => x.OrgInfoId == orgId && x.IsAction == true && DbFunctions.TruncateTime(x.InspectionDate) == DbFunctions.TruncateTime(today))
```

### Fix 5: View - Fix WorkOrderNo Property (Line 1027)

```html
<!-- BEFORE -->
<div class="ea-alert-title">{{m.WorkOrderNo}}</div>

<!-- AFTER -->
<div class="ea-alert-title">{{m.Title}}</div>
```

### Fix 6: View - Fix EmailId Property (Line 1148)

```html
<!-- BEFORE -->
<div class="ea-act-meta">{{u.EmailId}} - {{u.LastLogin}}</div>

<!-- AFTER -->
<div class="ea-act-meta">{{u.Email}} - {{u.LastLogin}}</div>
```

### Fix 7: View - Add Overdue Inspections Section (After Line 1131)

```html
<!-- Add after overdue calibrations list -->
<p style="font-size:12px;font-weight:600;color:#c62828;margin:12px 0 8px;">Overdue Inspections</p>
<ul class="ea-alert-list" ng-if="overdueInspections.length > 0">
    <li ng-repeat="i in overdueInspections | limitTo:5">
        <span class="ea-alert-dot green"></span>
        <div class="ea-alert-content">
            <div class="ea-alert-title">{{i.InspectionNo}}</div>
            <div class="ea-alert-meta">{{i.DaysOverdue}} days overdue</div>
        </div>
        <span class="ea-alert-badge overdue">Overdue</span>
    </li>
</ul>
```

---

## 10. MISSING FEATURES

| Feature | Current State | Recommended Action |
|---------|---------------|-------------------|
| Upcoming Maintenance DaysOverdue | Missing | Add `DaysOverdue` calculation to upcoming maintenance |
| assetsBySite | Uses `mSiteMasterId` grouping | Consider joining with `mSiteMaster` for site names |
| costDistribution | Uses "Group X" labels | Consider joining with `mGroupMaster` for group names |

---

## 11. COMPLETE FIX LIST

### File: EnterpriseAnalyticsDashboardController.cs

1. **Line 73**: Change `db.AssetCalibrations` → `db.tAssetCalibrations` and add `&& x.IsAction == true`
2. **Line 74**: Change `db.AssetCalibrations` → `db.tAssetCalibrations` and add `&& x.IsAction == true`
3. **Line 125**: Change `db.AssetCalibrations` → `db.tAssetCalibrations` and add `&& x.IsAction == true`
4. **Line 202**: Change `db.AssetCalibrations` → `db.tAssetCalibrations` and add `&& x.IsAction == true`
5. **Line 265**: Change `db.AssetCalibrations` → `db.tAssetCalibrations` and add `&& x.IsAction == true`
6. **Line 314**: Add null check `x.InspectionDate.HasValue &&` before property access
7. **Line 326**: Change `db.AssetCalibrations` → `db.tAssetCalibrations` and add `&& x.IsAction == true`
8. **Line 361**: Fix maintenance type query to filter by type
9. **Lines 404-407**: Add `IsAction` filters to dueTodayItems queries

### File: Index.cshtml

1. **Line 1027**: Change `m.WorkOrderNo` → `m.Title`
2. **Line 1148**: Change `u.EmailId` → `u.Email`
3. **After Line 1131**: Add overdueInspections section

---

## 12. CONCLUSION

The Enterprise Analytics Dashboard has significant data integrity issues primarily caused by:
1. Using wrong database tables (`AssetCalibrations` instead of `tAssetCalibrations`)
2. Missing `IsAction` filters on read operations
3. Property name mismatches between controller, JavaScript, and view
4. Missing null checks on nullable DateTime properties

These issues will result in:
- Charts showing incorrect calibration and inspection trends
- Tables missing calibration/inspection data
- Incorrect KPI counts for calibration due/overdue
- Potential runtime exceptions for null date values
- Missing data in upcoming/overdue sections

**Priority**: All Critical and High issues should be fixed immediately before production deployment.