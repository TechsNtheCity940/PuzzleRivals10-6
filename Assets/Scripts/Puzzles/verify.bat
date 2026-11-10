@echo off
echo Verifying C# syntax for modified files...
echo.

REM Check CircuitScrambleTraps.cs
echo Checking CircuitScrambleTraps.cs...
type NUL > temp.cs
type CircuitScrambleTraps.cs >> temp.cs
csc /nologo /warnaserror /target:library /out:temp.dll temp.cs 2>nul
if errorlevel 1 (
    echo CircuitScrambleTraps.cs: SYNTAX ERRORS
) else (
    echo CircuitScrambleTraps.cs: OK
)
del temp.cs temp.dll 2>nul

echo.

REM Check CircuitScramblePuzzle.cs
echo Checking CircuitScramblePuzzle.cs...
type NUL > temp.cs
type CircuitScramblePuzzle.cs >> temp.cs
csc /nologo /warnaserror /target:library /out:temp.dll temp.cs 2>nul
if errorlevel 1 (
    echo CircuitScramblePuzzle.cs: SYNTAX ERRORS
) else (
    echo CircuitScramblePuzzle.cs: OK
)
del temp.cs temp.dll 2>nul

echo.

REM Check MirrorGroupManager.cs
echo Checking MirrorGroupManager.cs...
type NUL > temp.cs
type MirrorGroupManager.cs >> temp.cs
csc /nologo /warnaserror /target:library /out:temp.dll temp.cs 2>nul
if errorlevel 1 (
    echo MirrorGroupManager.cs: SYNTAX ERRORS
) else (
    echo MirrorGroupManager.cs: OK
)
del temp.cs temp.dll 2>nul

echo.

REM Check MirrorMazePuzzle.cs
echo Checking MirrorMazePuzzle.cs...
type NUL > temp.cs
type MirrorMazePuzzle.cs >> temp.cs
csc /nologo /warnaserror /target:library /out:temp.dll temp.cs 2>nul
if errorlevel 1 (
    echo MirrorMazePuzzle.cs: SYNTAX ERRORS
) else (
    echo MirrorMazePuzzle.cs: OK
)
del temp.cs temp.dll 2>nul

echo.
echo Verification complete.