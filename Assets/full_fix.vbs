Set objShell = CreateObject("WScript.Shell")
cmd = "cmd /k ""cd /d C:\Users\aiban\Desktop\ESCALATOPIAREALCHAT && echo [1/3] Instalando dependencias (mlagents + setuptools + protobuf)... && pip install mlagents==0.28.0 setuptools==68.0.0 protobuf==3.20.3 --ignore-requires-python --force-reinstall --quiet && echo [2/3] Parcheando cattrs/dispatch.py para Python 3.11... && python Assets/patch_mlagents.py & echo [3/3] Iniciando entrenamiento... && echo. && mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1 --force"""
objShell.Run cmd, 1, False
