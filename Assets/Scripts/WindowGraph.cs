using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowGraph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    public List<float> valueList;

    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;

    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    private RectTransform windowGraph;

    private List<GameObject> gameObjectList;

    private void Awake()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        windowGraph = transform.GetComponent<RectTransform>();

        windowGraph.transform.position = new Vector2 (Screen.width - (540/2), (400/2));
        //CreateCircle(new Vector2(200, 200));

        gameObjectList = new List<GameObject>();

        valueList = new List<float>() { 0 };
        //ShowGraph(valueList, -1 ,(int _i) => ""+(_i+1),(float _f) => Mathf.RoundToInt(_f) + "\u00b0");
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    public void ShowGraph(List<float> valueList, int maxVisibleValueAmount = -1 ,Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }

        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }
        if (maxVisibleValueAmount < 0)
        {
            maxVisibleValueAmount = valueList.Count;
        }

        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();


        

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;
        float yMaximum = valueList[0];
        float yMinimum = valueList[0];

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount,0); i < valueList.Count; i++)
        {
            float value = valueList[i];
            if (value > yMaximum)
            {
                yMaximum = value;
            }
            if (value < yMinimum)
            {
                yMinimum = value;
            }
        }

        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0)
        {
            yDifference = 5f;
        }
        yMaximum = 10;// yMaximum + ((yDifference) * 0.1f);
        yMinimum = -10;// yMinimum - ((yDifference) * 0.1f);


        float xSize = graphWidth/(maxVisibleValueAmount+1);

        int xIndex = 0;

        GameObject lastCircleGameObject = null;

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            float xPosition = xSize + xIndex * xSize;
            float yPosition = ((valueList[i] - yMinimum)/ (yMaximum-yMinimum)) * graphHeight;

            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));

            gameObjectList.Add(circleGameObject);

            if (lastCircleGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(dotConnectionGameObject);
            }

            lastCircleGameObject = circleGameObject;

            //Creates Labels for X-axis
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPosition, 0f);
            labelX.GetComponent<Text>().text = getAxisLabelX(i);
            gameObjectList.Add(labelX.gameObject);

            //Creates dashes
            //RectTransform dashX = Instantiate(dashTemplateX);
            //dashX.SetParent(graphContainer, false);
            //dashX.gameObject.SetActive(true);
            //dashX.anchoredPosition = new Vector2(xPosition, 0f);
            //gameObjectList.Add(dashX.gameObject);

            xIndex++;

        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            // Creates label for Y-axis
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(-0f, normalizedValue*graphHeight);
            labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum-yMinimum)));
            gameObjectList.Add(labelY.gameObject);


            // Creates dashes for Y
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(-0f, normalizedValue * graphHeight);
            gameObjectList.Add(dashY.gameObject);
        }


    }

    private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(255, 0, 0, 1f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);

        float Angle = Mathf.Atan2(dir.y, dir.x);
        float degAngle = (Angle * Mathf.Rad2Deg);

        rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;

        rectTransform.localEulerAngles = new Vector3(0, 0, degAngle);

        return gameObject;
    }

}
