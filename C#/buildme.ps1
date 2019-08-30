[cmdletbinding()]
param (
    $GUID                 = 'b5fb75ca-9849-42c0-8aeb-101467e5c1e1',  #  ([guid]::NewGuid().Guid) -
    $PowerShellVersion    = "6.0",
    $CompatiblePSEditions = "Desktop",
    [switch]$Release,
    [switch]$Quick
)
Get-ChildItem *.csproj | ForEach-Object {
    $moduleName = $_.name -replace "\.csproj$",""

    $x = [xml](Get-Content $_)
    $pg = $x.Project.PropertyGroup | Select-Object -first 1
    $TargetFramework = $pg.TargetFramework
    if ($Release) {
        dotnet build --configuration release
        if (-not $?) {return}
        $path = ".\bin\release\$TargetFramework"
    }
    else {
        dotnet build
        if (-not $?) {return}
        $path =".\bin\Debug\$TargetFramework"
        Write-Host "Process ID for Debugging is $PID"
    }

    if ($Quick) {return}

    if (Test-path -Path .\mdHelp -PathType Container  ) {
        New-ExternalHelp -Path .\mdHelp\ -OutputPath $path  -Force
    }


    $path = "$path\$moduleName"
    if ($Release) {
        Remove-Item  "$path.deps.json" -ErrorAction SilentlyContinue
        Remove-Item  "$path.pdb"       -ErrorAction SilentlyContinue
    }
    Import-Module "$Path.dll"
    $CmdletsToExport = (Get-Command -Module $moduleName -CommandType Cmdlet).Name
    $AliasesToExport = (Get-Alias | Where-Object {$_.source -eq $moduleName}).Name

    $Params = @{
        Guid                 = $GUID
        RootModule           = ".\$moduleName.dll"
        PowerShellVersion    = $PowerShellVersion
        CompatiblePSEditions = $CompatiblePSEditions
        Path                 = "$Path.PSD1"
        CmdletsToExport      = $CmdletsToExport
        AliasesToExport      = $AliasesToExport
    }

    if ($pg.Authors)             {$Params["Author"]        = $pg.Authors }
    if ($pg.Company)             {$Params["CompanyName"]   = $pg.Company }
    if ($pg.Copyright)           {$Params["Copyright"]     = $pg.Copyright }
    if ($pg.Description)         {$Params["Description"]   = $pg.Description }
    if ($pg.Version)             {$Params["ModuleVersion"] = $pg.Version}
    if ($pg.PackageLicenseUrl)   {$Params["LicenseUri"]    = $pg.PackageLicenseUrl}
    if ($pg.PackageProjectUrl)   {$Params["ProjectUri"]    = $pg.PackageProjectUrl}
    if ($pg.PackageReleaseNotes) {$Params["ReleaseNotes"]  = $ps.PackageReleaseNotes}
    if ($pg.PackageTags)         {$Params["Tags"]          = $pg.PackageTags -split "\s*[,;]\s*"}

    New-ModuleManifest @Params

}


