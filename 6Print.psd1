#####################################################
# Copyright (c) James O'Neill. All rights reserved. #
# Licensed under the MIT License.                   #
#####################################################

# Manifest for module '6Print'
@{
    ModuleVersion        =   '1.0.1'
    CompatiblePSEditions = @('Core')
    GUID                 =   'b5fb75ca-9849-42c0-8aeb-101467e5c1e1'
    Author               =   "James O'Neill"
    CompanyName          =   'Mobula Consulting'
    Copyright            =   "© James O'Neill 2018/9. All rights reserved"
    Description          =   'This module adds Out-Printer Functionality back into PowerShell Core'
    PowerShellVersion    =   '5.1'
    NestedModules        =   'Out-Printer.ps1'
    FunctionsToExport    = @('Out-Printer')
    AliasesToExport      =   'lp'
    PrivateData = @{
        PSData = @{
            Tags         = @('Printing','Compatibility', 'Core')
            LicenseUri   =   'https://opensource.org/licenses/MIT'
            ProjectUri   =   'https://github.com/jhoneill/6Print'
            ReleaseNotes =   @'
This is intended to be the the last release of the module as script, future releases will be a DLL.
This release adds support for opening a print file when after printing is completed, and for page numbering.
It provides an alias of lp for Out-Printer and p to make it easier to shorten parameters there are
additional parameter aliases
It also provides progress indicators, better verbose messages and better help.
'@
        }
    }
}
