using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Fove;
using Fove.Unity;
using UXF;

public class Room_Creator : MonoBehaviour
{

    // This object is holding the Sprite that we added to the Editor (the Circle sprite)
    public GameObject myPrefab;

    // This is a List (a data structure) of GameObjects (see above)
    private List<GameObject> circles = new List<GameObject>();

    // This is a List that holds the calculated positions of the other circles
    private List<Vector3> circlePositions = new List<Vector3>();


    // Rotate items on the ceiling, walls and floor relative to the back wall
    // The back wall is the reference
    private Quaternion rotCeil = Quaternion.Euler(90, 0, 0);
    private Quaternion rotWall = Quaternion.Euler(0, 90, 0);

    // Boolean to control room rotation
    private bool spin = false;
    
    // This float holds the distance between the circle's edge and a corner
    private float edgeAdjust;

    // Boolean to determine whether it's the cylinder "Control" condition or
    // the Room condition
    private bool control;

    // This is a List that contains the list of arrays
    // that hold the "Independent" variables (in our case:
    // [rotation speed, buffer (adds a spacer between walls), control, over lap]
    public List<string[]> trialList;

    // Detect whether calibration was selected for the FOVE
    public Toggle calibrate;

    // Declaring a UnityEvent
    // This dude lets you stop a trial mid-session
    UnityEvent m_MyEvent = new UnityEvent();

    // These two things are used for loading a previously
    // done session
    private GameObject f_Handler;
    private File_Controller f_String;

    private bool pauseSession = false;

    private bool loaded = false;

    private bool overlap = false;

    //////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////
    public GameObject windowGraphGO;
    private WindowGraph windowGraph;

    private Texture2D texBoi;
    private RectTransform eyeContainer;
    [SerializeField]
    private RawImage rimage;

    IEnumerator Wait_for_calibration()
    {
        Debug.Log("IsCal?");
        Debug.Log(FoveManager.IsEyeTrackingCalibrating());
        yield return new WaitUntil(() => FoveManager.IsEyeTrackingCalibrating() == false);
        Debug.Log(FoveManager.IsEyeTrackingCalibrating());
    }

    void Awake()
    {
        //////////////////////////////////////////////////////////////////////////////////////////
        
        windowGraph = windowGraphGO.GetComponent<WindowGraph>();
        //eyeContainer = transform.Find("eyeContainer").GetComponent<RectTransform>();

        f_Handler = GameObject.Find("FileHolder");
        f_String = f_Handler.GetComponent<File_Controller>();

        //////////////////////////////////////////////////////////////////////////////////////////
        maxNumGraph = 60;
        graphCount = 0;
        windowGraph.valueList.Add(0.0f);

        //InvokeRepeating("createRandomFeed", 0.2f, 0.1f);
        InvokeRepeating("getTorsionValuesForGraph", 0.1f, 0.1f);
        InvokeRepeating("Temp_Texts", 0.1f, 0.5f);

        ////Eyes Stuff///////////////////////////
        texBoi = new Texture2D(480, 190);
        for (int i = 0; i < 480; i++)
            for (int j = 0; j < 190; j++)
                texBoi.SetPixel(i, j, Color.red);

        texBoi.Apply();
        rimage.texture = texBoi;
        //Apply(rimage, texBoi);

    }

    void Start()
    {
        m_MyEvent.AddListener(MyAction);
        if (calibrate.isOn)
        {
            StartCoroutine(Wait_for_calibration());//FoveManager.WaitForEyeTrackingCalibrationEnd);
        }
        else
            return;

        
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    
    private void Temp_Texts()
    {
        //eyes Handling
        //bkgd = new Color(
        //    Random.Range(0f, 1f),
        //    Random.Range(0f, 1f),
        //    Random.Range(0f, 1f)
        //);

        //texBoi = new Texture2D(480, 480);
        //for (int i = 0; i < 480; i++)
        //    for (int j = 0; j < 480; j++)
        //        texBoi.SetPixel(i, j, bkgd);

        //texBoi.Apply();
        var eyeTexture = FoveManager.GetEyesImage();
        rimage.texture = eyeTexture;//texBoi;
    }
    private float randTors;
    private Color bkgd;

    private void createRandomFeed()
    {
        
        randTors = Random.Range(-11f, 11f);
        //Debug.Log(randTors);
    }

    private int maxNumGraph;
    private int graphCount;

    private void getTorsionValuesForGraph()
    {
        var eyeTorsionL = FoveManager.GetEyeTorsion(Eye.Left);
        //var eyeTorsionL = randTors;

        if (eyeTorsionL < 10 & eyeTorsionL > -10)
        {
            if (windowGraph.valueList.Count < maxNumGraph)
            {
                windowGraph.valueList.Add(eyeTorsionL);
            }
            else
            {
                //windowGraph.valueList.RemoveAt(0);
                //windowGraph.valueList.Add(eyeTorsionL);
                if (graphCount < maxNumGraph)
                {
                    windowGraph.valueList[graphCount] = eyeTorsionL;
                    graphCount++;
                }
                else
                {
                    graphCount = 0;
                    windowGraph.ShowGraph(windowGraph.valueList, -1, (int _i) => "", (float _f) => Mathf.RoundToInt(_f) + "\u00b0");
                    windowGraph.valueList[graphCount] = eyeTorsionL;
                    graphCount++;
                }
            }

        }
        //Debug.Log(eyeTorsionL);

        //windowGraph.ShowGraph(windowGraph.valueList, -1, (int _i) => "" + (_i + 1), (float _f) => Mathf.RoundToInt(_f) + "\u00b0");
        

    }
    //////////////////////////////////////////////////////////////////////////////////////////
    public void GenerateSessions(Session session)
    {
        
        //Canvas canvas = GameObject.Find("[UXF_UI]").GetComponent<Canvas>();
        //canvas.enabled = false;
        GameObject.Find("RecorderObject").GetComponent<GazeRecorder>().shouldRecord = true;

        var overLap = session.settings.GetStringList("overlap");
        var rotSpd = session.settings.GetStringList("rotationSpeed");
        var buff = session.settings.GetStringList("buffer");
        var control = session.settings.GetStringList("control");

        trialList = new List<string[]>();

        if (f_String.getLoadingBool())
        {
            trialList = f_String.t_List();
        }
        else
        {

            foreach (var p in rotSpd)
            {
                foreach (var q in buff)
                {
                    foreach (var r in control)
                    {
                        foreach (var s in overLap)
                        {
                            string[] temp = new string[] { p, q, r, s };
                            trialList.Add(temp);
                        }
                    }
                }
            }

            trialList.Shuffle();
        }
        
        //session.settings.SetValue("trialOrder", trialList);
        //Debug.Log(trialList);

        int totalTrials = (rotSpd.Count * buff.Count * control.Count * overLap.Count);

        session.CreateBlock(totalTrials);

        // Set the number of circles in each wall
        int backCircles = session.settings.GetInt("backCircles");
        int ceilCircles = session.settings.GetInt("ceilCircles");
        int wallCircles = session.settings.GetInt("wallCircles");
        

        // What's the maximum number of circles that need to be created
        count = (backCircles + (2 * ceilCircles) + (2 * wallCircles) + 1);
        //session.settings.SetValue("n_circles", count);


        int width = session.settings.GetInt("width");
        int depth = session.settings.GetInt("depth");
        int height = session.settings.GetInt("height");

        // Coordinate Calculations
        float x_range_min;
        float x_range_max;
        float y_range_min;
        float y_range_max;
        float z_range_min;
        float z_range_max;

        // Calculating coordinates
        x_range_min = -width / 2; // 2.5
        x_range_max = width / 2;
        y_range_min = -height / 2; // 2.5
        y_range_max = height / 2;
        z_range_min = -depth / 2; // 5
        z_range_max = depth / 2;

        session.settings.SetValue("x_range_min", x_range_min);
        session.settings.SetValue("x_range_max", x_range_max);
        session.settings.SetValue("y_range_min", y_range_min);
        session.settings.SetValue("y_range_max", y_range_max);
        session.settings.SetValue("z_range_min", z_range_min);
        session.settings.SetValue("z_range_max", z_range_max);

        FoveManager.TareOrientation();

        //session.Begin();

        if (f_String.getLoadingBool())
        {
            session.SetCurrentTrial(f_String.getNextTrial());
        }

        session.BeginNextTrial();

    }

    private int count;
    // Set the number of circles in each wall
    int backCircles;
    int ceilCircles;
    int wallCircles;

    // Coordinate Calculations
    float x_range_min;
    float x_range_max;
    float y_range_min;
    float y_range_max;
    float z_range_min;
    float z_range_max;

    float scaler;

    float buffer;

    public void GenerateRoom(Trial trial)
    {
        scaler = Session.instance.settings.GetFloat("scaler");

        //count = Session.instance.settings.GetInt("n_circles");

        // Set the number of circles in each wall
        backCircles = Session.instance.settings.GetInt("backCircles");
        ceilCircles = Session.instance.settings.GetInt("ceilCircles");
        wallCircles = Session.instance.settings.GetInt("wallCircles");

        // Adjusts the position of the circles so they don't overlap
        // edgeAdjust = ((scaler - 1) * 1f) + buffer;

        //trialList is arranged in [rotation speed, buffer, control, overlap] order
        buffer = float.Parse(trialList[trial.number - 1][1]);
        control = bool.Parse(trialList[trial.number - 1][2]);
        rotSpeed = float.Parse(trialList[trial.number - 1][0]);
        overlap = bool.Parse(trialList[trial.number - 1][3]);

        edgeAdjust = scaler / 2.0f + buffer;

        Session.instance.CurrentTrial.result["Rotation Speed"] = rotSpeed;
        Session.instance.CurrentTrial.result["Buffer"] = buffer;
        Session.instance.CurrentTrial.result["Control"] = control;
        Session.instance.CurrentTrial.result["Overlap"] = overlap;


        if (loaded)
        {
            
            circles[0].transform.rotation = Quaternion.identity;
            //Instantiates the circles
            for (int i = 1; i < count; i++)
            {

                if (control)
                {
                    if (i < backCircles + 1)
                    {
                        circles[i].transform.position = circlePositions[i];
                    }
                    else
                    {
                        circles[i].transform.position = circlePositions[i];

                        circles[i].transform.LookAt(new Vector3(0, 0, circlePositions[i].z));
                    }

                }
                else
                {

                    if (i < backCircles + 1) // Circles in the Back wall
                    {
                        circles[i].transform.position = circlePositions[i];
                        circles[i].transform.rotation = Quaternion.identity;
                    }
                    else if (i < (backCircles + ceilCircles + 1))
                    {
                        circles[i].transform.position = circlePositions[i]; ;
                        circles[i].transform.rotation = rotCeil;
                    }
                    else if (i < (backCircles + (2 * ceilCircles) + 1))
                    {
                        circles[i].transform.position = circlePositions[i]; ;
                        circles[i].transform.rotation = rotCeil;
                    }
                    else if (i < (backCircles + (2 * ceilCircles) + wallCircles + 1))
                    {
                        circles[i].transform.position = circlePositions[i]; ;
                        circles[i].transform.rotation = rotWall;
                    }
                    else if (i < (backCircles + (2 * ceilCircles) + (2 * wallCircles) + 1))
                    {
                        circles[i].transform.position = circlePositions[i]; ;
                        circles[i].transform.rotation = rotWall;
                    }

                }

                circles[i].transform.gameObject.SetActive(true);

                // Scale the circles relative to 1 Unity Unit
                circles[i].transform.localScale = new Vector3(scaler, scaler, 1);

            }
        }
    }

    public void makeCircles()
    {
        scaler = Session.instance.settings.GetFloat("scaler");

        Debug.Log("Moo");

        // Set the number of circles in each wall
        backCircles = Session.instance.settings.GetInt("backCircles");
        ceilCircles = Session.instance.settings.GetInt("ceilCircles");
        wallCircles = Session.instance.settings.GetInt("wallCircles");

        // Coordinate Calculations
        x_range_min = Session.instance.settings.GetFloat("x_range_min");
        x_range_max = Session.instance.settings.GetFloat("x_range_max");
        y_range_min = Session.instance.settings.GetFloat("y_range_min");
        y_range_max = Session.instance.settings.GetFloat("y_range_max");
        z_range_min = Session.instance.settings.GetFloat("z_range_min");
        z_range_max = Session.instance.settings.GetFloat("z_range_max");

        // Adjusts the position of the circles so they don't overlap
        // edgeAdjust = ((scaler - 1) * 1f) + buffer;

        //trialList is arranged in [rotation speed, buffer, control, overlap] order
        buffer = float.Parse(trialList[Session.instance.CurrentTrial.number - 1][1]);
        control = bool.Parse(trialList[Session.instance.CurrentTrial.number - 1][2]);
        rotSpeed = float.Parse(trialList[Session.instance.CurrentTrial.number - 1][0]);
        overlap = bool.Parse(trialList[Session.instance.CurrentTrial.number - 1][3]);

        edgeAdjust = scaler / 2.0f + buffer;

        Session.instance.CurrentTrial.result["Rotation Speed"] = rotSpeed;
        Session.instance.CurrentTrial.result["Buffer"] = buffer;
        Session.instance.CurrentTrial.result["Control"] = control;
        Session.instance.CurrentTrial.result["Overlap"] = overlap;

        loaded = true;
        circles.Add(Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject);
        circlePositions.Add(circles[0].transform.position);

        //Instantiates the circles
        for (int i = 1; i < count; i++)
        {

            if (i < backCircles + 1) // Circles in the Back wall
                circles.Add(Instantiate(myPrefab, new Vector3(Random.Range(x_range_min + edgeAdjust, x_range_max - edgeAdjust), Random.Range(y_range_min + edgeAdjust, y_range_max - edgeAdjust), z_range_min), Quaternion.identity) as GameObject);
            else if (i < (backCircles + ceilCircles + 1)) // Circles in the Ceiling
                circles.Add(Instantiate(myPrefab, new Vector3(Random.Range(x_range_min + edgeAdjust, x_range_max - edgeAdjust), y_range_min, (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust))), rotCeil) as GameObject);
            else if (i < (backCircles + (2 * ceilCircles) + 1)) // Circles on the Floor
                circles.Add(Instantiate(myPrefab, new Vector3(Random.Range(x_range_min + edgeAdjust, x_range_max - edgeAdjust), y_range_max, (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust))), rotCeil) as GameObject);
            else if (i < (backCircles + (2 * ceilCircles) + wallCircles + 1)) // Circles on the Left Wall
                circles.Add(Instantiate(myPrefab, new Vector3(x_range_max, Random.Range(y_range_min + edgeAdjust, y_range_max - edgeAdjust), (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust))), rotWall) as GameObject);
            else if (i < (backCircles + (2 * ceilCircles) + (2 * wallCircles) + 1)) // Circles on the Right Wall
                circles.Add(Instantiate(myPrefab, new Vector3(x_range_min, Random.Range(y_range_min + edgeAdjust, y_range_max - edgeAdjust), (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust))), rotWall) as GameObject);

            if (control)
            {
                if (i < backCircles + 1)
                {
                    Vector3 pt = Random.insideUnitSphere * x_range_max;

                    Vector3 posi = new Vector3(pt.x, pt.y, z_range_min);
                    circles[i].transform.position = posi;
                }
                else
                {
                    float angle = Random.Range(0.0f, 2 * Mathf.PI);
                    float y = Mathf.Cos(angle) * (x_range_max - edgeAdjust);
                    float x = Mathf.Sin(angle) * (y_range_max - edgeAdjust);
                    float z = (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust));

                    Vector3 posi = new Vector3(x, y, z);
                    circles[i].transform.position = posi;

                    Vector3 temp = circles[i].transform.position;
                    circles[i].transform.LookAt(new Vector3(0, 0, temp.z));
                }

            }

            circlePositions.Add(circles[i].transform.position);
            // Setting the initial circles[] as the Parent Object
            // This allows any transfomr applied to the Parent to effect the Children as well
            circles[i].transform.parent = circles[0].transform;

            // Scale the circles relative to 1 Unity Unit
            circles[i].transform.localScale = new Vector3(scaler, scaler, 1);
            circles[i].SetActive(false);

        }
        // Creates a fixation point at index count (since c# is 0 indexed, any code to alter circles would be < count)
        circles.Add(Instantiate(myPrefab, new Vector3(0, 0, -4.97f), Quaternion.identity) as GameObject);
        circles[count].transform.localScale = new Vector3(0.25f, 0.25f, 1);
        circles[count].GetComponent<SpriteRenderer>().color = UnityEngine.Color.red;
        circles[0].GetComponent<Renderer>().enabled = false;
    }


    public void Delay()
    {
        StartCoroutine(Spin_delay());
    }

    private float rotSpeed;

    private bool inDark = false;
    private bool first = false;

    void Update()
    {
        if (spin)
        {

            circles[0].transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
            GameObject.Find("RecorderObject").GetComponent<GazeRecorder>().frameMarker = true;

        }

        if (Input.GetKeyDown("`") && m_MyEvent != null)
        {
            m_MyEvent.Invoke();
        }

        if (inDark)
        {
            NextPositions();
            inDark = false;
        }

        if (first)
        {
            NextPositions();
            GameObject.Find("RecorderObject").GetComponent<GazeRecorder>().frameMarker = false;
            first = false;
        }

    }

    public void NextPositions()
    {
        if (Session.instance.CurrentTrial.number == trialList.Count)
            return;

        if (first)
            control = bool.Parse(trialList[Session.instance.CurrentTrial.number - 1][2]);
        else
            control = bool.Parse(trialList[Session.instance.CurrentTrial.number][2]);

        for (int i = 1; i < count; i++)
        {
            if (control)
            {
                if (i < backCircles + 1)
                {
                    Vector3 pt = Random.insideUnitCircle * (x_range_max-(edgeAdjust + edgeAdjust/2));

                    Vector3 posi = new Vector3(pt.x, pt.y, z_range_min);
                    circlePositions[i] = posi;
                }
                else
                {
                    float angle = Random.Range(0.0f, 2 * Mathf.PI);
                    float y = Mathf.Cos(angle) * (x_range_max - edgeAdjust);
                    float x = Mathf.Sin(angle) * (y_range_max - edgeAdjust);
                    float z = (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust));

                    Vector3 posi = new Vector3(x, y, z);
                    circlePositions[i] = posi;
                }
                if (!overlap && i > 1)
                {
                    for (int j = (i - 1); j > 0; j--)
                    {
                        float dist = Vector3.Distance(circlePositions[j], circlePositions[i]);
                        if (dist < scaler)
                        {
                            circlePositions[i] = circlePositions[j];
                            if (i > backCircles + 1)
                            {
                                Vector3 temp = circles[i].transform.position;
                                circles[i].transform.LookAt(new Vector3(0, 0, temp.z));
                            }
                        }
                    }
                }
            }
            else
            {
                int second = (backCircles + ceilCircles + 1);
                int third = (backCircles + (2 * ceilCircles) + 1);
                int fourth = (backCircles + (2 * ceilCircles) + wallCircles + 1);
                int fifth = (backCircles + (2 * ceilCircles) + (2 * wallCircles) + 1);

                if (i < backCircles + 1) // Circles in the Back wall
                {
                    circlePositions[i] = new Vector3(Random.Range(x_range_min + edgeAdjust, x_range_max - edgeAdjust), Random.Range(y_range_min + edgeAdjust, y_range_max - edgeAdjust), z_range_min);
                }
                else if (i < second)
                {
                    circlePositions[i] = new Vector3(Random.Range(x_range_min + edgeAdjust, x_range_max - edgeAdjust), y_range_min, (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust)));
                }
                else if (i < third)
                {
                    circlePositions[i] = new Vector3(Random.Range(x_range_min + edgeAdjust, x_range_max - edgeAdjust), y_range_max, (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust)));
                }
                else if (i < fourth)
                {
                    circlePositions[i] = new Vector3(x_range_max, Random.Range(y_range_min + edgeAdjust, y_range_max - edgeAdjust), (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust)));
                }
                else if (i < fifth)
                {
                    circlePositions[i] = new Vector3(x_range_min, Random.Range(y_range_min + edgeAdjust, y_range_max - edgeAdjust), (Random.Range(z_range_min + edgeAdjust, z_range_max - edgeAdjust)));
                }

                if (!overlap && i > 1)
                {
                    if (i < backCircles + 1) // Circles in the Back wall
                    {
                        for (int j = (i - 1); j > 0; j--)
                        {
                            float dist = Vector3.Distance(circlePositions[j], circlePositions[i]);
                            if (dist < scaler)
                            {
                                circlePositions[i] = circlePositions[j];
                            }
                        }
                    }
                    else if (i < second)
                    {
                        for (int j = (i - 1); j > (backCircles); j--)
                        {
                            float dist = Vector3.Distance(circlePositions[j], circlePositions[i]);
                            if (dist < scaler)
                            {
                                circlePositions[i] = circlePositions[j];
                            }
                        }
                    }
                    else if (i < third)
                    {
                        for (int j = (i - 1); j > (second-1); j--)
                        {
                            float dist = Vector3.Distance(circlePositions[j], circlePositions[i]);
                            if (dist < scaler)
                            {
                                circlePositions[i] = circlePositions[j];
                            }
                        }
                    }
                    else if (i < fourth)
                    {
                        for (int j = (i - 1); j > (third - 1); j--)
                        {
                            float dist = Vector3.Distance(circlePositions[j], circlePositions[i]);
                            if (dist < scaler)
                            {
                                circlePositions[i] = circlePositions[j];
                            }
                        }
                    }
                    else if (i < fifth)
                    {
                        for (int j = (i - 1); j > (fourth - 1); j--)
                        {
                            float dist = Vector3.Distance(circlePositions[j], circlePositions[i]);
                            if (dist < scaler)
                            {
                                circlePositions[i] = circlePositions[j];
                            }
                        }
                    }

                    
                }
            }
        }
    }

    public void MyAction()
    {
        var tnum = trialList.Count;
        string[] cow = new string[tnum];

        int ctr = 0;
        foreach (var nums in trialList)
        {
            cow[ctr] = string.Join(", ", nums);
            ctr += 1;
        }

        var temp = string.Join(", ", cow);
        Session.instance.CurrentTrial.result["TrialList"] = temp;
        pauseSession = true;
    }

    private bool tick = true;

    IEnumerator Spin_delay()
    {
        //FoveManager.TareOrientation();
        GameObject.Find("RecorderObject").GetComponent<GazeRecorder>().frameMarker = true;
        first = true;
        if (tick)
        {
            yield return new WaitForSeconds(1);
            tick = false;
        }
        GenerateRoom(Session.instance.CurrentTrial);

        yield return new WaitForSeconds(2); // Wait a few seconds before spinning begins New Trials start here

        spin = true;
        
        yield return new WaitForSeconds(5); // Spinning time
        spin = false;
        foreach (Transform child in circles[0].transform)
        {
            child.gameObject.SetActive(false);
        }
        inDark = true;
        GameObject.Find("RecorderObject").GetComponent<GazeRecorder>().frameMarker = false;

        yield return new WaitForSeconds(2); // Pause for a few seconds after spin

        if (pauseSession)
        {

            Session.instance.End();
        }
        Session.instance.EndCurrentTrial();
        Session.instance.BeginNextTrial();
    }

}
