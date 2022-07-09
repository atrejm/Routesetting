using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class controller : MonoBehaviour {

    public menuController menuController;

    //public GameObject volumePrefab;
    //public GameObject climbingWallPrefab;
    //public GameObject climbingHoldPrefab;
    public Button volumeBtn;
    public Button holdBtn;

    public Image holdColorPreview;
    public Slider RColorH; // color slider for holds
    public Slider GColorH; // color slider for holds
    public Slider BColorH; // color slider for holds

    public Image volumeColorPreview;
    public Slider RColorV; // color slider for volumes
    public Slider GColorV; // color slider for volumes
    public Slider BColorV; // color slider for volumes

    [SerializeField] LayerMask wallLayerMask;
    [SerializeField] LayerMask volumeLayerMask;
    private Vector3 mousePos;
    private Color holdColor;
    private Color volumeColor;
    private bool ableToPlace = false;
    private bool scaling = false;
    private GameObject selectedHold; //the current volume or hold that's being rotated/scaled
    private GameObject currentObjectTypeToPlace; //the current volume or hold prefab that is loaded. This is not deleted when 'selectedHold' is Destroyed()


	// Use this for initialization
	void Start () {
        initializeListeners();
        selectVolume(); // volume is the starting holdType to use
        updateColor();

    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetButtonDown("Fire1"))
        {
            ableToPlace = true;
            mousePos = Input.mousePosition;
            rayCollisionHandling(mousePos);
        }
        if(Input.GetButton("Fire1"))
        {
            scaling = true;
            ableToPlace = false;
        }
        if(Input.GetButtonUp("Fire1"))
        {
            scaling = false;
            ableToPlace = false;
            removeVirtualPlanes();
        }

        if(Input.GetButtonDown("Fire2"))
        {
            mousePos = Input.mousePosition;
            rayCollisionDeleteObject(mousePos);
        }

        if(scaling)
        {
            rotateObject(); // rotate object while mouse is held down
            scaleObject(); // scale object while mouse is held down
        }
	}

    void createObject(Vector3 pos, Vector3 orientation)
    {
        Quaternion rotation = Quaternion.LookRotation(orientation);

        GameObject obj = Instantiate(currentObjectTypeToPlace,pos,rotation); // spawn the hold/volume and designated 'currently selected'
        selectedHold= obj;
        setColor(selectedHold); 

        createVirtualPlane(obj.transform.position, obj.transform.forward); // spawn a virtual plane at this holds position. Used for scaling object

    }

    void rayCollisionHandling(Vector3 mousePos)
    {
        // Check what objects we're hitting in the scene, and change our build-state accordingly
        bool createObj = false; //controls if we can place a hold/volume this iteration
        Vector3 hitpoint = Vector3.zero;
        Vector3 orientation = Vector3.zero;

        RaycastHit hit;
        int walllayermask = 1 << 8; // climbing wall layer mask
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray,out hit, Mathf.Infinity, walllayermask))
        {
            if (ableToPlace)
            {
                //enables placing holds/volumes if the raycast detects the wall
                createObj = true;
                hitpoint = hit.point; // point at which ray hits plane
                orientation = hit.normal;
            }
            
            else
            {
                //ensures that multiple holds/volumes won't be placed after the first frame
                createObj = false;
                hitpoint = hit.point; // point at which ray hits plane
                orientation = hit.normal;

            }
            

        }
        


        if(createObj)
        {
            createObject(hitpoint, orientation);
            createObj = false;
        }
    }

    void rayCollisionDeleteObject(Vector3 mousePos)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Volume") || hit.transform.gameObject.CompareTag("Hold"))
            {
                GameObject GO = hit.transform.gameObject;
                Destroy(GO);
                
            }
        }
    }

    void rotateObject()
    {
        Vector3 hitPoint = new Vector3();
        RaycastHit hit;
        int virtualMask = 1 << 10;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Plane vPlane = new Plane(selectedHold.transform.forward,selectedHold.transform.position);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, virtualMask))
        {
            hitPoint = hit.point;

            Vector3 relativePos = hitPoint - selectedHold.transform.position;

            float targetAngle = Vector3.SignedAngle(selectedHold.transform.right, relativePos, selectedHold.transform.forward);
            selectedHold.transform.RotateAround(selectedHold.transform.forward, targetAngle * Time.deltaTime);
        }   
    } // uses a generated plane orthogonal to the selectedHold to rotate

    void scaleObject()
    {

        Vector3 hitPoint = new Vector3();
        RaycastHit hit;
        int virtualMask = 1 << 10;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Plane vPlane = new Plane(selectedHold.transform.forward,selectedHold.transform.position);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, virtualMask))
        {
            hitPoint = hit.point;

            float scaleFactor = Vector3.Distance(selectedHold.transform.position, hitPoint) * .5f;
            Vector3 scaleChange = scaleFactor * new Vector3(1f, 1f, 1f);

            selectedHold.transform.localScale = scaleChange;
        }
    } // uses a generated plane orthogonal to the selectedHold to scale

    void createVirtualPlane(Vector3 pos, Vector3 normal)
    {
        // create a virtual plane at the location of current game object, used for scaling
        GameObject vPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        vPlane.AddComponent<MeshCollider>();
        vPlane.GetComponent<MeshRenderer>().enabled = false;
        vPlane.transform.localScale *= 5; // plane was too small  by default
        vPlane.layer = 10; // this is the virtual layer for the new plane.

        vPlane.transform.position = pos;
        vPlane.transform.up = normal;
    }

    void removeVirtualPlanes()
    {
        int vPlaneLayer = 10; // virtual layer for virtual planes

        var goArray = FindObjectsOfType<GameObject>();
        var goList = new System.Collections.Generic.List<GameObject>();

        for (int i = 0; i < goArray.Length; i++)
        {
            if(goArray[i].gameObject.layer == vPlaneLayer)
            {
                Destroy(goArray[i]);
            }
        }
    }

    void initializeListeners()
    {
        volumeBtn.onClick.AddListener(selectVolume);
        holdBtn.onClick.AddListener(selectHold);

        RColorH.onValueChanged.AddListener(delegate { updateColor(); });
        GColorH.onValueChanged.AddListener(delegate { updateColor(); });
        BColorH.onValueChanged.AddListener(delegate { updateColor(); });

        RColorV.onValueChanged.AddListener(delegate { updateColor(); });
        GColorV.onValueChanged.AddListener(delegate { updateColor(); });
        BColorV.onValueChanged.AddListener(delegate { updateColor(); });
    }

    public void selectVolume()
    {
        currentObjectTypeToPlace = menuController.GetComponent<menuController>().getVolumeType();
    }

    public void selectHold()
    {
        currentObjectTypeToPlace = menuController.GetComponent<menuController>().getHoldType();

    }

    void updateColor()
    {
        holdColor = new Color(RColorH.value, GColorH.value, BColorH.value);
        volumeColor = new Color(RColorV.value, GColorV.value, BColorV.value);

        holdColorPreview.GetComponent<Image>().color = holdColor;
        volumeColorPreview.GetComponent<Image>().color = volumeColor;
    }

    void setColor(GameObject obj)
    {
        Debug.Log("Updating color: " + obj.tag);
        if (obj.CompareTag("Volume"))
        {
            Debug.Log("Updating volume color");
            obj.GetComponentInChildren<Renderer>().material.color = volumeColor;
        }
        if (obj.CompareTag("Hold"))
        {
            obj.GetComponentInChildren<Renderer>().material.color = holdColor;
        }
    }
}
