Set objShell = CreateObject("WScript.Shell")
cmd = "cmd /k ""pip show cattrs && python -c """"import cattrs, os; print('cattrs location:', os.path.dirname(cattrs.__file__))"""""""
objShell.Run cmd, 1, False
