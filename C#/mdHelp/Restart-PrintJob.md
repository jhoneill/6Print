---
external help file: MSFT_PrintJob_v6.0.cdxml-help.xml
Module Name:
online version:
schema: 2.0.0
---

# Restart-PrintJob

## SYNOPSIS
Restarts a print job on the specified printer.

## SYNTAX

### UNNAMED_PARAMETER_SET_1
```
Restart-PrintJob [-PrinterObject] <CimInstance> [-ID] <UInt32> [-CimSession <CimSession[]>]
 [-ThrottleLimit <Int32>] [-Confirm] [-WhatIf] [<CommonParameters>]
```

### UNNAMED_PARAMETER_SET_2
```
Restart-PrintJob [-CimSession <CimSession[]>] [-ThrottleLimit <Int32>] [-Confirm] [-WhatIf]
 [<CommonParameters>]
```

### UNNAMED_PARAMETER_SET_3
```
Restart-PrintJob [-PrinterName] <String> [-ID] <UInt32> [-CimSession <CimSession[]>] [-ComputerName <String>]
 [-ThrottleLimit <Int32>] [-Confirm] [-WhatIf] [<CommonParameters>]
```

## DESCRIPTION
The Restart-PrintJob cmdlet restarts a print job on the specified printer.

You can specify the print job to restart by specifying the PrinterName and ID parameters, by specifying printer object and job ID parameter, or by specifying the job object as an input.

You cannot use wildcard characters with Restart-PrintJob.
You can use Restart-PrintJob in a Windows PowerShell remoting session.

You do not need administrator credentials to use Restart-PrintJob.

## EXAMPLES

### Example 1: Restart a selected print job
```
PS C:\> Restart-PrintJob -PrinterName "PrinterName" -ID 1
```

This command restarts the print job that has an ID of 1 on the printer named PrinterName.

### Example 2: Restart a print job by using printer object and the print job ID
```
PS C:\>$Printer = Get-Printer -PrinterName "PrinterName"
PS C:\> Restart-PrintJob -PrinterObject $Printer -ID 1
```

The first command gets a printer named PrinterName by using the Get-Printer cmdlet.
The command stores the result in the $Printer variable.

The second command restarts the print job that has the ID 1 on the printer in $Printer.

### Example 3: Restart print job using job object
```
PS C:\>$PrintJob = Get-PrintJob -PrinterName "PrinterName" -ID 1
PS C:\> Restart-PrintJob -InputObject $PrintJob
```

The first command gets a printer job that has the ID on a printer named PrinterName by using the Get-PrintJob cmdlet.
The command stores the result in the $PrintJob variable.

The second command restarts the print job in $PrintJob.

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
Specifies the name of the computer on which to restart the print job.

```yaml
Type: String
Parameter Sets: UNNAMED_PARAMETER_SET_3
Aliases: CN,computer

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ID
Specifies the ID of the print job to restart on the specified printer.
You can use wildcard characters.

```yaml
Type: UInt32
Parameter Sets: UNNAMED_PARAMETER_SET_1, UNNAMED_PARAMETER_SET_3
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrinterName
Specifies the printer name on which to restart the print job.

```yaml
Type: String
Parameter Sets: UNNAMED_PARAMETER_SET_3
Aliases: PN

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue, ByPropertyName)
Accept wildcard characters: False
```

### -PrinterObject
Specifies the printer object which contains the printer on which to restart the print job.

```yaml
Type: CimInstance
Parameter Sets: UNNAMED_PARAMETER_SET_1
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

### -Confirm
Prompts you for confirmation before running the cmdlet.Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_PrintJob
This cmdlet accepts one print job object.

## OUTPUTS

### This cmdlet produces no output.

## NOTES

## RELATED LINKS

[Get-PrintJob]()

[Remove-PrintJob]()

[Suspend-PrintJob]()

[Resume-PrintJob]()

