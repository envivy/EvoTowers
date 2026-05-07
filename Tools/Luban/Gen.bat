@echo off
setlocal

pushd "%~dp0"

if not exist "Luban\Luban.dll" (
    echo Missing Luban\Luban.dll
    echo Download Luban and place Luban.dll under Tools\Luban\Luban\
    popd
    exit /b 1
)

dotnet Luban\Luban.dll ^
    -t client ^
    -c cs-simple-json ^
    -d json ^
    --conf luban.conf ^
    -x outputCodeDir=../../Assets/Scripts/Generated/Luban ^
    -x outputDataDir=../../Assets/Resources/Generated/DataTables ^
    --validationFailAsError

set EXIT_CODE=%ERRORLEVEL%
popd
exit /b %EXIT_CODE%
