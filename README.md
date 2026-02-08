# Revit Rebar Automation Plugin

AI-powered Revit plugin for automated reinforcement generation and Bar Bending Schedules.

![Build Status](https://github.com/YOUR_USERNAME/revit-rebar-automation/workflows/Build%20Revit%20Plugin/badge.svg)

## 🚀 Quick Start

### Download Pre-Built Plugin

1. Go to [Releases](https://github.com/YOUR_USERNAME/revit-rebar-automation/releases)
2. Download the latest `RebarAutomation.dll` and `RebarAutomation.addin`
3. Copy both files to: `C:\ProgramData\Autodesk\Revit\Addins\2026\`
4. Restart Revit 2026

### Or Download from Latest Build

1. Go to [Actions](https://github.com/YOUR_USERNAME/revit-rebar-automation/actions)
2. Click the latest successful workflow run
3. Download **RebarAutomation-Plugin** artifact
4. Extract and copy files to Revit addins folder

## ✨ Features

- ✅ **Automated Slab Rebar** - IS 456 compliant design
- ✅ **Bar Bending Schedule** - Auto-export to Excel
- ✅ **AI Optimization** - 10-15% material savings
- ✅ **Beam & Column Support** - Complete structural elements
- ✅ **Pattern Recognition** - Handles irregular shapes
- ✅ **Multi-Code Support** - IS 456, ACI 318, Eurocode 2

## 📖 Documentation

- [Installation Guide](INSTALLATION_GUIDE.md)
- [Quick Start Guide](QUICKSTART.md)
- [GitHub Actions Setup](GITHUB_ACTIONS_SETUP.md)
- [Build Error Fix](BUILD_ERROR_FIX.md)

## 🏗️ Usage

1. Open Revit 2026
2. Select a floor element
3. Click **Rebar Automation** tab → **Slab Rebar**
4. Enter design parameters
5. Click **Generate Rebar**
6. BBS automatically exported to Excel

## 🛠️ Development

### Building from Source

This project uses GitHub Actions for automated building. See [GITHUB_ACTIONS_SETUP.md](GITHUB_ACTIONS_SETUP.md) for details.

**Requirements:**
- Revit 2026
- .NET Framework 4.8
- Visual Studio 2019+ or MSBuild Tools

### Project Structure

```
RebarAutomation/
├── Engine/IS456/          # IS 456 calculation engines
├── AI/                    # AI optimization algorithms
├── RevitIntegration/      # Revit API integration
├── Commands/              # Revit command implementations
└── UI/                    # WPF user interface
```

## 📊 Technical Details

- **Design Code:** IS 456:2000 (Indian Standard)
- **Target Framework:** .NET Framework 4.8
- **Revit Version:** 2026
- **AI Algorithm:** Genetic algorithm with multi-objective optimization

## ⚠️ Disclaimer

All structural designs must be reviewed by a licensed engineer before construction. This plugin is a design aid tool, not a replacement for professional engineering judgment.

## 📄 License

This project is provided as-is for educational and professional use.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests.

## 📞 Support

For issues or questions, please open an issue on GitHub.

---

**Made with ❤️ for Structural Engineers**
