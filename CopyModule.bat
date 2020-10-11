
setlocal EnableDelayedExpansion

FOR /F "tokens=1,2 delims==" %%A IN (mod.ini) DO (set %%A=%%B)
xcopy SubModule\* "%Bannerlord_Path%\Modules\SeparatistCrisis\*" /Y /R /E

cd "%Bannerlord_Path%\bin\Win64_Shipping_Client"

%Bannerlord_Root_Drive%

"%Bannerlord_Path%\bin\Win64_Shipping_Client\Bannerlord.exe" /singleplayer _MODULES_*Native*SandBoxCore*CustomBattle*SandBox*StoryMode*SeparatistCrisis*_MODULES_