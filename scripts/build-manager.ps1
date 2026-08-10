<#
.SYNOPSIS
    Builds the Unity project (WebGL or Desktop) via batch mode, and for
    WebGL, deploys the output to that person's existing S3/CloudFront
    target (provisioned separately by provision-aws-target.ps1) and
    invalidates the CloudFront cache.

.SCOPE (deliberately lean)
    - Trigger one Unity batch-mode build via BuildScript.BuildWebGL /
      BuildScript.BuildDesktop (the contract defined in the Fable 5 brief)
    - For WebGL only: sync Builds\WebGL to the person's S3 bucket, then
      invalidate CloudFront
    - Desktop builds are local-only — this script just confirms the build
      succeeded and reports the output path. No packaging/zipping, no
      distribution — that's a separate future step if ever needed.
    NOT in scope: provisioning AWS resources (that's provision-aws-target.ps1,
    not this script — this script assumes the target already exists and
    fails clearly if it doesn't), versioning, rollback, CI/CD.

.USAGE
    .\build-manager.ps1 -PersonName "alex" -Target WebGL
    .\build-manager.ps1 -PersonName "alex" -Target Desktop

.REQUIRES
    - Unity Editor installed, with UnityPath pointing at Unity.exe
    - For WebGL: that person's AWS target already provisioned via
      provision-aws-target.ps1 (same -PersonName)
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$PersonName,

    [ValidateSet("WebGL", "Desktop")]
    [string]$Target = "WebGL",

    [string]$ProjectPath = ".",

    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\<VERSION>\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"

$DistributionComment = "fighter-webgl-$PersonName"
$LogFile = ".\build-log-$Target.txt"
$OutputFolder = Join-Path $ProjectPath "Builds\$Target"

# ---------------------------------------------------------------------------
# 1. Run the Unity batch-mode build
# ---------------------------------------------------------------------------
Write-Host "== Building '$Target' for $PersonName ==" -ForegroundColor Cyan

if (-not (Test-Path $UnityPath)) {
    Write-Host "Unity not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Update -UnityPath to point at your installed Unity Editor version." -ForegroundColor Red
    exit 1
}

$method = "BuildScript.Build$Target"
Write-Host "Running: $UnityPath -batchmode -quit -projectPath $ProjectPath -executeMethod $method"

& $UnityPath -batchmode -quit -projectPath $ProjectPath -executeMethod $method -logFile $LogFile
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -ne 0) {
    Write-Host "Build FAILED (exit code $buildExitCode). See $LogFile for details." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $OutputFolder)) {
    Write-Host "Build reported success but output folder not found: $OutputFolder" -ForegroundColor Red
    exit 1
}

Write-Host "Build succeeded: $OutputFolder" -ForegroundColor Green

# ---------------------------------------------------------------------------
# 2. Desktop builds stop here — local only, nothing to deploy
# ---------------------------------------------------------------------------
if ($Target -eq "Desktop") {
    Write-Host ""
    Write-Host "Desktop build is local-only. Output at: $OutputFolder" -ForegroundColor Cyan
    exit 0
}

# ---------------------------------------------------------------------------
# 3. WebGL: find this person's existing AWS target (provisioned separately)
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Looking up AWS target for '$PersonName'..." -ForegroundColor Cyan

$distId = aws cloudfront list-distributions `
    --query "DistributionList.Items[?Comment=='$DistributionComment'].Id" --output text

if ([string]::IsNullOrWhiteSpace($distId)) {
    Write-Host "No CloudFront distribution found for '$PersonName'." -ForegroundColor Red
    Write-Host "Run provision-aws-target.ps1 -PersonName `"$PersonName`" first." -ForegroundColor Red
    exit 1
}

$distInfo = aws cloudfront get-distribution --id $distId | ConvertFrom-Json
$originDomain = $distInfo.Distribution.DistributionConfig.Origins.Items[0].DomainName
$bucketName = $originDomain.Split(".")[0]
$distDomain = $distInfo.Distribution.DomainName

Write-Host "Bucket: $bucketName"
Write-Host "Distribution: $distId"

# ---------------------------------------------------------------------------
# 4. Sync build to S3, then invalidate CloudFront
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Syncing build to S3..." -ForegroundColor Yellow
aws s3 sync $OutputFolder "s3://$bucketName/" --delete

Write-Host "Invalidating CloudFront cache..." -ForegroundColor Yellow
aws cloudfront create-invalidation --distribution-id $distId --paths "/*" | Out-Null

Write-Host ""
Write-Host "== Deployed: $PersonName ==" -ForegroundColor Cyan
Write-Host "Play URL: https://$distDomain"
