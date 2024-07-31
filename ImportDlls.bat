@echo off
setlocal

set destinationFolder=libs

robocopy "C:\Program Files\PowerToys" %destinationFolder% PowerToys.Common.UI.dll
robocopy "C:\Program Files\PowerToys" %destinationFolder% PowerToys.ManagedCommon.dll
robocopy "C:\Program Files\PowerToys" %destinationFolder% PowerToys.Settings.UI.Lib.dll
robocopy "C:\Program Files\PowerToys" %destinationFolder% Wox.Plugin.dll

endlocal