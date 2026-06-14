param(
    [switch]$Commit = $true,
    [string]$BackupDir = "D:\\PuzzleRivals\\backups"
)

Write-Output "Starting sprite automation for puzzles and effects..."

# Ensure Python available
$py = & python --version 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Python is required but not found in PATH."
}

# Ensure Pillow
try {
    python -c "import PIL" 2>$null
} catch {
    Write-Output "Installing Pillow..."
    python -m pip install --quiet Pillow
}

$ts = (Get-Date -Format yyyyMMdd-HHmmss)
$backup = Join-Path $BackupDir "sprites_run_$ts"
New-Item -ItemType Directory -Path $backup -Force | Out-Null

# Backup existing *_sheet.png files from puzzles and effects
Get-ChildItem -Path "D:\PuzzleRivals\Assets\Textures\Puzzles" -Recurse -Filter '*_sheet.png' | ForEach-Object {
    Copy-Item $_.FullName -Destination $backup -Force
}
Get-ChildItem -Path "D:\PuzzleRivals\Assets\Textures\Effects" -Recurse -Filter '*_sheet.png' | ForEach-Object {
    Copy-Item $_.FullName -Destination $backup -Force
}
Write-Output "Backed up existing sheets to $backup"

# Run composition/all scripts
Write-Output "Composing sprite sheets for puzzles and effects..."
python "D:\PuzzleRivals\tools\make_spritesheets_all.py"

Write-Output "Running visual audit (pre-redesign)..."
python "D:\PuzzleRivals\tools\visual_audit.py" | Tee-Object -FilePath (Join-Path $backup 'audit_pre.txt')

Write-Output "Auto-generating redesigns for flagged assets..."
python "D:\PuzzleRivals\tools\auto_redesign.py" | Tee-Object -FilePath (Join-Path $backup 'auto_redesign_log.txt')

# Replace originals with redesigns (non-destructive: originals in backup)
$replaced = 0
Get-ChildItem -Path "D:\PuzzleRivals\Assets\Textures" -Recurse -Filter '*_redesign_sheet.png' | ForEach-Object {
    $src = $_.FullName
    $target = $src -replace '_redesign_sheet.png','_sheet.png'
    Copy-Item $src -Destination $target -Force
    $replaced += 1
}
Write-Output "Replaced $replaced sheet files with redesigns"

Write-Output "Running visual audit (post-redesign)..."
python "D:\PuzzleRivals\tools\visual_audit.py" | Tee-Object -FilePath (Join-Path $backup 'audit_post.txt')

if ($Commit) {
    try {
        Write-Output "Committing changes to git"
        git add -A
        git commit -m "Automated: generated and applied 16-frame sprite sheets and redesigns for puzzle and effect assets (run $ts)`n`nCo-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>" | Out-Null
        git push || Write-Output "Push failed (credentials?). Commit created locally."
    } catch {
        Write-Warning "Git commit/push failed: $_"
    }
}

# Create zip of redesigns for easy download
$zipPath = Join-Path $backup "redesigns_$ts.zip"
Write-Output "Packaging redesigns into $zipPath"
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($backup, $zipPath)

Write-Output "Automation completed. Backup and artifacts at: $backup"
