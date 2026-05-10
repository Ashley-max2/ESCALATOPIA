# Guia Completa: Analytics MongoDB + Unity (Paso a Paso)

**Projecte:** EscalatopiaGit
**Versió Unity:** 2022.3.45f1

---

# PART 1: MONGODB ATLAS - CONFIGURACIÓ COMPLETA ✅

**Ya completado** → Ver archivo: `GUIA_MONGODB_ATLAS_PASO_A_PASO.md`

**Resumen de lo que ya tienes:**
- ✅ Compte creat en MongoDB Atlas
- ✅ Cluster: **EscalatopiaCluster**
- ✅ Usuario: `escal_user`
- ✅ Contraseña: `EscalatopiaPass123!`
- ✅ Network Access: Abierto (0.0.0.0/0 para desarrollo)
- ✅ Base de datos: `EscalatopiaAnalytics`
- ✅ Colección: `sessions`
- ✅ Connection String guardat

---

# PART 2: ESTRUCTURA DE DADES

## Las 20 Datas que Recolectarás

```
1. sessionId              → ID único de la sesión
2. timestamp             → Hora exacta de inicio
3. playerName            → Nombre del jugador
4. timeTotalSeconds      → Tiempo total de sesión
5. maxHeightReached      → Altura máxima alcanzada
6. checkpointsReached    → Checkpoints alcanzados
7. averageTimePerPuzzle  → Tiempo promedio por puzzle
8. totalDeaths           → Total de muertes
9. bossAttempts          → Intentos por boss
10. bossClearTimeSeconds → Tiempo para derrotar cada boss
11. hookUsesCount        → Veces que usó el gancho
12. puzzlesCompleted     → Puzzles completados
13. puzzlesTotal         → Puzzles totales
14. itemsCollected       → Items recogidos
15. itemIds              → Listado de IDs de items
16. movementStats        → Movimientos (arriba, abajo, izq, der)
17. maxLevelReached      → Nivel máximo alcanzado
18. hasCompletedGame     → Si completó el juego
19. sessionOutcome       → Resultado (victoria, muerte, abandono)
20. notes                → Notas o comentarios
```

---

# PART 3: UNITY - SCRIPTS Y INTEGRACIÓN

## Paso 1: Crear GameSessionAnalytics.cs

**Ubicación:** `Assets/Scripts/Systems/GameSessionAnalytics.cs`

```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class GameSessionAnalytics {
    public string sessionId;
    public string timestamp;
    public string playerName;
    public int timeTotalSeconds;
    public float maxHeightReached;
    public int checkpointsReached;
    public float averageTimePerPuzzleSeconds;
    public int totalDeaths;
    public Dictionary<string,int> bossAttempts;
    public Dictionary<string,float> bossClearTimeSeconds;
    public int hookUsesCount;
    public int puzzlesCompleted;
    public int puzzlesTotal;
    public int itemsCollected;
    public List<string> itemIds;
    public Dictionary<string,int> movementStats;
    public string maxLevelReached;
    public bool hasCompletedGame;
    public string sessionOutcome;
    public string notes;
}
```

**Esto define TODAS las datas que guardarás.**

---

## Paso 2: Crear AnalyticsManager.cs

**Ubicación:** `Assets/Scripts/Systems/AnalyticsManager.cs`

```csharp
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class AnalyticsManager : MonoBehaviour {
  public static AnalyticsManager Instance;
  public GameSessionAnalytics current;
  private float sessionStartTime;

  void Awake(){
    if(Instance==null){
      Instance=this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
  }

  /// Iniciar una nueva sesión
  public void StartSession(string playerName){
    current = new GameSessionAnalytics();
    current.sessionId = "SESSION_" + System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
    current.timestamp = System.DateTime.UtcNow.ToString("o");
    current.playerName = playerName;
    current.timeTotalSeconds = 0;
    current.maxHeightReached = 0;
    current.checkpointsReached = 0;
    current.totalDeaths = 0;
    current.bossAttempts = new System.Collections.Generic.Dictionary<string,int>();
    current.bossClearTimeSeconds = new System.Collections.Generic.Dictionary<string,float>();
    current.itemIds = new System.Collections.Generic.List<string>();
    current.movementStats = new System.Collections.Generic.Dictionary<string,int>{
      {"up",0}, {"down",0}, {"left",0}, {"right",0}
    };
    current.notes = "";
    sessionStartTime = Time.time;
    Debug.Log("[Analytics] ✓ Sesión iniciada: " + current.sessionId);
  }

  /// Cuando entra a un checkpoint
  public void RecordCheckpoint(){
    if(current != null){
      current.checkpointsReached++;
      SaveLocal();
      Debug.Log("[Analytics] ✓ Checkpoint #" + current.checkpointsReached);
    }
  }

  /// Cuando muere
  public void RecordDeath(){
    if(current != null){
      current.totalDeaths++;
      SaveLocal();
      Debug.Log("[Analytics] ✓ Muerte #" + current.totalDeaths);
    }
  }

  /// Cuando recoge un item
  public void RecordItem(string itemId){
    if(current != null){
      current.itemsCollected++;
      current.itemIds.Add(itemId);
      SaveLocal();
      Debug.Log("[Analytics] ✓ Item recogido: " + itemId);
    }
  }

  /// Guardar JSON localmente
  public void SaveLocal(){
    if(current == null) return;
    current.timeTotalSeconds = (int)(Time.time - sessionStartTime);

    string folder = Path.Combine(Application.persistentDataPath,"Analytics");
    Directory.CreateDirectory(folder);
    string path = Path.Combine(folder, current.sessionId + ".json");
    File.WriteAllText(path, JsonUtility.ToJson(current, true));
    Debug.Log("[Analytics] Guardado en: " + path);
  }

  /// Enviar datos al backend
  public void SendToServer(string url){
    StartCoroutine(PostCoroutine(url));
  }

  IEnumerator PostCoroutine(string url){
    current.timeTotalSeconds = (int)(Time.time - sessionStartTime);
    string json = JsonUtility.ToJson(current, true);

    var uwr = new UnityWebRequest(url, "POST");
    byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
    uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
    uwr.downloadHandler = new DownloadHandlerBuffer();
    uwr.SetRequestHeader("Content-Type", "application/json");

    yield return uwr.SendWebRequest();

    if (uwr.result != UnityWebRequest.Result.Success) {
      Debug.LogError("[Analytics] ❌ POST falló: " + uwr.error);
    } else {
      Debug.Log("[Analytics] ✓ POST OK!");
    }
  }
}
```

---

## Paso 3: Integrar con CheckpointSaveTrigger

**Archivo:** `Assets/Scripts/Mecanicas/CheckpointSaveTrigger.cs`

Buscar la función `OnTriggerEnter` y agregar:

```csharp
void OnTriggerEnter(Collider other){
  if(other.CompareTag("Player")){
    // ... código existente ...

    // AGREGAR ESTAS LÍNEAS:
    if(AnalyticsManager.Instance != null){
      AnalyticsManager.Instance.RecordCheckpoint();
    }
  }
}
```

---

## Paso 4: Integrar con SceneChangeTrigger

**Archivo:** `Assets/Scripts/Mecanicas/SceneChangeTrigger.cs`

Buscar la función `OnTriggerEnter` y agregar:

```csharp
void OnTriggerEnter(Collider other){
  if(other.CompareTag("Player")){
    string sceneName = SceneManager.GetActiveScene().name;

    // AGREGAR ESTAS LÍNEAS:
    if(AnalyticsManager.Instance != null){
      AnalyticsManager.Instance.current.maxLevelReached = sceneName;
      AnalyticsManager.Instance.SaveLocal();
    }

    // ... código para cargar escena ...
  }
}
```

---

## Paso 5: Integrar Métricas en Otros Scripts

### PlayerController (Muertes)
```csharp
void Die(){
  // ... código existente ...
  AnalyticsManager.Instance?.RecordDeath();
}
```

### ItemPickup (Items)
```csharp
void OnPickup(){
  // ... código existente ...
  AnalyticsManager.Instance?.RecordItem(itemId);
}
```

### BossRaceManager (Boss Intents)
```csharp
void OnBossAttempt(string bossName){
  if(!AnalyticsManager.Instance.current.bossAttempts.ContainsKey(bossName)){
    AnalyticsManager.Instance.current.bossAttempts[bossName] = 0;
  }
  AnalyticsManager.Instance.current.bossAttempts[bossName]++;
  AnalyticsManager.Instance.SaveLocal();
}
```

---

## Paso 6: Probar en Unity

1. **Crear GameObject vacío** con el script AnalyticsManager
2. **En el menú principal o inicio del juego:**
   ```csharp
   AnalyticsManager.Instance.StartSession("TestPlayer");
   ```
3. **Entrar a un checkpoint** → Ve al console: `✓ Checkpoint #1`
4. **Morir** → Ve al console: `✓ Muerte #1`
5. **Recoger item** → Ve al console: `✓ Item recogido: coin_001`
6. **Cambiar escena** → Se registra `maxLevelReached`

**Archivo JSON creado en:**
- Windows: `C:\Users\[TuUsuario]\AppData\LocalLow\DefaultCompany\Escalatopia\Analytics\`
- Archivo: `SESSION_20260508_120000.json`

Abrirlo y verificar que todas las datas están correctas.

---

# PART 4: BACKEND NODE.JS

El backend es el intermediario: recibe JSON de Unity y lo guarda en MongoDB.

## Paso 1: Crear Carpeta Backend

En la raíz del proyecto, crear carpeta: `backend/`

```
ESCALATOPIA/
├── Assets/
├── Packages/
├── backend/  ← CREAR AQUÍ
└── ...
```

## Paso 2: Inicializar Node.js

Abrir terminal en la carpeta `backend/` y ejecutar:

```bash
npm init -y
npm install express mongodb dotenv body-parser cors
```

Esto crea `package.json` y carpeta `node_modules/`.

---

## Paso 3: Crear archivo `.env`

**Ubicación:** `backend/.env`

```
MONGODB_URI=mongodb+srv://escal_user:EscalatopiaPass123!@cluster0.mongodb.net/?retryWrites=true&w=majority
MONGODB_DATABASE=EscalatopiaAnalytics
MONGODB_COLLECTION=sessions
PORT=3000
```

**IMPORTANTE:** Reemplazar `EscalatopiaPass123!` con tu contraseña real de MongoDB.

---

## Paso 4: Crear `server.js`

**Ubicación:** `backend/server.js`

```javascript
require('dotenv').config();
const express = require('express');
const { MongoClient } = require('mongodb');
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(bodyParser.json());

const uri = process.env.MONGODB_URI;
if (!uri) {
  console.error('❌ ERROR: Falta MONGODB_URI en .env');
  process.exit(1);
}

// Conectar a MongoDB
MongoClient.connect(uri, { useNewUrlParser: true, useUnifiedTopology: true })
  .then(client => {
    console.log("✓ Conectado a MongoDB Atlas!");

    const db = client.db(process.env.MONGODB_DATABASE || 'EscalatopiaAnalytics');
    const sessions = db.collection(process.env.MONGODB_COLLECTION || 'sessions');

    // ENDPOINT: Recibir datos de Unity
    app.post('/api/sessions', async (req, res) => {
      try {
        const doc = req.body;

        // Validar que tenga sessionId
        if (!doc || !doc.sessionId) {
          return res.status(400).json({
            ok: false,
            error: 'Falta sessionId'
          });
        }

        // Guardar en MongoDB
        await sessions.insertOne(doc);
        console.log("✓ Documento guardado:", doc.sessionId);

        res.status(200).json({
          ok: true,
          message: 'Sesión guardada correctamente'
        });
      } catch (err) {
        console.error("❌ Error:", err);
        res.status(500).json({ ok: false, error: err.message });
      }
    });

    // ENDPOINT: Health check (verificar que funciona)
    app.get('/api/health', (req, res) => {
      res.status(200).json({
        ok: true,
        message: 'Backend funcionando'
      });
    });

    // Iniciar servidor
    const port = process.env.PORT || 3000;
    app.listen(port, () => {
      console.log(`\n✓ API escuchando en http://localhost:${port}`);
      console.log(`✓ Endpoint POST: http://localhost:${port}/api/sessions`);
      console.log(`✓ MongoDB Database: ${process.env.MONGODB_DATABASE}`);
      console.log(`✓ MongoDB Collection: ${process.env.MONGODB_COLLECTION}\n`);
    });
  })
  .catch(err => {
    console.error('❌ Error conectando a MongoDB:', err);
    process.exit(1);
  });
```

---

## Paso 5: Iniciar el Backend

En la carpeta `backend/`, ejecutar:

```bash
node server.js
```

Deberías ver:
```
✓ Conectado a MongoDB Atlas!
✓ API escuchando en http://localhost:3000
✓ Endpoint POST: http://localhost:3000/api/sessions
✓ MongoDB Database: EscalatopiaAnalytics
✓ MongoDB Collection: sessions
```

**El backend está funcionando.**

---

## Paso 6: Probar el Backend

### Opción 1: Con curl (terminal)

```bash
curl -X POST http://localhost:3000/api/sessions \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"TEST_001","playerName":"TestPlayer","timestamp":"2026-05-08T12:00:00Z"}'
```

**Deberías ver:** `{"ok":true,"message":"Sesión guardada correctamente"}`

### Opción 2: Verificar en MongoDB Atlas

1. Ir a **MongoDB Atlas** → **Databases**
2. Click en **EscalatopiaAnalytics**
3. Click en colección **sessions**
4. Ver el documento guardado

✅ **El backend funciona correctamente.**

---

# PART 5: CONEXIÓN UNITY ↔ BACKEND

## Actualizar Unity para Enviar al Backend

En tu script de manager de juego, cuando termina la sesión:

```csharp
void EndGame(string outcome){
  if(AnalyticsManager.Instance != null){
    AnalyticsManager.Instance.current.sessionOutcome = outcome; // "won", "died", "abandoned"
    AnalyticsManager.Instance.SaveLocal();
    AnalyticsManager.Instance.SendToServer("http://localhost:3000/api/sessions");
  }
}
```

**Llamas esta función cuando:**
- El jugador gana
- El jugador muere (game over)
- El jugador abandona

---

# RESUMEN FINAL

## ✅ CHECKLIST - QUÉ COMPLETASTE

### MongoDB Atlas
- [x] Cuenta creada
- [x] Cluster EscalatopiaCluster
- [x] Usuario y contraseña
- [x] Network Access abierto
- [x] Base de datos y colección creadas
- [x] Connection String guardado

### Unity Scripts
- [ ] GameSessionAnalytics.cs creado
- [ ] AnalyticsManager.cs creado
- [ ] CheckpointSaveTrigger integrado
- [ ] SceneChangeTrigger integrado
- [ ] Métricas en PlayerController, ItemPickup, etc.
- [ ] JSON local se crea correctamente
- [ ] StartSession llamado al inicio

### Backend Node.js
- [ ] Carpeta backend creada
- [ ] npm init y dependencias instaladas
- [ ] .env con URI de MongoDB
- [ ] server.js creado
- [ ] Backend funcionando (node server.js)
- [ ] Probado con curl

### Integración Final
- [ ] Unity envía POST al backend
- [ ] Backend recibe y valida
- [ ] MongoDB guarda documentos
- [ ] Documentos visibles en MongoDB Atlas

---

## ESTRUCTURA FINAL DE CARPETAS

```
ESCALATOPIA/
├── Assets/
│   └── Scripts/
│       └── Systems/
│           ├── GameSessionAnalytics.cs
│           └── AnalyticsManager.cs
├── backend/
│   ├── package.json
│   ├── .env
│   ├── server.js
│   └── node_modules/
├── ProjectSettings/
├── Packages/
└── (resto del proyecto)
```

---

## PRÓXIMOS PASOS

1. **Si aún no has hecho MongoDB:** → Ver `GUIA_MONGODB_ATLAS_PASO_A_PASO.md`
2. **Crear los 2 scripts de Unity** → GameSessionAnalytics.cs + AnalyticsManager.cs
3. **Integrar con los triggers** → CheckpointSaveTrigger + SceneChangeTrigger
4. **Crear backend Node.js** → Crear carpeta + 3 archivos
5. **Probar todo** → Json local → POST → MongoDB

**Tiempo estimado: 2-3 horas**

---

## PROBLEMAS COMUNES

### "Cannot connect to MongoDB"
- Verificar Network Access en MongoDB Atlas (debe estar abierto)
- Verificar que la URI en .env es correcta
- Verificar que copiaste bien usuario y contraseña

### "node: command not found"
- Node.js no está instalado
- Descargar desde: https://nodejs.org/

### "JSON no se guarda"
- Verificar que AnalyticsManager está en la escena
- Verificar que StartSession se llama al inicio

### POST falla
- Backend no está corriendo (`node server.js`)
- URL incorrecta en Unity
- JSON mal formado

---

## DOCUMENTACIÓN COMPLEMENTARIA

Disponible en archivos:
- `GUIA_MONGODB_ATLAS_PASO_A_PASO.md` → MongoDB Atlas paso a paso
- `GUIA_IMPLEMENTACION_ANALITICAS_UNITY_MONGODB.md` → Guía original completa
