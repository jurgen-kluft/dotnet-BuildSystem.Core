SET PLATFORM=PC

SET CURRENT_PATH=%CD%
cd ..
SET DATA_PATH=%CD%
cd ..\Bin\Debug\BigfileViewer
SET TOOL_PATH=%CD%

DataBuildSystem.BigfileViewer.exe -name MJ -platform %PLATFORM% -territory Europe -config Config.%PLATFORM%.cs -srcpath "%DATA_PATH%" -dstpath "%DATA_PATH%\Bin.%PLATFORM%" -deppath "%DATA_PATH%\Dep.%PLATFORM%" -toolpath %TOOL_PATH% -pubpath %CURRENT_PATH% 

cd %CURRENT_PATH%