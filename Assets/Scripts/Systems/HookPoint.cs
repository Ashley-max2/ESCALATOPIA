using UnityEngine;

/// <summary>
/// Componente que marca un objeto como punto de gancho válido.
/// Debe estar en objetos con el tag "HookPoint" y layer configurado para hookableMask.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HookPoint : MonoBehaviour
{
    [Header("=== CONFIG ===")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private float highlightRange = 15f;
    
    [Header("=== VISUALS ===")]
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color highlightedColor = Color.green;
    
    private Renderer _renderer;
    private Material _material;
    private Transform _playerTransform;
    private bool _isHighlighted;
    
    public bool IsActive => isActive;
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _material = _renderer.material;
            UpdateColor();
        }
        
        // Ensure proper tag
        if (!CompareTag("HookPoint"))
        {
            Debug.LogWarning($"HookPoint '{gameObject.name}' should have tag 'HookPoint'");
        }
    }
    
    private void Start()
    {
        var player = FindObjectOfType<PlayerStateMachine>();
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }
    
    private void Update()
    {
        if (_playerTransform == null || _material == null) return;
        
        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        bool shouldHighlight = isActive && distance <= highlightRange;
        
        if (shouldHighlight != _isHighlighted)
        {
            _isHighlighted = shouldHighlight;
            UpdateColor();
        }
    }
    
    private void UpdateColor()
    {
        if (_material == null) return;
        
        if (!isActive)
        {
            _material.color = inactiveColor;
        }
        else if (_isHighlighted)
        {
            _material.color = highlightedColor;
        }
        else
        {
            _material.color = activeColor;
        }
    }
    
    /// <summary>
    /// Activa o desactiva el punto de gancho
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        UpdateColor();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? activeColor : inactiveColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, highlightRange);
    }
}
