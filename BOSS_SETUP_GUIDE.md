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

### Método 1: Puntos Manuales (Recomendado)

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
```

#### Ejemplo de distribución:
```
Ruta de 30 metros de altura:

Altura 0m:   Checkpoint_Start
Altura 5m:   Handhold_01, Handhold_02
Altura 10m:  LedgeRest_01 (descanso)
Altura 15m:  HookPoint_01 (salto grande)
Altura 20m:  Handhold_03, Handhold_04
Altura 25m:  LedgeRest_02
Altura 30m:  Checkpoint_Goal
```

### Método 2: Generación Automática

```csharp
// Script opcional para generar puntos automáticamente
// Añadir a ClimbingPathfinder y llamar en Start() o desde botón custom

public void GenerateClimbPointsAuto()
{
    // Buscar todas las paredes con tag "escalable"
    GameObject[] walls = GameObject.FindGameObjectsWithTag("escalable");
    
    foreach (GameObject wall in walls)
    {
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer == null) continue;
        
        Bounds bounds = renderer.bounds;
        
        // Generar grid de puntos
        for (float y = bounds.min.y; y < bounds.max.y; y += 2f)
        {
            for (float x = bounds.min.x; x < bounds.max.x; x += 1.5f)
            {
                Vector3 position = new Vector3(x, y, bounds.center.z);
                
                // Crear punto
                GameObject point = new GameObject("AutoPoint");
                point.transform.position = position;
                ClimbPointMarker marker = point.AddComponent<ClimbPointMarker>();
                marker.pointType = ClimbPointType.Handhold;
                marker.difficulty = Random.Range(0.3f, 0.7f);
            }
        }
    }
}
```

---

## 🗺️ PATHFINDING

### Configuración de ClimbingPathfinder

```
Seleccionar ClimbingPathfinder en Hierarchy:

Pathfinding Settings:
- Max Climb Distance: 2 
  (distancia máxima entre puntos conectables)
  
- Climbable Layer: Default
  (o tu layer personalizada para paredes)
  
- Debug Mode: ☑
  (activar para ver conexiones en Scene view)
```

### Inicialización
El pathfinder se inicializa automáticamente cuando comienza la carrera, pero puedes forzar la inicialización:

```csharp
// En el Inspector del ClimbingPathfinder, crear botón custom:
[Button("Initialize Climb Points")]
public void InitializeClimbPoints() { ... }
```

### Visualización en Scene View
Con Debug Mode activado verás:
- **Esferas verdes**: Puntos de agarre (Handhold)
- **Esferas cyan**: Puntos de gancho (HookPoint)
- **Esferas amarillas**: Repisas (LedgeRest)
- **Esferas magenta**: Checkpoints
- **Líneas grises**: Conexiones entre puntos

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
1. Verificar que hay ClimbPointMarkers en la escena
2. Ejecutar InitializeClimbPoints() manualmente
3. Verificar Max Climb Distance (aumentar a 3-4)
4. Activar Debug Mode y verificar conexiones
5. Verificar que los puntos estén dentro del rango
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
