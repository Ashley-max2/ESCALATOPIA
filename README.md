# ESCALATOPIA

Juego 3D de escalada y exploración inspirado en **Zelda: Breath of the Wild** (movimiento/cámara) y **Jusant** (mecánicas de escalada).

## 🎮 Características Principales

- **Movimiento estilo Zelda BotW**: Cámara orbital con rotación suave y movimiento relativo
- **Escalada estilo Jusant**: Sistema de escalada en cualquier dirección con consumo de estamina
- **Sistema de Gancho**: Enganche a puntos específicos para traversar el entorno
- **Sistema de Caída**: Sin barras de vida - caídas letales desde gran altura
- **Máquina de Estados**: Arquitectura modular y extensible para las mecánicas del jugador

## 🚀 Inicio Rápido

1. Abrir el proyecto en Unity (2021.3 LTS o superior)
2. Ir a `Tools > Escalatopia > Player Setup Wizard`
3. Configurar los Layers requeridos (Player, Ground, Climbable, HookPoint)
4. Click en "CREAR TODO"
5. ¡Dale a Play!

## 📁 Estructura del Proyecto

```
Assets/Scripts/
├── Camera/           → Cámara orbital estilo Zelda
├── Core/             → Eventos y managers
├── Editor/           → Herramientas de setup
├── Player/           → Sistema de jugador (State Machine)
└── Systems/          → Gancho, estamina, puntos de hook
```

## 🎮 Controles

| Acción | Tecla |
|--------|-------|
| Mover | WASD |
| Sprint | Shift |
| Saltar | Espacio |
| Escalar | E (mantener) |
| Salto de pared | Espacio (escalando) |
| Gancho | Click Derecho |
| Soltar gancho | Click Izquierdo |
| Pausar | ESC |

## 📚 Documentación

- `GUIA_TUTORIAL.txt` - Guía rápida
- `GUIA_CONFIGURACION_PLAYER.txt` - Configuración detallada
- `GDD proyecto.pdf` - Game Design Document

## 🏗️ Arquitectura

El jugador utiliza el patrón **State Machine** con los siguientes estados:

- `PlayerGroundedState` - En el suelo
- `PlayerJumpState` - Saltando
- `PlayerAirborneState` - En el aire
- `PlayerClimbState` - Escalando
- `PlayerWallJumpState` - Salto de pared
- `PlayerHookState` - Usando gancho
- `PlayerDeadState` - Muerte por caída

---
*Proyecto de desarrollo DAMVI*
