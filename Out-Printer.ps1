#if (-not (Get-Command -Name "Out-Printer" -CommandType Cmdlet)) {

function Out-Printer {
    <#
      .Synopsis
        Sends output to a printer.
      .Description
        Out-Printer sends output to the default printer or to an alternate printer,
        if one is specified. Font, paper-orientation, paper-size, and monochrome
        printing can be requested, and margins can be set.  Content can be piped into
        the command, or it can take the path to a text or image file as a parameter.
      .Example
        > dir | Out-Printer -PrinterName 'Microsoft Print to PDF' -PrintFileName .\files.pdf -LeftMargin 50 -Verbose
        Sends a directory listing to the 'Microsoft Print to PDF' printer, creating
        a file named "files.pdf" with a left margin of 0.50 inches. This uses
        portrait format on the default paper-size, with the default typeface and font
        size. Verbose output will show the printer name, file name, and paper size.
      .Example
        > Out-Printer -Path .\Out-Printer.ps1 -PrinterName 'Microsoft Print to PDF' -PrintFileName .\listing.pdf -Landscape -PaperSize a3 -FontSize 9
        Sends a text file to the 'Microsoft Print to PDF' printer, creating a file
        named "listing.pdf". Here the printing is rotated to LandScape and A3 size
        paper is used. The font uses the default typeface set in 9 point size.
      .Example
        >get-service | ft -a | out-printer -Name 'Send To OneNote 2016' -LeftMargin 0 -RightMargin 0 -TopMargin 50 -FontSize 8
        This time services are formatted as an autosized table, and sent to OneNote;
        the page margins are customized and font reduced to fit on the page.
    #>
    [cmdletbinding()]
    Param   (
        #Specifies the content to be sent to the printer. This can be objects to print, or the target for piped objects.
        [parameter(ValueFromPipeline=$true,ParameterSetName='Default',Position=0)]
        $InputObject ,
        #Path of text file to be printed.
        [parameter(ParameterSetName='TextPath',Position=0, Mandatory=$true)]
        [Alias("FileName")]
        [string]$Path,
        #Path to a BMP, GIF, JPEG, PNG or TIFF file to be printed.
        [parameter(ParameterSetName='ImagePath',Position=0, Mandatory=$true)]
        [string]$ImagePath,
        #Name of printer - can specify either -Printer or -Name. If not specified, the default printer will be used.
        [parameter(Position=1)]
        [Alias("Name")]
        [string]$PrinterName     ,
        #Name of a paper-size on the selected printer (e.g A4, Letter)
        [parameter(Position=2)]
        [String]$PaperSize,
        #Font name to use, e.g. Calibri, Arial, Consolas, "Courier New" (defaults to "Lucida Console")
        [parameter(ParameterSetName='Default',  Position=3)]
        [parameter(ParameterSetName='TextPath', Position=3)]
        $FontName = "Lucida Console" ,
        #Size of font, defaults to 10 point.
        [parameter(ParameterSetName='Default',  Position=4)]
        [parameter(ParameterSetName='TextPath', Position=4)]
        [int]$FontSize = 10   ,
        #Specifies the name of a file to print to so that PDF etc. don't require input. File will be deleted if it exists
        [parameter(Position=5)]
        [string]$PrintFileName,
        #By default printing is portrait, unless -Landscape is specified.
        [Switch]$Landscape,
        #Top Margin in units of 1/100th inch (10mm ~ .4" = 40 Units, if working in mm, divide by 0.254). Zero will be converted to minimum margin
        [int]$TopMargin ,
        #Bottom Margin in units of 1/100th inch. Zero will be converted to minimum margin
        [int]$BottonMargin  ,
        #Left Margin in units of 1/100th inch. Zero will be converted to minimum margin
        [Int]$LeftMargin  ,
        #Right Margin in units of 1/100th inch. Zero will be converted to minimum margin
        [int]$RightMargin ,
        #Disable colour printing (or force it on with -Monochrome:$false)
        [Switch]$MonoChrome,
        #Disable scaling of images when printing.
        [parameter(ParameterSetName='ImagePath')]
        [Switch]$NoImageScale
        #resolution #$PrintDocument.PrinterSettings.PrinterResolutions
    )

    Begin   {
        #Set-up the print font - check that the supplied font name is valid first.
        $installedFonts = New-Object -TypeName "System.Drawing.Text.InstalledFontCollection"
        if ($FontName -notin $installedFonts.Families.Name) {
            Write-Warning -Message "'$FontName' does not seem to be a valid font. Switching to default"
            $FontName =  "Lucida Console"
        }
        $printFont = New-Object -TypeName "System.Drawing.font" -ArgumentList $FontName,$FontSize
        #Lines to print will hold whatever is going to be printed as text.
        $linesToPrint   = @()
        #This script block does most of the work -it gets called when it is time to print a[nother] page.
        $pagePrintScriptBlock = {
            $leftEdge     = $_.MarginBounds.Left; #Margin bounds is area inside margins to 0.01"
            $topEdge      = $_.MarginBounds.Top ;
            $fontHeight   = $printFont.GetHeight($_.Graphics) #In units used by graphics
            $linesPerPage = [Math]::Truncate($_.MarginBounds.Height  / $fontHeight)
            $lineCount    = 0; #Print lines from 0..LinesPerPage-1 - if we don't run out first
            while (($lineCount -lt $linesPerPage) -and ($linesToPrint.count -gt $lineCount)) {
                $ypos = $topEdge + ($lineCount * $fontHeight)
                $_.graphics.drawString($linestoprint[$lineCount],$printFont, [System.Drawing.Brushes]::Black, $leftEdge, $yPos )
                $lineCount ++
            }
            #Change the lines to print variable in our parent scope. Continue printing if there is anything left.
            Set-Variable -Scope 1 -Name linestoprint -Value $linestoprint[$lineCount..$linestoprint.Count]
            $_.hasMorePages =  ($linestoprint.Count -gt 0)
        }
        #This script block scales and prints an image. It is also called when it is time to print a page.
        $imagePagePrintScriptBlock = {
            # Adapated from http://monadblog.blogspot.com/2006/02/msh-print-image.html
            if ($FitImageToPaper) {
                $fitToWidth = [bool] ($bitmap.Size.Width -gt $bitmap.Size.Height)
                if (($bitmap.Size.Width -le $_.MarginBounds.Width) -and ($bitmap.Size.Height -le $_.MarginBounds.Height)) {
                    $adjustedImageSize = New-Object -TypeName "System.Drawing.Size" -ArgumentList $bitmap.Size.Width, $bitmap.Size.Height #Adjusted size is two floats.
                }
                else {
                    if ($fitToWidth) {$ratio = [double] ($_.MarginBounds.Width  / $bitmap.Size.Width) }
                    else             {$ratio = [double] ($_.MarginBounds.Height / $bitmap.Size.Height)}
                    $adjustedImageSize = New-Object -TypeName "System.Drawing.Size" -ArgumentList ([int]($bitmap.Size.Width * $ratio)), ([int]($bitmap.Size.Height * $ratio))
                }
            }
            else {$adjustedImageSize = $bitmap.Size }

            Write-Verbose -Message "Orginal size $($bitmap.Size.Height) X $($bitmap.Size.Width); bounds = $($_.MarginBounds.Width) x $($_.MarginBounds.Height); ratio = $ratio; adjustedImageSize size = $($adjustedImageSize.width) x $($adjustedImageSize.Height)"
            # calculate destination and source sizes
            $recDest = New-Object -TypeName System.Drawing.Rectangle -ArgumentList $_.MarginBounds.Location, $adjustedImageSize
            $recSrc  = New-Object -TypeName System.Drawing.Rectangle -ArgumentList 0, 0, $bitmap.Width, $bitmap.Height

            $_.Graphics.DrawImage($bitmap, $recDest, $recSrc, [Drawing.GraphicsUnit]"Pixel")
            $bitmap.Dispose()
            $bitmap = $null

            # with printing an image , assume there is nothing else to print
            $_.HasMorePages = $false
        }
    }

    Process {
        #Collect output in $linesToPrint. Catch bitmap objects. Other objects will be turned to text later.
        if ($InputObject -is [System.Drawing.Bitmap]) {$bitmap = $InputObject }
        elseif ($Path) {$linesToPrint  = Get-Content -Path $Path}
        else           {$linestoprint += $InputObject           }
    }

    End     {
        $PrintDocument = New-Object -TypeName "System.Drawing.Printing.PrintDocument"
        #region apply printer Settings
        #If printer name is given, use it; otherwise use the default printer
        if ($PrinterName) {$PrintDocument.PrinterSettings.PrinterName = $PrinterName}
        Write-Verbose -Message "Sending output to printer '$($PrintDocument.PrinterSettings.PrinterName)'."
        if ($PrintFileName) { #printing to a file ..
            #Resolve the file name and delete it if exists.
            $PrintFileName =  $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PrintFileName)
            if (Test-Path -path $PrintFileName) {Remove-Item -Path $PrintFileName -ErrorAction Stop}
            $PrintDocument.PrinterSettings.PrintToFile   = $true
            $PrintDocument.PrinterSettings.PrintFileName = $PrintFileName
            Write-Verbose -Message "Output file is $($PrintDocument.PrinterSettings.PrintFileName)."
        }
        #If there is a paper size specified make sure it applies to this printer
        if ($PaperSize -and ($Paper = ($PrintDocument.PrinterSettings.PaperSizes.Where({$_.Kind -eq $PaperSize}) )) -and ($paper.count -eq 1)  ) {
                $PrintDocument.DefaultPageSettings.PaperSize = $paper[0]
        }
        elseif ($PaperSize) {Write-Warning -Message "$PaperSize doesn't seem to be a valid paper size on the printer '$($PrintDocument.PrinterSettings.PrinterName)'"}
        $msg = "Paper is $( $PrintDocument.DefaultPageSettings.PaperSize.Kind),"
        #Set Portait / landscape and Margins, (Convert zero to the minimum margin). Leave on default margins if no values passed
        if ($Landscape)   {
            $PrintDocument.DefaultPageSettings.Landscape = $true
            Write-Verbose -Message "$msg Landscape."
        }
        else {
            $PrintDocument.DefaultPageSettings.Landscape = $false
            Write-Verbose -Message "$msg Portrait."
        }
        if ($PSBoundParameters.ContainsKey('TopMargin'))    {
                if ($TopMargin -eq 0) {$TopMargin =  $PrintDocument.DefaultPageSettings.HardMarginY }
                $PrintDocument.DefaultPageSettings.Margins.Top    = $TopMargin
        }
        if ($PSBoundParameters.ContainsKey('BottonMargin')) {
            if ($BottonMargin -eq 0) {$BottonMargin =  $PrintDocument.DefaultPageSettings.HardMarginY }
            $PrintDocument.DefaultPageSettings.Margins.Bottom = $BottonMargin
        }
        if ($PSBoundParameters.ContainsKey('LeftMargin'))   {
            if ($LeftMargin -eq 0) {$LeftMargin =  $PrintDocument.DefaultPageSettings.HardMarginX }
            $PrintDocument.DefaultPageSettings.Margins.Left   = $LeftMargin
        }
        if ($PSBoundParameters.ContainsKey('RightMargin'))  {
            if ($RightMargin -eq 0) {$RightMargin =  $PrintDocument.DefaultPageSettings.HardMarginX }
            $PrintDocument.DefaultPageSettings.Margins.Right  = $RightMargin
        }
        #endregion
        #Set the title for the job in printer queue
        if ($ImagePath) { $PrintDocument.DocumentName = $ImagePath }
        elseif  ($Path) { $PrintDocument.DocumentName = $Path }
        else            { $PrintDocument.DocumentName = "PowerShell Print Job" }

        #If what was passed is not exclusively strings, feed it through Out-String, with a suitable width.
        if ($linesToPrint -and ($linesToPrint.where({$_ -isnot [String]}))) {
            $linesToPrint = $linesToPrint | Out-String -Width ( $PrintDocument.DefaultPageSettings.PrintableArea.Width * 1.2 /$FontSize)  -Stream
        }

        #If we have lines to print : print them ; if we have an image path read the bitmap. If we have a bitmap, print that.
        if  ($linesToPrint) {
            $PrintDocument.add_PrintPage($PagePrintScriptBlock) #All work gets done by the event handler for "print page" events
            $PrintDocument.Print()
        }
        elseif ($ImagePath) {
            Write-Verbose -Message "Reading $Path..."
            $bitmap = New-Object -TypeName "System.Drawing.Bitmap" -ArgumentList ([string](Resolve-Path $ImagePath))
        }
        if        ($bitmap) {
            $FitImageToPaper = -not $NoImageScale
            $printDocument.add_PrintPage($ImagePagePrintScriptBlock) #All work gets done by the event handler for "print page" events
            $printDocument.Print()
         }

        #And clean up
        $PrintDocument.Dispose()
        $PrintDocument = $null
    }
}

Function FontCompletion {
    #Argument completer for font names
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
    [System.Drawing.Text.InstalledFontCollection]::new().families.name.where({$_ -like "$wordToComplete*" }) |
         ForEach-Object {New-CompletionResult $_ $_
    }
}
Function PrinterCompletion {
    #Argument Completer for printer names
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
    [System.Drawing.Printing.PrinterSettings]::InstalledPrinters.where({$_ -like "*$wordToComplete*" }) |
         ForEach-Object {New-CompletionResult $_ $_
    }
}

#register the two completers for the parameters in Out-Printer
If (Get-Command -Name Register-ArgumentCompleter -ErrorAction SilentlyContinue) {
    Register-ArgumentCompleter -CommandName Out-Printer               -ParameterName FontName    -ScriptBlock $Function:FontCompletion
    Register-ArgumentCompleter -CommandName Out-Printer               -ParameterName PrinterName -ScriptBlock $Function:PrinterCompletion
}

#}