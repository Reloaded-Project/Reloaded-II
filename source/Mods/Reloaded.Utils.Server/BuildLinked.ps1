# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/Reloaded.Utils.Server/*" -Force -Recurse
dotnet publish "./Reloaded.Utils.Server.csproj" -c Release -o "$env:RELOADEDIIMODS/Reloaded.Utils.Server" /p:OutputPath="./bin/Release" /p:RobustILLink="true"

# Restore Working Directory
Pop-Location