---
external help file: MSFT_PrinterProperty_v6.0.cdxml-help.xml
Module Name:
online version:
schema: 2.0.0
---

# Get-PrinterProperty

## SYNOPSIS
Retrieves printer properties for the specified printer.

## SYNTAX

```
Get-PrinterProperty [-PrinterName] <String> [[-PropertyName] <String[]>] [-CimSession <CimSession[]>]
 [-ComputerName <String>] [-ThrottleLimit <Int32>] [<CommonParameters>]
```

## DESCRIPTION
The Get-PrinterProperty cmdlet retrieves one or more printer properties for the specified printer.

You cannot use wildcard characters with Get-PrinterProperty.
You can use Get-PrinterProperty in a Windows PowerShell remoting session.

You need administrator credentials to run Get-PrinterProperty.

## EXAMPLES

### Example 1: Get printer properties
```
PS C:\> Get-PrinterProperty -PrinterName "Printer name"
```

This command returns a list of properties for the printer specified by the PrinterName parameter.

### Example 2: Get printer properties for all installed printers
```
PS C:\>$Printers = Get-Printer * Foreach ($Printer in $Printers){     Get-PrinterProperty -PrinterName $Printer.name }
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
Specifies the name of the computer from which to retrieve the printer properties.

```yaml
Type: String
Parameter Sets: (All)
Aliases: CN

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrinterName
Specifies the name of the printer from which to retrieve the printer properties.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PropertyName
Specifies the property name to retrieve.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### None
This cmdlet accepts no input objects.

## OUTPUTS

### Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_PrinterProperty
This cmdlet returns a printer property object.

## NOTES

## RELATED LINKS

[Set-PrinterProperty]()

