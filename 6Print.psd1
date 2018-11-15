#####################################################
# Copyright (c) James O'Neill. All rights reserved. #
# Licensed under the MIT License.                   #
#####################################################

# Manifest for module '6Print'
@{
    ModuleVersion        =   '0.1.0'
    CompatiblePSEditions = @('Core')
    GUID                 =   'b5fb75ca-9849-42c0-8aeb-101467e5c1e1'
    Author               =   "James O'Neill"
    CompanyName          =   'Mobula Consulting'
    Copyright            =   "© James O'Neill 2018. All rights reserved"
    Description          =   'This module adds Out-Printer Functionality back into PowerShell Core'
    PowerShellVersion    =   '5.1'
    NestedModules        =   'Out-Printer.ps1'
    FunctionsToExport    = @('Out-Printer')
    PrivateData = @{
        PSData = @{
            Tags         = @('Printing','Compatibility', 'Core')
            LicenseUri   =   'https://opensource.org/licenses/MIT'
            ProjectUri   =   'https://github.com/jhoneill/6Print'
            ReleaseNotes =   'This is the first release of this module'
        }
    }
}
