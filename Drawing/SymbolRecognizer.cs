using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PDollarGestureRecognizer;
using QDollarGestureRecognizer;
using System;

public class SymbolRecognizer
{
    [SerializeField]
    private List<Gesture> trainingSet = new List<Gesture>();
    private List<Point> points = new List<Point>();
    private Result gestureResult;
    public float GetScore { get { return gestureResult.Score; } } //return the score of the result of the drawn gesture
    public string GetResultSymbol { get { return gestureResult.GestureClass; } }//return the name of the result of the drawn gesture, trimming to only be the name and not the timestamp
    //Adds the gestures to the training set on start up
    public void LoadTrainingSet()
    {
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("Symbols/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
    }

    //Recognizes the symbol of the current array of line renderers and returns a string with the symbol recognized
    public string RecognizeSymbol(LineRenderer[] lines, List<string> symbolsToRecognize)
    {
        int strokeId = 0;
        foreach (LineRenderer line in lines)
        {
            for (int i = 0; i < line.positionCount; i++)
            {
                Vector3 position = line.GetPosition(i);
                points.Add(new Point(position.x, position.y, strokeId));
            }

            strokeId++;
        }

        Gesture candidate = new Gesture(points.ToArray());
        gestureResult = QPointCloudRecognizer.Classify(candidate, trainingSet.ToArray(), symbolsToRecognize);
        points.Clear();
        return gestureResult.GestureClass + " " + gestureResult.Score;
    }

    public void AddSymbol(LineRenderer[] lines, string symbolName)
    {
        int strokeId = 0;
        foreach (LineRenderer line in lines)
        {
            for (int i = 0; i < line.positionCount; i++)
            {
                Vector3 position = line.GetPosition(i);
                points.Add(new Point((float)Math.Round(position.x, 4), (float)Math.Round(position.y, 4), strokeId));
            }

            strokeId++;
        }

        string fileName = string.Format("{0}/{1}.xml", Application.dataPath + "/Resources/Symbols", symbolName + "_" + System.DateTime.Now.ToString("MM-dd-yy-hh-mm-ss"));
        GestureIO.WriteGesture(points.ToArray(), symbolName, fileName);
        trainingSet.Add(new Gesture(points.ToArray(), symbolName));
    }
}