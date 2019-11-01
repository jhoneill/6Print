---
external help file: MSFT_Printer_v6.0.cdxml-help.xml
Module Name:
online version:
schema: 2.0.0
---

# Get-Printer

## SYNOPSIS
Retrieves a list of printers installed on a computer.

## SYNTAX

```
Get-Printer [[-PrinterName] <String[]>] [-CimSession <CimSession[]>] [-ComputerName <String>] [-Full]
 [-ThrottleLimit <Int32>] [<CommonParameters>]
```

## DESCRIPTION
The Get-Printer cmdlet retrieves a list of printers installed on a computer.
You can also use Get-Printer to retrieve the properties of a single printer, and then use that information as input into other cmdlets.

You can use wildcard characters with Get-Printer.
You can use a Get-Printer in a Windows PowerShell remoting session.

You do not need administrator credentials to run Get-Printer.

## EXAMPLES

### Example 1: Get a list of printers
```
PS C:\> Get-Printer
```

This command retrieves a list of printers and printer connections on the local computer.

### Example 2: Get the information for a specific printer
```
PS C:\>Get-Printer -Name "Microsoft XPS Document Writer"
```

This command retrieves information for one specific printer named Microsoft XPS Document Writer.

### Example 3: Get the detailed information for a specific printer
```
PS C:\>Get-Printer -Name "Microsoft XPS Document Writer" | Format-List
```

This command retrieves detailed information for one specific printer named Microsoft XPS Document Writer.

### Example 4: Get a list of printers on a remote computer
```
PS C:\>Get-Printer -ComputerName PrintServer
```

This command retrieves a list of printers from the computer named PrintServer.

### Example 5: Get a list of printer objects and then rename the printers
```
PS C:\>$Printer = Get-Printer -Name "Microsoft XPS Document Writer"
PS C:\> Rename-Printer -InputObject $printer "MXDW"
```

The first command gets the printer named Microsoft XPS Document Writer, and then stores it in the $Printer variable.

The second command renames the printer in $Printer by using the Rename-Printer cmdlet.

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
Specifies the name of the computer from which to retrieve the printer information.

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

### -Full
Specifies all the printer parameters to retrieve including RenderingMode and PermissionSDDL.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrinterName
Specifies the name of the printer about which to retrieve information.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: Name

Required: False
Position: 1
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

### Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_Printer
This cmdlet outputs one or more printer objects.

## NOTES

## RELATED LINKS

[Add-Printer]()

[Set-Printer]()

[Remove-Printer]()

[Rename-Printer]()

