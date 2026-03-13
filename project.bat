@echo off
setlocal enableextensions enabledelayedexpansion
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do set esc=%%b

set OUT_DIR=%~dp0out\
set TMP_DIR=%TEMP%\ink\

if "%1"=="" (call :PrintUsage && goto :ExitError)
for %%a in (%*) do (
    if "%%a"=="clean" (
        call :Clean || goto :ExitError
    ) else if "%%a"=="build" (
        call :Build || goto :ExitError
    ) else if "%%a"=="dist" (
        call :Dist || goto :ExitError
    ) else if "%%a"=="detect" (
        call :Detect || goto :ExitError
    ) else (
        echo Unknown command !esc![1;91m%%a!esc![0m
        goto :ExitError
    )
)
goto :Exit


:PrintUsage
echo Usage: project [commands...]
echo.
echo Available commands:
echo   build        Build everything.
echo   detect       Start Windows Ink Detector app.
echo   dist         Package plugin for distribution.
echo   clean        Remove build files.
echo.
echo Intermediate directory: %TMP_DIR%
echo.
exit /b 0


:Clean
rmdir /q /s "%TMP_DIR%." "%OUT_DIR%." 2> nul
exit /b 0


:Build
echo !esc![1;90mBuilding !esc![1;34mPlugin!esc![0m
mkdir "%TMP_DIR%plugin\obj" "%OUT_DIR%plugin" 2> nul
dotnet build "%~dp0plugin" -p:BaseIntermediateOutputPath="%TMP_DIR%plugin\obj/" ^
-p:AssemblyName=Ink -o "%OUT_DIR%plugin" > "%TMP_DIR%plugin\build.log" || (
    type "%TMP_DIR%plugin\build.log" && exit /b 1
)

echo !esc![1;90mBuilding !esc![1;34mDetector!esc![0m
mkdir "%TMP_DIR%detector" "%OUT_DIR%detector" 2> nul
cl.exe /options:strict /nologo /std:c11 "%~dp0tools\detector\main.c" /Fo:"%TMP_DIR%detector\main.obj" ^
/Fe:"%OUT_DIR%detector\detector.exe" > "%TMP_DIR%detector\build.log" || (
    type "%TMP_DIR%detector\build.log" && exit /b 1
)

echo !esc![1;90mBuilding !esc![1;34mMetadata Generator!esc![0m
mkdir "%TMP_DIR%meta" "%OUT_DIR%meta" 2> nul
cl.exe /options:strict /nologo /std:c11 "%~dp0tools\meta\main.c" /Fo:"%TMP_DIR%meta\main.obj" ^
/Fe:"%OUT_DIR%meta\meta.exe" > "%TMP_DIR%meta\build.log" || (
    type "%TMP_DIR%meta\build.log" && exit /b 1
)

echo !esc![1;90mGenerating metadata.json!esc![0m
for /f "tokens=*" %%i in ('git rev-parse --short HEAD') do set version=%%i
"%OUT_DIR%meta\meta.exe" "%OUT_DIR%plugin\Ink.dll" "!version!" "%OUT_DIR%plugin\metadata.json"
if !errorlevel! neq 0 (
    echo !esc![1;90mFailed with code !esc![1;91m!errorlevel!!esc![0m
    exit /b 1
)
exit /b 0


:Dist
echo !esc![1;90mPackaging !esc![1;34mPlugin!esc![0m
tar -acf "%OUT_DIR%Ink.zip" -C "%OUT_DIR%plugin" Ink.dll Ink.pdb metadata.json
exit /b 0


:Detect
echo !esc![1;90mRunning !esc![1;34mDetector!esc![0m
"%OUT_DIR%detector\detector.exe"
if !errorlevel! neq 0 (
    echo !esc![1;90mExited with code !esc![1;91m!errorlevel!!esc![0m
    exit /b 1
)
exit /b 0


:ExitError
cmd /c exit /b 1
:Exit
