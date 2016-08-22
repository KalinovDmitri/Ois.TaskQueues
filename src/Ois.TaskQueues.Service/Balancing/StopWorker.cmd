
powershell -Command "$workersvcs = Get-Service -Name TaskQueueWorker* | Where-Object { $_.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Running }; $servicescount = $workersvcs.Length; if ($servicescount -gt 0) { Stop-Service -InputObject $workersvcs[0]; }"
exit