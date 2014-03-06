cd %srcFolder%\nuget

mkdir lib
copy ..\lib\mongodb\*.* lib\*.*
copy ..\lib\sqlite\netFx45Win32\*.* lib\*.*

nuget pack ..\source\NBlackBox\nblackbox\nblackbox.csproj
nuget setApiKey %nugetApiKey%
nuget push nblackbox.%projectVersion%.0.nupkg

del lib\*.* /Q
rmdir lib