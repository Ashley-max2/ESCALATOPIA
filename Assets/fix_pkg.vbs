Set objShell = CreateObject("WScript.Shell")
cmd = "cmd /k ""cd /d C:\Users\aiban\Desktop\ESCALATOPIAREALCHAT && echo [FIX] Arreglando pkg_resources... && pip install setuptools==68.0.0 --force-reinstall --quiet && echo [OK] setuptools 68 instalado. && echo [INFO] Iniciando entrenamiento... && echo. && mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1 --force"""
objShell.Run cmd, 1, False
