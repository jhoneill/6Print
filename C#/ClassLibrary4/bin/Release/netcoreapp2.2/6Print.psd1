
#####################################################
# Module manifest for module '6Print'               #
# Copyright (c) James O'Neill. All rights reserved. #
# Licensed under the MIT License.                   #
#####################################################

@{
RootModule           =   '.\OutPrinter.dll'
ModuleVersion        =   '6.2.0'
CompatiblePSEditions =   'Desktop'
GUID                 =   'b5fb75ca-9849-42c0-8aeb-101467e5c1e1'
Author               =   'James O''Neill'
CompanyName          =   'Mobula Consulting Ltd'
Copyright            =   'Copyright James O''Neill 2019'
PowerShellVersion    =   '6.0'
CmdletsToExport      = @('Out-Printer',
                         'Get-DefaultPrinter',
                         'Set-DefaultPrinter'
)
AliasesToExport      =   'lp'
FunctionsToExport    = @()
VariablesToExport    = @{}
PrivateData          = @{
    PSData = @{
        # Tags applied to this module. These help with module discovery in online galleries.
        Tags         = @('Printing','Compatibility', 'Core', "PDF", "XDS", "OneNote")
        LicenseUri   = 'https://opensource.org/licenses/MIT'
        ProjectUri   = 'https://github.com/jhoneill/6Print'
        ReleaseNotes = 'This is the first release of this module as a dll instead of a powershell script'
    }
}
Description          = @'
This Windows-Specific module add the Out-Printer functionality seen in Windows PowerShell 5
and earlier to V6 and later. It also has basic functionality to get and set the default printer.
Out_printer takes input from the pipeline or a file and sends it to the default printer or to
a specified one. The paper-size, orientation, margins, typeface and point size can all be set,
and page numbers can be requested.
Destination files for Print-to-file operations can be specified as a parameter,
allowing "Print-to-pdf" functionality without the need for user input.
'@

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}
