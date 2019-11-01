---
external help file: MSFT_PrinterConfiguration_v6.0.cdxml-help.xml
Module Name:
online version:
schema: 2.0.0
---

# Get-PrintConfiguration

## SYNOPSIS
Gets the configuration information of a printer.

## SYNTAX

### UNNAMED_PARAMETER_SET_1
```
Get-PrintConfiguration [-PrinterName] <String> [-CimSession <CimSession[]>] [-ComputerName <String>]
 [-ThrottleLimit <Int32>] [<CommonParameters>]
```

### UNNAMED_PARAMETER_SET_2
```
Get-PrintConfiguration [-PrinterObject] <CimInstance> [-CimSession <CimSession[]>] [-ThrottleLimit <Int32>]
 [<CommonParameters>]
```

## DESCRIPTION
The Get-PrintConfiguration cmdlet gets the configuration information about the specified printer.
Using Get-PrintConfiguration cmdlet, you can manage the configuration of the following features:

-- Collate
-- Color
-- Duplexing Mode
-- N-Up
-- Paper Size

You cannot use wildcard characters with Get-PrintConfiguration.
You can use Get-PrintConfiguration in a Windows PowerShell remoting session.

You do not need administrator credentials to run Get-PrintConfiguration.

## EXAMPLES

### Example 1: Get the printer configuration
```
PS C:\> Get-PrintConfiguration -PrinterName "Microsoft XPS Document Writer"
```

This command returns the printer configuration for the printer named Microsoft XPS Document Writer.

### Example 2: Get the print configuration for all printers
```
PS C:\>$Printers = Get-Printer * Foreach ($Printer in $Printers){     Get-PrintConfiguration -PrinterName $Printer.name -DuplexingMode "TwoSidedLongEdge"}
```

This command gets all the printers into a variable $Printers and then loops through all the printers and displays the properties.

## PARAMETERS

### -CimSession
Runs the cmdlet in a remote session or on a remote computer.
Enter a computer name or a session object, such as the output of a New-CimSession or Get-CimSession cmdlet.
The default is the current session on the local computer.

```yaml
Type: CimSession[]
Parameter Sets: (All)
Aliases: Session

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ComputerName
Specifies the name of the computer from which to retrieve the printer configuration information.

```yaml
Type: String
Parameter Sets: UNNAMED_PARAMETER_SET_1
Aliases: Computer,CN

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue, ByPropertyName)
Accept wildcard characters: False
```

### -PrinterName
Specifies the name of the printer from which to retrieve the configuration information.

```yaml
Type: String
Parameter Sets: UNNAMED_PARAMETER_SET_1
Aliases: PN, Name

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue, ByPropertyName)
Accept wildcard characters: False
```

### -PrinterObject
Specifies the object which contains the printer from which to retrieve configuration information.

```yaml
Type: CimInstance
Parameter Sets: UNNAMED_PARAMETER_SET_2
Aliases: Printer

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue, ByPropertyName)
Accept wildcard characters: False
```

### -ThrottleLimit
Specifies the maximum number of concurrent operations that can be established to run the cmdlet.
If this parameter is omitted or a value of 0 is entered, then Windows PowerShell® calculates an optimum throttle limit for the cmdlet based on the number of CIM cmdlets that are running on the computer.
The throttle limit applies only to the current cmdlet, not to the session or to the computer.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_Printer
This cmdlet accepts one printer object.

## OUTPUTS

### Microsoft.Management.Infrastructure.CimInstance#MSFT_PrinterConfiguration
This cmdlet returns a printer configuration object.

## NOTES

## RELATED LINKS

[Set-PrintConfiguration]()

