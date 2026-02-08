# GitHub Actions Setup Guide

## 🎯 What This Does

GitHub Actions will automatically build your Revit plugin in the cloud whenever you push code. You'll get the compiled DLL files without needing Visual Studio!

---

## 📋 Step-by-Step Setup

### Step 1: Create a GitHub Account (if you don't have one)

1. Go to https://github.com
2. Click "Sign up"
3. Follow the registration process (FREE)

### Step 2: Create a New Repository

1. Log in to GitHub
2. Click the **"+"** icon (top right) → **"New repository"**
3. Fill in:
   - **Repository name:** `revit-rebar-automation`
   - **Description:** `AI-powered Revit plugin for automated rebar generation`
   - **Visibility:** Choose **Public** (free) or **Private** (also free)
   - ✅ Check **"Add a README file"**
4. Click **"Create repository"**

### Step 3: Upload Your Code to GitHub

**Option A: Using GitHub Desktop (Easiest)**

1. Download GitHub Desktop: https://desktop.github.com/
2. Install and sign in
3. Click **"Add"** → **"Add existing repository"**
4. Browse to: `C:\Users\ajith\new revit\RebarAutomation`
5. Click **"Publish repository"**

**Option B: Using Git Command Line**

```powershell
# Navigate to your project
cd "C:\Users\ajith\new revit\RebarAutomation"

# Initialize git (if not already)
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit - Revit Rebar Automation Plugin"

# Add remote (replace YOUR_USERNAME with your GitHub username)
git remote add origin https://github.com/YOUR_USERNAME/revit-rebar-automation.git

# Push to GitHub
git branch -M main
git push -u origin main
```

**Option C: Using GitHub Web Interface (Manual Upload)**

1. Go to your repository on GitHub
2. Click **"Add file"** → **"Upload files"**
3. Drag and drop all files from `C:\Users\ajith\new revit\RebarAutomation`
4. Click **"Commit changes"**

### Step 4: GitHub Actions Will Build Automatically!

Once you upload the code:

1. Go to your repository on GitHub
2. Click the **"Actions"** tab
3. You'll see the build running (yellow dot = in progress)
4. Wait 2-3 minutes for the build to complete (green checkmark = success)

### Step 5: Download the Built Plugin

**After the build succeeds:**

1. In the **Actions** tab, click on the completed workflow run
2. Scroll down to **"Artifacts"** section
3. Click **"RebarAutomation-Plugin"** to download
4. Extract the ZIP file
5. You'll find:
   - `RebarAutomation.dll`
   - `RebarAutomation.addin`

### Step 6: Install in Revit

1. Copy the 2 files from the downloaded ZIP
2. Paste to: `C:\ProgramData\Autodesk\Revit\Addins\2026\`
3. Restart Revit 2026
4. Look for "Rebar Automation" tab!

---

## 🔄 How It Works

```
┌─────────────────────────────────────────────────────────────┐
│ Your Computer                                               │
│ C:\Users\ajith\new revit\RebarAutomation\                   │
│ ├── All .cs files                                           │
│ ├── .github/workflows/build.yml  ← GitHub Actions config   │
│ └── ...                                                     │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ PUSH CODE
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ GitHub Repository                                           │
│ https://github.com/YOUR_USERNAME/revit-rebar-automation    │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ TRIGGERS
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ GitHub Actions (Cloud Build Server)                        │
│ ✅ Installs MSBuild                                         │
│ ✅ Restores NuGet packages                                  │
│ ✅ Builds the solution                                      │
│ ✅ Creates artifact ZIP                                     │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ DOWNLOAD
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ You Download:                                               │
│ RebarAutomation-Plugin.zip                                  │
│ ├── RebarAutomation.dll     ← Ready to use!                │
│ └── RebarAutomation.addin   ← Ready to use!                │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎉 Benefits

✅ **No Visual Studio needed** - GitHub builds it for you
✅ **Always works** - Uses Microsoft's official build servers
✅ **Free** - GitHub Actions is free for public repositories
✅ **Automatic** - Every code change triggers a new build
✅ **Version history** - All builds are saved for 90 days

---

## 🔧 Troubleshooting

### "Build failed" in GitHub Actions

**Check the build log:**
1. Click on the failed workflow
2. Click on the "build" job
3. Expand the failed step to see the error

**Common issues:**
- Missing Revit API references → The workflow handles this automatically
- Syntax errors in code → Fix the code and push again

### "Can't find the Artifacts section"

Make sure:
1. The build completed successfully (green checkmark)
2. You're looking at a completed workflow run (not the main Actions page)
3. Scroll down - Artifacts are at the bottom of the workflow run page

### "Repository is private and I ran out of free minutes"

GitHub gives:
- **Unlimited minutes** for public repositories
- **2,000 minutes/month** for private repositories (free tier)

Solution: Make your repository public or upgrade to GitHub Pro

---

## 📝 Next Steps After Setup

### Every Time You Make Changes:

1. Edit code on your computer
2. Push to GitHub (using GitHub Desktop or git commands)
3. GitHub Actions builds automatically
4. Download new DLL from Artifacts
5. Replace old DLL in Revit addins folder

### Create Releases (Optional):

To create official releases with version numbers:

```powershell
# Tag a version
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions will automatically create a release with the DLL files attached!

---

## 🎯 Quick Reference

| Action | Command/Link |
|--------|--------------|
| **Upload code** | Use GitHub Desktop or `git push` |
| **View builds** | Repository → Actions tab |
| **Download DLL** | Actions → Workflow run → Artifacts |
| **Install in Revit** | Copy to `C:\ProgramData\Autodesk\Revit\Addins\2026\` |

---

## 💡 Pro Tips

1. **Enable email notifications** - Get notified when builds fail
2. **Use branches** - Test changes in a branch before merging to main
3. **Add badges** - Show build status in your README
4. **Schedule builds** - Can build nightly even without code changes

---

## 📞 Need Help?

If you get stuck:
1. Check the GitHub Actions logs for error messages
2. The workflow file is at: `.github/workflows/build.yml`
3. You can edit it directly on GitHub if needed

---

**Ready to start?** Follow Step 1 above to create your GitHub account!
