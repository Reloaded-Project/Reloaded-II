# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/TestModControlParams/*" -Force -Recurse
dotnet publish "./TestModControlParams.csproj" -c Release -o "$env:RELOADEDIIMODS/TestModControlParams" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location