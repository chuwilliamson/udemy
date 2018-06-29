using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;

public class GridAcademy : Academy
{
    [HideInInspector]
    public List<GameObject> actorObjs;
    [HideInInspector]
    public int[] players;

    public GameObject trueAgent;

    public int gridSize;

    public GameObject camObject;
    Camera cam;
    Camera agentCam;

    public GameObject agentPref;
    public GameObject goalPref;
    public GameObject pitPref;
    GameObject[] objects;

    GameObject plane;
    GameObject sN;
    GameObject sS;
    GameObject sE;
    GameObject sW;

    public override void InitializeAcademy()
    {
        gridSize = (int)resetParameters["gridSize"];
        cam = camObject.GetComponent<Camera>();

        objects = new GameObject[3] {agentPref, goalPref, pitPref};

        agentCam = GameObject.Find("agentCam").GetComponent<Camera>();

        actorObjs = new List<GameObject>();

        plane = GameObject.Find("Plane");
        sN = GameObject.Find("sN");
        sS = GameObject.Find("sS");
        sW = GameObject.Find("sW");
        sE = GameObject.Find("sE");
    }

    public void SetEnvironment()
    {
        cam.transform.position = new Vector3(-((int)resetParameters["gridSize"] - 1) / 2f, 
                                             (int)resetParameters["gridSize"] * 1.25f, 
                                             -((int)resetParameters["gridSize"] - 1) / 2f);
        cam.orthographicSize = ((int)resetParameters["gridSize"] + 5f) / 2f;

        var playersList = new List<int>();

        for (var i = 0; i < (int)resetParameters["numObstacles"]; i++)
        {
            playersList.Add(2);
        }

        for (var i = 0; i < (int)resetParameters["numGoals"]; i++)
        {
            playersList.Add(1);
        }
        players = playersList.ToArray();

        plane.transform.localScale = new Vector3(gridSize / 10.0f, 1f, gridSize / 10.0f);
        plane.transform.position = new Vector3((gridSize - 1) / 2f, -0.5f, (gridSize - 1) / 2f);
        sN.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sS.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sN.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, gridSize);
        sS.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, -1);
        sE.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sW.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sE.transform.position = new Vector3(gridSize, 0.0f, (gridSize - 1) / 2f);
        sW.transform.position = new Vector3(-1, 0.0f, (gridSize - 1) / 2f);

        agentCam.orthographicSize = (gridSize) / 2f;
        agentCam.transform.position = new Vector3((gridSize - 1) / 2f, gridSize + 1f, (gridSize - 1) / 2f);

    }

    public override void AcademyReset()
    {
        foreach (var actor in actorObjs)
        {
            DestroyImmediate(actor);
        }
        SetEnvironment();

        actorObjs.Clear();

        var numbers = new HashSet<int>();
        while (numbers.Count < players.Length + 1)
        {
            numbers.Add(Random.Range(0, gridSize * gridSize));
        }
        var numbersA = numbers.ToArray();

        for (var i = 0; i < players.Length; i++)
        {
            var x = (numbersA[i]) / gridSize;
            var y = (numbersA[i]) % gridSize;
            var actorObj = Instantiate(objects[players[i]]);
            actorObj.transform.position = new Vector3(x, -0.25f, y);
            actorObjs.Add(actorObj);
        }

        var x_a = (numbersA[players.Length]) / gridSize;
        var y_a = (numbersA[players.Length]) % gridSize;
        trueAgent.transform.position = new Vector3(x_a, -0.25f, y_a);

    }

    public override void AcademyStep()
    {

    }
}
