$DisplayName = "SBSL ETL Service"

$ServiceName = "SBSLFileTransformer.exe"

$ServicePath = "..\$ServiceName"

$currentLocation = Get-Location

$executablePath = [IO.Path]::Combine($currentLocation, $ServicePath)

Write-Host "Installing $executablePath ..." -ForegroundColor Yellow

#https://stackoverflow.com/questions/37651152/cannot-start-service-on-computer
New-Service -Name $ServiceName -BinaryPathName $executablePath -Description "File Extraction Transformation and Loading tool" -StartupType Automatic -DisplayName $DisplayName -Verbose 

Start-Service -name $ServiceName

$action1 = "restart"
$action2 = "restart"
$actionLast = ""

$time1 = 5000
$time2 = 5000
$timeLast = 1000

$action = $action1+"/"+$time1+"/"+$action2+"/"+$time2+"/"+$actionLast+"//"+$timeLast

sc.exe failure $ServiceName reset= 86400 actions= $action