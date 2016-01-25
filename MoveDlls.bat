SET mypath=%~dp0
move %mypath%Build\Calibration_Data\Plugins\msvcp120.dll %mypath%Build\msvcp120.dll
move %mypath%Build\Calibration_Data\Plugins\msvcr120.dll %mypath%Build\msvcr120.dll
move %mypath%Build\Calibration_Data\Plugins\opencv_ffmpeg300.dll %mypath%Build\opencv_ffmpeg300.dll
move %mypath%Build\Calibration_Data\Plugins\opencv_world300.dll %mypath%Build\opencv_world300.dll
move %mypath%Build\Calibration_Data\Plugins\opencv_world300d.dll %mypath%Build\opencv_world300d.dll

pause