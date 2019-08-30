---
RFC: RFC<four digit unique incrementing number assigned by Committee, this shall be left blank by the author>
Author: <First Last>
Status: <Draft | Experimental | Accepted | Rejected | Withdrawn | Final>
SupercededBy: <link to another RFC>
Version: <Major>.<Minor>
Area: <Area within the PowerShell language>
Comments Due: <Date for submitting comments to current draft (minimum 1 month)>
Plan to implement: <Yes | No>
---

# Title

Support printing in PowerShell core by providing an Out-Printer command, similar to Windows PowerShell, .

## Motivation

    As a user of PowerShell,
    I can make a hard copy of data or scripts ,
    so that I have durable copy which is accessible away from my computer.

    As a user of PowerShell or a developer,
    I can output using "Print to file" drivers 
    to give the effect of ConvertTo-PDF, or put information into a One Note notebook.

    As former user of Windows PowerShell 
    I can continue to use the Out-Printer command  
    giving me continuity of experience

## User Experience

Users pipe the output of a command into Out-Printer, or run Out-Printer with a file name.
Their content will be sent to the default printer in a non-proportionally spaced font. 
Using an alias, command | lp  will print the output of command.
Command line switches allow them to change the selected printer, paper size, font, and margins. 
For output-to-file printer drivers the name of the file can be supplied on the command line to
avoid the need for user input. 

```powershell
Out-Printer   -ImagePath .\lewis.jpg  -LandScape     
```
Prints an image on the default printer using the default paper size and margins, in landscape orientation 

```powershell
dir | lp  -Dest ~\Desktop\test3.pdf -Font 'Comic Sans MS' -Size 8 -top 0 -bottom 0 -left 0 -right 0 -Num -Open    
```
Assumes the default printer is print to PDF, and outputs a directory to a PDF using custom font and margins, 
opens the PDF file after creation.


## Specification
The Windows PowerShell version of Out-Printer only supported parameters for its input and the printer name.
The new command should support the same parameter names (name for the printer and inputObject for the data to be printed).
The new command should not change the default printer unless specifically told to. (The Printer management CIM module 
does not support discovering or changing the default printer and commands could be provided to do this.) 
The new command should support changing paper size, margins and font. 
By sending non-string objects through Out-String, or similar, objects such as files or services can be rendered on 
the page as a they would be on screen. 
The command should support the printing of graphics as well as text, if a bitmap is piped it should be printed, 
and if a path to a support type of image file is provided it should be read and printed. 

## Alternate Proposals and Considerations
TBD