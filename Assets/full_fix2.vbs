Set objShell = CreateObject("WScript.Shell")

' Step 1: Install dependencies
r1 = objShell.Run("cmd /c pip install mlagents==0.28.0 setuptools==68.0.0 protobuf==3.20.3 --ignore-requires-python --force-reinstall --quiet", 0, True)

' Step 2: Patch cattrs dispatch.py directly using python -c one-liner
patchCmd = "cmd /c python -c """ & _
"import sys,os;" & _
"p=os.path.join(os.path.dirname(sys.executable),'Lib','site-packages','cattrs','dispatch.py');" & _
"f=open(p);c=f.read();f.close();" & _
"tag='# PY311_PATCH';" & _
"old='            self._single_dispatch.register(cls, handler)';" & _
"new='            try:  # PY311_PATCH\n                self._single_dispatch.register(cls, handler)\n            except TypeError:\n                _o=getattr(cls,chr(95)chr(95)+chr(111)+chr(114)+chr(105)+chr(103)+chr(105)+chr(110)+chr(95)chr(95),None)\n                if _o:\n                    try:self._single_dispatch.register(_o,handler)\n                    except:pass';" & _
"c=c.replace(old,new) if tag not in c else c;" & _
"open(p,'w').write(c);" & _
"print('DONE')" & _
""""
r2 = objShell.Run(patchCmd, 0, True)

' Step 3: Launch training
cmd = "cmd /k ""cd /d C:\Users\aiban\Desktop\ESCALATOPIAREALCHAT && mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1 --force"""
objShell.Run cmd, 1, False
