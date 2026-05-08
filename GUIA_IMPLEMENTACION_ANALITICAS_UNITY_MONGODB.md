# Guia d'implementacio del sistema d'analiticas de sessio

Projecte: EscalatopiaGit  
Objectiu de l'entrega: documentar i implementar la recollida d'analiticas per sessio, amb almenys 2 triggers, al menys 5 dades per membre de l'equip i capacitat d'enviament a MongoDB.

## 1. Objectiu de la implementacio

El sistema ha de registrar dades de cada sessio de joc, guardar-les en un JSON local i preparar-les per enviar-les a una base de dades MongoDB. La idea no es només guardar dades, sino poder analitzar com juga l'usuari, on es bloqueja, quines mecanicas usa i en quin punt abandona.

La arquitectura recomanada es aquesta:

1. Unity genera i actualitza les dades de la sessio.
2. Unity guarda un fitxer JSON local com a copia de seguretat.
3. Unity envia el JSON a una API intermèdia.
4. L'API desa el document a MongoDB.

Important: Unity no hauria de connectar directament a MongoDB en un projecte real, perquè exposaria credencials i faria la base de dades insegura. El correcte es passar per un backend o API.

## 2. Dades minimes que ha de recollir cada sessio

Per complir el requisit de l'activitat, la sessio ha de guardar com a minim aquestes dades. Si sou 4 membres, podeu repartir la responsabilitat en 5 camps per persona o repartir les 20 dades entre tots, segons us demanin a classe.

### Dades base recomanades
- `sessionId`
- `timestamp`
- `playerName`
- `timeTotalSeconds`
- `maxHeightReached`
- `checkpointsReached`
- `averageTimePerPuzzleSeconds`
- `totalDeaths`
- `bossAttempts`
- `bossClearTimeSeconds`
- `hookUsesCount`
- `puzzlesCompleted`
- `puzzlesTotal`
- `itemsCollected`
- `itemIds`
- `movementStats`
- `maxLevelReached`
- `hasCompletedGame`
- `sessionOutcome`
- `notes`

### Repartiment orientatiu per membre
Si us demanen justificacio per membre, podeu distribuir-ho així:

- Membre 1: temps de sessio, altura maxima, checkpoints, nivell maximo, notes.
- Membre 2: morts, intents de boss, temps de boss, completat del joc, bosses per nivell.
- Membre 3: ganxo, puzzles completats, temps mitja de puzzle, puzzles totals, triggers de mecanica.
- Membre 4: items recollits, IDs d'items, moviment per direccio, zones visitades, scene changes.

### Nota sobre el recompte
La llista anterior queda en 20 dades si afegiu `sessionOutcome`. Aquest camp serveix per indicar si la sessio ha acabat en victoria, abandonament, mort, sortida manual o error, i ajuda molt a interpretar la resta de metricas.

## 3. Triggers mínims a implementar

Cal utilitzar almenys 2 triggers reals del projecte. Els mes clars dins Escalatopia son aquests:

### Trigger 1: CheckpointSaveTrigger
Fitxer: [Assets/Scripts/Mecanicas/CheckpointSaveTrigger.cs](Assets/Scripts/Mecanicas/CheckpointSaveTrigger.cs)

Aquest trigger es activa quan el jugador entra al checkpoint. Es pot aprofitar per registrar:

- Increment de checkpoints.
- Actualitzacio de la posicio de respawn.
- Altura maxima si el checkpoint esta mes amunt.
- Moment exacte de pas pel checkpoint.

### Trigger 2: SceneChangeTrigger
Fitxer: [Assets/Scripts/Mecanicas/SceneChangeTrigger.cs](Assets/Scripts/Mecanicas/SceneChangeTrigger.cs)

Aquest trigger es activa quan el jugador entra a una zona que carrega una altra escena. Es pot usar per registrar:

- Canvi de nivell o acte.
- Progres maximal assolit.
- Abandonament parcial si el jugador no arriba al final.
- Temps entre escenes.

### Trigger opcional 3: BolaGuiaTrigger o un trigger de item
Si voleu reforcar la part d'analitica, podeu afegir un tercer trigger per comptar:

- Interaccio amb guia o NPC.
- Recollida d'item.
- Activacio d'un event de tutorial.

## 4. Què s'ha de fer a Unity

### Pas 1: Crear la classe serialitzable de sessio
Crear un script com `GameSessionAnalytics.cs` amb una classe serialitzable que guardi totes les dades de la sessio.

Ha de contenir com a minim:

- identificador de sessio;
- hora i data;
- nom del jugador;
- metricas de progressio;
- metricas de dificultat;
- metricas d'exploracio;
- diccionaris o llistes per bosses, items i moviment.

### Pas 2: Crear un manager d'analiticas
Crear un `AnalyticsManager.cs` com a singleton o gestor central. Aquest script ha de fer aquestes feines:

- iniciar una nova sessio quan comenca la partida;
- actualitzar el JSON en memoria durant el joc;
- guardar el fitxer local quan la sessio acaba;
- preparar el JSON per enviar-lo al backend;
- controlar que no es perdi informacio si el joc es tanca bruscament.

### Pas 3: Integrar els triggers existents

#### En `CheckpointSaveTrigger`
Quan el jugador entra al trigger:

- cridar `RecordCheckpoint()`;
- incrementar `checkpointsReached`;
- si cal, actualitzar `maxHeightReached`;
- guardar la sessio localment o marcar-la com a canvi important.

#### En `SceneChangeTrigger`
Quan el jugador entra al trigger:

- cridar `SetMaxLevelReached(sceneName)` o una funcio equivalent;
- registrar el canvi de nivell;
- si el canvi es fa al final d'un acte, marcar si ha completat la zona;
- fer un guardat abans de carregar la nova escena.

### Pas 4: Afegir registres en altres scripts
Per completar les 12 metricas, afegiu crides al gestor des d'altres scripts del projecte:

- `PlayerController` o `PlayerStateMachine`: altura maxima, mort, moviment.
- `BossRaceManager` o boss controller: intents de boss i temps de boss.
- `PuzzleController`: puzzles completats i temps invertit.
- `ItemPickup` o objecte equivalent: item recollit.
- `PlayerInputHandler`: usos del ganxo i moviment per direccio si es vol mes detall.

### Pas 5: Guardar el JSON local
Guardar la sessio amb `Application.persistentDataPath` en una carpeta `Analytics`.

Ruta orientativa:

- Windows: `...\AppData\LocalLow\CompanyName\ProjectName\Analytics\`

Nom orientatiu del fitxer:

- `SESSION_YYYYMMDD_HHMMSS.json`

### Pas 6: Fer proves a Unity
Cal provar que els triggers realment registren dades. Les comprovacions basiques son:

- entrar a un checkpoint i veure que puja el comptador;
- entrar a una escena nova i veure que s'actualitza el nivell maximo;
- morir una vegada i confirmar que el total de morts augmenta;
- recollir un item i veure el seu ID a la sessio;
- completar un puzzle i revisar el temps registrat.

## 5. Què s'ha de fer amb MongoDB

### Pas 1: Crear la base de dades
Podeu fer-ho amb MongoDB Atlas o amb una instancia local. Si comenceu de zero, la ruta mes recomanable es MongoDB Atlas, perquè us permet crear-ho tot des de la web sense instal·lar res al principi i deixa molt clara la part de desplegament per a la memoria o el PDF.

Si voleu treballar des de zero, aquest es el cami correcte:

1. Entrar a https://www.mongodb.com/atlas i crear un compte.
2. Fer login i crear un projecte nou, per exemple `EscalatopiaAnalytics`.
3. Crear un cluster gratuït o compartit si només es per proves i entrega academica.
4. Crear un usuari de base de dades amb nom i contrasenya propis.
5. A `Network Access`, afegir la vostra IP o permetre acces temporal per provar.
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