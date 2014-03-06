cd %srcFolder%\nuget

rem mkdir lib
rem copy ..\lib\mongodb\*.* lib\*.*
rem copy ..\lib\sqlite\netFx45Win32\*.* lib\*.*

nuget pack ..\source\NBlackBox\nblackbox\nblackbox.csproj
nuget setApiKey %nugetApiKey%
nuget push nblackbox.%projectVersion%.0.nupkg

rem del lib\*.* /Q
rem rmdir lib