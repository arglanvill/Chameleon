# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in `global.json` files (if any) is compatible with the .NET 10.0 upgrade.
3. Upgrade `Chameleon.csproj` (see Project upgrade details below).

---

## Settings

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|
| *(none)*                                       | No excluded projects        |


### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                        | Current Version | New Version | Description                                                                   |
|:------------------------------------|:---------------:|:-----------:|:------------------------------------------------------------------------------|
| Microsoft.Graphics.Win2D            |                 | 1.1.0       | Replacement for `Microsoft.NETCore.UniversalWindowsPlatform`                  |
| Microsoft.NETCore.UniversalWindowsPlatform | 6.2.10        |             | **Will be removed** and replaced by Windows App SDK + Win2D + Compatibility  |
| Microsoft.Toolkit.Uwp.UI.Controls.DataGrid | 6.0.0       |             | **Incompatible** — requires replacement or removal; manual migration needed   |
| Microsoft.Windows.Compatibility     |                 | 10.0.1      | Replacement for `Microsoft.NETCore.UniversalWindowsPlatform`                  |
| Microsoft.WindowsAppSDK             |                 | 1.8.251106002 | Replacement for `Microsoft.NETCore.UniversalWindowsPlatform`                |


### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### `Chameleon.csproj` modifications

Project properties changes:
  - Target framework should be changed from `net5.0` to `net10.0-windows10.0.22000.0`.
  - Convert the project file to **SDK-style** format (move to `Sdk="Microsoft.NET.Sdk.WindowsDesktop"` or appropriate SDK and replace classic project style with SDK-style `csproj`).

NuGet packages changes:
  - Remove `Microsoft.NETCore.UniversalWindowsPlatform` (`6.2.10`) and add replacements:
    - `Microsoft.WindowsAppSDK` `1.8.251106002` (for Windows App SDK functionality)
    - `Microsoft.Graphics.Win2D` `1.1.0` (for Win2D graphics)
    - `Microsoft.Windows.Compatibility` `10.0.1` (compat shims)
  - Investigate `Microsoft.Toolkit.Uwp.UI.Controls.DataGrid` (`6.0.0`) — it is flagged incompatible; decide to replace with a supported DataGrid (WinUI/Windows App SDK control) or remove dependency.

Feature upgrades and code changes:
  - Migrate UWP-specific APIs and packaging to **Windows App SDK / WinUI** patterns where applicable.
  - Update XAML usages and third-party controls (e.g., DataGrid) to Windows App SDK equivalents.
  - Convert package references to `PackageReference` elements in SDK-style project.

Other changes:
  - Update project assets and packaging (MSIX / AppX adjustments) if required for Windows App SDK.
  - Run full solution build and fix compilation issues introduced by package replacement or SDK changes.

---

**Summary:** This upgrade will convert `Chameleon.csproj` from a classic UWP project (non-SDK style) targeting `net5.0` to an SDK-style project targeting `net10.0-windows10.0.22000.0`. Key steps include converting the project file to SDK-style, replacing `Microsoft.NETCore.UniversalWindowsPlatform` with `Microsoft.WindowsAppSDK`, `Microsoft.Graphics.Win2D`, and `Microsoft.Windows.Compatibility`, and resolving the incompatible `Microsoft.Toolkit.Uwp.UI.Controls.DataGrid` dependency. After these changes, we will build and run tests (if present) to validate the upgrade.

Please open and review the generated `plan.md` file and let me know if you want any changes; once you approve, I will begin executing the steps sequentially.