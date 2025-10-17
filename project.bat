@echo off
setlocal enabledelayedexpansion

set OpenTabletDriverURL=https://github.com/OpenTabletDriver/OpenTabletDriver/releases/download/v0.6.6.2/OpenTabletDriver-0.6.6.2_win-x64.zip

set root_dir=%~dp0
set tester_dir=%root_dir%tester\
set plugin_dir=%root_dir%plugin\
set plugin_install_dir=%plugin_dir%dep\otd\userdata\Plugins\WM_POINTER\

if "%1"==""                         call :PrintUsage
if "%1"=="deps"   if "%2"==""       call :GetDeps     || exit /b 1
if "%1"=="pack"   if "%2"==""       call :Package     || exit /b 1
if "%1"=="build"  if "%2"=="tester" call :BuildTester || exit /b 1
if "%1"=="build"  if "%2"=="plugin" call :BuildPlugin || exit /b 1
if "%1"=="run"    if "%2"=="tester" call :RunTester   || exit /b 1
if "%1"=="run"    if "%2"=="plugin" call :RunPlugin   || exit /b 1

:Exit
exit /b 0

:PrintUsage
echo Usage: project COMMAND
echo.
echo Available commands:
echo   deps           Download OpenTabletDriver locally for testing the plugin.
echo   pack           Create plugin's distribution package.
echo   build plugin   build the plugin project.
echo   build tester   Build tester app.
echo   run tester     Start tester.exe.
echo   run plugin     Start local copy of OpenTabletDriver.UX.Wpf.
exit /b 0

:GetDeps
echo Downloading OpenTabletDriver ...
mkdir "%plugin_install_dir%" 2> nul
start /b /wait bitsadmin /transfer download /download /priority high "%OpenTabletDriverURL%" "%plugin_dir%dep\otd.zip" > "%plugin_dir%dep\bitsadmin-output.txt" 2>&1
if not exist "%plugin_dir%dep\otd.zip" (
    type "%plugin_dir%dep\bitsadmin-output.txt"
    exit /b 1
)
echo Extracting into "%plugin_dir%dep\otd" ...
pushd "%plugin_dir%dep\otd" && tar -xf "%plugin_dir%dep\otd.zip" && popd
exit /b 0

:Package
if not exist "%plugin_dir%bin\plugin.dll" call :BuildPlugin || exit /b 1
echo Packaging plugin ...
copy /y "%plugin_dir%metadata.json" "%plugin_dir%bin" > nul
pushd "%plugin_dir%bin"
:: FIXME: tar creates "broken" package, i.e. Windows Explorer can't open it for some reason 
::        but 7-Zip can.
tar -cf plugin.zip plugin.dll metadata.json
popd
echo Package path is "%plugin_dir%bin\plugin.zip"
exit /b 0

:BuildPlugin
echo Building plugin ...
dotnet build "%plugin_dir%." -o "%plugin_dir%bin" || exit /b 1
exit/b 0

:BuildTester
echo Building tester ...
cl /options:strict /nologo /std:c11 "%tester_dir%main.c" /Fo:"%tester_dir%main.obj" ^
/Fe:"%tester_dir%tester.exe" || exit /b 1
exit/b 0

:RunTester
if not exist "%tester_dir%tester.exe" call :BuildTester || exit /b 1
echo Running tester ...
"%tester_dir%tester.exe" || (
    echo Exited with code !errorlevel!
    exit /b 1
)
exit/b 0

:RunPlugin
if not exist "%plugin_install_dir%" call :GetDeps || exit /b 1
if not exist "%plugin_dir%bin\plugin.dll" call :BuildPlugin || exit /b 1
echo Running OpenTabletDriver.UX.Wpf ...
copy /y "%plugin_dir%bin\plugin.dll" "%plugin_install_dir%" > nul
copy /y "%plugin_dir%metadata.json" "%plugin_install_dir%" > nul
start %plugin_dir%dep\otd\OpenTabletDriver.UX.Wpf.exe
exit/b 0
