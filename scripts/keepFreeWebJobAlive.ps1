# WebJobs running under free Azure accounts are inactivated after ~20 minutes,
# even if set to run continuously. By poking the SCM API at 15 minute
# intervals, we can overcome that limitation and keep jobs alive.

$username = "`$___"
$password = "___"
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username, $password)))

$userAgent = "powershell/1.0"
$apiUrl = "https://zoot.scm.azurewebsites.net:443/api/webjobs"

while ($true) {
   Invoke-RestMethod -Uri $apiUrl -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -UserAgent $userAgent -Method GET
   [System.Threading.Thread]::Sleep(16 * 60 * 1000)
}

