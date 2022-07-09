using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class menuController : MonoBehaviour {

    public Dropdown holdSelection;
    public Dropdown volumeSelection;
    public controller gameController;

    [SerializeField] private GameObject[] volumeTypePrefabs;
    [SerializeField] private GameObject[] holdTypePrefabs;
    private GameObject selectedVolumeType;
    private GameObject selectedHoldType;
	// Use this for initialization
	void Start () {
        
        InitializeTypes(); // set the types of holds/volumes to default
        volumeSelection.onValueChanged.AddListener(delegate { updateVolumeSelection(); });
        holdSelection.onValueChanged.AddListener(delegate { updateHoldSelection(); });

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void updateVolumeSelection()
    {
        Debug.Log("Updating Volume selection to: " + volumeSelection.value + " - " + volumeTypePrefabs[volumeSelection.value]);

        selectedVolumeType = volumeTypePrefabs[volumeSelection.value]; // choose the volume type from the list of available prefabs
        gameController.GetComponent<controller>().selectVolume(); // ensures that the gamecontroller script updates to this new volume type (Kind of ghetto, fix later)
    }

    void updateHoldSelection()
    {
        Debug.Log("Updating hold selection to: " + holdSelection.value + " - " + holdTypePrefabs[holdSelection.value]);

        selectedHoldType = holdTypePrefabs[holdSelection.value]; // choose the volume type from the list of available prefabs
        gameController.GetComponent<controller>().selectHold(); // ensures that the gamecontroller script updates to this new hold type (Kind of ghetto, fix later)
    }

    private void InitializeTypes()
    {
        selectedHoldType = holdTypePrefabs[holdSelection.value];
        selectedVolumeType = volumeTypePrefabs[volumeSelection.value];
    }

    public GameObject getVolumeType()
    {
        return selectedVolumeType;
    }

    public GameObject getHoldType()
    {
        return selectedHoldType;
    }
}
