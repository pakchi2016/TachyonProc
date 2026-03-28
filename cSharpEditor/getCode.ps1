$basepath = Split-Path $MyInvocation.MyCommand.Path -Parent

$fileList = New-Object System.Collections.ArrayList
$log = "$($basepath)\codelist.txt"

Get-ChildItem -Path $basepath -File | ? {($_.Name -like "*.xaml") -or ($_.Name -like "*.cs")} | foreach { $fileList.Add($_.FullName) | Out-Null}
Get-ChildItem -Path $basepath -Directory | foreach { Get-ChildItem -Path $_.FullName -File | ? {($_.Name -like "*.xaml") -or ($_.Name -like "*.cs")} | foreach { $fileList.Add($_.FullName) | Out-Null } }

$fileList.ForEach{
    Write-Output (Split-Path $_ -Leaf) | Out-File -Encoding utf8 -FilePath $log -Append
    Write-Output "`n=========================================`n" | Out-File -Encoding utf8 -FilePath $log -Append
    Get-Content $_ | Out-File -Encoding utf8 -FilePath $log -Append
    Write-Output "`n=========================================`n" | Out-File -Encoding utf8 -FilePath $log -Append
}