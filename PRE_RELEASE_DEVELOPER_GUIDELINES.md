# Pre-Release Development Guidelines

## ✨ Quick Guide for Developers

### Before You Start Coding Any Feature

```
□ Application is PRE-RELEASE
□ No existing user data
□ No backward compatibility needed
□ Database can be reset anytime
□ Migrations should be simple
```

---

## 🏗️ Database Model Development

### ✅ DO THIS

```csharp
// 1. Create clean model
public class CashAccount
{
    public int AccountId { get; set; }
    public string AccountName { get; set; }
    // ... simple, clear design
}

// 2. Add to DbContext
public DbSet<CashAccount> CashAccounts { get; set; }

// 3. Create migration
Add-Migration AddCashAccount

// 4. Apply
Update-Database

// 5. Deploy to users
```

### ❌ DON'T DO THIS

```csharp
// ❌ Don't add compatibility checks
if (migrationBuilder.Sql("SELECT COUNT(*) FROM CashAccounts"))
{
    // Don't do this - table won't exist yet!
}

// ❌ Don't write data transformation
foreach (var oldData in oldTable)
{
    // Don't migrate old data - there is none!
}

// ❌ Don't add "legacy" support
if (entity.LegacyBalance != null)
{
    // Don't maintain old formats - not needed!
}

// ❌ Don't plan rollbacks
backup();
```

---

## 🗄️ EF Core Migrations

### Clean Migration Example

```bash
# GOOD - Simple and direct
Add-Migration AddCashBalanceSystem
Update-Database

# Output: ✅ Migration applied successfully
```

### Complex Migration Example (AVOID)

```bash
# BAD - Over-engineered for pre-release
Add-Migration AddCashBalanceWithBackwardCompat
# (Contains legacy support, data transformation, rollback logic)

# ❌ Don't do this!
```

---

## 🧪 Testing Approach

### ✅ DO THIS

```
Test Cases:
├─ Fresh installation
├─ First-time setup
├─ Feature functionality
├─ Data integrity
└─ User workflows
```

### ❌ DON'T DO THIS

```
Test Cases:
├─ Upgrade from v1.0 to v1.1 ❌
├─ Backward compatibility ❌
├─ Old data format handling ❌
├─ Migration rollback scenarios ❌
└─ Multiple version support ❌
```

---

## 📝 Code Guidelines

### Feature Checklist

```
For any new feature:

□ Design clean, modern architecture
□ No "legacy code" paths
□ Simple database models
□ Direct EF Core relationships
□ Fresh install assumption
□ No old version support
□ No data migration logic
□ No rollback procedures
□ Deploy directly to users
```

---

## 🚀 Deployment Process

### ✅ Pre-Release Deployment

```
1. Code complete → Push to main
2. Build succeeds → Ready to release
3. Release to users → Fresh install
4. User runs app → First-time setup runs
5. Done! ✅

Total: 2-3 hours
No rollback needed
No data migration
No version checking
Simple! 🎉
```

### ❌ Post-Release Deployment (FUTURE)

```
1. Code complete
2. Build succeeds
3. Migration scripts validated
4. Data transformation tested
5. Rollback plan reviewed
6. Release notes written
7. Gradual rollout planned
8. User communication sent
9. Deploy to production
10. Monitor for issues
11. Be ready to rollback

Total: 2-3 weeks
Complex process
Risky if not careful
NOT FOR NOW ⚠️
```

---

## 🔄 Making Database Changes

### Example: Add New Field

#### ✅ Current Approach (Pre-Release)

```csharp
// Step 1: Update model
public class Item
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string Description { get; set; } // ← NEW FIELD
}

// Step 2: Create migration
Add-Migration AddDescriptionToItem

// Step 3: Apply
Update-Database

// Step 4: Done! Deploy to users.
```

#### ❌ Old Approach (Post-Release)

```csharp
// Would need to:
// - Check if column exists
// - Handle old data format
// - Write migration script
// - Test upgrade path
// - Plan rollback
// - Document breaking change
// - Gradual rollout
// - Monitor for issues

// Don't do this yet!
```

---

## 📋 Code Review Checklist

### When Reviewing Code

```
□ Does it assume fresh install? ✅
□ Any legacy code paths? ❌
□ Data migration logic? ❌
□ Backward compatibility? ❌
□ Feature toggles? ❌
□ Version checking? ❌
□ Fallback logic? ❌

If all ✅ and all ❌ → Approve! ✨
```

---

## 🎯 What Each Developer Should Know

### Must Know
- ✅ App is pre-release
- ✅ Can make breaking changes
- ✅ Database can be reset
- ✅ No existing user data
- ✅ Migrations are simple

### Must NOT Do
- ❌ Add compatibility code
- ❌ Check for old tables
- ❌ Write data transformations
- ❌ Plan rollbacks
- ❌ Support old versions

### Can Assume
- ✅ Fresh database
- ✅ Latest schema
- ✅ No legacy data
- ✅ No upgrade paths needed
- ✅ Direct deployment

---

## 🚨 Red Flags

### If You See This → Question It

```
❌ "if (table exists in database)"
❌ "data migration from old format"
❌ "fallback to legacy behavior"
❌ "support version X compatibility"
❌ "prepare for rollback"
❌ "gradual feature rollout"
❌ "check which version is running"

These are NOT needed pre-release!
```

### If You See This → Approve It

```
✅ "create new table directly"
✅ "simple migration"
✅ "fresh install assumption"
✅ "no old data handling"
✅ "deploy immediately"
✅ "clean code design"
✅ "direct feature rollout"

These are appropriate pre-release!
```

---

## 📊 Time Expectations

### Feature Development

```
Design:       1 day
Development:  3-5 days
Testing:      1-2 days
Code Review:  1 day
Deployment:   Same day
──────────────
TOTAL:        6-9 days

✅ Fast!
✅ No migration time
✅ No rollback time
✅ Direct deployment
```

### Do NOT Add Time For

```
❌ Backward compatibility
❌ Data migration scripts
❌ Upgrade path testing
❌ Rollback planning
❌ Version management
❌ Gradual rollout
❌ Complex QA

Those are for AFTER publication!
```

---

## 🎓 Questions to Ask

### Before Starting a Feature

1. "Does this break existing code?"
   - ✅ YES → That's OK! No existing users.

2. "Do I need to migrate old data?"
   - ✅ NO → No old data exists.

3. "Should I support the old way too?"
   - ✅ NO → Use new way only.

4. "What if we need to rollback?"
   - ✅ DON'T PLAN FOR IT → Not needed pre-release.

5. "How long will this take?"
   - ✅ Much faster! ~40% reduction from normal.

---

## 📞 If You're Unsure

### Ask These Questions

1. "Is the app published?" 
   → NO (pre-release)

2. "Do users have data?"
   → NO (development only)

3. "Can we reset the database?"
   → YES (development, not production)

4. "Do we need backward compatibility?"
   → NO (not required pre-release)

5. "Can we make breaking changes?"
   → YES (acceptable pre-release)

### Then...
✅ Proceed with simple, clean implementation  
✅ No compatibility layer needed  
✅ Deploy directly when ready  

---

## ✨ Best Practices

### ✅ DO

```
1. Write clean code
2. Simple architecture
3. Direct relationships
4. Minimal abstractions
5. Fresh install tests
6. Direct deployments
7. Fast iterations
8. Move fast!
```

### ❌ DON'T

```
1. Add legacy support
2. Complex abstractions
3. Conditional logic
4. Migration scripts
5. Upgrade testing
6. Rollback plans
7. Multiple versions
8. Over-engineer it!
```

---

## 🚀 Launch Readiness

### Before Publishing (Release v1.0)

- [ ] Code is clean and modern
- [ ] No legacy code paths
- [ ] Database schema is final
- [ ] All tests pass
- [ ] Documentation complete
- [ ] User guide written
- [ ] Ready for fresh installs

### After Publishing (Release v1.1+)

- [ ] Create migration scripts
- [ ] Plan upgrade paths
- [ ] Test backward compatibility
- [ ] Document breaking changes
- [ ] Version your APIs
- [ ] Plan rollback procedures

---

## 📋 TL;DR (The Short Version)

```
Pre-Release Status: ✅ YES

What This Means:
├─ Build features cleanly
├─ Simple database migrations
├─ Fresh install assumption
├─ No backward compatibility needed
├─ No migration scripts
├─ Direct deployment
└─ 40-50% faster development!

What NOT To Do:
├─ Don't add compatibility code
├─ Don't write data migrations
├─ Don't plan rollbacks
├─ Don't support old versions
└─ Don't over-engineer it!

Go build! 🚀
```

---

**For**: All Development Team Members  
**Updated**: January 2, 2026  
**Status**: Active (Until First Publication)  
**Questions**: See PRE_RELEASE_STATUS_SUMMARY.md
