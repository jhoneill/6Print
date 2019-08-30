---
external help file: OutPrinterCommand.dll-Help.xml
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
  Content can be piped into the command, optionally using its alias 'lp' to give     
  a Unix like | lp, or it can take the path to a text or image file as a parameter.    
  If input is piped to the command it attempts to apply default formatting and to    
  wrap text to fit the page size, but text read from a file is printed "as-is".    
  When using Print to PDF or similar output-to-file printers, the file name   
  can be specified, and if it is, the file can be opened in its default viewer.    
  There is an option to add page numbers which are positioned at the top of text   
  pages (not image ones), if there is not sufficient border for the page number,   
  the top of the printing area is moved down to avoid overlap.     
  When setting margins, be aware that Windows specifies paper dimensions in   
  hundredths of an inch font sizes are in points - 1pt = 1/120th inch -so   
  inch-based makes more sense here than in other places.     
  If you are working in mm, multiplying by 4 converts to hundredths accurately    
  enough for most margins. (For absolute accuracy divide by 0.254)

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-Help Out-Printer | Out-Printer
```

Sends this help to the default printer using the default settings.

### Example 2
```powershell
PS C:\> dir | Out-Printer -PrinterName 'Microsoft Print to PDF' -PrintFileName .\files.pdf -LeftMargin 50 -Verbose
```

Sends a directory listing to the 'Microsoft Print to PDF' printer.    
If the file name is not specified, a dialog will appear, but in this case     
"files.pdf" is passed from the command line.    
The print will a have left margin of 0.50 inches, using the default paper-size     
in portrait format, with the default typeface and font size.    
Verbose output will show the printer name, file name, and paper size.    

### Example 3
```powershell
PS C:\> Out-Printer -Verbose -ImagePath .\lewis.jpg  -LandScape
```

Sends a picture to the default printer, printing in Landscape mode.     
Specifying -Verbose will give information on the scaling applied.

### Example 4
```powershell
PS C:\> Get-Service | ft -a | Out-Printer -Name 'Send To OneNote 2016' -LeftMargin 0 -RightMargin 0 -TopMargin 50 -FontSize 8 -PaperSize 'A3' 
```

This time services are formatted as an auto-sized table, and sent to OneNote.   
The page margins are customized and font reduced to fit on the page.   

### Example 5
```powershell
PS C:\> Out-Printer -Path .\6Print.psd1 -PrinterName 'Microsoft Print to PDF' -Destination .\listing.pdf -Landscape -PaperSize a3 -FontName 'Calibri' -FontSize 9
```

Sends a text file to the 'Microsoft Print to PDF' printer, creating a file named    
"listing.pdf". Here the printing is rotated to LandScape and the page is size to A3.    
The font uses the Calibri typeface set in 9 point size.   

### Example 6
```powershell
PS C:\> dir | lp  -Dest ~\Desktop\test3.pdf -Font 'Comic Sans MS' -Size 8 -top 0 -bottom 0 -left 0 -right 0 -Num -Open
```

This Example adds page numbers, but also uses the alias lp ; FontName can be shortened   
to Font, FontSize to size and "margin" omitted from each of the four margin parameters.    
The instructions to NumberPages and OpenDestinationFile can also be shortened.  

## PARAMETERS

### -BottomMargin
Bottom Margin in units of 1/100th inch.   
Zero will be converted to the minimum margin.

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
Specifies the name of a file to print to so that Print to PDF and similar drivers    
do not require user input. If the file already exists it will be overwritten.

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
Typeface to use, e.g. Consolas, "Courier New" (defaults to "Lucida Console").     
Non-proportionally spaced fonts will often work better, but no check is done    
to prevent the use of Comic Sans.

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
Size of font, defaults to 10 point.

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
The content to be sent to the printer.     
This can be objects to print, or the target for piped objects.

```yaml
Type: PSObject
Parameter Sets: Default
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LandScape
By default printing will be in portrait orientation, unless -Landscape is specified.

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
Left Margin in units of 1/100th inch. Zero will be converted to the minimum margin.

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
Disable scaling of images - by default images ARE scaled to fill the page.

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
If specified page numbers will be added at the top of the page.

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
If specified, the print is opened file after printing.    
If the destination file is not specified, this value is ignored. 

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Show:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PaperSize
Name of a paper-size on the selected printer (e.g A4, Letter).   
Some printers have been observed to have issues setting the size.

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
Name of printer - can specify either -Printer or -Name.     
If not specified, the default printer will be used.

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
Right Margin in units of 1/100th inch. Zero will be converted to the minimum margin.

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
Top Margin in units of 1/100th inch. Zero will be converted to the minimum margin

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
[Github](https://github.com/jhoneill/6Print)