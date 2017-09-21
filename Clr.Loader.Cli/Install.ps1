$installPath = Read-Host -Prompt "Input install path"

if(!(test-path $installPath))
{
	Write-host "Invalid path or path does not exist $installPath"
}
else 
{
	$msbuild = 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe'
	& $msbuild ../Clr.Loader.sln /p:Configuration=Release /p:OutDir=$installPath /t:Build 
	Write-host "Installed"
}
