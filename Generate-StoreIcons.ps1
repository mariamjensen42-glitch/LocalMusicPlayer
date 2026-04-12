# 生成 MSIX 所需的图标资源
# 需要先安装 ImageMagick 或使用内置方法

param(
    [string]$SourceIcon = "Assets\avalonia-logo.ico",
    [string]$OutputDir = "Assets"
)

$ErrorActionPreference = "Stop"

function Get-IconSizes {
    param([string]$IconPath)
    Add-Type -AssemblyName System.Drawing
    $icon = [System.Drawing.Icon]::ExtractAssociatedIcon($IconPath)
    $bitmap = $icon.ToBitmap()
    return @{
        Width = $bitmap.Width
        Height = $bitmap.Height
        Bitmap = $bitmap
    }
}

function New-Icon {
    param(
        [int]$Width,
        [int]$Height,
        [string]$OutputPath
    )
    Add-Type -AssemblyName System.Drawing
    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.Dispose()
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    Write-Host "Created: $OutputPath" -ForegroundColor Green
}

$requiredIcons = @(
    @{ Name = "StoreLogo"; Width = 50; Height = 50 },
    @{ Name = "Square44x44Logo"; Width = 44; Height = 44 },
    @{ Name = "Square150x150Logo"; Width = 150; Height = 150 },
    @{ Name = "Wide310x150Logo"; Width = 310; Height = 150 },
    @{ Name = "SplashScreen"; Width = 620; Height = 300 }
)

foreach ($icon in $requiredIcons) {
    $outputPath = Join-Path $OutputDir "$($icon.Name).png"
    if (Test-Path $SourceIcon) {
        try {
            Add-Type -AssemblyName System.Drawing
            $sourceBitmap = [System.Drawing.Icon]::ExtractAssociatedIcon($SourceIcon).ToBitmap()
            $newBitmap = New-Object System.Drawing.Bitmap($icon.Width, $icon.Height)
            $graphics = [System.Drawing.Graphics]::FromImage($newBitmap)
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.DrawImage($sourceBitmap, 0, 0, $icon.Width, $icon.Height)
            $graphics.Dispose()
            $newBitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
            $newBitmap.Dispose()
            $sourceBitmap.Dispose()
            Write-Host "Created: $outputPath" -ForegroundColor Green
        }
        catch {
            New-Icon -Width $icon.Width -Height $icon.Height -OutputPath $outputPath
        }
    }
    else {
        New-Icon -Width $icon.Width -Height $icon.Height -OutputPath $outputPath
    }
}

Write-Host "`n图标生成完成！" -ForegroundColor Cyan
Write-Host "请替换 Assets 目录下的图标为你的应用图标"
