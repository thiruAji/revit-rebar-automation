# Complete Installation Guide for Revit 2026

## 📋 Overview

You need to copy **2 files** from your build output to Revit's addins folder. This guide shows exactly where to find them and where to put them.

---

## 🔨 Step 1: Build the Project

First, you need to compile the C# code into a DLL file.

### Open PowerShell or Command Prompt:

1. Press `Windows + R`
2. Type `powershell` and press Enter
3. Navigate to the project folder:

```powershell
cd "C:\Users\ajith\new revit\RebarAutomation"
```

4. Build the project:

```powershell
dotnet build RebarAutomation.csproj
```

### What This Does:
- Compiles all the C# code files (.cs) into a single DLL
- Creates the output in the `bin\Debug\net48\` folder
- Takes about 10-30 seconds

### Expected Output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 📂 Step 2: Locate the Built Files

After building, you'll find the files here:

### Source Location:
```
C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\
```

### Files You Need to Copy:

#### File 1: `RebarAutomation.dll`
- **Full Path:** `C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.dll`
- **What it is:** The compiled plugin code (all your C# code in one file)
- **Size:** Approximately 50-100 KB
- **Type:** Application Extension (.dll)

#### File 2: `RebarAutomation.addin`
- **Full Path:** `C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.addin`
- **What it is:** XML manifest that tells Revit about your plugin
- **Size:** Less than 1 KB
- **Type:** Text file (.addin)

### How to Find These Files:

**Option A - Using File Explorer:**
1. Open File Explorer (`Windows + E`)
2. Paste this path in the address bar:
   ```
   C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48
   ```
3. Press Enter
4. You should see `RebarAutomation.dll` and `RebarAutomation.addin`

**Option B - Using PowerShell:**
```powershell
# List the files
dir "C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.*"
```

---

## 📥 Step 3: Copy to Revit Addins Folder

### Destination Location:
```
C:\ProgramData\Autodesk\Revit\Addins\2026\
```

### Important Notes:
- ⚠️ **ProgramData is a hidden folder** - You need to type the path manually or enable "Show hidden files"
- ⚠️ **You may need Administrator permissions** to copy files here
- ⚠️ **Create the folder if it doesn't exist**

### Method 1: Copy Using File Explorer (Recommended)

1. **Select the files:**
   - Navigate to: `C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\`
   - Hold `Ctrl` and click both files:
     - `RebarAutomation.dll`
     - `RebarAutomation.addin`
   - Press `Ctrl + C` to copy

2. **Navigate to destination:**
   - Press `Windows + R`
   - Type: `C:\ProgramData\Autodesk\Revit\Addins\2026`
   - Press Enter
   - If the folder doesn't exist, you'll get an error (see troubleshooting below)

3. **Paste the files:**
   - Press `Ctrl + V`
   - If prompted for administrator permission, click "Continue" or "Yes"

### Method 2: Copy Using PowerShell (Alternative)

```powershell
# Create destination folder if it doesn't exist
New-Item -ItemType Directory -Force -Path "C:\ProgramData\Autodesk\Revit\Addins\2026"

# Copy the DLL file
Copy-Item "C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.dll" -Destination "C:\ProgramData\Autodesk\Revit\Addins\2026\"

# Copy the .addin file
Copy-Item "C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.addin" -Destination "C:\ProgramData\Autodesk\Revit\Addins\2026\"
```

### Method 3: One-Line PowerShell Command

```powershell
Copy-Item "C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.*" -Destination "C:\ProgramData\Autodesk\Revit\Addins\2026\" -Include "*.dll","*.addin"
```

---

## ✅ Step 4: Verify Installation

### Check Files Are in Place:

1. Open File Explorer
2. Navigate to: `C:\ProgramData\Autodesk\Revit\Addins\2026\`
3. You should see:
   ```
   📁 2026
   ├── 📄 RebarAutomation.dll
   └── 📄 RebarAutomation.addin
   ```

### Using PowerShell to Verify:
```powershell
dir "C:\ProgramData\Autodesk\Revit\Addins\2026\RebarAutomation.*"
```

Expected output:
```
    Directory: C:\ProgramData\Autodesk\Revit\Addins\2026

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a----         2/8/2026   12:00 PM            414 RebarAutomation.addin
-a----         2/8/2026   12:00 PM          65536 RebarAutomation.dll
```

---

## 🚀 Step 5: Start Revit 2026

1. **Close Revit** if it's currently open (plugins only load at startup)
2. **Launch Revit 2026**
3. **Look for the "Rebar Automation" tab** in the ribbon

### What You Should See:

The Revit ribbon should have a new tab called **"Rebar Automation"** with these buttons:
- 🔧 **Slab Rebar** - Generate slab reinforcement
- 🔧 **Beam Rebar** - Generate beam reinforcement
- 🔧 **Column Rebar** - Generate column reinforcement
- 📊 **Generate BBS** - Create Bar Bending Schedule
- ⚙️ **Settings** - Configure plugin

---

## ❓ Troubleshooting

### Problem: "Folder doesn't exist: C:\ProgramData\Autodesk\Revit\Addins\2026"

**Solution 1:** Create the folder manually
```powershell
New-Item -ItemType Directory -Force -Path "C:\ProgramData\Autodesk\Revit\Addins\2026"
```

**Solution 2:** Check if Revit 2026 is installed
- Navigate to: `C:\Program Files\Autodesk\`
- Look for a folder named `Revit 2026`
- If it doesn't exist, Revit 2026 is not installed

### Problem: "Can't see ProgramData folder"

**Solution:** Enable hidden folders
1. Open File Explorer
2. Click **View** tab
3. Check **Hidden items**

OR type the full path directly in the address bar:
```
C:\ProgramData\Autodesk\Revit\Addins\2026
```

### Problem: "Access Denied" when copying

**Solution:** Run as Administrator
1. Right-click PowerShell
2. Select "Run as Administrator"
3. Run the copy commands again

### Problem: "Plugin doesn't appear in Revit"

**Check 1:** Verify files are in correct location
```powershell
Test-Path "C:\ProgramData\Autodesk\Revit\Addins\2026\RebarAutomation.dll"
Test-Path "C:\ProgramData\Autodesk\Revit\Addins\2026\RebarAutomation.addin"
```
Both should return `True`

**Check 2:** Look for errors in Windows Event Viewer
1. Press `Windows + R`
2. Type `eventvwr`
3. Navigate to: Windows Logs → Application
4. Look for errors from "Revit"

**Check 3:** Verify .addin file content
Open `RebarAutomation.addin` in Notepad and verify it contains:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>Rebar Automation</Name>
    <Assembly>RebarAutomation.dll</Assembly>
    ...
  </AddIn>
</RevitAddIns>
```

### Problem: "Build failed" errors

**Common causes:**
1. **Revit 2026 not installed** - The build needs Revit API DLLs
   - Install Revit 2026 first
   - Or update the path in `.csproj` if Revit is in a different location

2. **Missing .NET Framework 4.8**
   - Download from: https://dotnet.microsoft.com/download/dotnet-framework/net48

3. **EPPlus package not installed**
   ```powershell
   dotnet restore
   ```

---

## 📊 Visual Summary

```
BUILD PROCESS:
┌─────────────────────────────────────────────────────────────┐
│ C:\Users\ajith\new revit\RebarAutomation\                   │
│ ├── App.cs                                                  │
│ ├── Models\                                                 │
│ ├── Engine\                    ┌──────────────┐            │
│ └── ... (all .cs files) ──────>│ dotnet build │            │
│                                 └──────┬───────┘            │
│                                        │                    │
│                                        ▼                    │
│ bin\Debug\net48\                                           │
│ ├── ✅ RebarAutomation.dll     ◄── COPY THESE             │
│ └── ✅ RebarAutomation.addin   ◄── COPY THESE             │
└─────────────────────────────────────────────────────────────┘
                                        │
                                        │ COPY
                                        ▼
┌─────────────────────────────────────────────────────────────┐
│ C:\ProgramData\Autodesk\Revit\Addins\2026\                 │
│ ├── ✅ RebarAutomation.dll     ◄── PASTE HERE             │
│ └── ✅ RebarAutomation.addin   ◄── PASTE HERE             │
└─────────────────────────────────────────────────────────────┘
                                        │
                                        │ RESTART REVIT
                                        ▼
┌─────────────────────────────────────────────────────────────┐
│ Revit 2026 Ribbon                                          │
│ ┌─────────────────────────────────────────────────────┐   │
│ │ [Rebar Automation] Tab                              │   │
│ │  [Slab Rebar] [Beam] [Column] [BBS] [Settings]     │   │
│ └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Quick Reference

| What | Where |
|------|-------|
| **Source files** | `C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\` |
| **Files to copy** | `RebarAutomation.dll` + `RebarAutomation.addin` |
| **Destination** | `C:\ProgramData\Autodesk\Revit\Addins\2026\` |
| **When to copy** | After every build (if you make changes) |
| **Restart needed** | Yes, Revit must be restarted after copying |

---

## 💡 Pro Tips

1. **Create a batch file** to automate copying:
   ```batch
   @echo off
   echo Copying Rebar Automation to Revit 2026...
   copy "C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.dll" "C:\ProgramData\Autodesk\Revit\Addins\2026\"
   copy "C:\Users\ajith\new revit\RebarAutomation\bin\Debug\net48\RebarAutomation.addin" "C:\ProgramData\Autodesk\Revit\Addins\2026\"
   echo Done! Restart Revit to see changes.
   pause
   ```
   Save as `install.bat` and run after each build

2. **Keep a backup** of working versions in case you need to roll back

3. **Only copy when you make changes** - No need to copy every time you open Revit

---

**Need Help?** Check the [README.md](file:///C:/Users/ajith/new%20revit/RebarAutomation/README.md) or [QUICKSTART.md](file:///C:/Users/ajith/new%20revit/RebarAutomation/QUICKSTART.md) for more details.
