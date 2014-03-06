cd %srcFolder%\nuget

nuget pack ..\source\NBlackBox\nblackbox\nblackbox.csproj
nuget setApiKey %nugetApiKey%
nuget push nblackbox.%projectVersion%.0.nupkg
