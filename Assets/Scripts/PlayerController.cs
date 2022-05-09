using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Sooo, I wanted this to be a card based deckbuilder type thing,, waaay too ambitious also using pathfinding AI
// Instead, here's a placeholder simple controller to use some buttons
// Allows the player to instantiate lair contents during setup
// If I had the time I would make it so the player could only spawn things within the A* graph / playable area and snap to grid honestly
public class PlayerController : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabs;
    [SerializeField] List<int> maxCount;
    [SerializeField] List<Button> buttons;
    [SerializeField] List<Text> text; // I hate all these lists... they need to be indexed identically in the inspector, but they look so much cleaner
    [SerializeField] PlacementPreview preview; // Class present on the preview object, swaps preview graphic
    [SerializeField]  bool previewing;

    List<int> currentCount;

    private void Start()
    {
        preview.gameObject.SetActive(false);

        currentCount = new List<int>();
        for (int i = 0; i <= maxCount.Count - 1; i++)
        {
            currentCount.Add(maxCount[i]);
        }
        UpdateCountText();
    }

    // Instantiate prefab and adjust currentCount and UI text
    void SpawnContent(int index)
    {
        Instantiate(prefabs[index], preview.transform.position, Quaternion.identity);
        currentCount[index]--;
        UpdateCountText();
        if (currentCount[index] <= 0)
        {
            buttons[index].interactable = false;
        }
    }

    // Activate the faux hologram visualization
    public void PreviewSpawn(int index) {
        previewing = true;
        preview.gameObject.SetActive(true);
        preview.SwitchPlacementObject(index); // Trigger seperate class function thru reference
        StartCoroutine(DisplayPreviewPrefab());
        StartCoroutine(ListenForClick(index));
    }

    // Move the preview to the mouse cursor while active
    IEnumerator DisplayPreviewPrefab() {

        while(previewing)
        {
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            preview.gameObject.transform.position = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0);

            yield return new WaitForEndOfFrame();
        }
        preview.gameObject.SetActive(false);

    }

    // Wait for click while active, spawn object on click
    IEnumerator ListenForClick(int index)
    {
        while (previewing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SpawnContent(index);
                previewing = false;
            }
            yield return null;
        }

    }

    // Update UI Text objects
    void UpdateCountText()
    {
        for (int i = 0; i <= maxCount.Count - 1; i++)
        {
            text[i].text = "x " + currentCount[i].ToString();
        }
    }
}
