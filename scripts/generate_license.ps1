param (
    [string]$Owner = "Cherniajev Auto Import LLC",
    [string]$ExpiresAt = "2028-12-31T23:59:59Z",
    [string]$KeyBase64 = "yZ2TAVqB8U87+QznVdUgQhkwKX/KJhMletPb/B/IePQ=",
    [string]$OutputFile = "c:\Users\Asus\Desktop\БПЗ_Черняєв\AutoDealerCRM\Licenses\dealer_valid.lic"
)

$key = [Convert]::FromBase64String($KeyBase64)
$message = "$Owner|$ExpiresAt"
$messageBytes = [System.Text.Encoding]::UTF8.GetBytes($message)

$hmac = New-Object System.Security.Cryptography.HMACSHA256
$hmac.Key = $key
$signature = [Convert]::ToBase64String($hmac.ComputeHash($messageBytes))

$licenseObj = [PSCustomObject]@{
    Owner = $Owner
    ExpiresAt = $ExpiresAt
    Signature = $signature
}

$json = $licenseObj | ConvertTo-Json -Depth 5
[System.IO.File]::WriteAllText($OutputFile, $json, [System.Text.Encoding]::UTF8)

Write-Host "Ліцензію успішно згенеровано:" -ForegroundColor Green
Write-Host "Файл: $OutputFile"
Write-Host "Власник: $Owner"
Write-Host "Діє до: $ExpiresAt"
Write-Host "HMAC-SHA256 Підпис: $signature"
