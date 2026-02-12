$dll = Join-Path $PSScriptRoot '..\ComplyEA.Module\bin\Debug\netstandard2.1\ComplyEA.Module.dll'
$asm = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $dll))
$resources = $asm.GetManifestResourceNames() | Where-Object { $_ -like '*.Images.*' } | Sort-Object
Write-Host "Found $($resources.Count) image resources:"
$resources | ForEach-Object { Write-Host "  $_" }
