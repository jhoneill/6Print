---
external help file: OutPrinterCommand.dll-Help.xml
Module Name: OutPrinter
online version:
schema: 2.0.0
---

# Get-DefaultPrinter

## SYNOPSIS
Returns an object reflecting the default printer on the the local computer

## SYNTAX

```
Get-DefaultPrinter [<CommonParameters>]
```

## DESCRIPTION
The PrintManagement CIM module included with PowerShell does not provide a way    
to discover the default printer. This command adds one, returning an object which    
can be piped into, for example, the Get-PrintConfiguration Command from PrintManagement.    
Note that Print ManagementProvides formatting for MSFT_Printer CIM objects, which this    
command returns. Its output will be displayed differently depending on whether that    
module is loaded or not. 
## EXAMPLES

### Example 1
```powershell
PS C:\> (Get-DefaultPrinter).Name
```
Displays just the name of the default printer.

### Example 2
```powershell
PS C:\> Get-DefaultPrinter | Get-PrintConfiguration
```
Gets the default printer, and returns its configuration.

### Example 3
```powershell
PS C:\> Get-DefaultPrinter | Set-PrintConfiguration -PaperSize Letter
```
Gets the default printer, and sets the paper size to US Letter.    
Note that changing the printer configuration requires elevated permissions.

## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable.
For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_Printer

## NOTES

## RELATED LINKS
