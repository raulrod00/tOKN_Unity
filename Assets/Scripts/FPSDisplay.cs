using UnityEngine;
using System.Collections;


using System.Collections.Generic;

using UnityEngine.UI;
using System.Linq;

using UXF;

public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;

	public string trialStatus, folder, blockNum, trialNum;

    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>

    void SessionBegin(Session session)
    {
        trialStatus = "Awaiting trial start";
        folder = session.experimentName + " > " + session.ppid + " > " + session.number.ToString();
        trialNum = " ";
        blockNum = " ";
    }

    public void TrialBegin(Trial trial)
    {
        trialStatus = "Trial in progress";
        trialNum = FormatProgress("Trial", trial.number, trial.session.Trials.ToList().Count);
        blockNum = FormatProgress(" of Block", trial.block.number, trial.session.blocks.Count);
    }

    void TrialEnd(Trial trial)
    {
        trialStatus = "Trial finished";
    }

    void SessionEnd(Session session)
    {
        trialStatus = "Session Over, Data saved to: " + folder;
        trialNum = " ";
        blockNum = " ";
        //ResetToNormal();
    }

    string FormatProgress(string variable, int current, int max)
    {
        string fmt = "{0} {1}/{2}";
        if (max == 0)
        {
            return string.Format(fmt, variable, "??", "??");
        }
        else
        {
            return string.Format(fmt, variable, current, max);
        }
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, h-(h/40), w, h);// / 50);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h/40;// / 50;
		style.normal.textColor = new Color(255.0f, 0.0f, 0.0f, 1.0f);

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        string text = "Trial Status: " +  trialStatus + " " + trialNum + blockNum + " " +string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);

        //Rect graphHolder = new Rect(0, 0, 400, 400);

        //GUI.backgroundColor = Color.red;
        //GUI.Button(graphHolder, "red button", style);
        

    }
}