# $shell = New-Object -com WScript.shell
# $shell.Run("devenv")
# Start-Sleep 1
# $shell.SendKeys("^c")

Start "devenv" "NanobionicsTestVirusInstaller\NanobionicsTestVirusInstaller.vdproj /build Release"
Start-Sleep -s 60

Get-Process devenv | Stop-Process -Force
