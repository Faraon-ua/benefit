@echo off
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe /t:Rebuild %1 /p:Configuration=%3;DeployOnBuild=true;DeployTerget=Package;_PackageTempDir=%2 /p:AutoParameterizationWebConfigConnectionStrings=false /fileLogger
GOTO END
:NOARGUMENTS 
echo Usage
echo deploy.bat Path_to_project deploy_location configuration(Debug,Demo,Dev,QA,Staging,Production,SFkidsProd,SFkidsStag,SFkidsDev)
:END
pause