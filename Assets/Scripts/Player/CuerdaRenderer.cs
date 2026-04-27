using UnityEngine;

/// <summary>
/// Renderiza la cuerda del gancho con animación parabólica.
/// Compatible con GrapplingHook (state machine) y HookSystem (legacy).
/// 
/// Al lanzar el gancho la cuerda se dibuja progresivamente desde el
/// origen (hookOrigin) hasta el destino (hookpoint / objeto).
/// La cuerda sigue una parábola: primero sube y luego baja hasta el destino.
/// Cuando la cuerda llega al destino se tensa (desaparece la parábola).
/// Lo mismo para atracción de objetos.
/// </summary>
public class CuerdaRenderer : MonoBehaviour
{
    [Header("Configuración de Cuerda")]
    [Tooltip("Número de segmentos de la curva (más = más suave)")]
    [SerializeField] private int segmentos = 30;
    [Tooltip("Altura máxima de la parábola en metros (cuánto sube la cuerda al lanzarla)")]
    [SerializeField] private float alturaParabola = 4f;
    [Tooltip("Velocidad a la que la cuerda viaja hacia el destino (unidades/seg)")]
    [SerializeField] private float velocidadCuerda = 45f;
    [Tooltip("Velocidad de transición para tensar la cuerda")]
    [SerializeField] private float velocidadTensar = 10f;

    private LineRenderer lineRenderer;
    private HookSystem hookSystem;
    private GrapplingHook grapplingHook;
    private Rigidbody playerRb;
    
    // Progreso de la animación de la cuerda (0 = en origen, 1 = llegó al destino)
    private float _progresoCuerda;
    // La cuerda ha llegado al destino y debe tensarse
    private bool _cuerdaTensa;
    // Factor de comba actual para suavizar la tensión
    private float _combaFactor;
    // Último estado activo (para detectar reset)
    private bool _wasActive;

    /// <summary>
    /// Indica si la cuerda ha llegado al destino y se ha tensado completamente.
    /// Lo usa PlayerHookState para saber cuándo empezar a mover al jugador.
    /// </summary>
    public bool CuerdaLlegó { get; private set; }

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        hookSystem = GetComponentInParent<HookSystem>();
        grapplingHook = GetComponentInParent<GrapplingHook>();
        
        // Obtener Rigidbody del jugador para cálculo de velocidad
        PlayerStateMachine psm = GetComponentInParent<PlayerStateMachine>();
        if (psm != null) playerRb = psm.Rb;
        
        ResetEstado();
    }

    private void LateUpdate()
    {
        bool activo = false;
        Vector3 origen = transform.position;
        Vector3 destino = Vector3.zero;

        // Prioridad: GrapplingHook (state machine) > HookSystem (legacy)
        if (grapplingHook != null && grapplingHook.IsActive)
        {
            activo = true;
            
            // Si está atrayendo un objeto, la cuerda va hacia él
            if (grapplingHook.IsPulling && grapplingHook.PulledObject != null)
                destino = grapplingHook.PulledObject.position;
            else
                destino = grapplingHook.CurrentTarget;
        }
        else if (hookSystem != null && hookSystem.isHooking)
        {
            activo = true;
            destino = hookSystem.hookTarget;
        }

        lineRenderer.enabled = activo;
        if (activo)
        {
            // Detectar si se acaba de activar (reset)
            if (!_wasActive)
            {
                ResetEstado();
            }
            
            float distancia = Vector3.Distance(origen, destino);
            
            // Avanzar progreso de la cuerda
            if (_progresoCuerda < 1f)
            {
                float incremento = (velocidadCuerda / Mathf.Max(distancia, 0.1f)) * Time.deltaTime;
                _progresoCuerda = Mathf.Clamp01(_progresoCuerda + incremento);
                _cuerdaTensa = false;
                CuerdaLlegó = false;
            }
            else
            {
                // La cuerda ha llegado al destino, tensarla progresivamente
                _cuerdaTensa = true;
            }
            
            // Si está tensa, reducir la comba hasta 0
            if (_cuerdaTensa)
            {
                _combaFactor = Mathf.Lerp(_combaFactor, 0f, Time.deltaTime * velocidadTensar);
                if (_combaFactor < 0.01f)
                {
                    _combaFactor = 0f;
                    CuerdaLlegó = true;
                }
            }
            else
            {
                // Mientras viaja, la comba es 1 (parábola completa)
                _combaFactor = 1f;
            }
            
            DibujarCuerdaParabolica(origen, destino, distancia);
        }
        else
        {
            if (_wasActive)
            {
                ResetEstado();
            }
        }
        
        _wasActive = activo;
    }

    private void ResetEstado()
    {
        _combaFactor = 1f;
        _progresoCuerda = 0f;
        _cuerdaTensa = false;
        CuerdaLlegó = false;
    }

    /// <summary>
    /// Dibuja la cuerda con parábola de lanzamiento:
    ///   - La punta de la cuerda avanza según _progresoCuerda
    ///   - La trayectoria sube primero (como un lanzamiento) y luego baja al destino
    ///   - Cuando llega, la parábola se reduce y la cuerda se tensa
    /// </summary>
    private void DibujarCuerdaParabolica(Vector3 inicio, Vector3 fin, float distanciaTotal)
    {
        // Punto intermedio: hasta donde ha llegado la punta de la cuerda
        Vector3 puntoActual = Vector3.Lerp(inicio, fin, _progresoCuerda);
        
        // Calcular la altura de la parábola escalada por la distancia
        // La parábola es más alta para distancias más largas, con un tope
        float alturaEscalada = alturaParabola * Mathf.Clamp01(distanciaTotal / 15f);
        
        // Dibujar segmentos
        lineRenderer.positionCount = segmentos + 1;
        
        for (int i = 0; i <= segmentos; i++)
        {
            float t = (float)i / segmentos;
            
            // Posición base: lerp lineal entre origen y punto actual de la cuerda
            Vector3 punto = Vector3.Lerp(inicio, puntoActual, t);
            
            // Parábola: y = 4h * t * (1 - t) => máxima en t=0.5
            // Esto crea un arco que sube al principio y baja al final
            // Multiplicado por _combaFactor para tensarse gradualmente
            float arco = 4f * alturaEscalada * t * (1f - t) * _combaFactor;
            
            // La parábola va hacia ARRIBA (lanzamiento), no hacia abajo
            punto.y += arco;
            
            lineRenderer.SetPosition(i, punto);
        }
    }
}
