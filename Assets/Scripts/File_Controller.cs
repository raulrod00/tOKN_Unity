using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class File_Controller : MonoBehaviour
{
    private string csvFile;
    private bool loading;
    private string experimentName;
    private string ppid;
    private int sessNum;
    private int numTinB;
    private int currTrial;
    private List<string[]> trialList;

    public string getName()
    {
        return csvFile;
    }

    public void setName(string num)
    {
        csvFile = num;
    }

    ///////
    public void isLoading(bool temp)
    {
        loading = temp;
    }

    public bool getLoadingBool()
    {
        return loading;
    }

    ///////
    public void setExpName(string str)
    {
        experimentName = str;
    }
    public void setPPIDName(string str)
    {
        ppid = str;
    }
    public void sessionNum(int str)
    {
        sessNum = str + 100;
    }

    ///////
    public string getExpName()
    {
        return experimentName;
    }
    public string getPPIDName()
    {
        return ppid;
    }
    public int getSessionNum()
    {
        return sessNum;
    }

    ///////
    public void numTrialsInBlock(int num)
    {
        numTinB = num;
    }

    ///////
    public void currTrials(int num)
    {
        currTrial = num;
    }

    ///////
    public int getNextTrial()
    {
        return currTrial;
    }

    ///////
    public void listHandler(string list)
    {
        trialList = new List<string[]>();
        var combo = list.Split('_');
        for (var i = 0; i < combo.Length; i = i + 3)
        {
            string[] temp = new string[] { combo[i], combo[i + 1], combo[i + 2] };
            trialList.Add(temp);
        }

        numTinB = combo.Length/3;        

    }

    ///////
    public List<string[]> t_List()
    {

        return trialList;

    }
}
