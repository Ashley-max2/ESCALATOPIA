# RESUMEN TÉCNICO - SISTEMA DE BOSSES
## ESCALATOPIA - Unity 3D

---

## 📊 ARQUITECTURA DEL SISTEMA

### Componentes Principales

```
BossAIBase (Clase abstracta)
    ├── GOAP System (Goal-Oriented Action Planning)
    │   ├── GOAPPlanner
    │   └── GOAPAction (acciones disponibles)
    │
    ├── Pathfinding
    │   └── ClimbingPathfinder (A* para escalada)
    │
    └── State Machine
        ├── Idle
        ├── Moving
        ├── Climbing
        ├── Hooking
        └── Resting

Boss1Novice : BossAIBase
    └── IA simple con errores frecuentes

Boss2Tactical : BossAIBase
    └── Evaluación de rutas múltiples + adaptación

Boss3Learner : BossAIBase
    └── Q-Learning + Imitation Learning

Boss4Professional : BossAIBase
    └── Secuencias predefinidas + predicción

BossRaceManager
    └── Control de carreras y UI
```

---

## 🧠 TÉCNICAS DE IA IMPLEMENTADAS

### 1. GOAP (Goal-Oriented Action Planning)
**Usado por:** Todos los bosses

**Descripción:**
- Sistema de planificación de acciones orientado a objetivos
- Evalúa precondiciones y efectos de cada acción
- Genera planes dinámicos para alcanzar objetivos

**Implementación:**
```
Objetivo: "Llegar a la meta"
Acciones disponibles:
  - MoveToClimbPoint (costo: 1.0)
  - ClimbAction (costo: 1.5)
  - UseHookAction (costo: 2.0)
  - RestAction (costo: 3.0)

El planificador encuentra la secuencia de menor costo que:
  Estado Actual → [Acciones] → Estado Objetivo
```

### 2. A* Pathfinding
**Usado por:** Todos los bosses (ClimbingPathfinder)

**Descripción:**
- Algoritmo de búsqueda de caminos en grafos
- Encuentra la ruta más corta entre dos puntos
- Considera dificultad de cada punto

**Heurística:**
```csharp
H(n) = distancia_euclidiana + (diferencia_altura * 0.5)
Costo = distancia_base + (dificultad_punto * modificador)
```

### 3. Imitation Learning (Boss 3)
**Implementación:**
- Memoria de acciones exitosas/fallidas
- Sistema de recompensas basado en:
  - Progreso hacia la meta
  - Eficiencia de resistencia
  - Tiempo de ejecución
  - Éxito de la acción

**Fórmula de aprendizaje:**
```
tasa_nueva = tasa_actual + α * (recompensa - tasa_actual)
donde α = learning_rate = 0.1
```

### 4. Q-Learning Simplificado (Boss 3)
**Implementación:**
```
Q(s,a) = Q(s,a) + α[r + γ·max(Q(s',a')) - Q(s,a)]

donde:
s = estado actual
a = acción
r = recompensa
γ = discount_factor = 0.9
α = learning_rate = 0.1
s' = siguiente estado
```

**Estados considerados:**
- Tiene resistencia / No tiene resistencia
- En pared / No en pared
- Punto de gancho disponible / No disponible
- Cerca de meta / Lejos de meta

### 5. Secuencias Predefinidas (Boss 4)
**Descripción:**
- Acciones cronometradas con timing perfecto
- Secuencias optimizadas previamente
- Ejecución determinista

**Estructura:**
```csharp
SequenceStep {
    actionType: Move/Climb/Hook/Rest/Sprint/WallJump
    targetPosition: Vector3
    duration: float
    startTime: float
    requiresPrecision: bool
}
```

### 6. Predicción de Movimiento (Boss 4)
**Implementación:**
```csharp
posicion_predicha = posicion_actual + (velocidad * tiempo_prediccion)

Ajuste de estrategia:
if (distancia_jugador_meta < distancia_boss_meta):
    velocidad_boss *= 1.4
```

---

## 🎮 DIFERENCIACIÓN DE BOSSES

### Boss 1: El Novato
```
Dificultad: ⭐☆☆☆☆
IA: GOAP básico
Características:
  - Inteligencia: 30%
  - Velocidad: 70% de la base
  - Errores: 40% de probabilidad
  - Acciones: Move, Climb, Rest (no usa Hook)
  - Decisiones: Lentas (0.8s delay)
```

### Boss 2: El Táctico
```
Dificultad: ⭐⭐⭐☆☆
IA: GOAP + Multi-route evaluation
Características:
  - Inteligencia: 60%
  - Velocidad: 100% de la base
  - Evalúa 3 rutas alternativas
  - Se adapta al jugador
  - Balance: Velocidad 60% / Seguridad 40%
```

### Boss 3: El Aprendiz
```
Dificultad: ⭐⭐⭐⭐☆
IA: GOAP + Q-Learning + Imitation Learning
Características:
  - Inteligencia: 75% (aumenta con aprendizaje)
  - Velocidad: 110% de la base
  - Memoria: 50 acciones
  - Exploración: 20%
  - Mejora continua entre carreras
```

### Boss 4: El Profesional
```
Dificultad: ⭐⭐⭐⭐⭐
IA: Secuencias + GOAP avanzado + Predicción
Características:
  - Inteligencia: 95%
  - Velocidad: 120% de la base
  - Precisión: 95%
  - Sin errores
  - Predice movimiento del jugador
  - Optimización perfecta de resistencia
```

---

## 📈 VARIABLES AJUSTABLES

### Variables de IA Comunes

| Variable | Rango | Descripción |
|----------|-------|-------------|
| aiIntelligence | 0-1 | Inteligencia general del boss |
| speedModifier | 0.5-2 | Multiplicador de velocidad |
| decisionDelay | 0-2 | Retraso en toma de decisiones (s) |
| allowMistakes | bool | Permite errores ocasionales |

### Variables de Resistencia

| Variable | Valor Típico | Descripción |
|----------|--------------|-------------|
| maxStamina | 80-150 | Resistencia máxima |
| staminaDrainRate | 3-8 | Consumo por segundo |
| staminaRecoveryRate | 10-15 | Recuperación por segundo |

### Variables Específicas por Boss

**Boss1:**
- mistakeChance: 0.4 (40%)
- slowdownFactor: 0.7

**Boss2:**
- routeEvaluationInterval: 5s
- routeAlternatives: 3
- speedPreference: 0.6

**Boss3:**
- learningRate: 0.1
- maxMemorySize: 50
- explorationRate: 0.2

**Boss4:**
- sequencePrecision: 0.95
- reactionTime: 0.1
- perfectTiming: true

---

## 🔄 FLUJO DE EJECUCIÓN

### 1. Inicio de Carrera
```
BossRaceManager.StartRace()
    ↓
Boss.StartRace()
    ↓
Pathfinder.FindPath(inicio, meta)
    ↓
GOAP inicializado
    ↓
Carrera activa
```

### 2. Loop de Update (cada frame)
```
Boss.Update()
    ↓
UpdateStamina()
    ↓
CheckIfStuck()
    ↓
ExecuteGOAP()
    │   ↓
    │   UpdateWorldState()
    │   ↓
    │   Planner.Plan() → Queue<Actions>
    │   ↓
    │   ExecuteAction()
    ↓
UpdateAnimation()
```

### 3. Ejecución de GOAP
```
1. Actualizar estado del mundo
   - ¿Tiene resistencia?
   - ¿Está en pared?
   - ¿Hay gancho disponible?
   - ¿Está en meta?

2. Verificar si hay plan actual
   - Si no → Crear nuevo plan
   - Si sí → Continuar ejecución

3. Ejecutar acción actual
   - Perform()
   - Verificar IsComplete()
   - Si completa → Siguiente acción

4. Aplicar modificadores de IA
   - Retraso en decisiones
   - Posibilidad de error
```

### 4. Finalización de Carrera
```
CheckRaceCompletion()
    ↓
Jugador o Boss llega a meta
    ↓
EndRace()
    ↓
Determinar ganador
    ↓
ShowResults()
    ↓
BossRaceManager.StopRace()
```

---

## 🎯 REQUISITOS CUMPLIDOS

### ✅ IA Avanzada
- [x] **GOAP** - Goal-Oriented Action Planning
- [x] **Machine Learning** - Q-Learning (Boss 3)
- [x] **Imitation Learning** - Sistema de memoria (Boss 3)

### ✅ Selección de Camino
- [x] **A* Pathfinding** - Rutas óptimas
- [x] **Modificadores** - Inteligencia y velocidad ajustables
- [x] **Adaptación** - Rutas dinámicas según contexto

### ✅ Variabilidad
- [x] **Múltiples rutas** - Boss 2 evalúa 3 alternativas
- [x] **Decisiones dinámicas** - No siempre la misma ruta
- [x] **Aprendizaje** - Boss 3 varía según experiencia
- [x] **Exploración** - Boss 3 prueba nuevas acciones

### ✅ Secuencias Fijas (Boss 4)
- [x] **Timing predefinido** - SequenceSteps con startTime
- [x] **Acciones coherentes** - Secuencia lógica
- [x] **Precisión perfecta** - 95% de accuracy

---

## 🧪 TESTING RECOMENDADO

### Tests Funcionales

1. **Test de IA Individual**
   - Cada boss completa la carrera solo
   - Verifica comportamiento específico
   - Duración: 5 min por boss

2. **Test de Comparación**
   - Ejecutar todos contra reloj
   - Verificar diferencia de tiempos
   - Boss4 debe ser más rápido que Boss1

3. **Test de Aprendizaje**
   - Boss3: 5 carreras consecutivas
   - Verificar mejora de estadísticas
   - Tiempo debe reducirse

4. **Test de Pathfinding**
   - Diferentes distribuciones de ClimbPoints
   - Verificar rutas generadas
   - No debe haber rutas imposibles

### Tests de Balance

1. **Dificultad Progresiva**
   ```
   Boss1 tiempo: ~60s
   Boss2 tiempo: ~45s
   Boss3 tiempo: ~35s
   Boss4 tiempo: ~25s
   ```

2. **Tasa de Victoria**
   - Boss1: Jugador gana 80%
   - Boss2: Jugador gana 50%
   - Boss3: Jugador gana 30%
   - Boss4: Jugador gana 10%

---

## 🔧 OPTIMIZACIÓN

### Performance

**Recomendaciones:**
- Decision Delay > 0.3s reduce carga CPU
- Max Memory Size < 50 (Boss3)
- Route Alternatives ≤ 3 (Boss2)
- Debug Mode OFF en build final

**Complejidad Computacional:**
```
GOAP Planning: O(n * m)
  n = número de acciones
  m = profundidad del plan

A* Pathfinding: O(b^d)
  b = factor de ramificación
  d = profundidad de la solución

Q-Learning: O(s * a)
  s = número de estados
  a = número de acciones
```

---

## 📚 EXTENSIBILIDAD

### Añadir Nuevo Boss

```csharp
public class Boss5Custom : BossAIBase
{
    protected override void InitializeGOAP()
    {
        base.InitializeGOAP();
        // Añadir acciones personalizadas
        availableActions.Add(new CustomAction());
    }

    protected override void OnRaceStart()
    {
        // Comportamiento inicial
    }

    protected override void OnRaceEnd()
    {
        // Comportamiento final
    }
}
```

### Añadir Nueva Acción GOAP

```csharp
public class CustomAction : GOAPAction
{
    public CustomAction() : base("CustomAction")
    {
        cost = 2f;
        preconditions.Add("customCondition", true);
        effects.Add("customEffect", true);
    }

    public override bool CheckProceduralPrecondition(BossAIBase agent)
    {
        // Verificar si puede ejecutarse
        return true;
    }

    public override bool Perform(BossAIBase agent)
    {
        // Ejecutar acción
        return true;
    }

    public override bool IsComplete(BossAIBase agent)
    {
        // Verificar si se completó
        return true;
    }
}
```

---

## 🎓 CONCLUSIÓN

Este sistema implementa técnicas de IA avanzadas que cumplen todos los requisitos:

1. **GOAP** como sistema principal de decisión
2. **Machine Learning** a través de Q-Learning
3. **Imitation Learning** con sistema de memoria
4. **Pathfinding** con A* para selección de rutas
5. **Modificadores** de inteligencia y velocidad
6. **Variabilidad** en todas las decisiones
7. **Secuencias fijas** con timing perfecto

Cada boss ofrece un desafío único y progresivo, desde el novato predecible hasta el profesional casi perfecto.

**Total de Líneas de Código:** ~3000+
**Total de Clases:** 15+
**Técnicas de IA:** 6
**Bosses Únicos:** 4

---

**Autor:** GitHub Copilot
**Fecha:** Diciembre 2025
**Proyecto:** ESCALATOPIA
**Versión:** 1.0
