# PowerShell script to check brace balance in C# files
$files = @(
    "BinarySolver.cs",
    "BinaryBurstPuzzle.cs",
    "CircuitScrambleTraps.cs",
    "CircuitScramblePuzzle.cs",
    "MirrorGroupManager.cs",
    "MirrorMazePuzzle.cs",
    "GridGravitySystem.cs",
    "CollapseGridPuzzle.cs"
)

$allGood = $true

foreach ($file in $files) {
    $path = "D:\PuzzleRivals\Assets\Scripts\Puzzles\" + $file
    if (-not (Test-Path $path)) {
        Write-Host "ERROR: File not found - $file" -ForegroundColor Red
        $allGood = $false
        continue
    }

    $content = Get-Content $path -Raw
    $braceCount = 0
    $lineNumber = 0
    $lines = $content.Split("`n")

    foreach ($line in $lines) {
        $lineNumber++
        # Remove comments (single line)
        if ($line.Contains("//")) {
            $commentIndex = $line.IndexOf("//")
            $line = $line.Substring(0, $commentIndex)
        }
        # Count braces
        foreach ($char in $line.ToCharArray()) {
            if ($char -eq '{') { $braceCount++ }
            if ($char -eq '}') { $braceCount-- }
            if ($braceCount -lt 0) {
                Write-Host "ERROR in $file at line $lineNumber: Unmatched closing brace" -ForegroundColor Red
                $allGood = $false
                break
            }
        }
        if (-not $allGood) { break }
    }

    if ($braceCount -ne 0) {
        Write-Host "ERROR in $file: Unbalanced braces (open: $braceCount)" -ForegroundColor Red
        $allGood = $false
    } else {
        Write-Host "OK: $file" -ForegroundColor Green
    }
}

if ($allGood) {
    Write-Host "`nAll files passed brace balance check." -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nSome files failed the check." -ForegroundColor Red
    exit 1
}