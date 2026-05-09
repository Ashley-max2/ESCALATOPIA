Set objShell = CreateObject("WScript.Shell")
cmd = "cmd /k ""cd /d C:\Users\aiban\Desktop\ESCALATOPIAREALCHAT && echo [FIX] Instalando setuptools... && pip install setuptools --quiet && echo [FIX] Instalando mlagents... && pip install mlagents --quiet && echo. && echo [OK] Todo instalado. Iniciando entrenamiento... && echo. && mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1 --force"""
objShell.Run cmd, 1, False
