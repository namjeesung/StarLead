Add-Type -AssemblyName System.Drawing

$assetFolder = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\Assets'))
$pngPath = Join-Path $assetFolder 'StarLead.png'
$icoPath = Join-Path $assetFolder 'StarLead.ico'

if (-not (Test-Path -LiteralPath $pngPath)) {
    throw "Missing source image: $pngPath"
}

$source = [System.Drawing.Bitmap]::new($pngPath)
$iconBitmap = [System.Drawing.Bitmap]::new(256, 256, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($iconBitmap)
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.DrawImage($source, 0, 0, 256, 256)

$icon = [System.Drawing.Icon]::FromHandle($iconBitmap.GetHicon())
$stream = [System.IO.File]::Create($icoPath)
$icon.Save($stream)

$stream.Dispose()
$icon.Dispose()
$graphics.Dispose()
$iconBitmap.Dispose()
$source.Dispose()

Write-Host "Updated $icoPath"
