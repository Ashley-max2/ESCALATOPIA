using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistenceController : MonoBehaviour
{
    float MaxBar = 100;
    float ResistenceBar;
    public PlayerJump playerJump;
    public float velocidadRegeneracion = 2f;    


    // Start is called before the first frame update
    void Start()
    {
        ResistenceBar = MaxBar;

        if (playerJump == null)
        {
            playerJump = GetComponent<PlayerJump>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool estaEnSuelo = playerJump != null && playerJump.EstaEnSuelo();
        bool presionandoR = Input.GetKey(KeyCode.R);

        // Consumir resistencia si se presiona R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ConsumirResistencia(2f);
        }

        // Regenerar resistencia si está en el suelo y no se está presionando R
        if (estaEnSuelo && !presionandoR)
        {
            ResistenceBar += velocidadRegeneracion * Time.deltaTime;
            ResistenceBar = Mathf.Clamp(ResistenceBar, 0, MaxBar);
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;

        string estado = $"RESISTENCIA: {ResistenceBar:F1}/{MaxBar}";
        GUI.Label(new Rect(10, 160, 300, 100), estado, style);
    }

    public void ConsumirResistencia(float cantidad)
    {
        ResistenceBar -= cantidad;
        ResistenceBar = Mathf.Clamp(ResistenceBar, 0, MaxBar);
    }

    public bool TieneResistencia(float cantidad)
    {
        return ResistenceBar >= cantidad;

    }

}
