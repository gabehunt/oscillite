# Oscillite

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

Oscillite is a lightweight oscilloscope waveform viewer designed for analyzing and visualizing diagnostic data captured by popular Snap-On branded scopes.
It offers an enhanced visualization experience over the stock viewer.  The goal of this project is purely educational, to prove the Snap-On scope hardware is very capable when used with the right software capabilities.

---

## ✨ Features

- Renders automotive scope waveforms using Direct2D
- Drag-to-zoom and multi-trace support
- Custom rulers, grid overlays, and waveform measurements
- Loads Snap-On diagnostic files (.vsm, .vsc, etc.)

---

## 🚀 Getting Started

This application **requires Snap-On Scope Viewer or Shopstream Connect** and must be launched from a machine with that Snap-On software installed.

### Prerequisites

- Windows 10+
- .NET Framework 4.7.2
- Snap-On ShopStream Connect or Scope Viewer installed

### Setup

1. Clone the repo:
   ```bash
   git clone https://github.com/yourname/oscillite.git
   cd oscillite
   ```

2. Open in Visual Studio and build.

3. Ensure you're running from the applicable Snap-On directory (Ex.):
   ```
   C:\Program Files (x86)\Snap-on Incorporated\ShopStream Connect\ScopeDataViewer
   ```

4. Press F5 or launch the compiled `Oscillite.exe` directly from that folder.

---

## 🧪 Usage

1. Click `Open` and load a `.vsm` file.
2. Use your mouse to zoom, drag, and inspect waveforms.
3. Apply rulers and measurements for analysis.

See the `docs/` folder for more detailed feature explanations.

---

## 🔧 Development

To develop or debug this app outside the Snap-On directory:

- Use the debug option to **set the working directory**.

---

## ⚠️ Limitations

- Snap-On assemblies are ***not redistributed*** with this application due to licensing restrictions.
- You must run this program from a directory containing your own installation of `SSC.Scope.dll`, `SnapOn.Interfaces.dll`, and `SSC.Viewer.Interface.dll`.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Snap-On for their diagnostic tooling ecosystem
- SharpDX for Direct2D rendering
- Open-source .NET community

---

## 📛 Legal Disclaimer

Oscillite is an **independent open-source project** and is **not affiliated with, endorsed by, or supported by Snap-on Incorporated** in any way.

All product names, trademarks, and registered trademarks are property of their respective owners.  
**Snap-on®, Shopstream Connect®, Verus®, Modis®, and Zeus® are registered trademarks of Snap-on Incorporated.**

This software is provided "as is", **without any express or implied warranty**.  
In no event shall the authors or contributors be held liable for any damages arising from the use of this software.

Use of this tool is at your own risk. It is intended for **educational and diagnostic enhancement purposes** only, and must not be used to reverse engineer or redistribute proprietary Snap-on software or content.
