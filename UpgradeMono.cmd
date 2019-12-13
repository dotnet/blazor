@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0UpgradeMono.ps1""" %*"
exit /b %ErrorLevel%
