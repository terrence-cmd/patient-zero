<#
.SYNOPSIS
    Idempotently provisions ONE person's AWS hosting target for a Unity
    WebGL build: a private S3 bucket, a CloudFront distribution (pay-as-you-go,
    so it shares the account-wide free tier), and a deploy-only IAM policy
    scoped to that bucket. Safe to re-run — it checks before creating anything.

.SCOPE (deliberately lean — do not extend without a new gate)
    - One S3 bucket per person, private, CloudFront-only read access via OAC
    - One CloudFront distribution per bucket, pay-as-you-go pricing
    - One deploy-only IAM policy per person (upload + invalidate, that bucket only)
    NOT in scope: WAF, custom domains, Route 53, flat-rate CloudFront plans,
    logging/monitoring, multi-region. Those are HA-stack territory — out of
    scope for this build.

.USAGE
    .\provision-aws-target.ps1 -PersonName "alex"
    .\provision-aws-target.ps1 -PersonName "alex" -Region "us-west-2"

.REQUIRES
    AWS CLI v2, already configured (aws configure) with credentials that can
    create S3 buckets, CloudFront distributions, and IAM policies.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$PersonName,

    [string]$Region = "us-east-1"
)

$ErrorActionPreference = "Stop"

$AccountId = (aws sts get-caller-identity --query Account --output text)
$BucketName = "patient-zero-webgl-$($PersonName.ToLower())-$AccountId"
$DistributionComment = "patient-zero-webgl-$PersonName"

Write-Host "== Provisioning AWS target: $PersonName ==" -ForegroundColor Cyan
Write-Host "Bucket: $BucketName"
Write-Host ""

# ---------------------------------------------------------------------------
# 1. S3 bucket — private, no public access (CloudFront-only via OAC)
# ---------------------------------------------------------------------------
$bucketExists = $true
$prevEAP = $ErrorActionPreference
$ErrorActionPreference = "Continue"
aws s3api head-bucket --bucket $BucketName 2>$null
$ErrorActionPreference = $prevEAP
if ($LASTEXITCODE -ne 0) { $bucketExists = $false }

if (-not $bucketExists) {
    Write-Host "[1/5] Creating bucket..." -ForegroundColor Yellow
    if ($Region -eq "us-east-1") {
        aws s3api create-bucket --bucket $BucketName --region $Region | Out-Null
    } else {
        aws s3api create-bucket --bucket $BucketName --region $Region `
            --create-bucket-configuration LocationConstraint=$Region | Out-Null
    }
    aws s3api put-public-access-block --bucket $BucketName --public-access-block-configuration `
        "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true" | Out-Null
    Write-Host "      Bucket created and locked down." -ForegroundColor Green
} else {
    Write-Host "[1/5] Bucket already exists — skipping." -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# 2. CloudFront Origin Access Control — one shared OAC, reused for everyone
# ---------------------------------------------------------------------------
$oacName = "patient-zero-webgl-oac"
$oacId = aws cloudfront list-origin-access-controls `
    --query "OriginAccessControlList.Items[?Name=='$oacName'].Id" --output text
if ($oacId -eq "None") { $oacId = "" }

if ([string]::IsNullOrWhiteSpace($oacId)) {
    Write-Host "[2/5] Creating Origin Access Control..." -ForegroundColor Yellow
    $oacConfig = @{
        Name = $oacName
        SigningProtocol = "sigv4"
        SigningBehavior = "always"
        OriginAccessControlOriginType = "s3"
    } | ConvertTo-Json -Compress
    $oacConfig | Out-File ".\oac-config.tmp.json" -Encoding ascii
    $oacResult = aws cloudfront create-origin-access-control --origin-access-control-config file://oac-config.tmp.json | ConvertFrom-Json
    Remove-Item ".\oac-config.tmp.json"
    $oacId = $oacResult.OriginAccessControl.Id
    Write-Host "      OAC created: $oacId" -ForegroundColor Green
} else {
    Write-Host "[2/5] Reusing existing OAC: $oacId" -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# 3. CloudFront distribution — pay-as-you-go (shares the account-wide free tier)
# ---------------------------------------------------------------------------
$distId = aws cloudfront list-distributions `
    --query "DistributionList.Items[?Comment=='$DistributionComment'].Id" --output text
if ($distId -eq "None") { $distId = "" }

if ([string]::IsNullOrWhiteSpace($distId)) {
    Write-Host "[3/5] Creating CloudFront distribution..." -ForegroundColor Yellow
    $originDomain = "$BucketName.s3.$Region.amazonaws.com"
    $distConfig = @{
        CallerReference   = [guid]::NewGuid().ToString()
        Comment            = $DistributionComment
        Enabled            = $true
        DefaultRootObject  = "index.html"
        Origins = @{
            Quantity = 1
            Items = @(@{
                Id = "s3-origin"
                DomainName = $originDomain
                OriginAccessControlId = $oacId
                S3OriginConfig = @{ OriginAccessIdentity = "" }
            })
        }
        DefaultCacheBehavior = @{
            TargetOriginId = "s3-origin"
            ViewerProtocolPolicy = "redirect-to-https"
            AllowedMethods = @{ Quantity = 2; Items = @("GET", "HEAD") }
            ForwardedValues = @{ QueryString = $false; Cookies = @{ Forward = "none" } }
            MinTTL = 0
            Compress = $true
        }
        PriceClass = "PriceClass_100"
    } | ConvertTo-Json -Depth 10 -Compress
    $distConfig | Out-File ".\dist-config.tmp.json" -Encoding ascii
    $distResult = aws cloudfront create-distribution --distribution-config file://dist-config.tmp.json | ConvertFrom-Json
    Remove-Item ".\dist-config.tmp.json"
    $distId = $distResult.Distribution.Id
    $distDomain = $distResult.Distribution.DomainName
    Write-Host "      Distribution created: $distId ($distDomain)" -ForegroundColor Green
} else {
    $distInfo = aws cloudfront get-distribution --id $distId | ConvertFrom-Json
    $distDomain = $distInfo.Distribution.DomainName
    Write-Host "[3/5] Reusing existing distribution: $distId" -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# 4. Bucket policy — allow ONLY this CloudFront distribution to read
# ---------------------------------------------------------------------------
Write-Host "[4/5] Applying bucket policy (CloudFront-only read)..." -ForegroundColor Yellow
$bucketPolicy = @{
    Version = "2012-10-17"
    Statement = @(@{
        Sid = "AllowCloudFrontServicePrincipal"
        Effect = "Allow"
        Principal = @{ Service = "cloudfront.amazonaws.com" }
        Action = "s3:GetObject"
        Resource = "arn:aws:s3:::$BucketName/*"
        Condition = @{ StringEquals = @{ "AWS:SourceArn" = "arn:aws:cloudfront::${AccountId}:distribution/$distId" } }
    })
} | ConvertTo-Json -Depth 10 -Compress
$bucketPolicy | Out-File ".\bucket-policy.tmp.json" -Encoding ascii
aws s3api put-bucket-policy --bucket $BucketName --policy file://bucket-policy.tmp.json | Out-Null
Remove-Item ".\bucket-policy.tmp.json"
Write-Host "      Applied." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 5. Deploy-only IAM policy for this person (upload + invalidate, that bucket only)
# ---------------------------------------------------------------------------
$policyName = "patient-zero-webgl-deploy-$PersonName"
$policyArn = aws iam list-policies --scope Local `
    --query "Policies[?PolicyName=='$policyName'].Arn" --output text
if ($policyArn -eq "None") { $policyArn = "" }

if ([string]::IsNullOrWhiteSpace($policyArn)) {
    Write-Host "[5/5] Creating deploy-only IAM policy..." -ForegroundColor Yellow
    $deployPolicy = @{
        Version = "2012-10-17"
        Statement = @(
            @{
                Effect = "Allow"
                Action = @("s3:PutObject", "s3:ListBucket", "s3:DeleteObject")
                Resource = @("arn:aws:s3:::$BucketName", "arn:aws:s3:::$BucketName/*")
            },
            @{
                Effect = "Allow"
                Action = "cloudfront:CreateInvalidation"
                Resource = "arn:aws:cloudfront::${AccountId}:distribution/$distId"
            }
        )
    } | ConvertTo-Json -Depth 10 -Compress
    $deployPolicy | Out-File ".\deploy-policy.tmp.json" -Encoding ascii
    $policyResult = aws iam create-policy --policy-name $policyName --policy-document file://deploy-policy.tmp.json | ConvertFrom-Json
    Remove-Item ".\deploy-policy.tmp.json"
    $policyArn = $policyResult.Policy.Arn
    Write-Host "      Created: $policyArn" -ForegroundColor Green
} else {
    Write-Host "[5/5] Reusing existing deploy policy: $policyArn" -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "== Done: $PersonName ==" -ForegroundColor Cyan
Write-Host "Bucket:        $BucketName"
Write-Host "Distribution:  $distId"
Write-Host "Play URL:      https://$distDomain"
Write-Host "Deploy policy: $policyArn"
Write-Host ""
Write-Host "One manual step remains: attach '$policyArn' to that person's own IAM user" -ForegroundColor Yellow
Write-Host "(aws iam attach-user-policy --user-name <THEIR_USER> --policy-arn $policyArn)"
