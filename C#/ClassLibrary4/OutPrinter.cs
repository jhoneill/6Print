﻿using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace OutPrinter
{
    // Declare the class as a cmdlet and specify verb and noun for the cmdlet name.
    [Cmdlet(VerbsData.Out, "Printer", DefaultParameterSetName = "Default")]
    [Alias("lp")]
    public class OutPrinterCommand : Cmdlet
    {
        #region Declare the parameters for the cmdlet.
        //Specifies the content to be sent to the printer. This can be objects to print, or the target for piped objects.
        [Parameter(ValueFromPipeline = true, ParameterSetName = "Default", Position = 0, Mandatory = true)]
        public PSObject InputObject { get; set; }
        //Path of text file to be printed.
        [Parameter(ParameterSetName = "TextPath", Position = 0, Mandatory = true)]
        [Alias("FileName")]
        public string Path { get; set; }
        //Path to a BMP, GIF, JPEG, PNG or TIFF file to be printed.
        [Parameter(ParameterSetName = "ImagePath", Position = 0, Mandatory = true)]
        public string ImagePath { get; set; }
        //Name of printer - can specify either -Printer or -Name. If not specified, the default printer will be used
        [Parameter(Position = 1), ArgumentCompleter(typeof(PrinterNameCompleter))]
        [Alias("Name")]
        public string PrinterName { get; set; }
        //Name of a paper-size on the selected printer (e.g A4, Letter)
        [Parameter(Position = 2)]
        public string PaperSize { get; set; }
        //Font name to use, e.g. Calibri, Arial, Consolas, "Courier New" (defaults to "Lucida Console")
        [Parameter(ParameterSetName = "Default", Position = 3)]
        [Parameter(ParameterSetName = "TextPath", Position = 3)]
        [Alias("Font")]
        [ArgumentCompleter(typeof(FontNameCompleter))]
        public string FontName { get; set; } = "Lucida Console";
        //Size of font, defaults to 10 point.
        [Parameter(ParameterSetName = "Default", Position = 4)]
        [Parameter(ParameterSetName = "TextPath", Position = 4)]
        [Alias("Size")]
        public int FontSize { get; set; } = 10;
        //Specifies the name of a file to print to so that PDF etc.don't require input. File will be deleted if it exists
        [Parameter(Position = 5)]
        [Alias("PrintFileName")]
        public string Destination { get; set; }
        //If specified opens the print file (ignored if Print file is not specified) 
        [Parameter()]
        public SwitchParameter OpenDestinationFile { get; set; }
        //If specified page numbers will be added at the top of the page.
        [Parameter()]
        public SwitchParameter NumberPages { get; set; }
        //By default printing is portrait, unless -Landscape is specified.
        [Parameter()]
        public SwitchParameter LandScape { get; set; }
        //Top Margin in units of 1/100th inch (if working in mm, divide by 0.254). Zero will be converted to minimum margin
        [Parameter()]
        public int TopMargin { get; set; } = -1;
        //Bottom Margin in units of 1/100th inch. Zero will be converted to minimum margin
        [Parameter()]
        public int BottomMargin { get; set; } = -1;
        //Left Margin in units of 1/100th inch. Zero will be converted to minimum margin
        [Parameter()]
        public int LeftMargin { get; set; } = -1;
        [Parameter()]
        //Right Margin in units of 1/100th inch. Zero will be converted to minimum margin
        public int RightMargin { get; set; } = -1;
        // could support resolution and monochrome... 
        //[Parameter()]
        //public SwitchParameter MonoChrome { get; set; }
        // Disable scaling of images when printing.
        [Parameter()]
        public SwitchParameter NoImageScale { get; set; }
        #endregion
        #region Declare other variables. 
        // When printing text we the lines to print, print a page full, delete those lines, print the next page full. Until done 
        private List<string> LinesToPrint = new List<string>();
        // When data is piped in we collect the objects in the process block; in the end block Out-String to renders them and the results go into LinesToPrint. 
        private List<PSObject> ThingsToPrint = new List<PSObject>();
        // When printing an image BitMap holds the image read from a file or piped in. 
        private System.Drawing.Bitmap Bitmap;
        //The object which does all the actual printing. Parameters are set in BEGIN, BitMaps printed in PROCESS, and Text printed in END
        private System.Drawing.Printing.PrintDocument PrintDocument = new System.Drawing.Printing.PrintDocument();
        //The font that we use for the printing. 
        private System.Drawing.Font PrintFont;
        //Width of the page - this is calculated when the the parameters are set in BEGIN, and used by Out-String in the END block
        private int widthInChars = 80;
        //Used to track  the number of pages printed (may add an option to put the page number on the top of the page. 
        private int currentPageNo = 1;
        #endregion
      
        private class PrinterNameCompleter : IArgumentCompleter
        {
            IEnumerable<CompletionResult> IArgumentCompleter.CompleteArgument(string commandName,
                                                                              string parameterName,
                                                                              string wordToComplete,
                                                                              CommandAst commandAst,
                                                                              IDictionary fakeBoundParameters)
            {
                return System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().ToArray().
                       Where(new WildcardPattern("*" + wordToComplete + "*", WildcardOptions.IgnoreCase).IsMatch).
                       Select(s => new CompletionResult("'" + s + "'"));
            }
        }

        private class FontNameCompleter : IArgumentCompleter
        {
            IEnumerable<CompletionResult> IArgumentCompleter.CompleteArgument(string commandName,
                                                                              string parameterName,
                                                                              string wordToComplete,
                                                                              CommandAst commandAst,
                                                                              IDictionary fakeBoundParameters)
            {
                List<CompletionResult> completionResults = new List<CompletionResult>();
                System.Drawing.Text.InstalledFontCollection installedFonts = new System.Drawing.Text.InstalledFontCollection();
                foreach (System.Drawing.FontFamily f in installedFonts.Families)
                {
                    if (f.Name.Contains(wordToComplete,StringComparison.CurrentCultureIgnoreCase)) { completionResults.Add(new CompletionResult("'" + f.Name + "'")); }
                }
                return completionResults;
            }
        }
        private void pd_PrintText(object sender, PrintPageEventArgs ev)
        {
            float fontHeight = PrintFont.GetHeight(ev.Graphics);
            float linesPerPage = (ev.MarginBounds.Height / fontHeight) - 1;
            float leftEdge = ev.MarginBounds.Left;
            float topEdge = ev.MarginBounds.Top;
            float yPos = 0;
            int linecount = 0;
            if (NumberPages)
            {
                string PageLabel = "Page -- " + currentPageNo.ToString();
                float left = ((float)PrintDocument.DefaultPageSettings.PaperSize.Width - ev.Graphics.MeasureString(PageLabel, PrintFont).Width)/2;
                ev.Graphics.DrawString(PageLabel, PrintFont, System.Drawing.Brushes.Black, left, PrintDocument.DefaultPageSettings.PrintableArea.Top);
                if (PrintDocument.DefaultPageSettings.PrintableArea.Top + (2 * fontHeight) > ev.MarginBounds.Top)
                {
                    topEdge = topEdge + (2 * fontHeight);
                    linesPerPage = linesPerPage - 2; 
                }
            }
            if (currentPageNo == 1)
            {
                WriteVerbose("Printing with margins: top=" + topEdge.ToString("0") + ", left=" + leftEdge.ToString("0") + ". " + System.Math.Truncate(linesPerPage).ToString() + " Lines per page");
            }
            WriteProgress(new ProgressRecord(1, ("Printing to " + PrintDocument.PrinterSettings.PrinterName), ("Printing text. Page " + currentPageNo + ", " + LinesToPrint.Count + " lines in the buffer")));
            // Print lines from 0..LinesPerPage -1 if we don't run out first
            while (linecount < linesPerPage && LinesToPrint.Count > linecount)
            {
                yPos = topEdge + (linecount * fontHeight);
                ev.Graphics.DrawString(LinesToPrint[linecount], PrintFont, System.Drawing.Brushes.Black, leftEdge, yPos);
                linecount++;
            }

            LinesToPrint.RemoveRange(0, linecount);
            // If more lines exist, print another page.
            if (LinesToPrint.Count > 0)
            {
                ev.HasMorePages = true;
                currentPageNo++;
            }
            else
            {
                ev.HasMorePages = false;
            }
        }

        private void pd_PrintGraphic(object sender, System.Drawing.Printing.PrintPageEventArgs ev)
        {
            WriteProgress(new ProgressRecord(1, ("Printing to " + PrintDocument.PrinterSettings.PrinterName), ("Printing image. Page " + currentPageNo)));

            #region Figure out how the image should be scaled
            float ratio = 1;
            System.Drawing.Size AdjustedImagesize;
            if (NoImageScale)
            {
                AdjustedImagesize = Bitmap.Size;
            }
            else
            {
                bool FitToWidth = (Bitmap.Size.Width > Bitmap.Size.Height);
                if ((Bitmap.Size.Width < ev.MarginBounds.Width) && (Bitmap.Size.Height < ev.MarginBounds.Height))
                {
                     
                    AdjustedImagesize = new System.Drawing.Size(Bitmap.Size.Width, Bitmap.Size.Height);
                }
                else
                {
                    if (FitToWidth) { ratio = (float)ev.MarginBounds.Width / (float)Bitmap.Size.Width; }
                    else { ratio = (float)ev.MarginBounds.Height / (float)Bitmap.Size.Height; }
                    AdjustedImagesize = new System.Drawing.Size(System.Convert.ToInt32(Bitmap.Size.Width * ratio), System.Convert.ToInt32(Bitmap.Size.Height * ratio));
                }
            }
            WriteVerbose("Original image size: " + Bitmap.Size.Width.ToString() + " X " + Bitmap.Size.Height.ToString() + "; bounds: " + ev.MarginBounds.Width.ToString() + " X " + ev.MarginBounds.Height.ToString() + "; ratio: " + ratio.ToString("0.000") + "; Adjusted size: " + AdjustedImagesize.Width.ToString() + " X " + AdjustedImagesize.Height.ToString());
            #endregion
            //scaling is done saying with a source an destination rectangle. We say here's the bit map, and its size, this where we want it fitted. Make it so.
            System.Drawing.Rectangle recDest = new System.Drawing.Rectangle(ev.MarginBounds.Location, AdjustedImagesize);
            System.Drawing.Rectangle recSrc = new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
            ev.Graphics.DrawImage(Bitmap, recDest, recSrc, System.Drawing.GraphicsUnit.Pixel);
            Bitmap.Dispose();
            currentPageNo++;
            ev.HasMorePages = false;
        }
               
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            #region Select printer, paper size and orientation 
            if (null != PrinterName)
            {
                PrintDocument.PrinterSettings.PrinterName = PrinterName;
            }
            string layoutMsg = "Printing to '" + PrintDocument.PrinterSettings.PrinterName +"'";
            if (null != Destination)
            {
                Destination = System.IO.Path.GetFullPath(Destination);
                if (System.IO.File.Exists(Destination))
                {
                    System.IO.File.Delete(Destination);
                }
                PrintDocument.PrinterSettings.PrintToFile = true;
                PrintDocument.PrinterSettings.PrintFileName = Destination;
                layoutMsg = layoutMsg + " (" + Destination + ")";
            }
            if (null != PaperSize)
            {
                bool FoundSize = false;
                foreach (System.Drawing.Printing.PaperSize ps in PrintDocument.PrinterSettings.PaperSizes)
                {
                    if (ps.Kind.ToString() == PaperSize)
                    {
                        FoundSize = true;
                        PrintDocument.DefaultPageSettings.PaperSize = ps;
                    }
                }
                if (!FoundSize)
                {
                    WriteWarning(PaperSize + " doesn't appear to be a valid paper size.");
                }
            }
            layoutMsg = layoutMsg + ". Paper is " + PrintDocument.DefaultPageSettings.PaperSize.Kind.ToString();
            if (LandScape)
            {
                PrintDocument.DefaultPageSettings.Landscape = true;
                WriteVerbose(layoutMsg + " landscape.");
            }
            else
            {
                PrintDocument.DefaultPageSettings.Landscape = false;
                WriteVerbose(layoutMsg + " portrait.");
            }
            #endregion
            #region Set page margins (if passed), checking minimum values
            //DefaultPageSettings includes hard margins but they are calculated from the known printable area and ints. Better to work off printable area, 
            // area has X,Y of top left corner, width and height.  Min top/left margins set by X & Y;
            // min bottom is  paperHeight - Y - printable height ; min right margin is paperWdith - x - printable width
            //  if margin passed > min we set that, between zero and Min we set the min value, below zero tells us nothing was passed. 
            
            if (TopMargin > PrintDocument.DefaultPageSettings.PrintableArea.Y)
            {
                PrintDocument.DefaultPageSettings.Margins.Top = TopMargin;
            }
            else if ((TopMargin >= 0))
            {
                PrintDocument.DefaultPageSettings.Margins.Top = System.Convert.ToInt32(PrintDocument.DefaultPageSettings.PrintableArea.Y);
            }
            if (PrintDocument.DefaultPageSettings.PrintableArea.Y + PrintDocument.DefaultPageSettings.PrintableArea.Height + BottomMargin > PrintDocument.DefaultPageSettings.PaperSize.Height)
            {
                PrintDocument.DefaultPageSettings.Margins.Bottom = BottomMargin;
            }
            else if (BottomMargin >= 0)
            {
                PrintDocument.DefaultPageSettings.Margins.Bottom = PrintDocument.DefaultPageSettings.PaperSize.Height - System.Convert.ToInt32(PrintDocument.DefaultPageSettings.PrintableArea.Y + PrintDocument.DefaultPageSettings.PrintableArea.Height);
            }
            if (LeftMargin > PrintDocument.DefaultPageSettings.PrintableArea.X)
            {
                PrintDocument.DefaultPageSettings.Margins.Left = LeftMargin;
            }
            else if (LeftMargin >= 0)
            {
                PrintDocument.DefaultPageSettings.Margins.Left = System.Convert.ToInt32(PrintDocument.DefaultPageSettings.PrintableArea.X);
            }
            if (PrintDocument.DefaultPageSettings.PrintableArea.X + PrintDocument.DefaultPageSettings.PrintableArea.Width + RightMargin > PrintDocument.DefaultPageSettings.PaperSize.Width)
            {
                PrintDocument.DefaultPageSettings.Margins.Right = RightMargin;
            }
            else if (RightMargin >= 0)
            {
                PrintDocument.DefaultPageSettings.Margins.Right = PrintDocument.DefaultPageSettings.PaperSize.Width - System.Convert.ToInt32(PrintDocument.DefaultPageSettings.PrintableArea.X + PrintDocument.DefaultPageSettings.PrintableArea.Width);
            }
            WriteVerbose("Set margins: top=" + PrintDocument.DefaultPageSettings.Margins.Top.ToString() + ", bottom=" + PrintDocument.DefaultPageSettings.Margins.Bottom.ToString() + ", left=" + PrintDocument.DefaultPageSettings.Margins.Left.ToString() + ", right=" + PrintDocument.DefaultPageSettings.Margins.Right.ToString());
            float WidthInHundreths = PrintDocument.DefaultPageSettings.PaperSize.Width - PrintDocument.DefaultPageSettings.Margins.Left - PrintDocument.DefaultPageSettings.Margins.Right;
            float HeightInHundreths = PrintDocument.DefaultPageSettings.PaperSize.Height - PrintDocument.DefaultPageSettings.Margins.Top - PrintDocument.DefaultPageSettings.Margins.Bottom;
            #endregion
            #region Decide print job name - use file name if there is one 
            if (null != ImagePath)
            {
                PrintDocument.DocumentName = ImagePath;
            }
            else if (null != Path)
            {
                PrintDocument.DocumentName = Path;
            }
            else
            {
                PrintDocument.DocumentName = "PowerShell Print Job";
            }
            #endregion
            #region Check user-specificed font exists, or fall back to lucida console. Determine chars per line. 
            bool fontfound = false;
            System.Drawing.Text.InstalledFontCollection installedFonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily f in installedFonts.Families)
            {
                if (f.Name == FontName) { fontfound = true; }
            }
            if (!fontfound)
            {
                WriteWarning("'" + FontName + "' does not seem to be a valid font. Switching to default");
                FontName = "Lucida Console";
            }
            PrintFont = new System.Drawing.Font(FontName, FontSize);

            //Page size is hundreths of an inch. Font is points, 120 points to inch, so page 1.2 is width in points. 
            widthInChars = (int)Math.Truncate(WidthInHundreths * 1.2 / PrintFont.Size);
            
            WriteVerbose("Font is " + PrintFont.Name + " " + PrintFont.Size + "-point. Print area is " + (HeightInHundreths / 100).ToString("0.00") + "inches tall X " + (WidthInHundreths / 100).ToString("0.00") + "inches wide = " + widthInChars.ToString("0") + "chars");            
            #endregion
        }
        protected override void ProcessRecord()
        {
            #region Load any text file specified by -path - assume it fits to lines. 
            if (null != Path)
            {
                Path = System.IO.Path.GetFullPath(Path);
                if (System.IO.File.Exists(Path))
                {
                    WriteVerbose("Reading from " + Path);
                    string[] FileContent = System.IO.File.ReadAllLines(Path);
                    foreach (string FileLine in FileContent)
                    {
                        LinesToPrint.Add(FileLine);
                    }
                }
                else
                {
                    WriteWarning("Can't open file");
                }
            }
            #endregion
            #region Collect data based in InputObject (i.e. piped) special treatment for a bitmap 
            else if(null != InputObject)
            {
                if (InputObject.BaseObject is System.Drawing.Bitmap)
                {
                    WriteVerbose("Input is an image");
                    Bitmap = InputObject.BaseObject as System.Drawing.Bitmap;
                }
                else
                {
                    ThingsToPrint.Add(InputObject);
                }
            }
            #endregion
            #region Load any image specified by -imagepath; print it, or any bitmap piped in.
            else if (null != ImagePath)
            {
                ImagePath = System.IO.Path.GetFullPath(ImagePath);
                if (System.IO.File.Exists(ImagePath))
                {
                    WriteVerbose("Reading from " + ImagePath);
                    Bitmap = new System.Drawing.Bitmap(ImagePath);
                }
                else { WriteWarning("Can't open file"); }
            }
            if (null != Bitmap)
            {
                //Most of the work happens in the Print-a-page event-handler ...
                PrintDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.pd_PrintGraphic);
                PrintDocument.Print(); 
            }
            #endregion
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
            #region Print text
            if ((ThingsToPrint.Count > 0) )   
            {
                WriteProgress(new ProgressRecord(1, ("Printing to " + PrintDocument.PrinterSettings.PrinterName), "Renderding text"));
                // I think there must be a better way ( using Microsoft.PowerShell.Commands.OutStringCommand ? ) 
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Out-String");
                ps.AddParameter("InputObject", ThingsToPrint);
                ps.AddParameter("Width", widthInChars);
                ps.AddParameter("Stream");
                foreach (PSObject O in ps.Invoke()) {
                    LinesToPrint.Add(O.ToString());
                }
                WriteVerbose(LinesToPrint.Count.ToString() + " Lines to print.");
                //All work gets done by the event handler for "print page" events
                PrintDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.pd_PrintText);
                PrintDocument.Print();
            }
            #endregion

            if (PrintDocument.PrinterSettings.PrintToFile && OpenDestinationFile)
            {
                if (System.IO.File.Exists(PrintDocument.PrinterSettings.PrintFileName))
                    new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo(PrintDocument.PrinterSettings.PrintFileName)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
            }
            PrintDocument.Dispose();
        }
        protected override void StopProcessing()
        {
            PrintDocument.Dispose();
            base.StopProcessing();
        }

    }
}