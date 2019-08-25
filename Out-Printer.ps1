function Out-Printer {
    <#
      .Synopsis
        Sends output to a printer.
      .Description
        Out-Printer sends output to the default printer or to an alternate printer, if
        one is specified. Font, paper-orientation, paper-size, and margins can all be set.
        If input is piped to the command it attempts to apply default formatting and to
        wrap text to fit the page size, but text read from a file is printed "as-is".
        When using Print to PDF or similar output-to-file printers, the file name
        can be specified to avoid the need for user interaction.
        Windows specifies paper dimensions, including page margins in hundredths of
        an inch, if you are working in mm, multiplying by 4 converts to hundredths
        accurately enough for most margins (divide by 0.254 for total accuracy).
      .Example
        > Get-Help Out-Printer | Out-Printer
        Sends this help to the default printer using the default settings.
      .Example
        > dir | Out-Printer -PrinterName 'Microsoft Print to PDF' -PrintFileName .\files.pdf -LeftMargin 50 -Verbose
        Sends a directory listing to the 'Microsoft Print to PDF' printer, creating
        a file named "files.pdf" with a left margin of 0.50 inches. This uses
        portrait format on the default paper-size, with the default typeface and font
        size. Verbose output will show the printer name, file name, and paper size.
      .Example
        >get-service | ft -a | out-printer -Name 'Send To OneNote 2016' -LeftMargin 0 -RightMargin 0 -TopMargin 50 -FontSize 8
        This time services are formatted as an autosized table, and sent to OneNote;
        the page margins are customized and font reduced to fit on the page.
      .Example
        > Out-Printer -Path .\Out-Printer.ps1 -PrinterName 'Microsoft Print to PDF' -PrintFileName .\listing.pdf -Landscape -PaperSize a3 -FontSize 9
        Sends a text file to the 'Microsoft Print to PDF' printer, creating a file
        named "listing.pdf". Here the printing is rotated to LandScape and A3 size
        paper is used. The font uses the default typeface set in 9 point size.
      .Example
        >Out-Printer -Verbose -ImagePath .\lewis.jpg  -LandScape
        Sends a picture to the default printer, printing in Landscape mode.
        Specifying -Verbose will give information on the scaling applied.
      .Example
        > dir | lp  -Dest ~\Desktop\test3.pdf -Font 'Comic Sans MS' -Size 8 -top 0 -bottom 0 -left 0 -right 0 -Num -Open
        This Example adds page numbers, but also uses the alias lp ; FontName can be shortenedAlias        to Font, FontSize to size and "margin" omitted from each of the four margin parameters.
        The instructions to NumberPages and OpenDestinationFile can also be shorten
    #>
    [cmdletbinding(DefaultParameterSetName='Default')]
    [Alias('lp')]
    Param   (
        #Specifies the content to be sent to the printer. This can be objects to print, or the target for piped objects.
        [parameter(ValueFromPipeline=$true,ParameterSetName='Default',Position=0)]
        $InputObject ,
        #Path of text file to be printed.
        #Text read from a file is not be wrapped, to wrap text pipe the file into Out-Printer.
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
        #Name of a paper-size on the selected printer (e.g A4, Letter).
        [parameter(Position=2)]
        [String]$PaperSize,
        #Typeface to use, e.g. Consolas, "Courier New" (defaults to "Lucida Console"). Non-proportionally-spaced
        #fonts will often work better, but no check is done to prevent the use of Comic Sans.
        [parameter(ParameterSetName='Default',  Position=3)]
        [parameter(ParameterSetName='TextPath', Position=3)]
        [Alias("Font")]
        $FontName = "Lucida Console" ,
        #Size of font, defaults to 10 point.
        [parameter(ParameterSetName='Default',  Position=4)]
        [parameter(ParameterSetName='TextPath', Position=4)]
        [Alias("Size")]
        [int]$FontSize = 10   ,
        #Specifies the name of a file to print to so that PDF etc. don't require input. File will be deleted if it exists.
        [parameter(Position=5)]
        [Alias("Destination")]
        [string]$PrintFileName,
        #If specified, the print is opened file after printing.
        #If the destination file is not specified, this value is ignored.
        [switch]$OpenDestinationFile,
        #If specified page numbers will be added at the top of the page.
        [switch]$NumberPages ,
        #By default printing is portrait, unless -Landscape is specified.
        [Switch]$Landscape,
        #Top Margin in units of 1/100th inch. Zero will be converted to the minimum margin.
        [int]$TopMargin ,
        #Bottom Margin in units of 1/100th inch. Zero will be converted to the minimum margin.
        [Alias('BottonMargin')] #there was a typo for this parameter in an earlier version.
        [int]$BottomMargin  ,
        #Left Margin in units of 1/100th inch. Zero will be converted to the minimum margin.
        [Int]$LeftMargin  ,
        #Right Margin in units of 1/100th inch. Zero will be converted to the minimum margin.
        [int]$RightMargin ,
        #Disable scaling of images when printing.
        [parameter(ParameterSetName='ImagePath')]
        [Switch]$NoImageScale
        #resolution #$PrintDocument.PrinterSettings.PrinterResolutions
    )

    Begin   {
        #Set-up the print font - check that the supplied font name is valid first.
        $installedFonts      = New-Object -TypeName "System.Drawing.Text.InstalledFontCollection"
        if ($FontName -notin $installedFonts.Families.Name) {
            Write-Warning -Message "'$FontName' does not seem to be a valid font. Switching to default."
            $FontName =  "Lucida Console"
        }
        $printFont           = New-Object -TypeName "System.Drawing.font" -ArgumentList $FontName,$FontSize
        #Lines to print will hold whatever is going to be printed as text.
        $linesToPrint        = @()
        $currentPage         = 1
        #This script block does most of the work -it gets called when it is time to print a[nother] page.
        $pagePrintScriptBlock = {
            $leftEdge     = $_.MarginBounds.Left; #Margin bounds is area inside margins to 0.01"
            $topEdge      = $_.MarginBounds.Top ;
            $fontHeight   = $printFont.GetHeight($_.Graphics) #In units used by graphics
            $linesPerPage = [Math]::Truncate($_.MarginBounds.Height  / $fontHeight)
            $lineCount    = 0; #Print lines from 0..LinesPerPage-1 - if we don't run out first
            Write-Progress -Activity "Printing to ($PrintDocument.PrinterSettings.PrinterName)" -CurrentOperation "Printing text. Page $CurrentPageNo,$($linesToPrint.Count)lines in the buffer"
            if ($NumberPages) {
                $PageLabel = "Page -- $CurrentPageNo"
                $left = ($PrintDocument.DefaultPageSettings.PaperSize.Width - $_.Graphics.MeasureString($PageLabel, $PrintFont).Width) / 2
                $_.Graphics.DrawString($PageLabel, $PrintFont, [System.Drawing.Brushes]::Black, $left, $PrintDocument.DefaultPageSettings.PrintableArea.Top);
                if ($PrintDocument.DefaultPageSettings.PrintableArea.Top + (2 * $fontHeight) -gt $_.MarginBounds.Top) {
                    $topEdge = $topEdge + (2 * $fontHeight)
                    $linesPerPage = $linesPerPage - 2
                }
            }

            if ($CurrentPageNo -eq 1)
            {
                WriteVerbose("Printing with margins: top=$top, left=$leftEdge. " + [System.Math]::Truncate($linesPerPage) + " Lines per page");
            }
            while (($lineCount -lt $linesPerPage) -and ($linesToPrint.count -gt $lineCount)) {
                $ypos = $topEdge + ($lineCount * $fontHeight)
                $_.graphics.drawString($linestoprint[$lineCount],$printFont, [System.Drawing.Brushes]::Black, $leftEdge, $yPos )
                $lineCount ++
            }
            #Change the lines to print variable in our parent scope. Continue printing if there is anything left.
            Set-Variable -Scope 1 -Name linestoprint -Value $linestoprint[$lineCount..$linestoprint.Count]
            $_.hasMorePages  =  ($linestoprint.Count -gt 0)
            $currentPage ++
        }
        #This script block scales and prints an image. It is also called when it is time to print a page.
        $imagePagePrintScriptBlock = {
            # Adapated from http://monadblog.blogspot.com/2006/02/msh-print-image.html
            Write-Progress -Activity "Printing to ($PrintDocument.PrinterSettings.PrinterName)" -CurrentOperation "Printing image. Page $CurrentPageNo"
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
        if     ($InputObject -is [System.Drawing.Bitmap]) {$bitmap = $InputObject }
        elseif ($InputObject -is [string] ) {
                        $linesToPrint += $InputObject -split "\r\n+"
        }
        elseif ($Path) {$linesToPrint  = Get-Content -Path $Path}
        else           {$linesToPrint += $InputObject }
    }

    End     {
        $PrintDocument = New-Object -TypeName "System.Drawing.Printing.PrintDocument"
        #region apply printer Settings
        #If printer name is given, use it; otherwise use the default printer
        $OriginalPrinterName = $PrintDocument.PrinterSettings.PrinterName
        if ($PrinterName) {$PrintDocument.PrinterSettings.PrinterName = $PrinterName}
        $msg = "Printing to '$($PrintDocument.PrinterSettings.PrinterName)'"
        if ($PrintFileName) { #printing to a file ..
            #Resolve the file name and delete it if exists.
            $PrintFileName =  $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PrintFileName)
            if (Test-Path -path $PrintFileName) {Remove-Item -Path $PrintFileName -ErrorAction Stop}
            $PrintDocument.PrinterSettings.PrintToFile   = $true
            $PrintDocument.PrinterSettings.PrintFileName = $PrintFileName
            $msg += " ($($PrintDocument.PrinterSettings.PrintFileName))"
        }
        $OriginalPageSettings = $PrintDocument.DefaultPageSettings
        #If there is a paper size specified make sure it applies to this printer
        if ($PaperSize -and ($Paper = ($PrintDocument.PrinterSettings.PaperSizes.Where({$_.Kind -eq $PaperSize}) )) -and ($paper.count -eq 1)  ) {
                $PrintDocument.DefaultPageSettings.PaperSize = $paper[0]
        }
        elseif ($PaperSize) {Write-Warning -Message "$PaperSize doesn't seem to be a valid paper size on the printer '$($PrintDocument.PrinterSettings.PrinterName).'"}
        $msg += ". Paper is $( $PrintDocument.DefaultPageSettings.PaperSize.Kind),"
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
        if ($PSBoundParameters.ContainsKey('BottomMargin')) {
            if ($BottomMargin -eq 0) {$BottomMargin =  $PrintDocument.DefaultPageSettings.HardMarginY }
            $PrintDocument.DefaultPageSettings.Margins.Bottom = $BottomMargin
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
            Write-Progress -Activity "Printing to ($PrintDocument.PrinterSettings.PrinterName)" -CurrentOperation "Renderding text"
            $linesToPrint = $linesToPrint | Out-String -Width ( $PrintDocument.DefaultPageSettings.PrintableArea.Width * 1.2 /$FontSize)  -Stream
        }

        #If we have lines to print : print them ; if we have an image path read the bitmap. If we have a bitmap, print that.
        if  ($linesToPrint) {
            $PrintDocument.add_PrintPage($PagePrintScriptBlock) #All work gets done by the event handler for "print page" events
            $PrintDocument.Print()
        }
        elseif ($ImagePath) {
            Write-Verbose -Message "Reading $ImagePath..."
            $bitmap = New-Object -TypeName "System.Drawing.Bitmap" -ArgumentList ([string](Resolve-Path $ImagePath))
        }
        if        ($bitmap) {
            $FitImageToPaper = -not $NoImageScale
            $printDocument.add_PrintPage($ImagePagePrintScriptBlock) #All work gets done by the event handler for "print page" events
            $printDocument.Print()
         }
        Write-Progress -Activity "Printing to ($PrintDocument.PrinterSettings.PrinterName)"  -Completed
        if ($OpenDestinationFile -and $PrintDocument.PrinterSettings.PrintToFile -and  (Test-Path -Path $PrintFileName) ) {
            Start-Process -Path $PrintFileName
        }
        #And clean up
        $PrintDocument.DefaultPageSettings = $OriginalPageSettings
        $PrintDocument.PrinterSettings.PrinterName = $OriginalPrinterName
        $PrintDocument.Dispose()
        $PrintDocument = $null

    }
}

#region argument completers
Function FontCompletion {
    #Argument completer for font names
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
    [System.Drawing.Text.InstalledFontCollection]::new().families.name.where({$_ -like "$wordToComplete*" }) |
         ForEach-Object {
            New-Object -TypeName System.Management.Automation.CompletionResult -ArgumentList "'$_'" , $_ ,
            ([System.Management.Automation.CompletionResultType]::ParameterValue) , $_
          }
}
Function PrinterCompletion {
    #Argument Completer for printer names
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
    [System.Drawing.Printing.PrinterSettings]::InstalledPrinters.where({$_ -like "*$wordToComplete*" }) |
         ForEach-Object {
             New-Object -TypeName System.Management.Automation.CompletionResult -ArgumentList "'$_'" , $_ ,
            ([System.Management.Automation.CompletionResultType]::ParameterValue) , $_}
}

Function PaperSizeCompletion {
    #Argument completer for paper sizes.
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
    $pd = New-Object -TypeName System.Drawing.Printing.PrintDocument
    $pd.PrinterSettings.PaperSizes.Where({$_kind -like "*$wordToComplete*"}) |
        ForEach-Object {
            New-Object -TypeName System.Management.Automation.CompletionResult -ArgumentList "'$_.Kind'" , $_.Kind ,
            ([System.Management.Automation.CompletionResultType]::ParameterValue) , $_.PaperName
        }

}

#register the two completers for the parameters in Out-Printer
If (Get-Command -Name Register-ArgumentCompleter -ErrorAction SilentlyContinue) {
    Register-ArgumentCompleter -CommandName Out-Printer -ParameterName FontName    -ScriptBlock $Function:FontCompletion
    Register-ArgumentCompleter -CommandName Out-Printer -ParameterName PrinterName -ScriptBlock $Function:PrinterCompletion
    Register-ArgumentCompleter -CommandName Out-Printer -ParameterName PaperSize   -ScriptBlock $Function:PaperSizeCompletion
}
#endregion