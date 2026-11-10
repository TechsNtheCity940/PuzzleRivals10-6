@echo off
echo Basic syntax validation for C# files
echo ======================================

setlocal enabledelayedexpansion

:: Function to check braces in a file
:check_braces
set "file=%~1"
set "open=0"
set "line_num=0"

echo Checking %~nx1...

for /f "usebackq delims=" %%a in ("%file%") do (
    set /a line_num+=1
    set "line=!%%a!"
    
    !!! Remove comments first !!!
    set "temp=!line://=REM !"
    
    !!! Count braces !!!
    for /l %%i in (0,1,99) do (
        if "!temp:~%%i,1!"=="{" set /a open+=1
        if "!temp:~%%i,1!"=="}" set /a open-=1
    )
    
    !!! If we go negative, we have a closing without opening !!!
    if !open! lss 0 (
        echo ERROR: Line !line_num!: Unexpected closing brace
        echo Line: !line!
        exit /b 1
    )
)

if !open! neq 0 (
    echo ERROR: Unclosed braces remaining: !open!
    exit /b 1
)

echo OK: Braces balanced
exit /b 0

:: Main
for %%f in (CircuitScrambleTraps.cs CircuitScramblePuzzle.cs MirrorGroupManager.cs MirrorMazePuzzle.cs) do (
    call :check_braces %%f
    if errorlevel 1 (
        echo.
        echo Validation failed for %%f
        exit /b 1
    )
    echo.
)

echo All files passed basic brace validation.
endlocal