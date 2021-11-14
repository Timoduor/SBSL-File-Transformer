$ServiceName = "SBSLFileTransformer.exe"

#note you might need to end the task using Task Manager first due to multi-threading of the application
Stop-Service -Name $ServiceName

Remove-Service -Name $ServiceName

sc.exe delete $ServiceName