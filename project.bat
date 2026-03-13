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
    ) else if "%%a"=="test" (
        call :Test || goto :ExitError
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
echo   build        Build plugin and tester.
echo   clean        Remove build files.
echo.
exit /b 0


:Clean
rmdir /q /s "%TMP_DIR%." "%OUT_DIR%." 2> nul
exit /b 0


:Build
mkdir "%TMP_DIR%tester" "%TMP_DIR%plugin" "%OUT_DIR%tester" "%OUT_DIR%plugin" 2> nul
echo !esc![1;90mCompiling !esc![1;34mTester!esc![0m
cl.exe /options:strict /nologo /std:c11 "%~dp0tester\main.c" /Fo:"%TMP_DIR%tester\main.obj" ^
/Fe:"%OUT_DIR%tester\tester.exe" > "%TMP_DIR%tester\build.log" || (
    type "%TMP_DIR%tester\build.log" && exit /b 1
)
exit /b 0


:Test
echo !esc![1;90mTesting!esc![0m
"%OUT_DIR%tester\tester.exe"
if !errorlevel! neq 0 (
    echo !esc![1;90mExited with code !esc![1;91m!errorlevel!!esc![0m
    exit /b 1
)
exit /b 0


:ExitError
cmd /c exit /b 1
:Exit
