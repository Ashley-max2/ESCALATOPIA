# 🎯 Guía: NPC Interactuable con Subtítulos en ESCALATOPIA

> **Estado actual del proyecto**: No hay scripts de NPCs. Esta guía usa **solo GameObjects y componentes Unity nativos** + un script simple.

---

## 📋 Requisitos Previos

✅ TextMeshPro ya instalado en el proyecto  
✅ Player con tag `"Player"` asignado  
✅ Escena abierta (ej: `PlayerTesting.unity`)  

---

## ⚡ PASO 1: Preparar el Tag del Player

1. Selecciona el **Player** en la jerarquía
2. En el Inspector, arriba a la derecha → **Tag**
3. Si no existe "Player":
   - Haz clic en el dropdown
   - Selecciona **Add Tag**
   - Escribe `Player` y presiona Enter
4. Vuelve al Player y asigna el tag `"Player"`

---

## 🎬 PASO 2: Crear el GameObject del NPC

### En la escena:
1. Click derecho en la jerarquía → **Create Empty**
2. Renombra a `NPC` (o el nombre que prefieras)
3. Posiciona donde quieras con el **Transform**

### Agregar modelo visual (Cápsula):
1. **Selecciona el NPC** en la jerarquía
2. Click derecho sobre el NPC → **3D Object > Capsule**
3. La cápsula se crea como **hijo del NPC** automáticamente
4. La cápsula viene con su propio **Capsule Collider** (déjalo como está)

### Agregar trigger de interacción al NPC padre:
1. **Selecciona el NPC** (el padre, no la cápsula)
2. Haz clic en **Add Component**
3. Agrega **BoxCollider**
   - ✅ Marca **Is Trigger** (IMPORTANTE)
   - Ajusta el tamaño para que cubra el área de interacción (ej: Size X:2, Y:3, Z:2)

> **Nota**: El NPC padre tiene el BoxCollider trigger para detectar al jugador. La Cápsula hijo tiene su Capsule Collider normal para ser visible y físico.

---

## 🔤 PASO 3: Crear el Prompt "{E}"

### Sprite 3D (recomendado para que no se vea mal)

1. Selecciona el **NPC**
2. Click derecho → **Create Empty** y renombra a `PromptE`
3. Con `PromptE` seleccionado, **Add Component** → **SpriteRenderer**
4. Asigna un sprite con el texto "{E}"
5. Ajusta el **Transform**:
    - Position X: `0`, Y: `1` (encima del NPC)
    - Scale: `0.2, 0.2, 0.2` (ajusta segun el tamano del NPC)

> **Nota**: Evitamos Canvas encima del NPC porque se ve raro en escena 3D.

---

## 💬 PASO 4: Crear el Panel de Subtítulos

### Panel base:
1. Selecciona el **NPC**
2. Click derecho → **UI > Panel - Image**
3. Renombra a `SubtitlePanel`
4. En el Inspector > **Image**:
   - **Color**: `100, 100, 100, 200` (gris oscuro semi-transparente)
5. En **RectTransform**:
   - Position X: `0`, Y: `-200` (abajo)
   - Width: `800`, Height: `150`
   - Anchors: Bottom Center

### Agregar texto al panel:
1. Selecciona `SubtitlePanel`
2. Click derecho → **TextMeshPro - New Text**
3. Renombra a `SubtitleText`
4. En el Inspector > **TextMeshPro**:
   - **Text**: Dejar vacío
   - **Font Size**: `24`
   - **Color**: Blanco
   - **Alignment**: Center, Middle
5. En **RectTransform**:
   - Estira el texto para llenar el panel (Left 20, Right 20, Top 20, Bottom 20)

---

## 📜 PASO 5: Crear el Script Simple

Crea una carpeta `Assets/Scripts/NPCs/` y dentro un archivo:

**`NPCInteractable.cs`**

```csharp
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NPCInteractable : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptE;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private float subtitleDuration = 3f;

    [Header("Dialogue")]
    [SerializeField] private List<string> subtitles = new List<string>();

    private int currentSubtitleIndex = 0;
    private bool isPlayerNear = false;
    private float subtitleTimer = 0f;
    private bool showingSubtitle = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            promptE.SetActive(true);
            currentSubtitleIndex = 0;
            subtitleTimer = 0f;
            showingSubtitle = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            promptE.SetActive(false);
            subtitlePanel.SetActive(false);
            currentSubtitleIndex = 0;
            showingSubtitle = false;
        }
    }

    private void Update()
    {
        if (!isPlayerNear) return;

        // Escuchar tecla E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!showingSubtitle && currentSubtitleIndex < subtitles.Count)
            {
                ShowSubtitle();
            }
            else if (showingSubtitle && subtitleTimer > 0.5f) // Permite skip después de 0.5s
            {
                NextSubtitle();
            }
        }

        // Actualizar timer
        if (showingSubtitle)
        {
            subtitleTimer += Time.deltaTime;
            if (subtitleTimer >= subtitleDuration)
            {
                HideSubtitle();
            }
        }
    }

    private void ShowSubtitle()
    {
        if (currentSubtitleIndex >= subtitles.Count)
            return;

        promptE.SetActive(false);
        subtitlePanel.SetActive(true);
        subtitleText.text = subtitles[currentSubtitleIndex];
        subtitleTimer = 0f;
        showingSubtitle = true;
    }

    private void NextSubtitle()
    {
        currentSubtitleIndex++;
        if (currentSubtitleIndex < subtitles.Count)
        {
            subtitleText.text = subtitles[currentSubtitleIndex];
            subtitleTimer = 0f;
        }
        else
        {
            HideSubtitle();
        }
    }

    private void HideSubtitle()
    {
        subtitlePanel.SetActive(false);
        promptE.SetActive(true);
        showingSubtitle = false;
        subtitleTimer = 0f;
    }
}
```

---

## ⚙️ PASO 6: Agregar el Script al NPC

1. Selecciona el **NPC**
2. **Add Component** → Busca `NPCInteractable`
3. Selecciónalo

---

## 🔗 PASO 7: Asignar Referencias en el Inspector

1. Selecciona el **NPC**
2. En el componente `NPCInteractable`:
   - **Prompt E**: Arrastra `PromptE` desde la jerarquía
   - **Subtitle Text**: Arrastra `SubtitleText`
   - **Subtitle Panel**: Arrastra `SubtitlePanel`
   - **Subtitle Duration**: `3` (segundos)

---

## 💬 PASO 8: Agregar los Subtítulos

1. En el componente `NPCInteractable`, ve al array **Subtitles**
2. Establece **Size** al número de líneas de diálogo
3. Rellena cada elemento:

```
Element 0: "¡Hola viajero! Bienvenido a ESCALATOPIA."
Element 1: "¿Necesitas ayuda para navegar?"
Element 2: "Sigue adelante, hay muchas cosas que descubrir."
Element 3: "¡Buena suerte!"
```

---

## ✅ PASO 9: Prueba

### En el editor:
1. Presiona **Play** ▶️
2. Acerca el **Player** al **NPC**
3. Deberías ver el texto **{E}** encima del NPC
4. Presiona **E**:
   - El {E} desaparece
   - Aparece el primer subtítulo abajo
5. Presiona **E** de nuevo para el siguiente
6. Sal del rango del NPC → todo desaparece

---

## 📦 Estructura Final en la Jerarquía

```
NPC
├── Canvas (creado automáticamente)
│   ├── PromptE (TextMeshPro - muestra {E})
│   ├── SubtitlePanel (Image gris)
│   │   └── SubtitleText (TextMeshPro - diálogo)
├── BoxCollider (Is Trigger = ON)
└── NPCInteractable (Script)
```

---

## 🐛 Troubleshooting

| Problema | Solución |
|----------|----------|
| **{E} no aparece** | Verifica que `PromptE` esté asignado en el script |
| **Player no interactúa** | 1. BoxCollider debe tener **Is Trigger = ON** 2. Player debe tener tag **"Player"** |
| **Subtítulos no aparecen** | 1. Verifica que `SubtitlePanel` y `SubtitleText` estén asignados 2. Array de Subtitles debe tener contenido |
| **{E} no desaparece al presionar E** | Verifica que el Input esté funcionando (mira la consola para errores) |
| **Canvas aparece en la escena 3D** | Es normal si usaste UI. Si quieres solo 3D, usa la **Opción B** (Sprite 3D) |

---

## 🎮 Funcionalidad Completa

✅ **Player entra en rango** → Aparece **{E}**  
✅ **Player presiona E** → **{E}** desaparece, muestra **subtítulo 1**  
✅ **Player presiona E de nuevo** → Subtítulo 2, 3, 4... etc  
✅ **Se acaban subtítulos** → Panel desaparece, vuelve **{E}**  
✅ **Player sale del rango** → Todo se oculta, contador se resetea  

---

## 📝 Notas Importantes

- El script **NO modifica nada existente** del proyecto
- Puedes duplicar el NPC para crear múltiples NPCs con diferentes diálogos
- Si quieres cambiar la tecla de interacción, edita `KeyCode.E` en el script
- Para añadir más NPCs: repite los Pasos 2-8

---

**¡Listo! Ahora tienes un sistema de NPCs funcional y listo para usar.** 🎉
