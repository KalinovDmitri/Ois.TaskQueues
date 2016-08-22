
powershell -Command "$workersvcs = Get-Service -Name TaskQueueWorker* | Where-Object { $_.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Stopped }; $servicescount = $workersvcs.Length; if ($servicescount -gt 0) { Start-Service -InputObject $workersvcs[0]; }"
exit