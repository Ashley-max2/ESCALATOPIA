using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class AnalyticsManager : MonoBehaviour {
  public static AnalyticsManager Instance;
  public GameSessionAnalytics current;
  private float sessionStartTime;
  private Dictionary<string, float> _bossAttemptStartTimes = new Dictionary<string, float>();

  [SerializeField] private bool autoStartSession = true;
  [SerializeField] private string defaultPlayerName = "Player-001";
  [SerializeField] private bool autoSendToServer = true;
  [SerializeField] private string backendUrl = "http://localhost:3000/api/sessions";

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Bootstrap()
  {
    if (Instance != null)
      return;

    GameObject analyticsObject = new GameObject("AnalyticsManager");
    analyticsObject.AddComponent<AnalyticsManager>();
    DontDestroyOnLoad(analyticsObject);
  }

  void Awake(){
    if(Instance==null){
      Instance=this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
  }

  void Start(){
    Debug.Log("[Analytics] PersistentDataPath: " + Application.persistentDataPath);
    EnsureAnalyticsFolderExists();
    if (autoStartSession && current == null) {
      StartSession(defaultPlayerName);
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
    current.maxLevelReached = string.Empty;
    current.sessionOutcome = "in_progress";
    current.hasCompletedGame = false;
    current.puzzlesTotal = 0;
    current.puzzlesCompleted = 0;
    sessionStartTime = Time.time;
    _bossAttemptStartTimes.Clear();
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

  public void RecordMovement(Vector2 movement){
    if (current == null) return;

    if (movement.y > 0.1f) IncrementMovement("up");
    if (movement.y < -0.1f) IncrementMovement("down");
    if (movement.x < -0.1f) IncrementMovement("left");
    if (movement.x > 0.1f) IncrementMovement("right");
  }

  public void RecordMovementDelta(Vector3 worldDelta, Transform reference){
    if (current == null) return;
    if (worldDelta.sqrMagnitude < 0.0001f) return;

    Vector3 localDelta = reference != null ? reference.InverseTransformDirection(worldDelta) : worldDelta;

    if (worldDelta.y > 0.01f) IncrementMovement("up");
    if (worldDelta.y < -0.01f) IncrementMovement("down");
    if (localDelta.x < -0.01f) IncrementMovement("left");
    if (localDelta.x > 0.01f) IncrementMovement("right");
  }

  public void RecordHookUse(){
    if (current == null) return;
    current.hookUsesCount++;
    SaveLocal();
    Debug.Log("[Analytics] Hook registrado: " + current.hookUsesCount);
  }

  public void UpdateMaxHeight(float height){
    if (current == null) return;
    if (height > current.maxHeightReached)
      current.maxHeightReached = height;
  }

  public void SetPuzzleTotal(int total){
    if (current == null) return;
    current.puzzlesTotal = Mathf.Max(0, total);
    SaveLocal();
  }

  public void RecordPuzzleCompleted(string puzzleId = ""){
    if (current == null) return;
    current.puzzlesCompleted++;
    if (!string.IsNullOrWhiteSpace(puzzleId))
      current.notes = string.IsNullOrWhiteSpace(current.notes) ? puzzleId : current.notes + " | " + puzzleId;
    SaveLocal();
    Debug.Log("[Analytics] Puzzle completado: " + current.puzzlesCompleted);
  }

  public void SetMaxLevelReached(string sceneName){
    if (current == null) return;
    current.maxLevelReached = sceneName ?? string.Empty;
  }

  public void SetSessionOutcome(string outcome){
    if (current == null) return;
    current.sessionOutcome = outcome ?? string.Empty;
  }

  public void StartBossAttempt(string bossId){
    if (current == null) return;
    string key = NormalizeBossKey(bossId);
    current.bossAttempts.TryGetValue(key, out int count);
    current.bossAttempts[key] = count + 1;
    _bossAttemptStartTimes[key] = Time.time;
    SaveLocal();
    Debug.Log("[Analytics] Boss attempt: " + key + " => " + current.bossAttempts[key]);
  }

  public void FinishBossAttempt(string bossId, bool completed){
    if (current == null) return;
    string key = NormalizeBossKey(bossId);

    if (_bossAttemptStartTimes.TryGetValue(key, out float startTime)){
      current.bossClearTimeSeconds[key] = Time.time - startTime;
      _bossAttemptStartTimes.Remove(key);
    }

    if (completed)
      current.hasCompletedGame = true;

    SaveLocal();
    Debug.Log("[Analytics] Boss finish: " + key + " completed=" + completed);
  }

  public void SaveLocal(){
    if(current == null) {
      Debug.LogWarning("[Analytics] SaveLocal ignorado: current es null");
      return;
    }

    if (string.IsNullOrWhiteSpace(current.sessionId)) {
      Debug.LogWarning("[Analytics] sessionId vacio detectado, creando una sesion nueva antes de guardar");
      StartSession(defaultPlayerName);
    }

    current.timeTotalSeconds = (int)(Time.time - sessionStartTime);
    string folder = EnsureAnalyticsFolderExists();
    string safeSessionId = string.IsNullOrWhiteSpace(current.sessionId) ? "SESSION_" + System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") : current.sessionId;
    string path = Path.Combine(folder, safeSessionId + ".json");
    File.WriteAllText(path, BuildJson(current));
    Debug.Log("[Analytics] Guardado local: " + path);

    if (autoSendToServer && !string.IsNullOrWhiteSpace(backendUrl)) {
      Debug.Log("[Analytics] Enviando automaticamente al backend: " + backendUrl);
      SendToServer(backendUrl);
    }
  }

  public void SendToServer(string url){
    if(current == null) return;
    StartCoroutine(PostCoroutine(url));
  }

  IEnumerator PostCoroutine(string url){
    current.timeTotalSeconds = (int)(Time.time - sessionStartTime);
    string json = BuildJson(current);
    Debug.Log("[Analytics] POST body: " + json);

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

  private string BuildJson(GameSessionAnalytics data){
    StringBuilder json = new StringBuilder();
    json.Append("{");
    AppendString(json, "sessionId", data.sessionId); json.Append(",");
    AppendString(json, "timestamp", data.timestamp); json.Append(",");
    AppendString(json, "playerName", data.playerName); json.Append(",");
    AppendNumber(json, "timeTotalSeconds", data.timeTotalSeconds); json.Append(",");
    AppendNumber(json, "maxHeightReached", data.maxHeightReached); json.Append(",");
    AppendNumber(json, "checkpointsReached", data.checkpointsReached); json.Append(",");
    AppendNumber(json, "averageTimePerPuzzleSeconds", data.averageTimePerPuzzleSeconds); json.Append(",");
    AppendNumber(json, "totalDeaths", data.totalDeaths); json.Append(",");
    AppendDictionaryInt(json, "bossAttempts", data.bossAttempts); json.Append(",");
    AppendDictionaryFloat(json, "bossClearTimeSeconds", data.bossClearTimeSeconds); json.Append(",");
    AppendNumber(json, "hookUsesCount", data.hookUsesCount); json.Append(",");
    AppendNumber(json, "puzzlesCompleted", data.puzzlesCompleted); json.Append(",");
    AppendNumber(json, "puzzlesTotal", data.puzzlesTotal); json.Append(",");
    AppendNumber(json, "itemsCollected", data.itemsCollected); json.Append(",");
    AppendStringList(json, "itemIds", data.itemIds); json.Append(",");
    AppendDictionaryInt(json, "movementStats", data.movementStats); json.Append(",");
    AppendString(json, "maxLevelReached", data.maxLevelReached); json.Append(",");
    AppendBool(json, "hasCompletedGame", data.hasCompletedGame); json.Append(",");
    AppendString(json, "sessionOutcome", data.sessionOutcome); json.Append(",");
    AppendString(json, "notes", data.notes);
    json.Append("}");
    return json.ToString();
  }

  private string EnsureAnalyticsFolderExists(){
    string folder = Path.Combine(Application.persistentDataPath, "Analytics");
    Directory.CreateDirectory(folder);
    Debug.Log("[Analytics] Folder ready: " + folder);
    return folder;
  }

  private void IncrementMovement(string direction){
    if (current == null) return;
    if (!current.movementStats.ContainsKey(direction))
      current.movementStats[direction] = 0;

    current.movementStats[direction]++;
  }

  private string NormalizeBossKey(string bossId){
    return string.IsNullOrWhiteSpace(bossId) ? "default_boss" : bossId.Trim();
  }

  private void AppendString(StringBuilder json, string key, string value){
    json.Append('"').Append(key).Append("\":");
    if (value == null) {
      json.Append("null");
      return;
    }
    json.Append('"').Append(EscapeJson(value)).Append('"');
  }

  private void AppendNumber(StringBuilder json, string key, int value){
    json.Append('"').Append(key).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture));
  }

  private void AppendNumber(StringBuilder json, string key, float value){
    json.Append('"').Append(key).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture));
  }

  private void AppendBool(StringBuilder json, string key, bool value){
    json.Append('"').Append(key).Append("\":").Append(value ? "true" : "false");
  }

  private void AppendStringList(StringBuilder json, string key, List<string> values){
    json.Append('"').Append(key).Append("\":");
    if (values == null){
      json.Append("[]");
      return;
    }
    json.Append('[');
    for (int i = 0; i < values.Count; i++){
      if (i > 0) json.Append(',');
      json.Append('"').Append(EscapeJson(values[i] ?? string.Empty)).Append('"');
    }
    json.Append(']');
  }

  private void AppendDictionaryInt(StringBuilder json, string key, Dictionary<string, int> values){
    json.Append('"').Append(key).Append("\":");
    AppendDictionaryBody(json, values, value => value.ToString(CultureInfo.InvariantCulture));
  }

  private void AppendDictionaryFloat(StringBuilder json, string key, Dictionary<string, float> values){
    json.Append('"').Append(key).Append("\":");
    AppendDictionaryBody(json, values, value => value.ToString(CultureInfo.InvariantCulture));
  }

  private void AppendDictionaryBody<T>(StringBuilder json, Dictionary<string, T> values, System.Func<T, string> toText){
    if (values == null || values.Count == 0){
      json.Append("{}");
      return;
    }
    json.Append('{');
    bool first = true;
    foreach (KeyValuePair<string, T> pair in values){
      if (!first) json.Append(',');
      first = false;
      json.Append('"').Append(EscapeJson(pair.Key ?? string.Empty)).Append("\":");
      json.Append(toText(pair.Value));
    }
    json.Append('}');
  }

  private string EscapeJson(string value){
    return value
      .Replace("\\", "\\\\")
      .Replace("\"", "\\\"")
      .Replace("\n", "\\n")
      .Replace("\r", "\\r")
      .Replace("\t", "\\t");
  }
}
