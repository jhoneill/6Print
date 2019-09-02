using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OutPrinterCommand
{
    #region Argument Completion / Validation classes     
    public class PrinterNameValidator : IValidateSetValuesGenerator
    {
        public string[] GetValidValues()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
        }
    }
    class PrinterNameCompleter : IArgumentCompleter
    {
        IEnumerable<CompletionResult> IArgumentCompleter.CompleteArgument(string commandName,
                                                                          string parameterName,
                                                                          string wordToComplete,
                                                                          CommandAst commandAst,
                                                                          IDictionary fakeBoundParameters)
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToArray().
                   Where(new WildcardPattern("*" + wordToComplete + "*", WildcardOptions.IgnoreCase).IsMatch).
                   Select(s => new CompletionResult("'" + s + "'"));
        }
    }
    class PaperSizeCompleter : IArgumentCompleter
    {
        IEnumerable<CompletionResult> IArgumentCompleter.CompleteArgument(string commandName,
                                                                          string parameterName,
                                                                          string wordToComplete,
                                                                          CommandAst commandAst,
                                                                          IDictionary fakeBoundParameters)
        {
            List<CompletionResult> completionResults = new List<CompletionResult>();
            PrintDocument pd = new PrintDocument();
            foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
            {
                if (ps.Kind.ToString().Contains(wordToComplete, StringComparison.CurrentCultureIgnoreCase))
                {
                    completionResults.Add(new CompletionResult("'" + ps.Kind.ToString() + "'"));
                }
            }
            pd.Dispose();
            return completionResults;
        }
    }
    class FontNameCompleter : IArgumentCompleter
    {
        IEnumerable<CompletionResult> IArgumentCompleter.CompleteArgument(string commandName,
                                                                          string parameterName,
                                                                          string wordToComplete,
                                                                          CommandAst commandAst,
                                                                          IDictionary fakeBoundParameters)
        {
            List<CompletionResult> completionResults = new List<CompletionResult>();
            System.Drawing.Text.InstalledFontCollection installedFonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (FontFamily f in installedFonts.Families)
            {
                if (f.Name.Contains(wordToComplete, StringComparison.CurrentCultureIgnoreCase)) { completionResults.Add(new CompletionResult("'" + f.Name + "'")); }
            }
            installedFonts.Dispose();
            return completionResults;
        }
    }
    #endregion
    //Class to ensure we can set the default printer. Otherwise printing changes the default printer.
    class InteropPrinters
    {
            [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool SetDefaultPrinter(string name);
    }
        
    //Replacement for the the Out-Printer command which was in Windows PowerShell.
    //PSCmdlet supports GetUnresolvedProviderPathFromPSPath which we want. Cmdlet does not so we'll use that throughout
    [Cmdlet(VerbsData.Out, "Printer", DefaultParameterSetName = "Default")]
    [Alias("lp")]
    public class OutPrinterCommand : PSCmdlet
    {
        #region Param() block.
        //Name of printer - can specify either -Printer or -Name. If not specified, the default printer will be used
        [Parameter(Position = 1), ValidateSet(typeof(PrinterNameValidator), ErrorMessage = "'{0}' is not a valid printer on this computer.")] // ArgumentCompleter(typeof(PrinterNameCompleter))]
        [Alias("Name")]
        public string PrinterName { get; set; }       
        //Specifies the content to be sent to the printer. This can be objects to print, or the target for piped objects.
        [Parameter(ValueFromPipeline = true, ParameterSetName = "Default", Position = 1)]
        public PSObject InputObject { get; set; }
        //Path of text file to be printed.
        [Parameter(ParameterSetName = "TextPath", Position =1 , Mandatory = true)]
        [Alias("FileName")]
        public string Path { get; set; }
        //Path to a BMP, GIF, JPEG, PNG or TIFF file to be printed.
        [Parameter(ParameterSetName = "ImagePath", Position = 1, Mandatory = true)]
        public string ImagePath { get; set; }
        //Name of a paper-size on the selected printer (e.g A4, Letter)
        [Parameter(Position = 2), ArgumentCompleter(typeof(PaperSizeCompleter))]
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
        [Alias("Show")]
        public SwitchParameter OpenDestinationFile { get; set; }
        //If specified page a header will be added at the top of text pages. Supports  &[Page] &[Date] &[Time] &[Path] &[Line] &[HistoryId] 
        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "TextPath")]
        public String Header { get; set; }
        //If specified page a footer will be added at the bottom of text pages. Supports  &[Page] &[Date] &[Time] &[Path] &[Line] &[HistoryId] 
        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "TextPath")]
        public String Footer { get; set; }
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
        //Disable wrapping text
        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "TextPath")]
        [Alias("NoWrapText")]
        public SwitchParameter NoTextWrapping { get; set; }
        // Disable scaling of images when printing.
        [Parameter(ParameterSetName = "ImagePath")]
        public SwitchParameter NoImageScale { get; set; }
        #endregion
        #region Other properties of the class 
        // When printing text we the lines to print, print a page full, delete those lines, print the next page full. Until done 
        private List<string> LinesToPrint = new List<string>();
        // When data is piped in we collect the objects in the process block; in the end block Out-String to renders them and the results go into LinesToPrint. 
        private List<PSObject> ThingsToPrint = new List<PSObject>();
        // When printing an image BitMap holds the image read from a file or piped in. 
        private Bitmap Bitmap;
        //The object which does all the actual printing. Parameters are set in BEGIN, BitMaps printed in PROCESS, and Text printed in END
        private PrintDocument PrintDocument = new PrintDocument();
        //Store the printer name so it can be restored at the end.
        private string OriginalPrinterName;
        //The font that we use for the printing. 
        private Font PrintFont;
        //Width of the page - this is calculated when the the parameters are set in BEGIN, and used by Out-String in the END block
        private int WidthInChars = 80;
        //Unless wrapping is disabled, holds regex to wrap text at that width. 
        private Regex WrappingRegEx;
        //Used to preserve Multiple line breaks in the text. Matches on the point preceed by LF and followed by CR or LF. 
        private Regex CrLfRegEx = new Regex("(?<=\\n)(?=\\r|\\n)");
        
        //Used to track  the number of pages printed (may add an option to put the page number on the top of the page. 
        private int CurrentPageNo = 1;
        #endregion 
        #region Handlers for the Print Document's PrintPage event which do the actual printing of text or graphic
        private void pd_PrintText(object sender, PrintPageEventArgs ev)
        {
            float fontHeight = PrintFont.GetHeight(ev.Graphics);
            float linesPerPage = ev.MarginBounds.Height / fontHeight - 1;
            float leftEdge = ev.MarginBounds.Left;
            float topEdge = ev.MarginBounds.Top;
            float yPos = 0;
            int linecount = 0;
            WriteProgress(new ProgressRecord(1, string.Format("Printing to {0}",PrintDocument.PrinterSettings.PrinterName), string.Format("Printing text. Page {0}, {1} lines in the buffer.", CurrentPageNo ,LinesToPrint.Count))) ;
            if (CurrentPageNo == 1)
            {
                WriteVerbose(string.Format("Printing with margins: top={0:N0} left={1:N0}. {2} lines per page.", topEdge, leftEdge, Math.Truncate(linesPerPage)));
            }
            #region add header/footer if required
            if (! string.IsNullOrEmpty(Header))
            {
                //put the page number in. Set Left to center it, and as high as printable, but top at least 1 line from the paper edge. 
                //If there isn't 1 clear line between the bottom of the header and the printing area, take 2 lines off the top of the print area
                string PageHeader = Header.Replace("&[Page]",CurrentPageNo.ToString(),StringComparison.CurrentCultureIgnoreCase);
                float left = (PrintDocument.DefaultPageSettings.PaperSize.Width - ev.Graphics.MeasureString(PageHeader, PrintFont).Width) / 2;
                float top = PrintDocument.DefaultPageSettings.PrintableArea.Top;
                if (top < fontHeight) {
                    top = fontHeight;
                }
                ev.Graphics.DrawString(PageHeader, PrintFont, Brushes.Black, left, top);
                if (top + 2 * fontHeight > topEdge)
                {
                    topEdge = topEdge + 2 * fontHeight;
                    linesPerPage = linesPerPage - 2;
                }
            }
            if (! string.IsNullOrEmpty(Footer))
            { 
                //put the page number in. Set Left to center it, and as low as printable, but top at least 1 line from the paper edge. 
                //If there isn't 1 clear line between the top of the header and the printing area, take 2 lines off the top of the print area
                string PageFooter = Footer.Replace("&[Page]",CurrentPageNo.ToString(),StringComparison.CurrentCultureIgnoreCase );
                float left = (PrintDocument.DefaultPageSettings.PaperSize.Width - ev.Graphics.MeasureString(PageFooter, PrintFont).Width) / 2;
                float bottom = PrintDocument.DefaultPageSettings.PrintableArea.Bottom;
                if (bottom > (PrintDocument.DefaultPageSettings.PrintableArea.Bottom -fontHeight) )
                {
                    bottom = (PrintDocument.DefaultPageSettings.PrintableArea.Bottom -fontHeight);
                }
                ev.Graphics.DrawString(PageFooter, PrintFont, Brushes.Black, left, (bottom -fontHeight));
                if (bottom - 2 * fontHeight < ev.MarginBounds.Bottom)
                {
                    linesPerPage = linesPerPage - 2;
                }
            }
            #endregion

            // Print lines from 0..LinesPerPage -1 if we don't run out first; then remove those lines from buffer
            while (linecount < linesPerPage && LinesToPrint.Count > linecount)
            {
                yPos = topEdge + linecount * fontHeight;
                ev.Graphics.DrawString(LinesToPrint[linecount], PrintFont, Brushes.Black, leftEdge, yPos);
                linecount++;
            }
            LinesToPrint.RemoveRange(0, linecount);
            
            // If more lines exist, print another page.
            if (LinesToPrint.Count > 0)
            {
                ev.HasMorePages = true;
                CurrentPageNo++;
            }
            else
            {
                ev.HasMorePages = false;
            }
        }
        private void pd_PrintGraphic(object sender, PrintPageEventArgs ev)
        {
            WriteProgress(new ProgressRecord(1, string.Format("Printing to {0}",PrintDocument.PrinterSettings.PrinterName), string.Format("Printing image Page {0}", CurrentPageNo)));

            #region Figure out how the image should be scaled
            float ratio = 1;
            Size AdjustedImagesize;
            if (NoImageScale)
            {
                AdjustedImagesize = Bitmap.Size;
            }
            else
            {
                //are we fitting by width or height ? 
                bool FitToWidth = Bitmap.Size.Width > Bitmap.Size.Height;
                //if the bitmap is smaller than the printing are, just print it. If it is bigger work out the ratio to scale by
                if (Bitmap.Size.Width < ev.MarginBounds.Width && Bitmap.Size.Height < ev.MarginBounds.Height)
                {
                    AdjustedImagesize = new Size(Bitmap.Size.Width, Bitmap.Size.Height);
                }
                else
                {
                    // Scale by the tighter constraint of fitting to width or to height. 
                    float hRatio = (ev.MarginBounds.Width / (float)Bitmap.Size.Width );
                    float vRatio = (ev.MarginBounds.Height / (float)Bitmap.Size.Height);
                    ratio = (hRatio < vRatio) ? hRatio : vRatio;  
                    AdjustedImagesize = new Size(Convert.ToInt32(Bitmap.Size.Width * ratio), Convert.ToInt32(Bitmap.Size.Height * ratio));
                }
            }
            WriteVerbose(string.Format("Original image size: {0} X {1}; bounds: {2} X {3}; ratio: {4:N3}; Adjusted size: {5} X {6}.",new object [] {Bitmap.Size.Width, Bitmap.Size.Height, ev.MarginBounds.Width, ev.MarginBounds.Height,  ratio,  AdjustedImagesize.Width,  AdjustedImagesize.Height}));
            #endregion
            //scaling is done saying with a source an destination rectangle. We say here's the bit map, and its size, this where we want it fitted. Make it so.
            Rectangle recDest = new Rectangle(ev.MarginBounds.Location, AdjustedImagesize);
            Rectangle recSrc = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
            ev.Graphics.DrawImage(Bitmap, recDest, recSrc, GraphicsUnit.Pixel);
            Bitmap.Dispose();
            CurrentPageNo++;
            ev.HasMorePages = false;
        }
        #endregion
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            OriginalPrinterName = PrintDocument.PrinterSettings.PrinterName;
            #region Select printer (and set any output file), paper size and orientation 
            // The printer name paramter has been validated, if a bad one got in it would cause an error here (don't want to continue with the wrong printer so that's OK).
            // Paper size and font parameters have not been validated yet - check here and use defaults if invalid ones. 
            // Could allow a Printer object instead of the name as a string (ditto paper size)
            //but these aren't piped in - it is not asking too much to say "pass the .Name property not the whole object" 
            if (null != PrinterName)
            {
                PrintDocument.PrinterSettings.PrinterName = PrinterName;
            }           
            string layoutMsg = string.Format("Printing to '{0}'",PrintDocument.PrinterSettings.PrinterName);
            if (null != Destination)
            {
                Destination = GetUnresolvedProviderPathFromPSPath(Destination);
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
                var psize = from ps in PrintDocument.PrinterSettings.PaperSizes.Cast<PaperSize>().ToArray()
                            where string.Equals(ps.Kind.ToString(), PaperSize, StringComparison.CurrentCultureIgnoreCase)
                            select ps;
                if (psize.Count() == 0)
                {
                    WriteWarning(string.Format("{0} doesn't appear to be a valid paper size; will use the default:{1}." , PaperSize, PrintDocument.DefaultPageSettings.PaperSize.Kind ));
                }
                else
                {
                    PrintDocument.DefaultPageSettings.PaperSize = psize.First();    
                }
            }
            layoutMsg = layoutMsg + string.Format(". Paper is {0}", PrintDocument.DefaultPageSettings.PaperSize.Kind);
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
            #region Set page margins (if any were passed), checking minimum values
            //DefaultPageSettings includes hard margins but they are calculated from the known printable area and ints. Better to work off printable area, 
            // area has X,Y of top left corner, width and height.  Min top/left margins set by X & Y;
            // min bottom is  paperHeight - Y - printable height ; min right margin is paperWdith - x - printable width
            //  if margin passed > min we set that, between zero and Min we set the min value, below zero tells us nothing was passed. 
            PageSettings DefPS =  PrintDocument.DefaultPageSettings ;
            if (TopMargin > DefPS.PrintableArea.Y)
            {
                PrintDocument.DefaultPageSettings.Margins.Top = TopMargin;
            }
            else if (TopMargin >= 0)
            {
                PrintDocument.DefaultPageSettings.Margins.Top = Convert.ToInt32(DefPS.PrintableArea.Y);
            }
            if (DefPS.PrintableArea.Y + DefPS.PrintableArea.Height + BottomMargin > DefPS.PaperSize.Height)
            {
                PrintDocument.DefaultPageSettings.Margins.Bottom = BottomMargin;
            }
            else if (BottomMargin >= 0)
            {
                PrintDocument.DefaultPageSettings.Margins.Bottom = DefPS.PaperSize.Height - Convert.ToInt32(DefPS.PrintableArea.Y + DefPS.PrintableArea.Height);
            }
            if (LeftMargin > DefPS.PrintableArea.X)
            {
                PrintDocument.DefaultPageSettings.Margins.Left = LeftMargin;
            }
            else if (LeftMargin >= 0)
            {
                PrintDocument.DefaultPageSettings.Margins.Left = Convert.ToInt32(DefPS.PrintableArea.X);
            }
            if (DefPS.PrintableArea.X + DefPS.PrintableArea.Width + RightMargin > DefPS.PaperSize.Width)
            {
                PrintDocument.DefaultPageSettings.Margins.Right = RightMargin;
            }
            else if (RightMargin >= 0)
            {
                PrintDocument.DefaultPageSettings.Margins.Right = DefPS.PaperSize.Width - Convert.ToInt32(DefPS.PrintableArea.X + DefPS.PrintableArea.Width);
            }
            WriteVerbose(string.Format("Set margins to: top={0}, bottom={1}, left={2}, right={3}.", new object [] {DefPS.Margins.Top, DefPS.Margins.Bottom,  DefPS.Margins.Left, DefPS.Margins.Right}));
            
            float WidthInHundreths = DefPS.PaperSize.Width - DefPS.Margins.Left - DefPS.Margins.Right;
            float HeightInHundreths = DefPS.PaperSize.Height - DefPS.Margins.Top - DefPS.Margins.Bottom;
            #endregion
            #region Decide print job name - use file name if there is one and construct header/footers
            if (null != ImagePath)
            {
                PrintDocument.DocumentName = ImagePath;
            }
            else if (null != Path)
            {
                PrintDocument.DocumentName = Path;
                if (! String.IsNullOrEmpty(Header)) {
                    Header = Header.Replace("&[Path]",Path,StringComparison.CurrentCultureIgnoreCase);
                }
                if (! String.IsNullOrEmpty(Footer)) {
                    Footer = Footer.Replace("&[Path]",Path,StringComparison.CurrentCultureIgnoreCase);
                }
            }
            else
            {
                PrintDocument.DocumentName = "PowerShell Print Job";
            }
            Regex reg = new Regex("\\s*\\|\\s*(lp|Out-Printer).*$")  ;       
            if (! String.IsNullOrEmpty(Header)) {
                    Header = Header.Replace("&[Date]",System.DateTime.Now.ToString("d"),StringComparison.CurrentCultureIgnoreCase);
                    Header = Header.Replace("&[Time]",System.DateTime.Now.ToString("t"),StringComparison.CurrentCultureIgnoreCase);
                    Header = Header.Replace("&[Line]",reg.Replace(MyInvocation.Line,""),StringComparison.CurrentCultureIgnoreCase);
                    Header = Header.Replace("&[HistoryId]",MyInvocation.HistoryId.ToString(),StringComparison.CurrentCultureIgnoreCase);
            }
            if (! String.IsNullOrEmpty(Footer)) {
                    Footer = Footer.Replace("&[Date]",System.DateTime.Now.ToString("d"),StringComparison.CurrentCultureIgnoreCase);
                    Footer = Footer.Replace("&[Time]",System.DateTime.Now.ToString("t"),StringComparison.CurrentCultureIgnoreCase);
                    Footer = Footer.Replace("&[Line]",reg.Replace(MyInvocation.Line,""),StringComparison.CurrentCultureIgnoreCase);
                    Footer = Footer.Replace("&[HistoryId]",MyInvocation.HistoryId.ToString(),StringComparison.CurrentCultureIgnoreCase);
            }
            #endregion
            #region Check user-specificed font exists, or fall back to lucida console. Determine chars per line. 
            System.Drawing.Text.InstalledFontCollection installedFonts = new System.Drawing.Text.InstalledFontCollection();
            var fontq = from font in installedFonts.Families where font.Name == FontName select font;
            if (fontq.Count() == 0)
            {
                WriteWarning(string.Format("'{0}' does not seem to be a valid font. Switching to default.",FontName));
                FontName = "Lucida Console";
            }
            installedFonts.Dispose();
            PrintFont = new Font(FontName, FontSize);

            //Page size is hundreths of an inch. Font is in points @ 120 points : 1 inch, so page width in points is 1.2 x width in Hundredths. 
            WidthInChars = (int)Math.Truncate(WidthInHundreths * 1.2 / PrintFont.Size);
            //We won't text wrap if the regex is left empty. 
            //Regex will treat end of line as $ and match either on a line of 1 to Width, or that number of chars -1  followed by space(s) or punctuation
            if (! NoTextWrapping) {
                string r = "(.{1,XXX}$)|".Replace("XXX",WidthInChars.ToString());
                r = r + "(.{1,XXX}[\\s:;,.-?!…]+)(?=\\w+)".Replace("XXX",(WidthInChars-1).ToString()) ; 
                WrappingRegEx = new Regex(r,RegexOptions.Multiline ) ;
            }
            WriteVerbose(string.Format("Any text will be printed in {0} {1}-point. Print area is {2:N2} inches tall X {3:N2} inches wide = {4:N0} characters.",new object[] {PrintFont.Name , PrintFont.Size , (HeightInHundreths / 100), (WidthInHundreths / 100), WidthInChars}));
            #endregion
        }
        protected override void ProcessRecord()
        {
            #region Load any text file specified by -path split it into lines 
            if (null != Path)
            {
                Path = GetUnresolvedProviderPathFromPSPath(Path);
                if (System.IO.File.Exists(Path))
                {
                    WriteVerbose(string.Format("Reading from {0}.",Path));
                    string text = System.IO.File.ReadAllText(Path) ;
                    //Convert any blank lines in the source text to spaces to stop split discarding them as empty   
                    text = CrLfRegEx.Replace(text," ");
                    //If we are wrapping text use a predefined regex to make a long string into a multi-line string.
                    if (null != WrappingRegEx) {
                        text =  WrappingRegEx.Replace(text,"$1$2\n");
                    }
                    //Split a multi-line string of n lines into n strings and add them individually;
                    //there won't be other objects so add them to "lines to print" and skip 
                    foreach (string s in text.Split(new string[] {"\n","\r\n"},System.StringSplitOptions.RemoveEmptyEntries)) {
                        LinesToPrint.Add(s);
                    }
                }
                else
                {
                    WriteWarning(string.Format("Cannot find file '{0}.", Path));
                }
            }
            #endregion
            #region Collect data based in InputObject (i.e. piped).3 cases: "bitmap", "String" & "Object for Out-String to process"
            else if (null != InputObject)
            {
                if (InputObject.BaseObject is Bitmap)
                {
                    WriteVerbose("Input is an image");
                    Bitmap = InputObject.BaseObject as Bitmap;
                }
                else if (InputObject.BaseObject is string) {
                    //Convert any blank lines in the source text to spaces to stop split discarding them as empty   
                    //and if we are wrapping text, use a predefined regex to make a long string into a multi-line string.
                    string text = "";
                    if (null != WrappingRegEx) {
                        text =  CrLfRegEx.Replace(InputObject.BaseObject.ToString()," ");
                        text =  WrappingRegEx.Replace(text.ToString(),"$1$2\n");
                    }
                    else
                    {
                        text = CrLfRegEx.Replace(InputObject.BaseObject.ToString()," ");
                    }
                    //Split a multi-line string of n lines into n strings and add them individually; 
                    //We may have a mix of objects so add them to "things to print" so the objects get formatted by out-string.
                    foreach (string s in text.Split(new string[] {"\n","\r\n"},System.StringSplitOptions.RemoveEmptyEntries)) {
                        ThingsToPrint.Add(s);
                    }
                }
                else
                {
                    ThingsToPrint.Add(InputObject);
                }
            }
            #endregion
            #region Load any image specified by -imagepath; print it.
            else if (null != ImagePath)
            {
                ImagePath = GetUnresolvedProviderPathFromPSPath(ImagePath);
                if (System.IO.File.Exists(ImagePath))
                {
                    WriteVerbose(string.Format("Reading from {0}.",ImagePath));
                    Bitmap = new Bitmap(ImagePath);
                }
                else { WriteWarning("Can't open file"); }
            }
            if (null != Bitmap)
            {
                //Most of the work happens in the Print-a-page event-handler ...
                PrintDocument.PrintPage += new PrintPageEventHandler(pd_PrintGraphic);
                PrintDocument.Print();
            }
            #endregion
        }
        protected override void EndProcessing()
        {
            #region Print text
            if (ThingsToPrint.Count > 0)
            {
                WriteProgress(new ProgressRecord(1, string.Format("Printing to {0}",PrintDocument.PrinterSettings.PrinterName), "Renderding text"));
                // I think there must be a better way ( using Microsoft.PowerShell.Commands.OutStringCommand ? ) 
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Out-String");
                ps.AddParameter("InputObject", ThingsToPrint);
                ps.AddParameter("Width", WidthInChars);
                ps.AddParameter("Stream");
                foreach (PSObject O in ps.Invoke())
                {
                    LinesToPrint.Add(O.ToString());
                }
                ps.Dispose();
            }
            if (LinesToPrint.Count > 0)
            {
                WriteVerbose(LinesToPrint.Count.ToString() + " Lines to print.");
                //All work gets done by the event handler for "print page" events
                PrintDocument.PrintPage += new PrintPageEventHandler(pd_PrintText);
                PrintDocument.Print();
            }
            #endregion
            #region Open the print file, if there is one and we were told to
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
            #endregion 
            //Restore printer settings 
            InteropPrinters.SetDefaultPrinter(OriginalPrinterName);
            PrintDocument.Dispose();
            base.EndProcessing();
        }
        protected override void StopProcessing()
        {
            //Restore printer settings 
            InteropPrinters.SetDefaultPrinter(OriginalPrinterName);
            PrintDocument.Dispose();
            base.StopProcessing();
        }
    }
    
    //Powershell includes CIM commands for getting printers but not for finding or changing the default one. 
    //Easy to get the name and/or settings but we want a CIM object to be able to pass it into the other printer commands.
    //For setting we need a dll import...  
    [Cmdlet(VerbsCommon.Get, "DefaultPrinter"), OutputType("Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_Printer")]
    public class GetDefaultPrinterCommand : PSCmdlet
    {
        //no Parameters and only has one property. 
        private readonly PrintDocument PrintDocument = new PrintDocument();
        //Overrides for the begin,process,end and Stop! pscmdlet methods
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }
        protected override void EndProcessing()
        {
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Get-CimInstance");
            ps.AddParameter("Namespace", "Root/StandardCimv2");
            ps.AddParameter("Query", "Select * from MSFT_Printer Where Name = '" + PrintDocument.PrinterSettings.PrinterName + "'");
            foreach (PSObject O_O in ps.Invoke())
            {
                WriteObject(O_O);
            }
            ps.Dispose();
            PrintDocument.Dispose();
            base.EndProcessing();
        }
        protected override void StopProcessing()
        {
            base.StopProcessing();
        }
    }
    
    [Cmdlet(VerbsCommon.Set, "DefaultPrinter" , SupportsShouldProcess = true), OutputType("Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_Printer")]
    public class SetDefaultPrinterCommand : PSCmdlet
    {
        #region Param() block.
        [Parameter(Position = 0, ValueFromPipeline= true, Mandatory=true),  ArgumentCompleter(typeof(PrinterNameCompleter))]
        [Alias("Name")]
        public PSObject Printer { get; set; }

        [Parameter()]
        [Alias("Show")]
        public SwitchParameter Passthru { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }
        protected override void EndProcessing()
        {
            bool result = false;
            string PrinterName = "" ;
            if (Printer == null)
            {
                return ;
            }
            if (Printer.BaseObject is string)
            {
                PrinterName =  Printer.BaseObject.ToString();
            }
            else if (Printer.Properties["Name"] != null)
            {
                PrinterName =  Printer.Properties["Name"].Value.ToString();
            }
            else if (Printer.Properties["PrinterName"] != null)
            {
                PrinterName = Printer.Properties["PrinterName"].Value.ToString() ;
            }
            if (PrinterName != string.Empty  && ShouldProcess(PrinterName,"Set as default printer") ) 
            {
                result = InteropPrinters.SetDefaultPrinter(PrinterName);
                if (!result) 
                { 
                    WriteWarning(string.Format("Could not set the printer to '{0}'." ,PrinterName) ); 
                    return ;
                }
            }
            else {return;}
            if (Passthru)
            {
                PrintDocument pd = new PrintDocument();
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Get-CimInstance");
                ps.AddParameter("Namespace", "Root/StandardCimv2");
                ps.AddParameter("Query", "Select * from MSFT_Printer Where Name = '" + pd.PrinterSettings.PrinterName + "'");
                foreach (PSObject O_O in ps.Invoke())
                {
                    WriteObject(O_O);
                }
                ps.Dispose();
                pd.Dispose();
            }
            base.EndProcessing();
        }
        protected override void StopProcessing()
        {
            base.StopProcessing();
        }
    }
}
