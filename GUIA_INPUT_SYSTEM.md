# GUÍA COMPLETA: Integrar Input System (Gamepad) sin romper Teclado/Ratón

> ⚠️ **IMPORTANTE**: Esta guía está diseñada para añadir soporte de gamepad SIN romper tu sistema actual de teclado/ratón. Ambos funcionarán en paralelo.

## 📋 ESTADO ACTUAL

✅ **Scripts ya creados y listos:**
- `GamePadInputBridge.cs` - Versión básica que compila sin errores
- `GamepadInputBridge_COMPLETE.cs.txt` - Versión completa para usar DESPUÉS de instalar Input System
- `MainMenuGamepadNavigation.cs` - Navegación gamepad en menús
- `PauseMenuGamepadNavigation.cs` - Navegación gamepad en pausa
- Scripts de cámara actualizados con soporte para gamepad

✅ **Lo que ya funciona:**
- Tu sistema actual de teclado/ratón sigue funcionando 100%
- Los scripts están listos para activarse cuando instales Input System

---

## ✅ PASO 0: Preparar Unity

1. **Instalar Input System:**
   - `Window > Package Manager`
   - Cambia a `Unity Registry`
   - Busca `Input System`
   - Click `Install`
   - **ACEPTA el reinicio cuando lo pida**

2. **Configurar Input Handling:**
   - `Edit > Project Settings > Player`
   - Busca `Active Input Handling`
   - Selecciona `Both` (permite usar Input antiguo Y nuevo)
   - ⚠️ **IMPORTANTE**: NO selecciones "Input System Package (New)" o romperás el teclado/ratón

3. **Activar el script completo:**
   - Ve a `Assets/Scripts/Controles/`
   - **OPCIÓN A**: Abre `GamepadInputBridge_COMPLETE.cs.txt`, copia todo el contenido
   - Abre `GamePadInputBridge.cs` y pega el contenido (reemplazando todo)
   - **OPCIÓN B**: Renombra `GamePadInputBridge.cs` a `GamePadInputBridge_OLD.cs`
   - Renombra `GamepadInputBridge_COMPLETE.cs.txt` a `GamePadInputBridge.cs`

---

## 📁 PASO 1: Crear Input Actions Asset

1. En la ventana `Project`:
   - Click derecho en `Assets`
   - `Create > Input Actions`
   - Nómbralo: `GamepadControls`

2. Doble click en `GamepadControls` para abrirlo

---

## 🎮 PASO 2: Configurar Action Maps y Acciones

### 2.1 Crear Action Maps

En el panel izquierdo del editor de Input Actions:

1. Click `+` junto a "Action Maps"
2. Crea: `Gameplay`
3. Crea: `UI`

### 2.2 Configurar GAMEPLAY Actions

Selecciona `Gameplay` y añade estas acciones:

#### **Move** (Movimiento)
- Action Type: `Value`
- Control Type: `Vector2`
- Binding:
  - Click `+` > `Add Binding`
  - Click en el binding > `Listen`
  - Mueve el **Left Stick** del mando
  - Debería quedar: `<Gamepad>/leftStick`
- Processor (opcional):
  - Click en el binding > `Add Processor` > `Stick Deadzone`

#### **Look** (Cámara)
- Action Type: `Value`
- Control Type: `Vector2`
- Binding:
  - `<Gamepad>/rightStick`
- Processors (recomendado):
  - `Stick Deadzone` (min: 0.125)
  - `Scale Vector2` (X: 2, Y: 2) para sensibilidad

#### **Jump** (Saltar)
- Action Type: `Button`
- Binding: `<Gamepad>/buttonSouth` (botón A)

#### **Run** (Correr)
- Action Type: `Button`
- Binding: `<Gamepad>/leftShoulder` (LB)

#### **Climb** (Escalar)
- Action Type: `Button`
- Binding: `<Gamepad>/rightShoulder` (RB)

#### **Grapple** (Gancho)
- Action Type: `Button`
- Binding: `<Gamepad>/rightTrigger` (RT)

#### **AimToggle** (Primera Persona - Mantener)
- Action Type: `Button`
- Binding: `<Gamepad>/leftTrigger` (LT)

#### **ToggleCamera** (Alternar Cámara)
- Action Type: `Button`
- Binding: `<Gamepad>/buttonNorth` (Y)

#### **Pause** (Pausar)
- Action Type: `Button`
- Binding: `<Gamepad>/start`

**Guarda el asset: `Ctrl+S`**

### 2.3 Configurar UI Actions

Selecciona `UI` y añade:

#### **Navigate** (Navegación menú)
- Action Type: `Value`
- Control Type: `Vector2`
- Binding 1 - Left Stick:
  - Click `+` > `Add Up/Down/Left/Right Composite`
  - Renombra a "GamepadNavigate"
  - Up: `<Gamepad>/leftStick/up`
  - Down: `<Gamepad>/leftStick/down`
  - Left: `<Gamepad>/leftStick/left`
  - Right: `<Gamepad>/leftStick/right`
- Binding 2 - D-Pad (opcional):
  - Otro composite con:
  - Up: `<Gamepad>/dpad/up`
  - Down: `<Gamepad>/dpad/down`
  - Left: `<Gamepad>/dpad/left`
  - Right: `<Gamepad>/dpad/right`

#### **Submit** (Confirmar)
- Action Type: `Button`
- Binding 1: `<Gamepad>/buttonSouth` (A)
- Binding 2 (opcional): `<Keyboard>/enter`

#### **Cancel** (Cancelar)
- Action Type: `Button`
- Binding 1: `<Gamepad>/buttonEast` (B)
- Binding 2 (opcional): `<Keyboard>/escape`

#### **Point** (Puntero)
- Action Type: `Value`
- Control Type: `Vector2`
- Binding: `<Mouse>/position`

#### **Click** (Click izquierdo)
- Action Type: `Button`
- Binding: `<Mouse>/leftButton`

#### **RightClick** (opcional)
- Action Type: `Button`
- Binding: `<Mouse>/rightButton`

#### **ScrollWheel** (opcional)
- Action Type: `Value`
- Control Type: `Vector2`
- Binding: `<Mouse>/scroll`

**Guarda: `Ctrl+S`**

---

## 🎯 PASO 3: Configurar el Jugador

### 3.1 Añadir PlayerInput al Jugador

1. Selecciona tu prefab/objeto del **jugador** en la escena
2. `Add Component` > busca `Player Input`
3. Configura:
   - **Actions**: Arrastra `GamepadControls` desde Project
   - **Default Map**: Selecciona `Gameplay`
   - **Behavior**: Selecciona `Invoke Unity Events`
   - **Control Schemes**: Déjalo vacío (detecta gamepad automáticamente)

### 3.2 Añadir GamepadInputBridge

1. En el MISMO objeto del jugador:
   - `Add Component` > busca `Gamepad Input Bridge`
2. Asigna referencias en el Inspector:
   - **Player Controller**: El PlayerController del jugador
   - **Fp Camera**: La cámara de primera persona (hijo del jugador)
   - **Tp Camera**: La cámara de tercera persona
   - **Camera Manager**: El CameraManager
   - **Hook System**: El HookSystem (hijo del jugador)
   - **Pause Menu**: Búscalo en la escena (GameObject con PauseMenu)
   - **Look Sensitivity**: 2 (ajusta al gusto)

### 3.3 Conectar Unity Events

En el componente `Player Input`:

1. Expande **Events**
2. Para cada acción, conecta al script `GamepadInputBridge`:

   - **Move** → `GamepadInputBridge.OnMove`
   - **Look** → `GamepadInputBridge.OnLook`
   - **Jump** → `GamepadInputBridge.OnJump`
   - **Run** → `GamepadInputBridge.OnRun`
   - **Climb** → `GamepadInputBridge.OnClimb`
   - **Grapple** → `GamepadInputBridge.OnGrapple`
   - **AimToggle** → `GamepadInputBridge.OnAimToggle`
   - **ToggleCamera** → `GamepadInputBridge.OnToggleCamera`
   - **Pause** → `GamepadInputBridge.OnPause`

---

## 🖥️ PASO 4: Configurar Menús (MainMenu)

### 4.1 En la escena del MainMenu:

1. Selecciona el GameObject con el script `MainMenu`
2. `Add Component` > `Main Menu Gamepad Navigation`
3. Asigna en el Inspector:
   - **Play Button**: Tu botón de jugar
   - **Config Button**: Botón de configuración
   - **Exit Button**: Botón de salir
   - **Song Button**: Botón del menú de sonido
   - **Screen Button**: Botón del menú de pantalla
   - **Gamepad Button**: Botón del menú de controles
   - **Back Button**: Botón de volver
   - **Main Menu**: El script MainMenu del mismo objeto
   - **Event System**: Se encuentra automáticamente

### 4.2 EventSystem (si existe en la escena):

1. Selecciona el `EventSystem`
2. Si tiene `Standalone Input Module`, quítalo
3. `Add Component` > `Input System UI Input Module`
4. Asigna `GamepadControls` al campo **Actions Asset**
5. Configura los mappings:
   - Point → `UI/Point`
   - Left Click → `UI/Click`
   - Submit → `UI/Submit`
   - Cancel → `UI/Cancel`
   - Navigate → `UI/Navigate`

---

## ⏸️ PASO 5: Configurar PauseMenu

1. Selecciona el GameObject con `PauseMenu`
2. `Add Component` > `Pause Menu Gamepad Navigation`
3. Asigna referencias:
   - **Resume Button**: Botón de continuar
   - **Song Button**: Botón de sonido
   - **Screen Button**: Botón de pantalla
   - **Main Menu Button**: Botón de volver al menú
   - **Pause Menu**: El script PauseMenu del mismo objeto
   - **Event System**: Se encuentra automáticamente

---

## 🔧 PASO 6: Configurar Navigation en Botones

Para CADA botón de tus menús:

1. Selecciona el botón en la jerarquía
2. En el componente `Button`:
   - Busca la sección **Navigation**
   - Cambia de `Automatic` a `Explicit`
   - Asigna manualmente:
     - **Select On Up**: El botón de arriba
     - **Select On Down**: El botón de abajo

---

## 🎮 CONTROLES FINALES

### En el Juego:
- **Left Stick**: Mover jugador
- **Right Stick**: Rotar cámara
- **A (buttonSouth)**: Saltar
- **B (buttonEast)**: (reservado)
- **Y (buttonNorth)**: Alternar 3ª/1ª persona
- **LB (leftShoulder)**: Correr
- **RB (rightShoulder)**: Escalar
- **LT (leftTrigger)**: Mantener para primera persona + aiming
- **RT (rightTrigger)**: Disparar gancho
- **Start**: Pausar

### En Menús:
- **Left Stick / D-Pad**: Navegar arriba/abajo
- **Left Stick horizontal**: Ajustar sliders
- **A**: Confirmar/Seleccionar
- **B**: Cancelar/Volver
- **Start**: Pausar

---

## ✅ VERIFICACIONES

### Antes de probar:

1. ✅ `Both` está seleccionado en Active Input Handling
2. ✅ `GamepadControls` tiene todas las acciones configuradas
3. ✅ `PlayerInput` está en el jugador con `Gameplay` como Default Map
4. ✅ `GamepadInputBridge` está en el jugador con todas las referencias
5. ✅ Todos los Unity Events están conectados
6. ✅ Los menús tienen `MainMenuGamepadNavigation` y `PauseMenuGamepadNavigation`
7. ✅ Navigation está configurado en los botones

### Al probar:

1. ❌ **Si no funciona el teclado/ratón**: Verifica que Active Input Handling sea `Both`
2. ❌ **Si no funciona el gamepad**: Conecta el mando ANTES de darle Play
3. ❌ **Si la cámara no rota**: Verifica Look Sensitivity en GamepadInputBridge
4. ❌ **Si no navega menús**: Verifica que Navigation esté en Explicit en los botones

---

## 🚨 IMPORTANTE

- **NO elimines ningún `Input.GetAxis` o `Input.GetKey` de tus scripts actuales**
- **El gamepad funciona EN PARALELO con teclado/ratón**
- **Ambos sistemas pueden usarse simultáneamente**
- **Si algo no funciona, revisa la consola de Unity para errores**

---

## 📝 Scripts Creados

Los siguientes scripts fueron añadidos al proyecto:

1. `GamepadInputBridge.cs` - Conecta Input System con tus controles existentes
2. `MainMenuGamepadNavigation.cs` - Navegación de gamepad en menú principal
3. `PauseMenuGamepadNavigation.cs` - Navegación de gamepad en menú de pausa

Todos están en `Assets/Scripts/Controles/` y `Assets/Scripts/Menus/`

---

## 🎉 ¡Listo!

Ahora tu juego funciona con:
- ✅ Teclado + Ratón (como antes)
- ✅ Gamepad de Xbox/PlayStation
- ✅ Ambos al mismo tiempo sin conflictos
