<?xml version="1.0" encoding="utf-8"?>

<nlog xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">

  <variable name="logDirectory"
          value="F:/Logs"
          xdt:Transform="SetAttributes"
          xdt:Locator="Match(name)" />

  <variable name="archiveDirectory"
          value="F:/Logs/Archive"
          xdt:Transform="SetAttributes"
          xdt:Locator="Match(name)" />

</nlog>
