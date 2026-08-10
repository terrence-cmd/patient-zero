<#
.SYNOPSIS
    Scaffolds the folder structure for a Unity project built to support
    both a browser (WebGL) build and a desktop (Windows Standalone) build
    from the same codebase.

.USAGE
    .\1-scaffold-project.ps1 -ProjectRoot "C:\Dev\PatientZero"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectRoot
)

$folders = @(
    "Assets\Scenes",
    "Assets\Scripts\Characters",
    "Assets\Scripts\Combat",
    "Assets\Scripts\Input",
    "Assets\Scripts\UI",
    "Assets\Scripts\Netcode",
    "Assets\Prefabs\Characters",
    "Assets\Prefabs\Stages",
    "Assets\Art\Characters",
    "Assets\Art\Stages",
    "Assets\Art\UI",
    "Assets\Audio\SFX",
    "Assets\Audio\Music",
    "Assets\Editor",
    "Builds\WebGL",
    "Builds\Desktop",
    "Backend"
)

Write-Host "Creating project scaffold at: $ProjectRoot" -ForegroundColor Cyan

foreach ($folder in $folders) {
    $path = Join-Path $ProjectRoot $folder
    New-Item -ItemType Directory -Force -Path $path | Out-Null
    Write-Host "  Created: $folder"
}

$backendReadme = @"
# Backend (future — HA stack)

This folder is a placeholder for the eventual matchmaking/relay backend.

Not needed until the game has real online players (see Stage 4-5 of the
dev roadmap: rollback netcode + scaling to a real player base). At that
point this is where the high-availability infrastructure config
(load balancer, auto-scaling group, multi-AZ database) will live —
separate from the game client itself.
"@

Set-Content -Path (Join-Path $ProjectRoot "Backend\README.md") -Value $backendReadme -Encoding utf8

$notesReadme = @"
# Project Notes

## Build targets
- **WebGL** -> Builds\WebGL  (early-stage prototyping, S3 + CloudFront hosting)
- **Desktop (Windows Standalone)** -> Builds\Desktop  (frame-perfect combat, netcode, shipping)

## Workflow
- Ideas start in the browser build. Web-side work is done with Claude in
  Chrome + Cursor.
- Once an idea matures past basic movement/hitboxes, development shifts
  to the desktop build target. Desktop-side work is done with Claude Code
  + Cursor.
- Same Unity project, same Assets\ folder, both build targets throughout —
  no fork/rewrite when a project graduates to desktop.
"@

Set-Content -Path (Join-Path $ProjectRoot "PROJECT_NOTES.md") -Value $notesReadme -Encoding utf8

Write-Host ""
Write-Host "Scaffold complete." -ForegroundColor Green
Write-Host "Next: open Unity Hub, create/open the project at $ProjectRoot, then run 2-init-git.ps1 from that same folder."
