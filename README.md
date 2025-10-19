

**Status**: SAI2 works as well as other apps that support WM_POINTER messages. Buttons, tilt, and 
rotation are not implemented.

[otd]: https://opentabletdriver.net/
[wmp]: https://learn.microsoft.com/en-us/windows/win32/inputmsg/wm-pointerdown
[sai]: https://www.systemax.jp/en/sai/

Requirements for building the **plugin**:
- [.NET 6.0 SDK][dotnet]
- [OpenTabletDriver.Plugin NuGet package][otd-nuget]

[dotnet]: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
[otd-nuget]: https://www.nuget.org/packages/OpenTabletDriver.Plugin/

Requirements for building **tester** app (optional):
- [Build Tools for Visual Studio][vstools] (*MSVC* and *Windows 10/11 SDK*)
- [64-bit command prompt][x64prompt]

[vstools]: https://visualstudio.microsoft.com/downloads/?q=build+tools#build-tools-for-visual-studio-2022
[x64prompt]: https://learn.microsoft.com/en-us/cpp/build/how-to-enable-a-64-bit-visual-cpp-toolset-on-the-command-line?view=msvc-170

Using `project` script:
```
Usage: project COMMAND

Available commands:
  deps           Download OpenTabletDriver locally for testing the plugin.
  pack           Create plugin's distribution package.
  build plugin   build the plugin project.
  build tester   Build tester app.
  run tester     Start tester.exe.
  run plugin     Start local copy of OpenTabletDriver.UX.Wpf.
```
