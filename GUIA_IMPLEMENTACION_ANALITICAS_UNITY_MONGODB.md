# Guia Completa: Analytics en Escalatopia con MongoDB Atlas y Unity

**Projecte:** EscalatopiaGit
**Versió Unity:** 2022.3.45f1
**Objectiu:** Implementar recollida d'analiticas per sessio (almenys 2 triggers + 5 dades per membre) i enviament a MongoDB.

---

# PART 1: MONGODB ATLAS - CONFIGURACIÓ COMPLETA

La arquitectura final serà:
```
Unity (recull dades) → JSON local → POST a Backend → Backend guarda a MongoDB
```

**Important:** Unity NO es connecta directament a MongoDB. MongoDB es només el servidor de base de dades al núvol.

## PASO 1: Crear Compte en MongoDB Atlas

### 1.1 Anar al web
- Obrir navegador: Chrome, Firefox, Edge
- Entrar a: **https://www.mongodb.com/cloud/atlas**

### 1.2 Registrarse
- Click en **"Sign Up"** (botó verd arriba a la dreta)
- Omplir:
  - **Email:** el teu correu
  - **Password:** contrasenya segura
  - **First Name:** el teu nom
  - **Last Name:** el teu cognom
- Click **"Create your Atlas account"**

### 1.3 Verificar email
- MongoDB t'enviarà un correu
- Obre la teva bústia i click a l'enllaç
# Guia Completa: Analytics en Escalatopia con MongoDB Atlas y Unity

**Projecte:** EscalatopiaGit
**Versió Unity:** 2022.3.45f1
**Objectiu:** Implementar recollida d'analiticas per sessio (almenys 2 triggers + 5 dades per membre) i enviament a MongoDB.

---

Nota ràpida: he integrat els scripts i les crides als triggers al repo. Aquesta guia conté tot: instruccions Atlas, backend (Node.js), scripts Unity (C#) i exemples JSON.

---

# PART 1 — MongoDB Atlas (resum i dades que has proporcionat)

Connection string original (SRV):

```
mongodb+srv://anas:Anas_712066@escalatopiaanalytics.lurpmjs.mongodb.net/?appName=EscalatopiaAnalytics
```

Ús: en Windows, si Node falla amb `mongodb+srv://`, usa la URI estàndard `mongodb://` de la PART 4.

---

# PART 2 — Esquema de la sessió (exemple JSON)

Aquest és l'exemple que m'has donat (he afegit `sessionOutcome` recomanat):

```json
{
  "sessionId": "SESSION_20260507_143000",
  "timestamp": "2026-05-07T14:30:00Z",
  "playerName": "Player-001",
  "timeTotalSeconds": 1250,
  "maxHeightReached": 542.3,
  "checkpointsReached": 8,
  "averageTimePerPuzzleSeconds": 85.3,
  "totalDeaths": 15,
  "bossAttempts": { "BOSS_ACT1": 5, "BOSS_ACT2": 12 },
  "bossClearTimeSeconds": { "BOSS_ACT1": 180.5 },
  "hookUsesCount": 87,
  "puzzlesCompleted": 5,
  "puzzlesTotal": 7,
  "itemsCollected": 12,
  "itemIds": ["COIN_001", "GEM_RED_01"],
  "movementStats": { "up": 450, "down": 200, "left": 380, "right": 420 },
  "maxLevelReached": "ACT_2_BOSS",
  "hasCompletedGame": false,
  "sessionOutcome": "abandoned",
  "notes": "Abandono en Boss Act 2"
}
```

Comentari: `sessionOutcome` ajuda a analitzar abandonaments/completions. Valors típics: `"completed"`, `"died"`, `"abandoned"`.

---

# PART 3 — Unity: scripts i integració

Col·loca aquests fitxers a `Assets/Scripts/Systems/`.

1) `GameSessionAnalytics.cs`

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

2) `AnalyticsManager.cs` (singleton, guardat local i POST al backend)

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

  public void StartSession(string playerName){
    current = new GameSessionAnalytics();
    current.sessionId = "SESSION_" + System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
    current.timestamp = System.DateTime.UtcNow.ToString("o");
    current.playerName = playerName;
    current.timeTotalSeconds = 0;
    current.maxHeightReached = 0f;
    current.checkpointsReached = 0;
    current.totalDeaths = 0;
    current.bossAttempts = new System.Collections.Generic.Dictionary<string,int>();
    current.bossClearTimeSeconds = new System.Collections.Generic.Dictionary<string,float>();
    current.itemIds = new System.Collections.Generic.List<string>();
    current.movementStats = new System.Collections.Generic.Dictionary<string,int>{ {"up",0}, {"down",0}, {"left",0}, {"right",0} };
    current.notes = "";
    sessionStartTime = Time.time;
    Debug.Log("[Analytics] Sesión iniciada: " + current.sessionId);
  }

  public void RecordCheckpoint(){
    if(current == null) return;
    current.checkpointsReached++;
    SaveLocal();
    Debug.Log("[Analytics] Checkpoint registrado: " + current.checkpointsReached);
  }

  public void RecordDeath(){
    if(current == null) return;
    current.totalDeaths++;
    SaveLocal();
    Debug.Log("[Analytics] Death registrado: " + current.totalDeaths);
  }

  public void RecordItem(string itemId){
    if(current == null) return;
    current.itemsCollected++;
    current.itemIds.Add(itemId);
    SaveLocal();
    Debug.Log("[Analytics] Item registrado: " + itemId);
  }

  public void SaveLocal(){
    if(current == null) return;
    current.timeTotalSeconds = (int)(Time.time - sessionStartTime);
    string folder = Path.Combine(Application.persistentDataPath, "Analytics");
    Directory.CreateDirectory(folder);
    string path = Path.Combine(folder, current.sessionId + ".json");
    File.WriteAllText(path, JsonUtility.ToJson(current, true));
    Debug.Log("[Analytics] Guardado local: " + path);
  }

  public void SendToServer(string url){
    if(current == null) return;
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
      Debug.LogError("[Analytics] POST failed: " + uwr.error);
    } else {
      Debug.Log("[Analytics] POST OK: " + uwr.downloadHandler.text);
    }
  }
}
```

3) Integració als triggers (ja aplicada al repo):

- `Assets/Scripts/Mecanicas/CheckpointSaveTrigger.cs` — crida a `RecordCheckpoint()` i `SaveLocal()` després de guardar el checkpoint.
- `Assets/Scripts/Mecanicas/SceneChangeTrigger.cs` — registra `current.maxLevelReached = sceneName;` i `SaveLocal()` abans de carregar la nova escena.

---

# PART 4 — Backend Node.js (col·loca a `backend/`)

1) `package.json` (indicatiu):

```json
{
  "name": "escalatopia-analytics-backend",
  "version": "1.0.0",
  "main": "server.js",
  "scripts": { "start": "node server.js" },
  "dependencies": {
    "body-parser": "^1.20.0",
    "cors": "^2.8.5",
    "dotenv": "^16.0.0",
    "express": "^4.18.0",
    "mongodb": "^4.12.0"
  }
}
```

2) `.env` (crear a `backend/.env`) — en aquest projecte, millor fer servir la URI estàndard per evitar el problema SRV de Node en Windows:

```
MONGODB_URI=mongodb://anas:Anas_712066@ac-9hhbda3-shard-00-00.lurpmjs.mongodb.net:27017,ac-9hhbda3-shard-00-01.lurpmjs.mongodb.net:27017,ac-9hhbda3-shard-00-02.lurpmjs.mongodb.net:27017/?replicaSet=atlas-rsveae-shard-0&authSource=admin&tls=true&retryWrites=true&w=majority&appName=EscalatopiaAnalytics
MONGODB_DATABASE=EscalatopiaAnalytics
MONGODB_COLLECTION=sessions
PORT=3000
```

3) `server.js` (a `backend/server.js`):

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
if (!uri) { console.error('Falta MONGODB_URI a .env'); process.exit(1); }

MongoClient.connect(uri, { useNewUrlParser: true, useUnifiedTopology: true })
  .then(client => {
    const db = client.db(process.env.MONGODB_DATABASE || 'EscalatopiaAnalytics');
    const sessions = db.collection(process.env.MONGODB_COLLECTION || 'sessions');

    app.post('/api/sessions', async (req, res) => {
      try {
        const doc = req.body;
        if (!doc || !doc.sessionId) return res.status(400).json({ ok: false, error: 'Missing sessionId' });
        await sessions.insertOne(doc);
        res.status(200).json({ ok: true });
      } catch (err) {
        console.error(err);
        res.status(500).json({ ok: false, error: err.message });
      }
    });

    const port = process.env.PORT || 3000;
    app.listen(port, () => console.log(`API listening on http://localhost:${port}`));
  })
  .catch(err => console.error('Mongo connect error', err));
```

4) Comandes per instal·lar i executar (des de `backend/`):

```bash
npm init -y
npm install express mongodb dotenv body-parser cors
node server.js
```

---

# PART 5 — Proves i passos ràpids

1. Obrir Unity.
2. Afegir un GameObject buit amb el component `AnalyticsManager` en l'escena inicial (o crear un prefab persistent).
3. Cridar `AnalyticsManager.Instance.StartSession("PlayerName");` al començar la partida (p.ex. en el menú principal o GameManager).
4. Entrar a un checkpoint — comprovar consola i el fitxer JSON en `Application.persistentDataPath/Analytics/`.
5. Tenir el backend corrent i enviar:

```csharp
AnalyticsManager.Instance.SendToServer("http://localhost:3000/api/sessions");
```

Prova manual amb `curl` (backend actiu):

```bash
curl -X POST http://localhost:3000/api/sessions \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"TEST_001","playerName":"TestPlayer","timestamp":"2026-05-08T12:00:00Z","timeTotalSeconds":60,"sessionOutcome":"test"}'
```

---

# NOTES I RECOMANACIONS

- `sessionOutcome` és recomanat per analitzar abandonaments/completions.
- No posis la URI pública amb credencials dins del build de Unity; utilitza el backend i variables d'entorn.
- Quan lliuris, restringeix `Network Access` en Atlas al rang d'IP del professor o al teu backend.

---

# Fitxers creats/actualitzats en el repo

- `Assets/Scripts/Systems/GameSessionAnalytics.cs`  (classe de dades)
- `Assets/Scripts/Systems/AnalyticsManager.cs`      (gestor singleton)
- `Assets/Scripts/Mecanicas/CheckpointSaveTrigger.cs`  (afegides crides analytics)
- `Assets/Scripts/Mecanicas/SceneChangeTrigger.cs`     (afegides crides analytics)

Si vols, puc crear ara la carpeta `backend/` amb `server.js` i `.env` i el `package.json` mínim dins el repo. Vols que ho faci?

6. A `Database`, crear la base de dades `EscalatopiaAnalytics` i la colleccio `sessions`.
7. Copiar la connection string que ofereix Atlas per al backend.

La part important es aquesta: la web de MongoDB Atlas no es per guardar dades des de Unity directament, sino per preparar la base de dades i obtenir les credencials i la cadena de connexio. Les insercions reals les ha de fer el backend.

Recomanacio d'estructura:

- Base de dades: `EscalatopiaAnalytics`
- Colleccio: `sessions`

### Si no voleu usar Atlas
També podeu instal·lar MongoDB localment i treballar amb una instancia al vostre ordinador. Aquesta opcio va be per proves, pero per entrega final Atlas queda millor perquè mostra clarament una configuracio real de base de dades al núvol.

### Pas 2: Definir l'esquema del document
MongoDB guardara cada sessio com un document JSON. El document hauria de conservar exactament les dades principals recollides per Unity.

Exemple d'estructura:

```json
{
  "sessionId": "SESSION_20260507_143000",
  "timestamp": "2026-05-07T14:30:00Z",
  "playerName": "Player-001",
  "timeTotalSeconds": 1250,
  "maxHeightReached": 542.3,
  "checkpointsReached": 8,
  "averageTimePerPuzzleSeconds": 85.3,
  "totalDeaths": 15,
  "bossAttempts": {
    "BOSS_ACT1": 5,
    "BOSS_ACT2": 12
  },
  "bossClearTimeSeconds": {
    "BOSS_ACT1": 180.5
  },
  "hookUsesCount": 87,
  "puzzlesCompleted": 5,
  "puzzlesTotal": 7,
  "itemsCollected": 12,
  "itemIds": ["COIN_001", "GEM_RED_01"],
  "movementStats": {
    "up": 450,
    "down": 200,
    "left": 380,
    "right": 420
  },
  "maxLevelReached": "ACT_2_BOSS",
  "hasCompletedGame": false,
  "notes": "Abandono en Boss Act 2"
}
```

### Pas 3: Crear indexes utiles
Per consultar millor les dades, MongoDB hauria de tenir indexos en camps com:

- `sessionId`
- `timestamp`
- `playerName`
- `maxLevelReached`
- `hasCompletedGame`

Aixo ajuda a fer consultes com:

- sessions d'un jugador concret;
- sessions d'una data concreta;
- jugadores que arriben a un acte concret;
- sessions completades vs no completades.

### Pas 4: Decidir com s'envia des de Unity
La manera correcta es enviar el JSON a una API REST. El flux seria:

1. Unity serialitza la sessio.
2. Unity fa un `POST` a una API.
3. L'API valida el JSON.
4. L'API insereix el document a MongoDB.

No es recomana posar usuari i contrasenya de MongoDB dins Unity.

### Pas 5: Que heu de fer exactament si comenceu de zero

Si ara mateix no teniu res creat, el pla minim es aquest:

1. Crear el compte a MongoDB Atlas.
2. Crear el cluster.
3. Crear l'usuari i la contrasenya.
4. Obrir l'accés de xarxa.
5. Crear la base de dades i la colleccio.
6. Fer la API amb Node.js o ASP.NET.
7. Connectar Unity amb la API, no amb MongoDB directament.
8. Provar primer guardant un sol document de prova.
9. Despres afegir els triggers i les metricas completes.

A nivell de lliurament, el professor normalment vol veure que sabeu justificar el flux complet: Unity recull dades, el backend les rep i MongoDB les desa. Amb aquesta estructura ja teniu la part web, la part de joc i la part de base de dades ben separades.

## 6. Backend recomanat entre Unity i MongoDB

Per a la implementacio podeu fer servir qualsevol backend senzill. Les opcions mes normals son:

- Node.js + Express + MongoDB Driver o Mongoose;
- ASP.NET Web API + MongoDB Driver.

Si voleu una solucio rapida, Node.js + Express sol ser la mes simple per fer demostracio.

Si encara no teniu backend creat, el mes facil per començar es Node.js + Express per aquests motius:

- es ràpid de muntar;
- hi ha moltes guies i exemples;
- funciona molt be amb JSON;
- connecta facilment amb MongoDB Atlas.

Si el vostre grup treballa millor amb C#, ASP.NET Web API també es una opcio correcta i mes coherent amb Unity.

### Tasques del backend

- rebre el JSON des de Unity;
- validar que els camps basics existeixen;
- guardar el document a MongoDB;
- retornar un `200 OK` si tot ha anat be;
- retornar un error si falta informacio o el JSON esta mal format.

### Exemple de flux HTTP

```text
Unity -> POST /api/sessions -> Backend -> MongoDB
```

### Exemple de variables que haureu de guardar fora de Unity

No poseu aquestes dades dins codi dur de Unity. Guardeu-les al backend o en variables d'entorn:

- `MONGODB_URI`
- `MONGODB_DATABASE`
- `MONGODB_COLLECTION`
- `API_KEY` o credencial equivalent si en feu servir

Això us evita exposar contrasenyes dins del joc i fa que el projecte estigui millor justificat tecnicament.

## 7. Quines funcions ha de tenir el sistema final

El sistema complet ha de permetre:

- iniciar una sessio nova;
- registrar dades en temps real;
- reaccionar a triggers;
- guardar JSON local;
- enviar les dades a MongoDB;
- consultar sessions des de la base de dades;
- analitzar dificultat, exploracio i progressio.

## 8. Com repartir la feina entre Unity i MongoDB

### Unity

- detectar triggers;
- sumar comptadors;
- actualitzar altura maxima i progressio;
- crear el JSON;
- fer l'enviament al backend.

### MongoDB

- guardar cada sessio com un document;
- permetre filtres per data, jugador i nivell;
- oferir historials de partides;
- servir per analisi posterior i grafiques.

## 9. Checklist de l'entrega

Abans de generar el PDF final, comprova que compleixes aquests punts:

- tens almenys 2 triggers implementats i documentats;
- cada sessio guarda com a minim 5 dades per membre o el nombre acordat pel grup;
- el JSON es crea dins Unity;
- el JSON es pot enviar a MongoDB a traves d'una API;
- tens exemple de document de MongoDB;
- expliques per a que serveix cada metrica durant i despres del desenvolupament.

## 10. Proposta de treball recomanada

Si voleu fer-ho de forma ordenada, aquest seria el pla ideal:

1. Crear la classe `GameSessionAnalytics`.
2. Crear `AnalyticsManager`.
3. Integrar checkpoint i canvi de escena.
4. Afegir morts, boss, puzzles, ganxo i items.
5. Guardar JSON local.
6. Crear API simple per rebre dades.
7. Guardar a MongoDB.
8. Provar amb 5 o 10 sessions.
9. Exportar el resultat a PDF.

## 11. Conclusio

La implementacio correcta consisteix a fer que Unity reculli la informacio de la sessio i que MongoDB la desi en documents consultables. Amb aquest plantejament, els triggers serveixen per detectar moments importants del gameplay, les dades de sessio serveixen per justificar el disseny i la base de dades serveix per analitzar resultats reals.

Si voleu, a partir d'aquesta guia es pot fer un segon document amb el codi base inicial de `GameSessionAnalytics`, `AnalyticsManager` i l'API per MongoDB.

## 14. Anex: Checklist de tasques (TODO)

Copieu aquesta checklist al vostre document o gestor d'eines; està alineada amb el pla de treball i us ajudarà a documentar l'estat per la entrega.

- [in-progress] Crear compte i cluster en MongoDB Atlas
- [not-started] Crear usuari DB i configurar Network Access
- [not-started] Crear base de dades i colecció `sessions`
- [not-started] Montar backend Node.js (inicialitzar projecte)
- [not-started] Afegir `server.js` i `.env.example`
- [not-started] Provar endpoint amb `curl` i verificar en Atlas
- [not-started] Crear `GameSessionAnalytics.cs` en Unity
- [not-started] Crear `AnalyticsManager.cs` i guardar JSON local
- [not-started] Integrar `CheckpointSaveTrigger` i `SceneChangeTrigger`
- [not-started] Integrar mètriques addicionals (morts, boss, puzzles, items, ganxo)
- [not-started] Enviar JSON al backend des de Unity i comprovar en Atlas
- [not-started] Generar PDF final amb evidències

Marca els punts a mesura que els completeu i adjunteu captures a la carpeta `Docs/Evidences/` per generar el PDF final.

## 12. Resum ultra pratic per començar avui mateix

Si voleu arrancar ja, feu exactament això:

1. Creeu el compte a MongoDB Atlas.
2. Creeu cluster, usuari i colleeccio.
3. Creeu la API.
4. Feu la classe de sessio a Unity.
5. Integreu `CheckpointSaveTrigger` i `SceneChangeTrigger`.
6. Afegiu morts, bosses, puzzles, items i ganxo.
7. Proveu que Unity genera JSON.
8. Envie-ho al backend.
9. Comproveu que apareix a MongoDB Atlas.
10. Feu captures o evidencies per al PDF final.

---

## Anex II: Fitxers d'exemple i com executar-los (crear ràpidament)

Si vols que tot funcioni avui, crea aquests fitxers exactes. Coloca'ls a les rutes indicades.

### Backend (carpeta `backend/`)

1) `package.json` (només per referència — `npm init -y` crea un bàsic):

```json
{
  "name": "escalatopia-analytics-backend",
  "version": "1.0.0",
  "main": "server.js",
  "scripts": {
    "start": "node server.js"
  },
  "dependencies": {
    "body-parser": "^1.20.0",
    "cors": "^2.8.5",
    "dotenv": "^16.0.0",
    "express": "^4.18.0",
    "mongodb": "^4.12.0"
  }
}
```

2) `.env.example` (copia a `.env` i omple la URI):

```
MONGODB_URI=mongodb+srv://escal_user:<password>@cluster0.mongodb.net/?retryWrites=true&w=majority
MONGODB_DATABASE=EscalatopiaAnalytics
MONGODB_COLLECTION=sessions
PORT=3000
```

3) `server.js` (col·locar a `backend/server.js`):

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
  console.error('Falta MONGODB_URI a .env');
  process.exit(1);
}

MongoClient.connect(uri, { useNewUrlParser: true, useUnifiedTopology: true })
  .then(client => {
    const db = client.db(process.env.MONGODB_DATABASE || 'EscalatopiaAnalytics');
    const sessions = db.collection(process.env.MONGODB_COLLECTION || 'sessions');

    app.post('/api/sessions', async (req, res) => {
      try {
        const doc = req.body;
        // opcional: validacions basiques
        if (!doc || !doc.sessionId) return res.status(400).json({ ok: false, error: 'Missing sessionId' });
        await sessions.insertOne(doc);
        res.status(200).json({ ok: true });
      } catch (err) {
        console.error(err);
        res.status(500).json({ ok: false, error: err.message });
      }
    });

    const port = process.env.PORT || 3000;
    app.listen(port, () => console.log(`API listening on ${port}`));
  })
  .catch(err => console.error('Mongo connect error', err));
```

4) Comandes per executar (a `backend/`):

```bash
npm init -y
npm install express mongodb dotenv body-parser cors
cp .env.example .env
# editar .env i posar la MONGODB_URI
node server.js
```

5) Prova amb curl (des de la mateixa màquina on corre el servidor):

```bash
curl -X POST http://localhost:3000/api/sessions -H "Content-Type: application/json" -d '{"sessionId":"TEST01","timestamp":"2026-05-08T12:00:00Z","playerName":"tester"}'
```

### Unity (carpeta dins del projecte)

Col·loca aquests fitxers a `Assets/Scripts/Systems/`.

1) `GameSessionAnalytics.cs`:

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

2) `AnalyticsManager.cs`:

```csharp
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class AnalyticsManager : MonoBehaviour {
  public static AnalyticsManager Instance;
  public GameSessionAnalytics current;

  void Awake(){
    if(Instance==null){ Instance=this; DontDestroyOnLoad(gameObject); } else Destroy(gameObject);
  }

  public void StartSession(string playerName){
    current = new GameSessionAnalytics();
    current.sessionId = "SESSION_" + System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
    current.timestamp = System.DateTime.UtcNow.ToString("o");
    current.playerName = playerName;
    current.bossAttempts = new System.Collections.Generic.Dictionary<string,int>();
    current.bossClearTimeSeconds = new System.Collections.Generic.Dictionary<string,float>();
    current.itemIds = new System.Collections.Generic.List<string>();
    current.movementStats = new System.Collections.Generic.Dictionary<string,int>{{"up",0},{"down",0},{"left",0},{"right",0}};
  }

  public void RecordCheckpoint(){
    current.checkpointsReached++;
    SaveLocal();
  }

  public void SaveLocal(){
    string folder = Path.Combine(Application.persistentDataPath,"Analytics");
    Directory.CreateDirectory(folder);
    string path = Path.Combine(folder, current.sessionId + ".json");
    File.WriteAllText(path, JsonUtility.ToJson(current));
    Debug.Log("[Analytics] Saved local: " + path);
  }

  public void SendToServer(string url){
    StartCoroutine(PostCoroutine(url));
  }

  IEnumerator PostCoroutine(string url){
    string json = JsonUtility.ToJson(current);
    var uwr = new UnityWebRequest(url, "POST");
    byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
    uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
    uwr.downloadHandler = new DownloadHandlerBuffer();
    uwr.SetRequestHeader("Content-Type", "application/json");
    yield return uwr.SendWebRequest();
    if (uwr.result != UnityWebRequest.Result.Success) Debug.LogError("Analytics POST failed: " + uwr.error);
    else Debug.Log("Analytics POST OK: " + uwr.downloadHandler.text);
  }
}
```

3) Integració ràpida als triggers (exemples):

- A `CheckpointSaveTrigger.OnTriggerEnter` inserir:

```csharp
AnalyticsManager.Instance?.RecordCheckpoint();
AnalyticsManager.Instance?.SaveLocal();
```

- A `SceneChangeTrigger.OnTriggerEnter` inserir:

```csharp
AnalyticsManager.Instance?.current.maxLevelReached = sceneName;
AnalyticsManager.Instance?.SaveLocal();
```

### Notes finals

- Assegura't d'afegir `using System.Collections.Generic;` a la capçalera dels scripts C# si cal.
- Per provar localment utilitza `http://localhost:3000/api/sessions` si el backend i Unity estan a la mateixa màquina.
- Recorda restringir l'accés a Atlas abans d'entregar (no deixis `0.0.0.0/0`).

Aquest annex completa el MD amb tot el necessari per arrencar: fitxers, comandes i ubicacions. Vols que creï aquests 5 fitxers directament al repo (`backend/*` i `Assets/Scripts/Systems/*`)?
