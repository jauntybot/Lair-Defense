using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Something quick for the spawn preview function, changes the preview graphic by toggling active states

public class PlacementPreview : MonoBehaviour
{
    [SerializeField] GameObject minion, chest, box;


    public void SwitchPlacementObject(int index)
    {
        switch (index)
        {
            case 0:
                minion.SetActive(true);
                chest.SetActive(false);
                box.SetActive(false);
                break;

            case 1:
                minion.SetActive(false);
                chest.SetActive(true);
                box.SetActive(false);
                break;

            case 2:
                minion.SetActive(false);
                chest.SetActive(false);
                box.SetActive(true);
                break; 
        }
    }
}
