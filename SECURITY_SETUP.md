# Security Setup Guide

## ⚠️ CRITICAL: Your appsettings.Development.json Contains Sensitive Data

This file currently contains:
- Redis password
- Azure AD B2C Client ID
- Database credentials
- Production Redis endpoint

## What Has Been Done

✅ Created `.gitignore` - Now blocks `appsettings.Development.json` from being committed  
✅ Created `appsettings.Development.json.example` - Template for team members  
✅ Updated README.md - Setup instructions for new developers  
✅ Fixed Dockerfile - Updated to .NET 9.0  
✅ Fixed docker-compose.yml - Corrected database name in healthcheck  
✅ Added Serilog - Structured logging with Console and File sinks  

## What You Need to Do (When You Install Git)

### If You Haven't Committed Yet:
```powershell
# Just commit your changes - .gitignore will protect your sensitive files
git add .
git commit -m "Add project with security improvements"
git push
```

### If appsettings.Development.json Was Already Committed:

**Step 1: Remove from Git history (keeps local file)**
```powershell
git rm --cached SimpleApp/appsettings.Development.json
git commit -m "Remove sensitive configuration file from tracking"
git push
```

**Step 2: Rotate Compromised Credentials**  
Since your credentials may have been exposed:
1. Change Redis password in Redis Cloud dashboard
2. Regenerate Azure AD B2C client secret if exposed
3. Update your local `appsettings.Development.json` with new values

**Step 3 (Optional): Clean Git History Completely**  
If the file was committed before, use BFG Repo-Cleaner:
```powershell
# Download BFG from https://rtyley.github.io/bfg-repo-cleaner/
java -jar bfg.jar --delete-files appsettings.Development.json
git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push --force
```

## Environment-Specific Configuration

### Development (Local)
Use `appsettings.Development.json` with:
- Local PostgreSQL (localhost:5432)
- Development Redis instance
- Test Azure AD B2C tenant
- Serilog `Default` level: **Debug** (verbose, includes `{SourceContext}`)

### Production
Use environment variables or Azure Key Vault:
```powershell
# Example: Set environment variables
$env:ConnectionStrings__Default = "Host=prod-server;..."
$env:ConnectionStrings__Redis = "prod-redis:6379,password=..."
```
- Serilog `Default` level: **Information**
- Logs written to `logs/app-{date}.log` (rolls daily)

## Logging (Serilog)

Serilog is configured via `appsettings.json` / `appsettings.Development.json`.

| Sink    | Output                        |
|---------|-------------------------------|
| Console | Colored, human-readable       |
| File    | `logs/app-{date}.log` (daily) |

Enrichers: `FromLogContext`, `WithMachineName`, `WithThreadId`

> The `logs/` folder is excluded from Git via `.gitignore`.

## Docker Security

The `.env` file is also ignored. Create it from `.env.example`:
```powershell
Copy-Item SimpleApp\.env.example SimpleApp\.env
```

## Best Practices Going Forward

1. ✅ Never commit files with credentials
2. ✅ Always use `.example` templates
3. ✅ Use environment variables for production
4. ✅ Use Azure Key Vault for sensitive production data
5. ✅ Rotate credentials if accidentally exposed
6. ✅ Use .NET User Secrets for local development:
   ```powershell
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:Redis" "your-value"
   ```

## Verification

Check what Git will commit:
```powershell
git status
git add .
git status  # Should NOT show appsettings.Development.json
```

## Need Help?

- [.NET User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/)
- [BFG Repo-Cleaner](https://rtyley.github.io/bfg-repo-cleaner/)
- [Serilog](https://serilog.net/)
