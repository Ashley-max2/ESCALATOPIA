using UnityEngine;

[CreateAssetMenu(fileName = "New Gadget", menuName = "Escalatopia/Gadget")]
public class Gadget : ScriptableObject
{
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public int cost;
    public bool isUnlocked;

    // Optional: Reference to prefab or ability script?
}
