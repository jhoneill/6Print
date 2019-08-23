---
external help file: OutPrinter.dll-Help.xml
Module Name: Out-Printer
online version:
schema: 2.0.0
---

# Out-Printer

## SYNOPSIS
Sends output to a printer.

## SYNTAX

### Default (Default)
```
Out-Printer [-InputObject] <PSObject> [[-PrinterName] <String>] [[-PaperSize] <String>] [[-FontName] <String>]
 [[-FontSize] <Int32>] [[-Destination] <String>] [-OpenDestinationFile] [-NumberPages] [-LandScape]
 [-TopMargin <Int32>] [-BottomMargin <Int32>] [-LeftMargin <Int32>] [-RightMargin <Int32>] [-NoImageScale]
 [<CommonParameters>]
```

### TextPath
```
Out-Printer [-Path] <String> [[-PrinterName] <String>] [[-PaperSize] <String>] [[-FontName] <String>]
 [[-FontSize] <Int32>] [[-Destination] <String>] [-OpenDestinationFile] [-NumberPages] [-LandScape]
 [-TopMargin <Int32>] [-BottomMargin <Int32>] [-LeftMargin <Int32>] [-RightMargin <Int32>] [-NoImageScale]
 [<CommonParameters>]
```

### ImagePath
```
Out-Printer [-ImagePath] <String> [[-PrinterName] <String>] [[-PaperSize] <String>] [[-Destination] <String>]
 [-OpenDestinationFile] [-NumberPages] [-LandScape] [-TopMargin <Int32>] [-BottomMargin <Int32>]
 [-LeftMargin <Int32>] [-RightMargin <Int32>] [-NoImageScale] [<CommonParameters>]
```

## DESCRIPTION
  Out-Printer sends output to the default printer or to an alternate printer,
  if one is specified. Font, paper-orientation, paper-size, and margins can all 
  be set, and page numbers can be requested.  
  Content can be piped into the command, or it can take the path to a text or image 
  file as a parameter. 
  When using Print to PDF or similar output-to-file type printers, the file name
  can be specified, and if it is, the file can be opened in its default viewer. 

## EXAMPLES

### Example 1
```powershell
PS C:\> dir | Out-Printer -PrinterName 'Microsoft Print to PDF' -PrintFileName .\files.pdf -LeftMargin 50 -Verbose  
```

Sends a directory listing to the 'Microsoft Print to PDF' printer, creating
a file named "files.pdf" with a left margin of 0.50 inches. 
This uses portrait format on the default paper-size, with the default typeface and font size.
Verbose output will show the printer name, file name, and paper size. 

### Example 2
```powershell
PS C:\> Out-Printer -Path .\Out-Printer.psd1 -PrinterName 'Microsoft Print to PDF' -PrintFileName .\listing.pdf -Landscape -PaperSize a3 -FontSize 9 
```

Sends a text file to the 'Microsoft Print to PDF' printer, creating a file named "listing.pdf". 
Here the printing is rotated to LandScape and A3 size paper is used. 
The font uses the default typeface set in 9 point size.

### Example 3
```powershell
PS C:\> Get-Service | ft -a | Out-Printer -Name 'Send To OneNote 2016' -LeftMargin 0 -RightMargin 0 -TopMargin 50 -FontSize 8 
```

This time services are formatted as an autosized table, and sent to OneNote.
The page margins are customized and font reduced to fit on the page.


## PARAMETERS

### -BottomMargin
Bottom Margin in units of 1/100th inch. (10mm ~ .4" = 40 Units, if working in mm, divide by 0.254).
Zero will be converted to minimum margin

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

### -Destination
Specifies the name of a file to print to so that PDF etc. don't require input. 
If the file already exists it will be overwritten.

```yaml
Type: String
Parameter Sets: (All)
Aliases: PrintFileName

Required: False
Position: 5
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontName
Typeface to use, e.g. Calibri, Arial, Consolas, "Courier New" (defaults to "Lucida Console")

```yaml
Type: String
Parameter Sets: Default, TextPath
Aliases: Font

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontSize
Size of font, defaults to 10 point

```yaml
Type: Int32
Parameter Sets: Default, TextPath
Aliases: Size

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ImagePath
Path to a BMP, GIF, JPEG, PNG or TIFF file to be printed.

```yaml
Type: String
Parameter Sets: ImagePath
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
The content to be sent to the printer. This can be objects to print, or the target for piped objects.

```yaml
Type: PSObject
Parameter Sets: Default
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LandScape
By default printing is portrait, unless -Landscape is specified.

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

### -LeftMargin
Left Margin in units of 1/100th inch. Zero will be converted to minimum margin

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

### -NoImageScale
Disable scaling of images - by default images are scaled to fill the page

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

### -NumberPages
f specified page numbers will be added at the top of the page.

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

### -OpenDestinationFile
If specified opens the print file after printing (ignored if the destination file is not specified) 

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

### -PaperSize
Name of a paper-size on the selected printer (e.g A4, Letter)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Path of text file to be printed. 
Text read from a file is not be wrapped, to wrap text pipe the file into Out-Printer.

```yaml
Type: String
Parameter Sets: TextPath
Aliases: FileName

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrinterName
Name of printer - can specify either -Printer or -Name. If not specified, the default printer will be used.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Name

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RightMargin
Right Margin in units of 1/100th inch. Zero will be converted to minimum margin

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

### -TopMargin
Top Margin in units of 1/100th inch. Zero will be converted to minimum margin

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
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable.
For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
