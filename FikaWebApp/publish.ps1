Write-Host "Publishing Windows x64 (Framework-Dependent Single-File)..." -ForegroundColor Green
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:UseAppHost=true -o ./publish/win-x64

Write-Host "Publishing Linux x64 (Framework-Dependent Single-File)..." -ForegroundColor Green
dotnet publish -c Release -r linux-x64 --self-contained false -p:PublishSingleFile=true -p:UseAppHost=true -o ./publish/linux-x64

Write-Host "Publish completed." -ForegroundColor Cyan