SET INSTPATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Team Tools\Performance Tools\
SET PERFPATH=C:\Program Files (x86)\Microsoft Visual Studio\Shared\Common\VSPerfCollectionTools\vs2019\
SET TESTPATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\Extensions\TestPlatform\

"%INSTPATH%VsInstr.exe" /coverage .\UtilTests\bin\debug\Util.dll
"%INSTPATH%VsInstr.exe" /coverage .\UtilTests\bin\debug\RSS.dll

"%INSTPATH%VsInstr.exe" /coverage .\RSSTests\bin\debug\RSS.dll

"%INSTPATH%VsInstr.exe" /coverage .\ServiceTests\bin\debug\Util.dll
"%INSTPATH%VsInstr.exe" /coverage .\ServiceTests\bin\debug\RSS.dll
"%INSTPATH%VsInstr.exe" /coverage .\ServiceTests\bin\debug\RSSWCFSvc.dll

"%PERFPATH%VSPerfCmd.exe" /start:coverage /output:utiltests.coverage /cs /user:"Everyone"
"%TESTPATH%vstest.console.exe" .\UtilTests\bin\debug\UtilTests.dll
"%PERFPATH%VSPerfCmd.exe" /shutdown

"%PERFPATH%VSPerfCmd.exe" /start:coverage /output:rsstests.coverage /cs /user:"Everyone"
"%TESTPATH%vstest.console.exe" .\RSSTests\bin\debug\RSSTests.dll
"%PERFPATH%VSPerfCmd.exe" /shutdown

"%PERFPATH%VSPerfCmd.exe" /start:coverage /output:servicetests.coverage /cs /user:"Everyone"
"%TESTPATH%vstest.console.exe" .\ServiceTests\bin\debug\RSSWCFSvcTests.dll
"%PERFPATH%VSPerfCmd.exe" /shutdown
