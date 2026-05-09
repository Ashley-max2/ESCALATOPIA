@echo off
title ML-Agents Boss Training - ESCALATOPIA
color 0A
cd /d "C:\Users\aiban\Desktop\ESCALATOPIAREALCHAT"

echo ============================================================
echo   ESCALATOPIA - ML Boss Training
echo ============================================================
echo.

:: Verificar si mlagents está instalado
python -c "import mlagents" 2>nul
if %errorlevel% neq 0 (
    echo [!] ML-Agents no encontrado. Instalando...
    echo.
    pip install mlagents
    echo.
    echo [OK] ML-Agents instalado.
    echo.
)

:: Verificar si hay un entrenamiento previo
if exist "results\boss_v1" (
    echo [INFO] Se encontro una sesion previa 'boss_v1'.
    echo [INFO] Resumiendo entrenamiento...
    echo.
    mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1 --resume
) else (
    echo [INFO] Iniciando entrenamiento nuevo...
    echo.
    mlagents-learn Assets/ML/boss_training.yaml --run-id=boss_v1
)

echo.
echo ============================================================
echo   Entrenamiento terminado o interrumpido.
echo   Resultados en: results\boss_v1\
echo   Ver stats: tensorboard --logdir results
echo ============================================================
pause
