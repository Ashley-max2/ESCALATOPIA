# GUÍA DE CONFIGURACIÓN - SISTEMA DE BOSSES DE ESCALADA
## Unity 3D - ESCALATOPIA

---

## 📋 ÍNDICE
1. [Requisitos Previos](#requisitos-previos)
2. [Instalación de Scripts](#instalación-de-scripts)
3. [Configuración de Escena](#configuración-de-escena)
4. [Configuración de Bosses](#configuración-de-bosses)
5. [Sistema de UI](#sistema-de-ui)
6. [Puntos de Escalada](#puntos-de-escalada)
7. [Pathfinding](#pathfinding)
8. [Testing y Ajustes](#testing-y-ajustes)
9. [Troubleshooting](#troubleshooting)

---

## 🔧 REQUISITOS PREVIOS

### Paquetes de Unity necesarios:
- **TextMesh Pro** (para UI)
- **Input System** (si usas el nuevo sistema de input)

### Estructura del proyecto:
Todos los scripts deben estar en: `Assets/Scripts/Bosses/`

**Lista de archivos:**
- `GOAPAction.cs`
- `GOAPPlanner.cs`
- `BossAIBase.cs`
- `Boss1Novice.cs`
- `Boss2Tactical.cs`
- `Boss3Learner.cs`
- `Boss4Professional.cs`
- `ClimbingPathfinder.cs`
- `BossRaceManager.cs`

---

## 📁 INSTALACIÓN DE SCRIPTS

### Paso 1: Crear la estructura de carpetas
```
Assets/
  Scripts/
    Bosses/          ← Aquí van todos los scripts de bosses
```

### Paso 2: Importar los scripts
1. Copia todos los archivos `.cs` a `Assets/Scripts/Bosses/`
2. Espera a que Unity compile (no debe haber errores)
3. Si hay errores de TextMesh Pro, importa el paquete desde Window → Package Manager

---

## 🎬 CONFIGURACIÓN DE ESCENA

### Paso 1: Crear GameObjects básicos

#### 1.1 Crear Race Manager
```
1. Hierarchy → Click derecho → Create Empty
2. Nombrar: "BossRaceManager"
3. Add Component → BossRaceManager
```

#### 1.2 Crear Puntos de Inicio y Meta
```
1. Hierarchy → Create Empty → Nombrar: "RaceStartPoint"
   - Posicionar en el punto de inicio de la carrera
   
2. Hierarchy → Create Empty → Nombrar: "RaceGoalPoint"
   - Posicionar en la meta de la carrera
```

#### 1.3 Crear Pathfinder
```
1. Hierarchy → Create Empty → Nombrar: "ClimbingPathfinder"
2. Add Component → ClimbingPathfinder
3. Configurar:
   - Max Climb Distance: 2
   - Climbable Layer: (seleccionar la layer de paredes escalables)
   - Debug Mode: ☑ (activar para ver las rutas)
```

---

## 🤖 CONFIGURACIÓN DE BOSSES

### Crear Boss 1: El Novato (Fácil)

#### Paso 1: Crear el GameObject
```
1. Hierarchy → 3D Object → Capsule
2. Nombrar: "Boss1_Novice"
3. Escalar: (1, 2, 1) - similar al jugador
4. Posición: cerca del RaceStartPoint
```

#### Paso 2: Añadir componentes requeridos
```
1. Add Component → Rigidbody
   ☑ Use Gravity: true
   ☐ Is Kinematic: false
   Mass: 70
   Drag: 2
   Angular Drag: 0.05
   
2. Add Component → Capsule Collider
   Radius: 0.5
   Height: 2
   Center: (0, 0, 0)
```

#### Paso 3: Añadir script del Boss
```
1. Add Component → Boss1Novice
2. Configurar en el Inspector:
```

**Boss Configuration:**
- Boss Name: "El Novato"
- Difficulty: Easy
- Base Speed: 5

**AI Settings:**
- AI Intelligence: 0.3
- Speed Modifier: 0.7
- Decision Delay: 0.8
- Allow Mistakes: ☑

**Stamina System:**
- Max Stamina: 100
- Current Stamina: 100
- Stamina Drain Rate: 5
- Stamina Recovery Rate: 10

**Components:**
- Rb: (arrastrar el Rigidbody del mismo GameObject)
- Pathfinder: (arrastrar el ClimbingPathfinder de la escena)

**Race Settings:**
- Start Point: (arrastrar RaceStartPoint)
- Goal Point: (arrastrar RaceGoalPoint)

**Boss 1 Specific:**
- Mistake Chance: 0.4
- Slowdown Factor: 0.7
- Prefer Safe Routes: ☑

---

### Crear Boss 2: El Táctico (Medio)

Repetir proceso anterior pero usar:
```
GameObject: "Boss2_Tactical"
Component: Boss2Tactical

Boss Configuration:
- Boss Name: "El Táctico"
- Difficulty: Medium
- Base Speed: 6

AI Settings:
- AI Intelligence: 0.6
- Speed Modifier: 1.0
- Decision Delay: 0.4

Boss 2 Specific:
- Route Evaluation Interval: 5
- Route Alternatives: 3
- Adapt To Player: ☑
- Speed Preference: 0.6
- Stamina Conservation: 0.5
```

---

### Crear Boss 3: El Aprendiz (Difícil)

```
GameObject: "Boss3_Learner"
Component: Boss3Learner

Boss Configuration:
- Boss Name: "El Aprendiz"
- Difficulty: Hard
- Base Speed: 6.5

AI Settings:
- AI Intelligence: 0.75
- Speed Modifier: 1.1
- Decision Delay: 0.3

Boss 3 Specific:
- Learning Rate: 0.1
- Max Memory Size: 50
- Enable Learning: ☑
- Exploration Rate: 0.2
- Confidence Threshold: 0.7
```

---

### Crear Boss 4: El Profesional (Experto)

```
GameObject: "Boss4_Professional"
Component: Boss4Professional

Boss Configuration:
- Boss Name: "El Profesional"
- Difficulty: Expert
- Base Speed: 7

AI Settings:
- AI Intelligence: 0.95
- Speed Modifier: 1.2
- Decision Delay: 0.1
- Allow Mistakes: ☐

Boss 4 Specific:
- Use Predefined Sequence: ☑
- Sequence Precision: 0.95
- Perfect Timing: ☑
- Predict Player Movement: ☑
- Reaction Time: 0.1
- Optimize Stamina Usage: ☑
```

---

## 🎨 SISTEMA DE UI

### Paso 1: Crear Canvas
```
1. Hierarchy → UI → Canvas
2. Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
```

### Paso 2: Crear elementos de UI

#### 2.1 Countdown Text
```
1. Canvas → Create → TextMeshPro - Text
2. Nombrar: "CountdownText"
3. Configurar:
   - Font Size: 120
   - Alignment: Center, Middle
   - Color: Blanco
   - Posición: Centro de la pantalla
```

#### 2.2 Race Status Text
```
1. Canvas → Create → TextMeshPro - Text
2. Nombrar: "RaceStatusText"
3. Configurar:
   - Font Size: 40
   - Posición: Top Center
   - Anchor: Top Center
```

#### 2.3 Player Progress Text
```
1. Canvas → Create → TextMeshPro - Text
2. Nombrar: "PlayerProgressText"
3. Configurar:
   - Font Size: 30
   - Posición: Top Left
   - Color: Azul
```

#### 2.4 Boss Progress Text
```
1. Canvas → Create → TextMeshPro - Text
2. Nombrar: "BossProgressText"
3. Configurar:
   - Font Size: 30
   - Posición: Top Right
   - Color: Rojo
```

#### 2.5 Results Panel
```
1. Canvas → Create → Panel
2. Nombrar: "RaceResultsPanel"
3. Añadir hijo: TextMeshPro - Text → "ResultsText"
   - Font Size: 50
   - Alignment: Center
   - Color: Amarillo

4. Añadir botones:
   - "RestartButton" → OnClick: BossRaceManager.RestartRace
   - "NextBossButton" → OnClick: BossRaceManager.NextBoss
   - "PreviousBossButton" → OnClick: BossRaceManager.PreviousBoss
```

### Paso 3: Conectar UI al Race Manager
```
Seleccionar BossRaceManager en Hierarchy:

UI References:
- Countdown Text: (arrastrar CountdownText)
- Race Status Text: (arrastrar RaceStatusText)
- Player Progress Text: (arrastrar PlayerProgressText)
- Boss Progress Text: (arrastrar BossProgressText)
- Race Results Panel: (arrastrar RaceResultsPanel)
- Results Text: (arrastrar ResultsText del panel)
```

---

## 🧗 PUNTOS DE ESCALADA

### Sistema Automático Basado en Layers (RECOMENDADO)

El sistema ahora detecta automáticamente las superficies escalables basándose en **Layers** y **Tags**. ¡No necesitas crear marcadores manuales!

#### Paso 1: Configurar tus objetos de escena

**Para puntos de gancho:**
```
1. Seleccionar objetos donde el boss puede usar el gancho
2. Inspector → Layer → "agarre"
```

**Para paredes escalables:**
```
Opción A - Por Layer:
1. Seleccionar paredes
2. Inspector → Layer → "pared"

Opción B - Por Tag:
1. Seleccionar paredes
2. Inspector → Tag → "escalable"
```

**Para superficies caminables:**
```
1. Seleccionar suelos/plataformas
2. Inspector → Layer → "suelo"
```

#### Paso 2: Configurar el ClimbingPathfinder

```
1. Seleccionar ClimbingPathfinder en Hierarchy
2. Configurar Layer Detection:
   - Agarre Layer: marcar "agarre"
   - Pared Layer: marcar "pared"
   - Suelo Layer: marcar "suelo"
3. Tag Detection:
   - Escalable Tag: "escalable"
```

#### Paso 3: Inicializar

```
1. Play
2. El sistema se inicializa automáticamente
3. O usar Context Menu: ClimbingPathfinder → Initialize Climb Points
```

### Ventajas del Sistema Automático

✅ **No requiere marcadores manuales** - Detecta automáticamente las superficies
✅ **Adaptativo** - Se ajusta a cambios en el nivel
✅ **Inteligente** - Distingue entre suelo, paredes y puntos de gancho
✅ **Eficiente** - Genera solo los puntos necesarios
✅ **Visual** - Debug mode muestra todos los puntos detectados

### Comportamiento del Boss según Superficie

El boss adaptará su movimiento automáticamente:

```
📍 Foothold (Layer "suelo"):
   → Camina o corre
   → Velocidad normal
   → No consume tanta resistencia

📍 Handhold (Tag "escalable" o Layer "pared"):
   → Escala la superficie
   → Velocidad reducida (70%)
   → Consume resistencia media

📍 HookPoint (Layer "agarre"):
   → Usa el gancho para impulsarse
   → Velocidad aumentada (150%)
   → Consume resistencia alta
   → Alcance extendido (2.5x)
```

### Método Manual (Opcional - Para control preciso)

Si prefieres control total sobre cada punto:

#### Paso 1: Crear GameObject para puntos
```
1. Hierarchy → Create Empty → Nombrar: "ClimbPoints"
2. Este será el contenedor de todos los puntos
```

#### Paso 2: Crear marcadores individuales
```
Para cada punto de agarre en la pared:

1. ClimbPoints → Create Empty
2. Nombrar según tipo:
   - "Handhold_01" (agarre de mano)
   - "HookPoint_01" (punto de gancho)
   - "LedgeRest_01" (repisa para descansar)
   - "Foothold_01" (punto de suelo)
   - "Checkpoint_01" (checkpoint)

3. Add Component → ClimbPointMarker
4. Configurar:
   - Point Type: (seleccionar tipo)
   - Difficulty: 0.5 (ajustar según dificultad del punto)
```

#### Paso 3: Posicionar puntos
```
- Distribuir los puntos a lo largo de la pared/ruta de escalada
- Distancia recomendada: 1.5 - 2 metros entre puntos
- Colocar más puntos en áreas complejas
- Colocar HookPoints en áreas con saltos grandes
- Colocar LedgeRest en zonas de descanso estratégicas
- Colocar Footholds en superficies planas
```

#### Ejemplo de distribución manual:
```
Ruta de 30 metros de altura:

Altura 0m:   Foothold_Start (suelo inicial)
Altura 2m:   Handhold_01, Handhold_02 (inicio escalada)
Altura 5m:   Handhold_03, Handhold_04
Altura 10m:  LedgeRest_01 (descanso)
Altura 15m:  HookPoint_01 (salto grande con gancho)
Altura 20m:  Handhold_05, Handhold_06
Altura 25m:  LedgeRest_02 (descanso final)
Altura 28m:  Handhold_07
Altura 30m:  Foothold_Goal (suelo final)
```

---

## 🗺️ PATHFINDING

### Configuración de ClimbingPathfinder

```
Seleccionar ClimbingPathfinder en Hierarchy:

Pathfinding Settings:
- Max Climb Distance: 2 
  (distancia máxima entre puntos conectables)
  
- Point Spacing: 1.5
  (espaciado entre puntos generados automáticamente)
  
- Debug Mode: ☑
  (activar para ver conexiones en Scene view)

Layer Detection:
- Agarre Layer: (seleccionar layer de puntos de gancho)
- Pared Layer: (seleccionar layer de paredes escalables)
- Suelo Layer: (seleccionar layer de suelo/ground)

Tag Detection:
- Escalable Tag: "escalable"
  (tag para objetos escalables)
```

### Sistema Automático de Detección

**¡NUEVO!** El pathfinder ahora detecta automáticamente las superficies según layers y tags:

#### Layer "agarre" → HookPoints (Puntos de Gancho)
- El boss detecta automáticamente estos puntos para usar su gancho
- Conexiones a mayor distancia (2.5x maxClimbDistance)
- Color en Scene View: **Cyan**

#### Tag "escalable" o Layer "pared" → Handholds (Puntos de Escalada)
- El boss puede escalar estas superficies
- Se generan automáticamente puntos en toda la superficie
- Color en Scene View: **Verde**

#### Layer "suelo" → Footholds (Puntos de Camino)
- El boss puede caminar y correr sobre estas superficies
- Conexiones a mayor distancia (3x maxClimbDistance)
- Color en Scene View: **Azul**

### Configuración de Layers en Unity

**Paso 1: Crear las Layers necesarias**
```
Edit → Project Settings → Tags and Layers

Layers:
- User Layer 8: "agarre"
- User Layer 9: "pared"
- User Layer 10: "suelo"
```

**Paso 2: Asignar Layers a tus objetos**
```
Puntos de gancho:
- Seleccionar objeto
- Inspector → Layer → "agarre"

Paredes escalables:
- Seleccionar pared
- Inspector → Layer → "pared"
- O usar Tag: "escalable"

Suelos/plataformas:
- Seleccionar suelo
- Inspector → Layer → "suelo"
```

**Paso 3: Configurar Layer Masks en ClimbingPathfinder**
```
Seleccionar ClimbingPathfinder:
- Agarre Layer: marcar solo "agarre"
- Pared Layer: marcar solo "pared"
- Suelo Layer: marcar solo "suelo"
```

### Inicialización
El pathfinder se inicializa automáticamente cuando comienza la carrera. Para forzar la inicialización manualmente:

```
Método 1 - Context Menu:
1. Seleccionar ClimbingPathfinder en Hierarchy
2. Inspector → Botón derecho en el componente
3. Click en "Initialize Climb Points"

Método 2 - Código:
ClimbingPathfinder pathfinder = GetComponent<ClimbingPathfinder>();
pathfinder.InitializeClimbPoints();
```

### Visualización en Scene View
Con Debug Mode activado verás:
- **Esferas azules**: Puntos de suelo (Foothold) - para caminar/correr
- **Esferas verdes**: Puntos de agarre (Handhold) - para escalar
- **Esferas cyan**: Puntos de gancho (HookPoint) - para usar gancho
- **Esferas amarillas**: Repisas (LedgeRest) - para descansar
- **Esferas magenta**: Checkpoints
- **Líneas grises semi-transparentes**: Conexiones entre puntos

### Tipos de Movimiento del Boss

El boss detecta automáticamente qué tipo de acción realizar según el punto:

```csharp
// Foothold (suelo) → Caminar/Correr
if (surfaceType == SurfaceType.Ground) {
    // El boss camina o corre normalmente
    // Velocidad: baseSpeed
}

// Handhold (pared) → Escalar
if (surfaceType == SurfaceType.Climbable) {
    // El boss escala la pared
    // Velocidad: baseSpeed * 0.7
}

// HookPoint (agarre) → Usar Gancho
if (surfaceType == SurfaceType.Hookable) {
    // El boss usa el gancho para impulsarse
    // Velocidad: baseSpeed * 1.5
}
```

### Ajuste Fino de la Generación

Si hay demasiados o muy pocos puntos:

```
Point Spacing (en ClimbingPathfinder):
- Valor bajo (1.5): Más puntos, más precisión, más procesamiento
- Valor medio (2.5-3.0): Balance recomendado ✓
- Valor alto (4.0-5.0): Menos puntos, mejor rendimiento

Max Climb Distance:
- Aumentar si el boss "no encuentra caminos"
- Reducir si el boss "toma atajos imposibles"
```

### ⚠️ IMPORTANTE: Prevenir Lag al Inicializar

**ANTES de inicializar, configura:**
```
Point Spacing: 3.0 (para empezar)
Max Climb Distance: 2.5
```

**El sistema tiene límites automáticos:**
- Máximo 100 puntos por pared
- Máximo 50 puntos por suelo
- Advertencia si detecta >500 puntos totales

**Si Unity se congela al inicializar:**
1. Para Unity (Stop)
2. Aumenta Point Spacing a 4.0 o 5.0
3. Divide objetos grandes en partes más pequeñas
4. Reinicia Unity y prueba de nuevo

### Console Output (con Debug Mode)

Cuando se inicializa correctamente verás:
```
=== Iniciando detección automática de puntos de escalada ===
→ Detectados 5 puntos de gancho (HookPoints)
→ Generados 127 puntos de escalada (Handholds)
→ Generados 45 puntos de camino (Ground)
→ Creadas 324 conexiones entre puntos
✓ ClimbingPathfinder inicializado: 177 puntos detectados
  • Handholds (escalada): 127
  • HookPoints (gancho): 5
  • LedgeRest (repisas): 0
  • Footholds (suelo): 45
```

---

## 🎮 CONFIGURACIÓN DEL BOSS RACE MANAGER

### Configuración Completa

```
Seleccionar BossRaceManager:

Race Configuration:
- Race Start Point: (arrastrar RaceStartPoint)
- Race Goal Point: (arrastrar RaceGoalPoint)
- Race Start Delay: 3 (segundos de cuenta regresiva)
- Auto Start Race: ☐ (dejar desactivado para control manual)

Boss Setup:
- Active Boss: (se configurará automáticamente)
- Available Bosses: 
  Size: 4
  Element 0: Boss1_Novice
  Element 1: Boss2_Tactical
  Element 2: Boss3_Learner
  Element 3: Boss4_Professional
- Current Boss Index: 0

Player Reference:
- Player Transform: (arrastrar al jugador)
- Player Controller: (arrastrar al jugador)

UI References:
- (configuradas anteriormente en sección de UI)

Race Progress Tracking:
- Checkpoint Radius: 3
- Checkpoints: (se generan automáticamente o añadir manualmente)
```

---

## 🧪 TESTING Y AJUSTES

### Fase 1: Test Individual de Bosses

#### Test Boss 1 (Novato)
```
1. Desactivar Boss2, Boss3, Boss4
2. Activar solo Boss1_Novice
3. En BossRaceManager: Current Boss Index = 0
4. Play
5. Presionar en Inspector de BossRaceManager el botón "Start Race Countdown"

Verificar:
☑ El boss comete errores visibles
☑ Se mueve lento
☑ Descansa frecuentemente
☑ A veces toma rutas subóptimas
```

#### Test Boss 2 (Táctico)
```
Similar al anterior pero:
- Current Boss Index = 1

Verificar:
☑ Evalúa rutas cada 5 segundos
☑ Se adapta a tu posición
☑ Cambia velocidad según contexto
☑ Toma decisiones más inteligentes
```

#### Test Boss 3 (Aprendiz)
```
Similar al anterior pero:
- Current Boss Index = 2

Verificar:
☑ Mejora con el tiempo
☑ Console muestra estadísticas de aprendizaje
☑ Tasa de éxito aumenta
☑ Se adapta a errores
```

#### Test Boss 4 (Profesional)
```
Similar al anterior pero:
- Current Boss Index = 3

Verificar:
☑ Movimientos precisos
☑ Usa secuencias predefinidas
☑ Predice tu movimiento
☑ Timing casi perfecto
☑ No comete errores
```

### Fase 2: Test de Carrera Completa

```
1. Activar todos los bosses
2. Auto Start Race: ☑
3. Play
4. Completar carrera

Verificar:
☑ Countdown funciona
☑ Boss y jugador inician simultáneamente
☑ Progreso se actualiza en UI
☑ Checkpoints se detectan
☑ Ganador se determina correctamente
☑ Panel de resultados aparece
☑ Botones de reinicio/siguiente funcionan
```

### Fase 3: Ajustes de Balance

#### Si el Boss es muy fácil:
```
Aumentar:
- AI Intelligence (+0.1)
- Speed Modifier (+0.1)
Reducir:
- Decision Delay (-0.1)
```

#### Si el Boss es muy difícil:
```
Reducir:
- AI Intelligence (-0.1)
- Speed Modifier (-0.1)
Aumentar:
- Decision Delay (+0.1)
- Mistake Chance (+0.1) (solo Boss 1 y 2)
```

#### Ajuste de Resistencia:
```
Si el boss se cansa muy rápido:
- Max Stamina: aumentar a 120-150
- Stamina Drain Rate: reducir a 3-4

Si el boss nunca se cansa:
- Max Stamina: reducir a 80
- Stamina Drain Rate: aumentar a 7-8
```

---

## 🐛 TROUBLESHOOTING

### Problema: Boss no se mueve
```
Soluciones:
1. Verificar que el Rigidbody no esté en Kinematic
2. Verificar que Race Started = true
3. Verificar que Goal Point esté asignado
4. Verificar que Pathfinder esté inicializado
5. Console: buscar errores de pathfinding
```

### Problema: Pathfinder no encuentra rutas
```
Soluciones:
1. Verificar que las layers están configuradas correctamente:
   - Edit → Project Settings → Tags and Layers
   - Crear layers: "agarre", "pared", "suelo"
   
2. Verificar que los objetos tienen las layers asignadas:
   - Puntos de gancho → Layer "agarre"
   - Paredes → Layer "pared" o Tag "escalable"
   - Suelos → Layer "suelo"
   
3. Verificar Layer Masks en ClimbingPathfinder:
   - Agarre Layer debe estar marcado
   - Pared Layer debe estar marcado
   - Suelo Layer debe estar marcado
   
4. Ejecutar InitializeClimbPoints() manualmente:
   - ClimbingPathfinder → Context Menu → Initialize Climb Points
   
5. Verificar Max Climb Distance (aumentar a 3-4)

6. Verificar Point Spacing:
   - Reducir si hay muy pocos puntos (1.0)
   - Aumentar si hay demasiados puntos (2.5)
   
7. Activar Debug Mode y verificar en Scene View:
   - Deben aparecer esferas de colores
   - Deben haber líneas grises conectándolas
   
8. Console: verificar el log de inicialización:
   - Debe mostrar puntos detectados
   - Debe mostrar conexiones creadas
```

### Problema: Boss atraviesa paredes
```
Soluciones:
1. Añadir colliders a las paredes
2. Ajustar collision detection del Rigidbody a Continuous
3. Aumentar Drag del Rigidbody a 3-4
4. Reducir Base Speed
```

### Problema: UI no aparece
```
Soluciones:
1. Verificar que Canvas esté en modo Screen Space - Overlay
2. Verificar que RaceResultsPanel esté desactivado al inicio
3. Verificar referencias en BossRaceManager
4. Verificar que TextMeshPro esté importado
```

### Problema: Carrera no inicia
```
Soluciones:
1. Verificar Player Reference en BossRaceManager
2. Verificar que Active Boss esté asignado
3. Llamar manualmente StartRaceCountdown()
4. Verificar que los puntos de inicio/meta existan
```

### Problema: Boss3 no aprende
```
Soluciones:
1. Verificar Enable Learning = ☑
2. Aumentar Learning Rate a 0.2-0.3
3. Correr varias carreras (el aprendizaje es acumulativo)
4. Console: verificar logs de aprendizaje
```

### Problema: Boss4 no usa secuencias
```
Soluciones:
1. Verificar Use Predefined Sequence = ☑
2. Generar secuencias manualmente (requiere edición de código)
3. O dejar que use GOAP avanzado (muy efectivo igual)
```

---

## 🎯 CONSEJOS AVANZADOS

### Optimización de Performance
```
Si tienes lag con múltiples bosses:

1. Reducir Debug Mode en producción
2. Aumentar Decision Delay a 0.5-0.8
3. Reducir Route Alternatives (Boss 2) a 2
4. Reducir Max Memory Size (Boss 3) a 30
5. Usar Object Pooling para efectos visuales
```

### Personalización de Secuencias (Boss 4)
```csharp
// En Boss4Professional.GenerateOptimalSequences()
// Personalizar según tu nivel:

fastSequence.steps.Add(new SequenceStep
{
    actionType = SequenceActionType.Move,
    targetPosition = new Vector3(10, 5, 0), // Tu posición específica
    duration = 2f,
    startTime = 0f,
    requiresPrecision = false
});

fastSequence.steps.Add(new SequenceStep
{
    actionType = SequenceActionType.Hook,
    targetPosition = new Vector3(15, 10, 0), // Punto de gancho
    duration = 1.5f,
    startTime = 2f,
    requiresPrecision = true
});

// Continuar añadiendo pasos...
```

### Añadir Animator
```
1. Importar modelo 3D con animaciones
2. Reemplazar Capsule con tu modelo
3. Add Component → Animator
4. Crear Animator Controller con estados:
   - Idle
   - Walking
   - Climbing
   - Hooking
   - Resting
   
5. Parámetros:
   - IsClimbing (Bool)
   - IsMoving (Bool)
   - IsResting (Bool)
   - Speed (Float)
```

### Añadir Efectos Visuales
```
Sugerencias:
- Trail Renderer en el gancho
- Particle System para polvo al escalar
- Line Renderer para visualizar ruta planeada
- Glow effect cuando el boss está en "sprint"
```

---

## ✅ CHECKLIST FINAL

Antes de dar por terminada la configuración:

**Escena:**
- [ ] RaceStartPoint posicionado
- [ ] RaceGoalPoint posicionado
- [ ] ClimbingPathfinder configurado
- [ ] ClimbPoints distribuidos en la ruta
- [ ] Paredes tienen tag "escalable"

**Bosses:**
- [ ] Boss1_Novice configurado
- [ ] Boss2_Tactical configurado
- [ ] Boss3_Learner configurado
- [ ] Boss4_Professional configurado
- [ ] Todos tienen Rigidbody y Collider
- [ ] Referencias asignadas (Start Point, Goal Point, Pathfinder)

**UI:**
- [ ] Canvas creado
- [ ] CountdownText funciona
- [ ] RaceStatusText funciona
- [ ] PlayerProgressText funciona
- [ ] BossProgressText funciona
- [ ] RaceResultsPanel funciona
- [ ] Botones conectados

**BossRaceManager:**
- [ ] Todas las referencias asignadas
- [ ] Available Bosses lista completa
- [ ] Player Reference asignado
- [ ] UI References asignadas

**Testing:**
- [ ] Cada boss testeado individualmente
- [ ] Carrera completa funciona
- [ ] UI se actualiza correctamente
- [ ] Ganador se determina bien
- [ ] No hay errores en Console

---

## 📞 SOPORTE

Si encuentras problemas no cubiertos aquí:

1. **Revisar Console** - Los scripts tienen muchos Debug.Log útiles
2. **Activar Debug Mode** - En ClimbingPathfinder para visualizar
3. **Verificar Scene View** - Con Gizmos activados
4. **Revisar referencias** - La mayoría de problemas son referencias faltantes

---

## 🎓 EXPLICACIÓN TÉCNICA

### IA Utilizada por cada Boss:

**Boss 1 - Novato:**
- GOAP básico con alta tasa de error
- Acciones limitadas (Move, Climb, Rest)
- No usa gancho
- Errores simulados: movimientos incorrectos, descansos innecesarios

**Boss 2 - Táctico:**
- GOAP + Evaluación de rutas múltiples
- A* Pathfinding con diferentes parámetros
- Decisiones basadas en contexto (resistencia, posición jugador)
- Adaptación dinámica de velocidad

**Boss 3 - Aprendiz:**
- GOAP + Q-Learning simplificado
- Imitation Learning a través de memoria de acciones
- Sistema de recompensas
- Mejora continua con exploración/explotación

**Boss 4 - Profesional:**
- GOAP avanzado + Secuencias predefinidas
- Timing perfecto (cronometrado)
- Predicción de movimiento del jugador
- Optimización de resistencia
- Sin errores

---

## 🎮 CÓMO JUGAR

**Para iniciar una carrera:**
1. Play
2. Presiona botón UI "Start Race" O
3. En Inspector: BossRaceManager → Métodos → StartRaceCountdown()

**Controles del jugador:** (tus controles existentes)
- WASD: Movimiento
- E: Escalar
- Space: Saltar
- Mouse: Gancho

**Objetivo:**
- Llegar a la meta antes que el boss
- Completar checkpoints
- Gestionar resistencia

---

¡Sistema de Bosses completamente configurado! 🎉

**Creado para ESCALATOPIA**
**Fecha:** Diciembre 2025
**Versión:** 1.0
