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

    Write-Host -ForegroundColor Green "Building help files..."

    if (Test-path -Path .\mdHelp -PathType Container  ) {
        New-ExternalHelp -Path .\mdHelp\ -OutputPath $path  -Force | ForEach-Object -MemberName Name
     }

    $FormatsToProcess = (Get-ChildItem "$path\*.format.ps1xml").Name
    $TypesToProcess   = (Get-ChildItem "$path\*.types.ps1xml").Name
    $NestedModules    = @()
    $functions  = @() ;  Get-ChildItem "$path\*.cdxml" | ForEach-Object {
        $NestedModules += $_.name
        $x=[xml](Get-Content $_)
        $cimNoun =   $x.PowerShellMetadata.Class.DefaultNoun
        $cimverbs = @()
        $cimVerbs += $x.PowerShellMetadata.Class.StaticCmdlets.Cmdlet.CmdletMetaData.verb
        $cimVerbs += $x.PowerShellMetadata.Class.InstanceCmdlets.Cmdlet.CmdletMetaData.verb
        foreach ($v in $cimVerbs) {if ($null -ne $v) {$functions += "$v-$cimNoun"}}
    }
    $NestedModules +=  (Get-ChildItem "$path\*.ps1").Name
    Select-String -Path "$path\*.ps1" -Pattern ("function\s+((" +  ((get-verb).verb -join "|" ) + ")-\w+)")   |
        ForEach-Object {$functions += $_.Matches.Groups[1].value}

    $path = "$path\$moduleName"
    if ($Release) {
        Remove-Item  "$path.deps.json" -ErrorAction SilentlyContinue
        Remove-Item  "$path.pdb"       -ErrorAction SilentlyContinue
    }

    Import-Module "$Path.dll"

    $CmdletsToExport = (Get-Command -Module $moduleName -CommandType Cmdlet).Name
    $AliasesToExport = (Get-Alias | Where-Object {$_.source -eq $moduleName}).Name
    Write-Host -ForegroundColor Green  "Exporting $($CmdletsToExport.Count ) cmdlet(s), $($functions.count) function(s) and $($AliasesToExport.Count) Aliase(s)"
    Write-Host -ForegroundColor Green "Importing $($NestedModules.count) module(s), $($FormatsToProcess.count) format file(s), and $($TypesToProcess.count) type file(s)."
    $Params = @{
        Path                 = "$Path.PSD1"
        Guid                 = $GUID
        RootModule           = ".\$moduleName.dll"
        NestedModules        = $NestedModules
        PowerShellVersion    = $PowerShellVersion
        CompatiblePSEditions = $CompatiblePSEditions
        FunctionsToExport    = $functions
        CmdletsToExport      = $CmdletsToExport
        AliasesToExport      = $AliasesToExport
        TypesToProcess       = $TypesToProcess
        FormatsToProcess     = $FormatsToProcess
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

    Import-Module $Params.Path
}
