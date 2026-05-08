using UnityEngine;

public class SwordPuzzleManager : MonoBehaviour
{
    public static SwordPuzzleManager Instance { get; private set; }

    [Header("Player carry")]
    public Transform holdPoint;
    public Camera playerCamera;
    public float highlightDistance = 5f;

    [Header("Puzzle")]
    public PuzzleDoor puzzleDoor;
    public int totalStoneSlots;

    private SwordPickup currentSwordPickup;
    private SwordPickup highlightedSword;
    private StoneSlot highlightedStone;
    private int placedStoneCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (totalStoneSlots <= 0)
            totalStoneSlots = FindObjectsOfType<StoneSlot>().Length;

        AnalyticsManager.Instance?.SetPuzzleTotal(totalStoneSlots);
    }

    private void Update()
    {
        UpdateLookedTargets();

        if (highlightedStone != null && currentSwordPickup != null && Input.GetKeyDown(KeyCode.E))
        {
            TryPlaceSwordOnStone();
        }
    }

    public bool CanPickupSword()
    {
        return currentSwordPickup == null;
    }

    public bool HasSwordInHand()
    {
        return currentSwordPickup != null;
    }

    public bool PickupSword(SwordPickup sword)
    {
        if (sword == null)
            return false;

        if (!CanPickupSword())
            return false;

        if (holdPoint == null)
        {
            Debug.LogWarning("SwordPuzzleManager: holdPoint no está asignado.");
            return false;
        }

        if (highlightedSword == sword)
        {
            highlightedSword.SetHighlighted(false);
            highlightedSword = null;
        }

        currentSwordPickup = sword;
        currentSwordPickup.PickUp(holdPoint);
        return true;
    }

    private void TryPlaceSwordOnStone()
    {
        if (highlightedStone == null || currentSwordPickup == null)
            return;

        if (highlightedStone.PlaceSword(currentSwordPickup))
        {
            currentSwordPickup = null;
            placedStoneCount++;

            if (placedStoneCount >= totalStoneSlots && puzzleDoor != null)
            {
                puzzleDoor.OpenDoor();
                AnalyticsManager.Instance?.RecordPuzzleCompleted("SwordPuzzle");
                Debug.Log("Puzzle completado: puerta abierta.");
            }
        }
    }

    private void UpdateLookedTargets()
    {
        if (playerCamera == null)
            return;

        SwordPickup newLookedSword = null;
        StoneSlot newLookedStone = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, highlightDistance))
        {
            var sword = hit.collider.GetComponentInParent<SwordPickup>();
            if (sword != null && CanPickupSword() && sword.IsPickable)
                newLookedSword = sword;

            var stone = hit.collider.GetComponentInParent<StoneSlot>();
            if (stone != null && currentSwordPickup != null && stone.CanPlaceSword)
                newLookedStone = stone;
        }

        if (highlightedSword != newLookedSword)
        {
            if (highlightedSword != null)
                highlightedSword.SetHighlighted(false);

            highlightedSword = newLookedSword;

            if (highlightedSword != null)
                highlightedSword.SetHighlighted(true);
        }

        if (highlightedStone != newLookedStone)
        {
            if (highlightedStone != null)
                highlightedStone.SetHighlighted(false);

            highlightedStone = newLookedStone;

            if (highlightedStone != null)
                highlightedStone.SetHighlighted(true);
        }
    }
}
