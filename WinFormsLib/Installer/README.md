# Launching an application after installation

`EnableLaunchApplication.js` adds a checked launch option to the standard `FinishedForm` completion dialog produced by Microsoft Visual Studio Installer Projects. It modifies an existing MSI; it does not build, install or launch the application.

Run it with Windows Script Host:

```bat
cscript.exe //nologo "EnableLaunchApplication.js" "Application.msi" "Application.exe" "Launch [ProductName]"
```

All three arguments are required: the MSI path, the executable's file name inside the MSI, and the checkbox text. The text supports MSI properties such as `[ProductName]`. Keep the script in the shared WinFormsLib checkout and reference it from the setup project's post-build event. Visual Studio Installer Projects spells the output macro `$(BuiltOuputPath)`.

The executable is resolved from the MSI's File table. Clicking the completion button launches it through `MsiLaunchApp` when the checkbox is selected, before closing the dialog. The checkbox is hidden and disabled during maintenance and removal. Silent installations do not launch the application. Running the script again updates its own entries without adding duplicates. Run it before signing the MSI. Supply checkbox text in the same language as the MSI dialogs; the script does not translate the installer.

This script targets the standard Visual Studio `FinishedForm` layout, including `BodyText`, `BodyTextRemove` and `UpdateText`. It requires Windows Installer 5.0 or later for the launch event.

Reference: [MsiLaunchApp ControlEvent](https://learn.microsoft.com/en-us/windows/win32/msi/msilaunchapp-controlevent).
