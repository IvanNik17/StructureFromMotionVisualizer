using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.EventSystems;


using UnityEngine.Experimental.Rendering;

public class SelectImages : MonoBehaviour {

    public GameObject mainCam;
    public float sensorHeight;

    public float focalLength;

    public GameObject pivotObj;

    public GameObject rawImageObj;

    public Material[] camMaterials;


    public Canvas canvasObj;

    GameObject camHolder;
    GameObject imageHolder;

    List<Vector3> camPositions;

    List<Vector3> camNormals;
    List<Vector3> camPerps;

    public bool backToMain = true;

    GameObject currSelectedObjCam;


    GameObject[] gameObj;
    Texture2D[] textList;

    string[] files;
    string pathPreFix;

    GameObject[] camerasObj;



    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;


    

    Vector2 currClickOnFig;



    Texture2D selectedTexture;


    int worldOrOverlay = 0; // 0-overlay, 1-world


    GameObject hoveredObj;


	// Use this for initialization
	void Start () {

        camHolder = GameObject.Find("CamHolder");
        imageHolder = GameObject.Find("ImageHolder");

        camPositions = new List<Vector3>();
        camNormals = new List<Vector3>();
        camPerps = new List<Vector3>();


        
        string camPath = "Assets/CameraTransformation/";


        string cameraPath = camPath + "cameraPos.txt";

        LoadStuff(cameraPath, out camPositions);


        string normalPath = camPath + "cameraNorm.txt";

        LoadStuff(normalPath, out camNormals);

        string perpPath = camPath + "cameraPerp.txt";

        LoadStuff(perpPath, out camPerps);


        camerasObj = new GameObject[camPositions.Count];

        Vector3 centerPos = Vector3.zero;

        for (int i = 0; i < camPositions.Count; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            cube.transform.name = (i+1).ToString();

            cube.transform.position = camPositions[i];

            cube.transform.localScale = new Vector3(0.8f, 0.4f, 0.1f);

            cube.transform.Rotate(new Vector3(0, 180, 0));

            cube.transform.SetParent(camHolder.transform);

            Quaternion rotation = Quaternion.LookRotation(camNormals[i], camPerps[i]);
            cube.transform.rotation = rotation;



            cube.AddComponent<Camera>();


            cube.GetComponent<Camera>().fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan(sensorHeight / (2 * focalLength));

            cube.GetComponent<Camera>().nearClipPlane = 0.5f;

            cube.GetComponent<Camera>().enabled = false;

            cube.GetComponent<Renderer>().material = camMaterials[0];

            camerasObj[i] = cube;



            centerPos.x += camPositions[i].x;
            centerPos.y += camPositions[i].y;
            centerPos.z += camPositions[i].z;

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

            plane.transform.name = "image_" + (i + 1);

            plane.transform.SetParent(imageHolder.transform);

            plane.transform.tag = "Pics";

            plane.transform.localScale = new Vector3(4f, 1f, 3f);

            plane.transform.position = new Vector3(10, 10, 10);

        }

        centerPos /= camPositions.Count;

        pivotObj.transform.position = centerPos;


        string path = @"D:\CodeForGit_Unity\ProjectWithAnne\ProjectWithAnne\Assets\Images\";

        pathPreFix = @"file://";

        files = System.IO.Directory.GetFiles(path, "*.jpg");

        gameObj = GameObject.FindGameObjectsWithTag("Pics");

        StartCoroutine(LoadImages());



        m_Raycaster = canvasObj.GetComponent<GraphicRaycaster>();
        m_EventSystem = canvasObj.GetComponent<EventSystem>();
		
	}
	
	// Update is called once per frame
	void Update () {


        Ray ray = mainCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);

        RaycastHit hitHover;

        if (Physics.Raycast(ray, out hitHover))
        {

            if (hoveredObj != null)
            {
                hoveredObj.GetComponent<Renderer>().material = camMaterials[0];
            }

            hoveredObj = hitHover.transform.gameObject;

            hoveredObj.GetComponent<Renderer>().material = camMaterials[1];

        }
        else
        {
            if (hoveredObj != null)
            {
                hoveredObj.GetComponent<Renderer>().material = camMaterials[0];
            }
        }


        if (Input.GetMouseButtonDown(0))
        {

            if (backToMain)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log("HERE");
                    mainCam.GetComponent<Camera>().enabled = false;
                    hit.transform.GetComponent<Camera>().enabled = true;

                    //if (currSelectedObjCam != null)
                    //{
                    //    currSelectedObjCam.GetComponent<Renderer>().material = camMaterials[0];
                    //}


                    currSelectedObjCam = hit.transform.gameObject;
                    backToMain = false;

                    //currSelectedObjCam.GetComponent<Renderer>().material = camMaterials[2];

                    
                }

            }
            //else
            //{
            //    currSelectedObjCam.GetComponent<Camera>().enabled = false;
            //    mainCam.GetComponent<Camera>().enabled = true;
            //    backToMain = true;
            //}

        }

        if (Input.GetKeyDown(KeyCode.Escape) && !backToMain)
        {

                currSelectedObjCam.GetComponent<Camera>().enabled = false;
                mainCam.GetComponent<Camera>().enabled = true;
                backToMain = true;

                rawImageObj.GetComponent<RawImage>().enabled = false;

                
        }


        if (Input.GetKeyDown(KeyCode.Z) && !backToMain)
        {
            if (rawImageObj.GetComponent<RawImage>().enabled == false)
            {
                rawImageObj.GetComponent<RawImage>().enabled = true;

                selectedTexture = textList[int.Parse(currSelectedObjCam.name) - 1];

                rawImageObj.GetComponent<RawImage>().texture = selectedTexture;
                
                
            }
            else
            {
                rawImageObj.GetComponent<RawImage>().enabled = false;
            }
            

        }

        if (Input.GetKeyDown(KeyCode.X) && !backToMain)
        {
            if (worldOrOverlay == 0)
            {
                canvasObj.GetComponent<Canvas>().worldCamera = currSelectedObjCam.GetComponent<Camera>();

                worldOrOverlay = 1;
            }
            else if (worldOrOverlay == 1)
            {
                canvasObj.GetComponent<Canvas>().worldCamera = null;

                worldOrOverlay = 0;
            }
            
        }


        //if (Input.GetKeyDown(KeyCode.Mouse0) && !backToMain)
        //{
        //    //Set up the new Pointer Event
        //    m_PointerEventData = new PointerEventData(m_EventSystem);
        //    //Set the Pointer Event Position to that of the mouse position
        //    m_PointerEventData.position = Input.mousePosition;

        //    //Create a list of Raycast Results
        //    List<RaycastResult> results = new List<RaycastResult>();

        //    //Raycast using the Graphics Raycaster and mouse click position
        //    m_Raycaster.Raycast(m_PointerEventData, results);

        //    //For every result returned, output the name of the GameObject on the Canvas hit by the Ray



        //    foreach (RaycastResult result in results)
        //    {
        //        Debug.Log(result.screenPosition);
        //        float xPos = result.screenPosition.x - (Screen.width / 2) + ((rawImageObj.GetComponent<RectTransform>().rect.width * rawImageObj.GetComponent<RectTransform>().localScale.x) / 2);

        //        float yPos = result.screenPosition.y - (Screen.height / 2) + ((rawImageObj.GetComponent<RectTransform>().rect.height * rawImageObj.GetComponent<RectTransform>().localScale.y) / 2);

        //        var tex = new Texture2D(Screen.width, Screen.height);

        //        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        //        tex.Apply();

        //        Color pixelPos = tex.GetPixel( (int)result.screenPosition.x, (int)result.screenPosition.y );

        //        Debug.Log(pixelPos);
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.Mouse0) && !backToMain)
        {

            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray



            foreach (RaycastResult result in results)
            {
                
                

                


                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        float xPos = result.screenPosition.x + i - (Screen.width / 2) + ((rawImageObj.GetComponent<RectTransform>().rect.width * rawImageObj.GetComponent<RectTransform>().localScale.x) / 2);

                        float yPos = result.screenPosition.y + j - (Screen.height / 2) + ((rawImageObj.GetComponent<RectTransform>().rect.height * rawImageObj.GetComponent<RectTransform>().localScale.y) / 2);

                        float xPos_norm = xPos / (rawImageObj.GetComponent<RectTransform>().rect.width * rawImageObj.GetComponent<RectTransform>().localScale.x);
                        float yPos_norm = yPos / (rawImageObj.GetComponent<RectTransform>().rect.height * rawImageObj.GetComponent<RectTransform>().localScale.y);

                        Debug.Log(selectedTexture.GetPixel((int)(xPos_norm * selectedTexture.width), (int)(yPos_norm * selectedTexture.height)));

                        selectedTexture.SetPixel((int)(xPos_norm * selectedTexture.width), (int)(yPos_norm * selectedTexture.height), Color.red);

                        selectedTexture.Apply();

                        



                        
                        

                        //for (int y = 0; y < selectedTexture.height; y++)
                        //{
                        //    for (int x = 0; x < selectedTexture.width; x++)
                        //    {

                        //        targetTexture.SetPixel(x, y, Color.green);

                        //    }
                        //}
                        


                       // rawImageObj.GetComponent<RawImage>().texture = textList[1];
                        rawImageObj.GetComponent<RawImage>().texture = selectedTexture;

                       
                       
                    }
                }


                

                //Debug.Log(selectedTexture.GetPixel((int)(xPos_norm * selectedTexture.width), (int)(yPos_norm * selectedTexture.height)));

                //textList[int.Parse(currSelectedObjCam.name) - 1] = selectedTexture;


               // rawImageObj.GetComponent<RawImage>().texture = textList[1];
               

                //currClickOnFig = result.screenPosition;
                //Debug.Log(xPos + " | " + yPos);

                //Debug.Log((int) (xPos_norm * selectedTexture.width) + "  " + (int) (yPos_norm * selectedTexture.height));
            }


            
        }







	}




    void LoadStuff(string loadPath, out List<Vector3> loadTo)
    {
        StreamReader reader = new StreamReader(loadPath);

        loadTo = new List<Vector3>();
        string itemStrings = reader.ReadLine();
        char[] delimiter = { ',' };

        while (itemStrings != null)
        {
            string[] fields = itemStrings.Split(delimiter);



            Vector3 currVec3 = Vector3.zero;
            for (int i = 0; i < fields.Length; i++)
            {
                //Debug.Log(i + "  " + fields[i]);
                if (i == 0)
                {
                    currVec3[i] = -float.Parse(fields[i],
                      CultureInfo.InvariantCulture);

                }
                else
                {
                    currVec3[i] = float.Parse(fields[i],
                      CultureInfo.InvariantCulture);

                }


                // Debug.Log(currVec3);

            }


            loadTo.Add(currVec3);

            itemStrings = reader.ReadLine();
        }
    }


    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }


    private IEnumerator LoadImages()
    {
        //load all images in default folder as textures and apply dynamically to plane game objects.
        //6 pictures per page
        textList = new Texture2D[files.Length];

        int counter = 0;
        foreach (string tstring in files)
        {

            string pathTemp = pathPreFix + tstring;
            WWW www = new WWW(pathTemp);
            yield return www;
            Texture2D texTmp = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
            www.LoadImageIntoTexture(texTmp);

            textList[counter] = texTmp;
            //gameObj[counter].GetComponent<Renderer>().material.SetTexture("_MainTex", texTmp);





            //gameObj[counter].transform.position = mainCam.transform.position + mainCam.transform.forward;

            //gameObj[counter].transform.LookAt(mainCam.transform.position);

            //gameObj[counter].transform.Rotate(new Vector3(90, 0, 180));

            //rawImageObj.GetComponent<RawImage>().enabled = true;
            //rawImageObj.GetComponent<RawImage>().texture = texTmp;

            counter++;
            
        }

    }

}
