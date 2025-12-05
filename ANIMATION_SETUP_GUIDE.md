# ⚡ GUÍA RÁPIDA - MÁQUINAS DE ESTADO 2 ESTADOS
## Configuración en 5 Minutos - ESCALATOPIA

---

## 🚀 INICIO RÁPIDO - ¿QUÉ HACER?

**✅ LOS SCRIPTS YA ESTÁN INTEGRADOS CON TU CÓDIGO**
- `ClimbingState.cs` ya llama a `ClimbingHangingAnimController`
- `HookMovementController.cs` ya llama a `HookedFreeAnimController`
- `IdleWalkAnimController` funciona automáticamente con Rigidbody

**TU TRABAJO:**
1. Crear 3 Animator Controllers (2 estados cada uno)
2. Añadir parámetros Bool/Float
3. Crear transiciones simples
4. Añadir los scripts al Player GameObject
5. ¡Play y funciona!

**TIEMPO TOTAL: ~15 minutos para las 3 máquinas**

---

## 📋 ÍNDICE
1. [Máquina 1: Idle / Walk](#máquina-1-idle--walk)
2. [Máquina 2: Climbing / Hanging](#máquina-2-climbing--hanging)
3. [Máquina 3: Hooked / Free](#máquina-3-hooked--free)
4. [Troubleshooting Rápido](#troubleshooting-rápido)

---

## 🎯 MÁQUINA 1: IDLE / WALK

**Para:** Movimiento básico en suelo (Player y Bosses)

### PASO 1: Crear Animator Controller
```
1. Project → Create → Animator Controller
2. Nombrar: "IdleWalk_Controller"
3. Doble click para abrir Animator window
```

### PASO 2: Añadir Estados
```
1. En Animator Window → Click derecho → Create State → Empty
2. Nombrar primer estado: "Idle"
3. Click derecho → Create State → Empty
4. Nombrar segundo estado: "Walk"
5. Click derecho en "Idle" → Set as Layer Default State (naranja)
```

### PASO 3: Añadir Parámetro
```
1. En Animator Window → Panel "Parameters" (izquierda)
2. Click en "+" → Bool
3. Nombrar: "IsMoving"
```

### PASO 4: Crear Transiciones
```
TRANSICIÓN 1: Idle → Walk
1. Click derecho en "Idle" → Make Transition
2. Click en "Walk"
3. Seleccionar la flecha blanca
4. Inspector → Conditions → + → IsMoving: true
5. Settings → ☐ Has Exit Time (desmarcar)
6. Transition Duration: 0.15

TRANSICIÓN 2: Walk → Idle
1. Click derecho en "Walk" → Make Transition
2. Click en "Idle"
3. Seleccionar la flecha
4. Conditions → + → IsMoving: false
5. Settings → ☐ Has Exit Time (desmarcar)
6. Transition Duration: 0.15
```

### PASO 5: Asignar Animaciones (si las tienes)
```
1. Seleccionar estado "Idle"
2. Inspector → Motion → Arrastra tu clip "Idle"
3. Seleccionar estado "Walk"
4. Inspector → Motion → Arrastra tu clip "Walk"

SI NO TIENES ANIMACIONES:
- Deja los estados vacíos por ahora
- El script funcionará igual para testing
```

### PASO 6: Añadir Script al GameObject
```
1. Seleccionar tu personaje (Player o Boss) en Hierarchy
2. Inspector → Add Component → "IdleWalkAnimController"
3. El script auto-asigna Animator y Rigidbody
4. Configurar:
   - Movement Threshold: 0.1 (por defecto, está bien)
   - Show Debug: ☑ (para ver logs en Console)
```

### PASO 7: Conectar Animator Controller
```
1. Seleccionar personaje en Hierarchy
2. Buscar componente "Animator"
3. Controller → Arrastra "IdleWalk_Controller"
```

### ✅ PRUEBA
```
1. Play
2. Mueve el personaje
3. Console mostrará: "[IdleWalk] Speed: X | IsMoving: true/false"
4. En Animator window verás las transiciones en tiempo real
```

---

## 🧗 MÁQUINA 2: CLIMBING / HANGING

**Para:** Escalada en paredes (Player y Bosses)

### PASO 1: Crear Animator Controller
```
1. Project → Create → Animator Controller
2. Nombrar: "ClimbingHanging_Controller"
3. Doble click para abrir
```

### PASO 2: Añadir Estados
```
1. Create State → Empty → Nombrar: "Hanging" (colgado sin moverse)
2. Create State → Empty → Nombrar: "Climbing" (escalando activamente)
3. Click derecho en "Hanging" → Set as Layer Default State
```

### PASO 3: Añadir Parámetros
```
1. Parameters → + → Bool → "IsClimbing"
2. Parameters → + → Float → "ClimbSpeed"
```

### PASO 4: Crear Transiciones
```
TRANSICIÓN 1: Hanging → Climbing
1. Hanging → Make Transition → Climbing
2. Seleccionar flecha
3. Conditions → + → IsClimbing: true
4. ☐ Has Exit Time (desmarcar)
5. Transition Duration: 0.1

TRANSICIÓN 2: Climbing → Hanging
1. Climbing → Make Transition → Hanging
2. Conditions → + → IsClimbing: false
3. ☐ Has Exit Time (desmarcar)
4. Transition Duration: 0.1
```

### PASO 5: Asignar Animaciones
```
Estado "Hanging":
- Motion → Clip de personaje agarrado a pared (estático)

Estado "Climbing":
- Motion → Clip de personaje escalando
- O usar Blend Tree 1D con ClimbSpeed (avanzado)
```

### PASO 6: Añadir Script
```
1. Seleccionar personaje
2. Add Component → "ClimbingHangingAnimController"
3. Show Debug: ☑
```

### PASO 7: Conectar Animator Controller
```
Componente Animator → Controller → "ClimbingHanging_Controller"
```

### PASO 8: Integrar con tu Sistema de Escalada
```csharp
// YA ESTÁ INTEGRADO AUTOMÁTICAMENTE
// El script ClimbingState.cs ya tiene la integración completa:

// ✅ En Enter() llama a: animController.StartClimbing()
// ✅ En Exit() llama a: animController.StopClimbing()  
// ✅ En Escalar() actualiza: animController.SetClimbingSpeed(climbSpeed)

// NO NECESITAS AÑADIR CÓDIGO ADICIONAL
// Solo asegúrate de:
// 1. Añadir el componente ClimbingHangingAnimController al Player
// 2. Configurar el Animator Controller
// 3. ¡Listo! El sistema ya está conectado
```

### ✅ PRUEBA
```
1. Play
2. Llama manualmente desde Inspector:
   - Botón: StartClimbing()
   - Ajusta ClimbSpeed: 1 o -1
3. Verás transición Hanging ↔ Climbing
```

---

## 🪝 MÁQUINA 3: HOOKED / FREE

**Para:** Animaciones de gancho/impulso (Player y Bosses)

### PASO 1: Crear Animator Controller
```
1. Project → Create → Animator Controller
2. Nombrar: "HookedFree_Controller"
3. Doble click para abrir
```

### PASO 2: Añadir Estados
```
1. Create State → Empty → Nombrar: "Free" (normal, sin gancho)
2. Create State → Empty → Nombrar: "Hooked" (enganchado/impulso)
3. "Free" → Set as Layer Default State
```

### PASO 3: Añadir Parámetro
```
Parameters → + → Bool → "IsHooked"
```

### PASO 4: Crear Transiciones
```
TRANSICIÓN 1: Free → Hooked
1. Free → Make Transition → Hooked
2. Conditions → IsHooked: true
3. ☐ Has Exit Time (desmarcar)
4. Transition Duration: 0.05 (muy rápido)

TRANSICIÓN 2: Hooked → Free
1. Hooked → Make Transition → Free
2. Conditions → IsHooked: false
3. ☐ Has Exit Time (desmarcar)
4. Transition Duration: 0.1
```

### PASO 5: Asignar Animaciones
```
Estado "Free":
- Motion → Idle o caminando (o dejar vacío)

Estado "Hooked":
- Motion → Animación de impulso/gancho
- O pose de brazos extendidos
```

### PASO 6: Añadir Script
```
1. Seleccionar personaje
2. Add Component → "HookedFreeAnimController"
3. Configurar:
   - Min Hook Duration: 0.2 (evita flicker)
   - Show Debug: ☑
```

### PASO 7: Conectar Animator Controller
```
Componente Animator → Controller → "HookedFree_Controller"
```

### PASO 8: Integrar con tu HookSystem existente
```csharp
// YA ESTÁ INTEGRADO AUTOMÁTICAMENTE
// El script HookMovementController.cs ya tiene la integración:

// ✅ En LaunchHook() llama a: animController.OnHookConnected()
// ✅ En CancelHook() llama a: animController.OnHookReleased()

// NO NECESITAS AÑADIR CÓDIGO ADICIONAL
// Solo asegúrate de:
// 1. Añadir el componente HookedFreeAnimController al Player
// 2. Configurar el Animator Controller
// 3. ¡Listo! El sistema ya está conectado
```

### ✅ PRUEBA
```
1. Play
2. Usa tu gancho normalmente
3. Console mostrará: "[HookedFree] Hook CONNECTED/RELEASED"
4. Verás transiciones instantáneas en Animator
```

---

## 🔧 TROUBLESHOOTING RÁPIDO

### ❌ Las transiciones no funcionan
```
SOLUCIÓN:
1. Verificar que el parámetro existe (mismo nombre exacto)
2. Verificar que "Has Exit Time" está desmarcado
3. Verificar que las condiciones están bien (IsMoving: true/false)
4. Activar "Show Debug" en el script para ver logs
```

### ❌ El Animator no cambia de estado
```
SOLUCIÓN:
1. Verificar que el Controller está asignado en componente Animator
2. Verificar que el GameObject tiene Animator component
3. Console: buscar errores (parámetro no existe, etc.)
4. En Animator window → Play mode → ver qué estado está activo
```

### ❌ Script no detecta movimiento (IdleWalk)
```
SOLUCIÓN:
1. Verificar que el GameObject tiene Rigidbody
2. Verificar que Movement Threshold no es muy alto (usar 0.1)
3. Ver logs en Console con Show Debug activo
4. Verificar que el personaje efectivamente se mueve (rb.velocity > 0)
```

### ❌ Climbing/Hanging no funciona
```
SOLUCIÓN:
1. Llamar manualmente StartClimbing() cuando toca pared
2. Llamar SetClimbingSpeed(float) cada frame con input vertical
3. Llamar StopClimbing() cuando se suelta
4. Ver logs para verificar que isOnWall = true
```

### ❌ HookedFree parpadea (flicker)
```
SOLUCIÓN:
1. Aumentar Min Hook Duration a 0.3 o 0.5
2. Verificar que no llamas OnHookReleased() inmediatamente después de OnHookConnected()
3. Transition Duration más largo (0.15 en lugar de 0.05)
```

### ❌ Animaciones no se ven / personaje estático
```
SOLUCIÓN:
- Es normal si no has asignado clips de animación
- Los scripts funcionan igual, solo no verás animación visual
- Para testing, deja estados vacíos y verifica transiciones en Animator window
- Cuando tengas clips, arrástralos a Motion en cada estado
```

---

## 📊 RESUMEN DE PARÁMETROS

| Máquina | Parámetros | Tipo | Uso |
|---------|-----------|------|-----|
| **IdleWalk** | IsMoving | Bool | true = caminando, false = idle |
| | Speed | Float | Velocidad actual (opcional, para blend trees) |
| **ClimbingHanging** | IsClimbing | Bool | true = escalando, false = colgado |
| | ClimbSpeed | Float | Velocidad vertical (positivo = subir) |
| **HookedFree** | IsHooked | Bool | true = gancho conectado, false = libre |

---

## ⚙️ CONFIGURACIÓN RECOMENDADA

### Transition Settings (todas las máquinas)
```
Has Exit Time: ☐ (desmarcar para respuesta inmediata)
Exit Time: 0.75 (ignorado si Has Exit Time = false)
Fixed Duration: ☑ (marcar)
Transition Duration: 0.1 - 0.15 segundos (suave pero rápido)
Transition Offset: 0
Interruption Source: None (por defecto)
```

### Performance Tips
```
- Evita Blend Trees si solo necesitas 2 estados
- Usa bool en lugar de float cuando sea posible (más eficiente)
- Desactiva "Apply Root Motion" si controlas movimiento por código
- Activa "Culling Mode: Cull Completely" si el personaje no es visible
```

---

## 🎮 TESTING RÁPIDO

### Test sin Animaciones
```
1. Crea los Animator Controllers con estados vacíos
2. Añade scripts
3. Activa Show Debug
4. Play y verifica logs en Console
5. En Animator window (Play mode) verás transiciones funcionando
```

### Test con Animaciones
```
1. Arrastra clips a los estados
2. Play y verifica visualmente las animaciones
3. Ajusta Transition Duration si hay cortes bruscos
```

---

## 🚀 PRÓXIMOS PASOS

Después de configurar estas 3 máquinas:

1. **Combinar máquinas** → Usa Layers en Animator para mezclar (ej: IdleWalk en Base Layer, HookedFree en Overlay)
2. **Blend Trees** → Expande IdleWalk a Idle/Walk/Run con blend tree 1D usando Speed
3. **Animaciones** → Importa/crea clips y asígnalos a estados
4. **Integración AI** → Conecta scripts con tus Bosses (GOAP actions)

---

## ⚡ CONFIGURACIÓN RÁPIDA PARA TU PROYECTO

### Paso 1: Añadir componentes al Player (1 minuto)
```
1. Hierarchy → Seleccionar "Player 1" (tu personaje)
2. Add Component → "IdleWalkAnimController"
3. Add Component → "ClimbingHangingAnimController"
4. Add Component → "HookedFreeAnimController"
5. Marcar "Show Debug" en los 3 para testing
```

### Paso 2: Crear los 3 Animator Controllers (5 minutos)
```
Sigue las secciones anteriores para crear:
- IdleWalk_Controller (Máquina 1)
- ClimbingHanging_Controller (Máquina 2)
- HookedFree_Controller (Máquina 3)
```

### Paso 3: Combinar en un solo Animator (avanzado - opcional)
```
1. Usar IdleWalk_Controller como base
2. Crear Layer: "UpperBody" para Hooked
3. Crear Layer: "Climbing" para escalada
4. O mantenerlos separados y cambiar Controller según estado
```

### Paso 4: Testing inmediato
```
1. Play
2. Mueve el personaje → verás logs "[IdleWalk] Speed..."
3. Escala una pared → verás "[ClimbingHanging] IsOnWall: true"
4. Usa el gancho → verás "[HookedFree] Hook CONNECTED"
5. En Animator window verás las transiciones en tiempo real
```

### Archivos Modificados (ya listos):
```
✅ ClimbingState.cs → Integrado con ClimbingHangingAnimController
✅ HookMovementController.cs → Integrado con HookedFreeAnimController
✅ IdleWalkAnimController.cs → Funciona solo (detecta velocidad)
```

### Lo que NO necesitas hacer:
```
❌ NO modificar PlayerController.cs
❌ NO tocar código de escalada (ya integrado)
❌ NO tocar HookSystem (ya integrado)
❌ Solo añadir componentes y crear Animator Controllers
```

---

## ✅ CHECKLIST FINAL

Antes de cerrar:

**IdleWalk:**
- [ ] Controller creado con 2 estados
- [ ] Parámetro "IsMoving" (Bool) añadido
- [ ] Transiciones configuradas (Has Exit Time = false)
- [ ] Script añadido al personaje
- [ ] Controller asignado en Animator component

**ClimbingHanging:**
- [ ] Controller creado con 2 estados
- [ ] Parámetros "IsClimbing" (Bool) y "ClimbSpeed" (Float) añadidos
- [ ] Transiciones configuradas
- [ ] Script añadido al personaje
- [ ] Integrado con sistema de escalada (StartClimbing/StopClimbing)

**HookedFree:**
- [ ] Controller creado con 2 estados
- [ ] Parámetro "IsHooked" (Bool) añadido
- [ ] Transiciones configuradas (muy rápidas)
- [ ] Script añadido al personaje
- [ ] Integrado con HookSystem (OnHookConnected/Released)

---

**¡Todo listo en menos de 15 minutos!** 🎉

**Creado para ESCALATOPIA**
**Fecha:** Diciembre 2025
**Versión:** 1.0
