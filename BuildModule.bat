
FOR /F "tokens=1,2 delims==" %%A IN (mod.ini) DO (set %%A=%%B)
xcopy "%Bannerlord_Path%\bin\Win64_Shipping_Client\*.dll" "lib\*" /Y /R /E
xcopy "%Bannerlord_Path%\Modules\CustomBattle\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.CustomBattle.dll" "lib\*" /Y /R /E

cd .\Source
dotnet build /p:Configuration=Release /nologo

xcopy .\SeparatistCrisis\bin\x64\Release\netstandard2.0\SeparatistCrisis.dll ".\SubModule\bin\Win64_Shipping_Client\" /Y /R /E
xcopy .\SeparatistCrisis\bin\x64\Release\netstandard2.0\SeparatistCrisis.dll ".\SubModule\bin\Win64_Shipping_wEditor\" /Y /R /E