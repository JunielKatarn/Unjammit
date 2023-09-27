# escape=`

# Use the latest Windows Server Core 2022 image.
FROM mcr.microsoft.com/windows/servercore:ltsc2022

SHELL ["powershell"]

RUN `
    # Download PowerShell.
    #curl -SL --output PowerShell-7.3.7-win-x64.msi https://github.com/PowerShell/PowerShell/releases/download/v7.3.7/PowerShell-7.3.7-win-x64.msi `
    Invoke-WebRequest `
      -Uri      https://github.com/PowerShell/PowerShell/releases/download/v7.3.7/PowerShell-7.3.7-win-x64.msi `
      -OutFile  PowerShell-7.3.7-win-x64.msi `
    `
    # Install PowerShell.
    Start-Process msiexec.exe -Wait -ArgumentList '/I PowerShell-7.3.7-win-x64.msi /qn' `
    Remove-Item PowerShell-7.3.7-win-x64.msi `
    # https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/cpp/libraries/c-runtime-packages-desktop-bridge
    # https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx
    `
    # Download and install Git.
    Invoke-WebRequest `
      -Uri      https://github.com/git-for-windows/git/releases/download/v2.42.0.windows.2/Git-2.42.0.2-64-bit.exe `
      -OutFile  Git-2.42.0.2-64-bit.exe `
    `
    Start-Process -FilePath Git-2.42.0.2-64-bit.exe -ArgumentList '/VERYSILENT /NORESTART' `
    Remove-Item Git-2.42.0.2-64-bit.exe `
    `
    # Download the Build Tools bootstrapper.
    curl -SL --output vs_buildtools.exe https://aka.ms/vs/17/release/vs_buildtools.exe `
    `
    # Install Build Tools with the Microsoft.VisualStudio.Workload.AzureBuildTools workload, excluding workloads and components with known issues.
    # && (start /w vs_buildtools.exe --quiet --wait --norestart --nocache `
    #     --installPath "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools" `
    #     --add Microsoft.VisualStudio.Workload.XamarinBuildTools `
    #     --add Microsoft.VisualStudio.Workload.UniversalBuildTools `
    #     || IF "%ERRORLEVEL%"=="3010" EXIT 0) `
    #`
    # Cleanup
    #&& del /q vs_buildtools.exe
    Start-Process `
      -FilePath vs_buildtools.exe `
      -Wait `
      -ArgumentList `
        "--quiet `
        --wait `
        --norestart `
        --nocache `
        --installPath `"${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools`"" `
    Remove-Item vs_buildtools.exe `
`
#Start-Process -FilePath vs_buildtools.exe -Wait -ArgumentList "--quiet --wait --norestart --nocache --installPath "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools""
`
# Define the entry point for the docker container.
# This entry point starts the developer command prompt and launches the PowerShell shell.
ENTRYPOINT ["C:\\Program Files (x86)\\Microsoft Visual Studio\\2022\\BuildTools\\Common7\\Tools\\VsDevCmd.bat", "&&", "powershell.exe", "-NoLogo", "-ExecutionPolicy", "Bypass"]
