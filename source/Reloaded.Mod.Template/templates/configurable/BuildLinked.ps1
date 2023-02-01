# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/Reloaded.Mod.Template/*" -Force -Recurse
dotnet publish "./Reloaded.Mod.Template.csproj" -c Release -o "$env:RELOADEDIIMODS/Reloaded.Mod.Template" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location