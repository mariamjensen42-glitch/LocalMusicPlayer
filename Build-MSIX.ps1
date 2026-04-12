# Build-MSIX.ps1
# 构建 MSIX 安装包

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [string]$OutputDir = ".\msix-output",
    [string]$CertPath = "",
    [string]$CertPassword = ""
)

$ErrorActionPreference = "Stop"

$projectPath = ".\LocalMusicPlayer.csproj"
$publishDir = ".\msix-package"
$contentDir = ".\msix-content"

Write-Host "=== LocalMusicPlayer MSIX 打包工具 ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. 清理旧文件..." -ForegroundColor Yellow
if (Test-Path $publishDir) { Remove-Item -Path $publishDir -Recurse -Force }
if (Test-Path $contentDir) { Remove-Item -Path $contentDir -Recurse -Force }
if (Test-Path $OutputDir) { Remove-Item -Path $OutputDir -Recurse -Force }

Write-Host ""
Write-Host "2. 发布应用 ($Configuration - $Platform)..." -ForegroundColor Yellow
dotnet publish $projectPath -c $Configuration -r win-$Platform --self-contained -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "发布失败！" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "3. 复制清单文件..." -ForegroundColor Yellow
New-Item -Path $contentDir -ItemType Directory -Force | Out-Null
Copy-Item -Path "$publishDir\*" -Destination $contentDir -Recurse
Copy-Item -Path ".\Package.appxmanifest" -Destination $contentDir

Write-Host ""
Write-Host "4. 生成 MSIX 包..." -ForegroundColor Yellow
New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null

$msixPath = Join-Path $OutputDir "LocalMusicPlayer.msix"
$makeappx = Get-ChildItem -Path "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\makeappx.exe" -ErrorAction SilentlyContinue | Select-Object -Last 1

if (-not $makeappx) {
    $makeappx = Get-ChildItem -Path "C:\Program Files\Windows Kits\10\bin\*\x64\makeappx.exe" -ErrorAction SilentlyContinue | Select-Object -Last 1
}

if ($makeappx) {
    & $makeappx.FullName pack /d $contentDir /p $msixPath /o
    Write-Host "MSIX 包已生成: $msixPath" -ForegroundColor Green

    if ($CertPath -and (Test-Path $CertPath)) {
        Write-Host ""
        Write-Host "5. 签名包..." -ForegroundColor Yellow
        $signtool = $makeappx.DirectoryName + "\signtool.exe"
        if (Test-Path $signtool) {
            if ($CertPassword) {
                & $signtool sign /fd SHA256 /f $CertPath /p $CertPassword $msixPath
            } else {
                & $signtool sign /fd SHA256 /a /f $CertPath $msixPath
            }
            Write-Host "签名完成！" -ForegroundColor Green
        }
    }
} else {
    Write-Host "未找到 makeappx.exe，请安装 Windows SDK" -ForegroundColor Red
    Write-Host "下载: https://developer.microsoft.com/windows/downloads/windows-sdk/" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== 打包完成 ===" -ForegroundColor Cyan
Write-Host "发布目录: $publishDir" -ForegroundColor White
Write-Host "MSIX 包: $msixPath" -ForegroundColor White
