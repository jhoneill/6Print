---
external help file: OutPrinter.dll-Help.xml
Module Name: OutPrinter
online version:
schema: 2.0.0
---

# Set-DefaultPrinter

## SYNOPSIS
Sets the default printer.

## SYNTAX

```
Set-DefaultPrinter [-Name] <String> [-Passthru] [<CommonParameters>]
```

## DESCRIPTION
The PrintManagement CIM module included with PowerShell does not provide a way to     
set the default printer. This command adds one, and if -PassThru is specified,    
the selected printer is returned as a CIM object which can be used with the     
commands in the PrintManagement module. 

## EXAMPLES

### Example 1
```powershell
PS C:\> Set-DefaultPrinter 'Brother HL-1110 series' -Passthru | Set-PrintConfiguration -PaperSize A4 
```
Sets the default printer, and then sets its paper size to DIN A4  

### Example 2
```powershell
PS C:\> Get-Printer *PDF | Set-DefaultPrinter
```
Finds the "Microsoft Print To PDF" printer and Makes it the default printer   

### Example 3
```powershell
PS C:\> get-printer | where {$_.drivername -notmatch "microsoft" -AND (Get-PrintConfiguration $_).color}  | Set-DefaultPrinter 
```
Finds a color printer excluding the Onenote, PDF and XPS drivers which have     
"Microsoft" in their names, printer and Makes it the default printer. 
(If more than one driver matches these criteria the last one "wins". )   

## PARAMETERS

### -Printer
A printer name or object. If this is a string, it must exactly match one of the      
printer names returned by Get-Printer. Wildcards are NOT supported.     
If it is an object, it must have a Name or PrinterName property which matches.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases: Name

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Passthru
If specified, an object representing the selected printer will be returned,    
otherwise the command runs silently.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Show

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable.
For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_Printer

## NOTES

## RELATED LINKS
