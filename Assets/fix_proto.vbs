Set objShell = CreateObject("WScript.Shell")
cmd = "cmd /k ""cd /d C:\Users\aiban\Desktop\ESCALATOPIAREALCHAT && echo [FIX] Arreglando protobuf... && pip install protobuf==3.20.3 --quiet && echo [OK] protobuf 3.20.3 instalado. && echo [INFO] Iniciando entrenamiento... && echo. && mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1 --force"""
objShell.Run cmd, 1, False
