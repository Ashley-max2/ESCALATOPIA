"""
Parche directo en cattrs/dispatch.py para Python 3.11.
El error real: functools.singledispatch.register() rechaza typing.Dict[K,V] en Python 3.11.
Parcheamos dispatch.py directamente.
"""
import site
import os
import sys
import re

def find_file(relative_path):
    # Check system site-packages
    candidates = list(site.getsitepackages())
    # Also check user site-packages
    try:
        candidates.append(site.getusersitepackages())
    except Exception:
        pass
    # Also check the Python prefix directly
    import sys
    candidates.append(os.path.join(sys.prefix, "Lib", "site-packages"))
    candidates.append(os.path.join(sys.exec_prefix, "Lib", "site-packages"))
    for sp in candidates:
        candidate = os.path.join(sp, relative_path)
        if os.path.exists(candidate):
            return candidate
    # Debug: print all checked paths
    print(f"[DEBUG] Buscado en: {candidates}")
    return None

def patch_cattrs_dispatch():
    path = find_file("cattrs/dispatch.py")
    if not path:
        print("[ERROR] No se encontro cattrs/dispatch.py")
        return False

    print(f"[INFO] Parcheando: {path}")

    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
        lines = f.readlines() if False else content.split("\n")

    if "# PY311_PATCH" in content:
        print("[OK] cattrs/dispatch.py ya esta parcheado.")
        return True

    # Buscar la linea: self._single_dispatch.register(cls, handler)
    # En cattrs 22+, esta en register_cls_list
    old_line = "            self._single_dispatch.register(cls, handler)"
    new_line = """            try:  # PY311_PATCH
                self._single_dispatch.register(cls, handler)
            except TypeError:
                # Python 3.11+: singledispatch rechaza tipos genericos como typing.Dict[K,V]
                # Usar un wrapper que hace la dispersion manualmente
                _origin = getattr(cls, "__origin__", None)
                _args = getattr(cls, "__args__", None)
                if _origin is not None:
                    # Registrar para el tipo base (dict, list, etc.)
                    try:
                        self._single_dispatch.register(_origin, handler)
                    except Exception:
                        pass
                # else: ignorar el tipo no soportado"""

    if old_line in content:
        content = content.replace(old_line, new_line)
        with open(path, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"[OK] cattrs/dispatch.py parcheado exitosamente.")
        return True
    else:
        # Mostrar las lineas alrededor de register para debug
        for i, line in enumerate(lines):
            if "_single_dispatch.register" in line:
                print(f"[INFO] Linea {i+1}: {line!r}")
        print("[WARN] No se encontro la linea exacta. Intentando parche alternativo...")

        # Parche alternativo: parchear por numero de linea
        for i, line in enumerate(lines):
            if "_single_dispatch.register" in line and "cls" in line:
                indent = len(line) - len(line.lstrip())
                sp = " " * indent
                lines[i] = f"{sp}try:  # PY311_PATCH\n{sp}    {line.strip()}\n{sp}except TypeError:\n{sp}    _o = getattr(cls, '__origin__', None)\n{sp}    if _o: self._single_dispatch.register(_o, handler)\n"
                content = "\n".join(lines)
                with open(path, "w", encoding="utf-8") as f:
                    f.write(content)
                print(f"[OK] Parche alternativo aplicado en linea {i+1}.")
                return True

        print("[ERROR] No se pudo encontrar la linea a parchear.")
        return False

def patch_torch_utils():
    """Parchea el problema de pkg_resources en torch.py"""
    path = find_file("mlagents/torch_utils/torch.py")
    if not path:
        print("[WARN] No se encontro torch_utils/torch.py")
        return True

    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    if "importlib.metadata" in content or "pkg_resources" not in content:
        print("[OK] torch.py ya parcheado o sin problema.")
        return True

    old = "import pkg_resources"
    new = """try:
    import pkg_resources
except ImportError:
    import importlib.metadata as _imeta
    class pkg_resources:
        @staticmethod
        def get_distribution(name):
            class _D:
                try:
                    version = _imeta.version(name)
                except Exception:
                    version = "0.0.0"
            return _D()"""

    if old in content:
        content = content.replace(old, new, 1)
        with open(path, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"[OK] torch.py parcheado: {path}")
    return True

if __name__ == "__main__":
    print("=== Parcheando cattrs y mlagents para Python 3.11 ===")
    r1 = patch_torch_utils()
    r2 = patch_cattrs_dispatch()
    if r1 and r2:
        print("\n[OK] Parches aplicados. Prueba mlagents-learn ahora.")
    else:
        print("\n[WARN] Algunos parches fallaron.")
        sys.exit(1)
