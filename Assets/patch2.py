"""
Parche directo para cattrs/dispatch.py - Python 3.11 fix.
Usa sys.executable para encontrar el path exacto.
"""
import sys
import os

def get_site_packages():
    """Deriva site-packages desde sys.executable."""
    python_dir = os.path.dirname(sys.executable)
    # sys.executable = C:\...\Python311\python.exe
    # python_dir = C:\...\Python311\
    return os.path.join(python_dir, "Lib", "site-packages")

def patch_dispatch():
    sp = get_site_packages()
    path = os.path.join(sp, "cattrs", "dispatch.py")

    print(f"[INFO] Python: {sys.executable}")
    print(f"[INFO] Buscando: {path}")

    if not os.path.exists(path):
        # Try also Scripts/../
        python_dir2 = os.path.dirname(os.path.dirname(sys.executable))
        path2 = os.path.join(python_dir2, "Lib", "site-packages", "cattrs", "dispatch.py")
        print(f"[INFO] Intentando: {path2}")
        if os.path.exists(path2):
            path = path2
        else:
            print(f"[ERROR] No encontrado en ninguna ruta.")
            print(f"[DEBUG] sys.prefix = {sys.prefix}")
            # Try prefix
            path3 = os.path.join(sys.prefix, "Lib", "site-packages", "cattrs", "dispatch.py")
            print(f"[INFO] Intentando: {path3}")
            if os.path.exists(path3):
                path = path3
            else:
                return False

    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    if "# PY311_PATCH" in content:
        print(f"[OK] Ya parcheado: {path}")
        return True

    # The target line (12 spaces indent in cattrs 22.x)
    old_line = "            self._single_dispatch.register(cls, handler)"

    if old_line not in content:
        print(f"[WARN] Linea exacta no encontrada. Buscando variantes...")
        # Try with different indentation
        for indent in range(4, 20, 4):
            candidate = " " * indent + "self._single_dispatch.register(cls, handler)"
            if candidate in content:
                old_line = candidate
                print(f"[INFO] Encontrado con {indent} espacios de indentacion.")
                break
        else:
            print(f"[ERROR] No se encontro la linea de registro. Contenido parcial:")
            for i, line in enumerate(content.split("\n")):
                if "_single_dispatch" in line or "register" in line.lower():
                    print(f"  L{i+1}: {repr(line)}")
            return False

    indent_count = len(old_line) - len(old_line.lstrip())
    sp_indent = " " * indent_count
    inner_indent = " " * (indent_count + 4)

    new_block = (
        f"{sp_indent}try:  # PY311_PATCH\n"
        f"{inner_indent}self._single_dispatch.register(cls, handler)\n"
        f"{sp_indent}except TypeError:\n"
        f"{inner_indent}_o = getattr(cls, '__origin__', None)\n"
        f"{inner_indent}if _o is not None:\n"
        f"{inner_indent}    try:\n"
        f"{inner_indent}        self._single_dispatch.register(_o, handler)\n"
        f"{inner_indent}    except Exception:\n"
        f"{inner_indent}        pass"
    )

    content = content.replace(old_line, new_block)
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"[OK] Parcheado exitosamente: {path}")
    return True

if __name__ == "__main__":
    print("=== Patch cattrs/dispatch.py para Python 3.11 ===")
    ok = patch_dispatch()
    if ok:
        print("[OK] Listo. Corre mlagents-learn ahora.")
    else:
        print("[FAIL] El parche fallo.")
        sys.exit(1)
