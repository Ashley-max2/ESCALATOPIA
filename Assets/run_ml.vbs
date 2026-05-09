Set objShell = CreateObject("WScript.Shell")
cmd = "cmd /k ""cd /d C:\Users\aiban\Desktop\ESCALATOPIAREALCHAT && pip install mlagents --quiet && echo. && echo [OK] mlagents listo && echo. && mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1 --force"""
objShell.Run cmd, 1, False
