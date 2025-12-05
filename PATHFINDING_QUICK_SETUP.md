# 🗺️ SISTEMA DE PATHFINDING AUTOMÁTICO - GUÍA RÁPIDA
## ClimbingPathfinder con Detección por Layers

---

## ⚡ CONFIGURACIÓN RÁPIDA (5 minutos)

### Paso 1: Crear Layers en Unity
```
Edit → Project Settings → Tags and Layers

Añadir estas User Layers:
- User Layer 8: "agarre"
- User Layer 9: "pared"
- User Layer 10: "suelo"
```

### Paso 2: Asignar Layers a tus objetos

**Puntos de gancho:**
- Selecciona los objetos donde el boss puede usar el gancho
- Inspector → Layer → **"agarre"**

**Paredes escalables:**
- Selecciona las paredes/superficies escalables
- Inspector → Layer → **"pared"**
- O usa Tag → **"escalable"**

**Suelos/Plataformas:**
- Selecciona superficies caminables
- Inspector → Layer → **"suelo"**

### Paso 3: Configurar ClimbingPathfinder

```
Seleccionar ClimbingPathfinder en Hierarchy:

Layer Detection:
✓ Agarre Layer: marcar solo "agarre"
✓ Pared Layer: marcar solo "pared"
✓ Suelo Layer: marcar solo "suelo"

Pathfinding Settings:
✓ Max Climb Distance: 2.0
✓ Point Spacing: 1.5
✓ Debug Mode: ☑ (activar)
```

### Paso 4: Probar

1. **Play**
2. El sistema detecta automáticamente todas las superficies
3. En Scene View verás:
   - 🔵 Puntos azules = Suelo (caminar)
   - 🟢 Puntos verdes = Paredes (escalar)
   - 🔷 Puntos cyan = Ganchos (impulso)
   - ⚪ Líneas grises = Conexiones

---

## 🎯 CÓMO FUNCIONA

### Detección Automática por Layer/Tag

| Layer/Tag | Tipo de Punto | Acción del Boss | Color Debug |
|-----------|--------------|-----------------|-------------|
| Layer "agarre" | HookPoint | Usa gancho (impulso rápido) | Cyan 🔷 |
| Layer "pared" o Tag "escalable" | Handhold | Escala la superficie | Verde 🟢 |
| Layer "suelo" | Foothold | Camina/corre | Azul 🔵 |

### Conexiones Inteligentes

El sistema conecta automáticamente puntos cercanos con distancias adaptativas:

- **Foothold → Foothold**: 3x maxDistance (correr en suelo)
- **HookPoint → Cualquiera**: 2.5x maxDistance (alcance del gancho)
- **Foothold → Handhold**: 1.5x maxDistance (inicio de escalada)
- **Handhold → Handhold**: 1x maxDistance (escalada normal)

### Generación de Puntos

El sistema genera automáticamente una malla de puntos en cada superficie:

1. **Detecta objetos** por layer/tag
2. **Escanea la superficie** con Raycasts
3. **Genera puntos** según `Point Spacing`
4. **Crea conexiones** según distancias y línea de visión
5. **Listo para pathfinding** A*

---

## 🐛 SOLUCIÓN RÁPIDA DE PROBLEMAS

### ❌ Boss no se mueve / No encuentra camino

**Verificar:**
1. ¿Layers creadas? → Edit → Project Settings → Tags and Layers
2. ¿Objetos tienen layers asignadas? → Inspector → Layer
3. ¿Layer Masks configurados en ClimbingPathfinder?
4. ¿Debug Mode activado? → Deben verse esferas de colores en Scene View

**Ejecutar manualmente:**
```
ClimbingPathfinder → Context Menu (botón derecho) → Initialize Climb Points
```

**Revisar Console:**
```
Debe mostrar:
✓ ClimbingPathfinder inicializado: X puntos detectados
  • Handholds (escalada): X
  • HookPoints (gancho): X
  • Footholds (suelo): X
```

### ❌ Muy pocos puntos detectados

**Solución:**
- Reducir `Point Spacing` de 1.5 a **1.0**
- Aumentar `Max Climb Distance` a **3.0**

### ❌ Demasiados puntos (lag/rendimiento)

**Solución:**
- Aumentar `Point Spacing` de 1.5 a **2.5**
- Reducir `Max Climb Distance` a **1.5**
- Desactivar `Debug Mode` en producción

### ❌ Boss atraviesa paredes

**Solución:**
1. Verificar que las paredes tienen **Colliders**
2. Rigidbody del Boss → Collision Detection: **Continuous**
3. Aumentar Drag del Rigidbody a **3-4**

---

## 📊 LEYENDA DE COLORES (Debug Mode)

Cuando Debug Mode está activado, verás en Scene View:

| Color | Tipo de Punto | Significado |
|-------|--------------|-------------|
| 🔵 **Azul** | Foothold | Superficie caminable (suelo) |
| 🟢 **Verde** | Handhold | Superficie escalable (pared) |
| 🔷 **Cyan** | HookPoint | Punto de gancho (agarre) |
| 🟡 **Amarillo** | LedgeRest | Repisa para descansar |
| 🟣 **Magenta** | Checkpoint | Punto de control |
| ⚪ **Gris** | Conexión | Línea entre puntos conectados |

---

## 🎮 COMPORTAMIENTO DEL BOSS

El boss adapta automáticamente su movimiento según el tipo de punto:

### En Foothold (Suelo) 🔵
```
✓ Camina o corre
✓ Velocidad: 100% (baseSpeed)
✓ Resistencia: Bajo consumo
✓ Conexiones: Largas (3x)
```

### En Handhold (Pared) 🟢
```
✓ Escala la superficie
✓ Velocidad: 70% (baseSpeed * 0.7)
✓ Resistencia: Consumo medio
✓ Conexiones: Normales (1x)
```

### En HookPoint (Gancho) 🔷
```
✓ Usa gancho para impulsarse
✓ Velocidad: 150% (baseSpeed * 1.5)
✓ Resistencia: Alto consumo
✓ Conexiones: Extra largas (2.5x)
✓ Requiere: Sistema de gancho activo
```

---

## 🔧 AJUSTES AVANZADOS

### Densidad de Puntos

```csharp
// En ClimbingPathfinder Inspector:

Point Spacing:
  1.0 = Muy denso (más precisión, más lag)
  1.5 = Balance recomendado ✓
  2.0 = Normal
  2.5 = Espaciado (menos puntos, mejor performance)
  3.0 = Muy espaciado (solo para áreas grandes)
```

### Alcance de Conexiones

```csharp
Max Climb Distance:
  1.5 = Escalada muy precisa, movimientos cortos
  2.0 = Balance recomendado ✓
  2.5 = Alcance normal
  3.0 = Alcance extendido
  4.0 = Alcance muy largo (permite "saltos" grandes)
```

### Capas de Detección

```csharp
// Puedes combinar múltiples layers en cada LayerMask:

Agarre Layer:
  - Puede incluir: "agarre", "HookPoint", etc.

Pared Layer:
  - Puede incluir: "pared", "Wall", "Cliff", etc.

Suelo Layer:
  - Puede incluir: "suelo", "Ground", "Floor", "Platform", etc.
```

---

## ✅ CHECKLIST DE CONFIGURACIÓN

Antes de usar el sistema de bosses:

**Unity Layers:**
- [ ] Layer "agarre" creada
- [ ] Layer "pared" creada
- [ ] Layer "suelo" creada

**Objetos de Escena:**
- [ ] Puntos de gancho tienen Layer "agarre"
- [ ] Paredes tienen Layer "pared" o Tag "escalable"
- [ ] Suelos tienen Layer "suelo"

**ClimbingPathfinder:**
- [ ] Agarre Layer configurado
- [ ] Pared Layer configurado
- [ ] Suelo Layer configurado
- [ ] Max Climb Distance = 2.0
- [ ] Point Spacing = 1.5
- [ ] Debug Mode activado (para testing)

**Testing:**
- [ ] Play → Scene View muestra puntos de colores
- [ ] Console muestra puntos detectados
- [ ] Boss puede moverse entre puntos
- [ ] Conexiones visibles en Scene View

---

## 🎓 VENTAJAS DEL SISTEMA AUTOMÁTICO

✅ **Sin marcadores manuales** - No necesitas crear GameObjects para cada punto
✅ **Detección inteligente** - Distingue automáticamente entre suelo, pared y gancho
✅ **Adaptativo** - Se ajusta a cambios en el nivel
✅ **Eficiente** - Solo genera puntos en superficies válidas
✅ **Escalable** - Funciona con niveles de cualquier tamaño
✅ **Visual** - Debug Mode muestra toda la información
✅ **Compatible con GOAP** - Se integra perfectamente con la IA de los bosses

---

## 🚀 PRÓXIMOS PASOS

Después de configurar el pathfinding:

1. **Configurar BossRaceManager** → Ver BOSS_SETUP_GUIDE.md sección "Configuración de Escena"
2. **Crear Bosses** → Ver BOSS_SETUP_GUIDE.md sección "Configuración de Bosses"
3. **Configurar UI** → Ver BOSS_SETUP_GUIDE.md sección "Sistema de UI"
4. **Testing** → Ver BOSS_SETUP_GUIDE.md sección "Testing y Ajustes"

---

**Creado para ESCALATOPIA**
**Sistema de Pathfinding Automático v2.0**
**Compatible con GOAP AI + Machine Learning Bosses**
