# ⚡ Resumen Rápido: NPC Interactuable

## 5 PASOS ESENCIALES

### 1️⃣ Tag del Player
```
Selecciona Player → Inspector → Tag → "Player"
```

### 2️⃣ Crear el NPC
```
Click derecho en jerarquía → Create Empty → Llama "NPC"
Agrega: BoxCollider (Is Trigger = ON)
```

### 3️⃣ Crear {E} flotante
```
NPC → Click derecho → UI > Text - TextMeshPro
Renombra: PromptE
Texto: {E}
Font Size: 32
```

### 4️⃣ Crear Panel de Subtítulos
```
NPC → Click derecho → UI > Panel - Image
Renombra: SubtitlePanel
Color: 100,100,100,200 (gris oscuro)

En SubtitlePanel → Click derecho → TextMeshPro - New Text
Renombra: SubtitleText
Font Size: 24
Color: Blanco
Alignment: Center
```

### 5️⃣ Agregar Script
```
NPC → Add Component → NPCInteractable
Arrastra referencias:
  - Prompt E → PromptE
  - Subtitle Text → SubtitleText
  - Subtitle Panel → SubtitlePanel
  - Subtitle Duration → 3 (segundos)

En array Subtitles, agrega el diálogo
```

---

## ✨ Resultado

```
{E} aparece al acercarse
↓ Presiona E
Subtítulo 1 aparece 3 segundos
↓ Presiona E
Subtítulo 2 aparece
↓ Al terminar
{E} vuelve a aparecer
```

---

## 📂 Archivos Creados

✅ `Assets/Scripts/NPCs/NPCInteractable.cs` → Script listo para usar
✅ `GUIA_NPC_INTERACTUABLE.md` → Guía completa paso a paso

---

## 🎯 Si algo no funciona

| Síntoma | Fix |
|--------|-----|
| {E} no aparece | ✓ Verifica PromptE asignado |
| No funciona E | ✓ Player debe tener tag "Player" |
| Subtítulos no salen | ✓ Verifica SubtitlePanel y SubtitleText asignados |

---

## 💡 Tip Profesional

Para crear múltiples NPCs: **copia el NPC y edita solo los diálogos** en el array Subtitles.

No necesitas crear nada más. ¡Todo está listo! 🚀
