
[scriptblock]$sb={
    param($CommandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
    [System.Drawing.Printing.PrinterSettings]::InstalledPrinters.Where({$_ -like "*$wordToComplete*"}).foreach({
        [System.Management.Automation.CompletionResult]::new("'$_'") })
}

foreach ($cn in @('Add-PrinterPort', 'Get-PrintConfiguration', 'Get-Printer', 'Get-PrinterProperty', 'Get-PrintJob',
  'Remove-Printer', 'Remove-PrintJob', 'Rename-Printer', 'Restart-PrintJob', 'Resume-PrintJob',
  'Set-PrintConfiguration', 'Set-Printer', 'Set-PrinterProperty', 'Suspend-PrintJob')) {
      Register-ArgumentCompleter -CommandName $cn -ParameterName "PrinterName" -ScriptBlock $sb }
