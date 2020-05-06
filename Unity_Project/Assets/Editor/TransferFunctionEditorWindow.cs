﻿/* 
 * Copyright (c) 2019 Matias Lavik MIT License
 * https://github.com/mlavik1/UnityVolumeRendering
 */
using UnityEngine;
using UnityEditor;

public class TransferFunctionEditorWindow : EditorWindow
{
    private TransferFunction tf = null;

    private int movingColPointIndex = -1;
    private int movingAlphaPointIndex = -1;
    private int selectedColPointIndex = -1;
    VolumeController volRend;

   [MenuItem("Volume Rendering/1D Transfer Function Generator")]
    static void ShowWindow()
    {
        TransferFunctionEditorWindow wnd = (TransferFunctionEditorWindow)EditorWindow.GetWindow(typeof(TransferFunctionEditorWindow));
        wnd.Show();
    }

    private Material tfGUIMat = null;
    private Material tfPaletteGUIMat = null;
    private float leftOff = 20.0f;
    private void OnEnable()
    {
        tfGUIMat = Resources.Load("TransferFunctionGUIMat", typeof(Material)) as Material;//.Load<Material>("TransferFunctionGUIMat");
        tfPaletteGUIMat = Resources.Load("TransferFunctionPaletteGUIMat", typeof(Material)) as Material;// as Material;Resources.Load<Material>("TransferFunctionPaletteGUIMat");
    }

    private void OnGUI()
    {
       volRend = FindObjectOfType<VolumeController>();
        if (volRend == null)
            return;
        tf = volRend.getTransferFunction();

        Color oldColour = GUI.color;
        float bgWidth = Mathf.Min(this.position.width - 40.0f, (this.position.height - 50.0f) * 2.0f);
        Rect bgRect = new Rect(leftOff, 0.0f, bgWidth, bgWidth * 0.5f);

        tf.GenerateTexture();

        tfGUIMat.SetTexture("_TFTex", tf.GetTexture());
        tfGUIMat.SetTexture("_HistTex", tf.getHistogramTexture());
        Graphics.DrawTexture(bgRect, tf.GetTexture(), tfGUIMat);

        Texture2D tfTexture = tf.GetTexture();

        tfPaletteGUIMat.SetTexture("_TFTex", tf.GetTexture());
        Graphics.DrawTexture(new Rect(bgRect.x, bgRect.y + bgRect.height + 20, bgRect.width, 20.0f), tfTexture, tfPaletteGUIMat);

        // Colour control points
        for (int iCol = 0; iCol < tf.colourControlPoints.Count; iCol++)
        {
            TFColourControlPoint colPoint = tf.colourControlPoints[iCol];
            Rect ctrlBox = new Rect(bgRect.x + bgRect.width * colPoint.dataValue, bgRect.y + bgRect.height + 20, 10, 20);
            GUI.color = Color.red;
            GUI.skin.box.fontSize = 8;
            GUI.Box(ctrlBox, "|");
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && ctrlBox.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y))) {
                movingColPointIndex = iCol;
                selectedColPointIndex = iCol;
            }
            else if(movingColPointIndex == iCol) {
                colPoint.dataValue = Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f);
            }
            tf.colourControlPoints[iCol] = colPoint;
        }

        // Alpha control points
        for (int iAlpha = 0; iAlpha < tf.alphaControlPoints.Count; iAlpha++) {
            TFAlphaControlPoint alphaPoint = tf.alphaControlPoints[iAlpha];
            Rect ctrlBox = new Rect(bgRect.x + bgRect.width * alphaPoint.dataValue, bgRect.y + (1.0f - alphaPoint.alphaValue) * bgRect.height, 10, 10);
            GUI.color = oldColour;
            GUI.skin.box.fontSize = 6;
            GUI.Box(ctrlBox, "a");
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && ctrlBox.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y))) {
                movingAlphaPointIndex = iAlpha;
            }
            else if (movingAlphaPointIndex == iAlpha) {
                alphaPoint.dataValue = Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f);
                alphaPoint.alphaValue = Mathf.Clamp(1.0f - (Event.current.mousePosition.y - bgRect.y) / bgRect.height, 0.0f, 1.0f);
            }
            tf.alphaControlPoints[iAlpha] = alphaPoint;
        }

        if (Event.current.type == EventType.MouseUp) {
            movingColPointIndex = -1;
            movingAlphaPointIndex = -1;
        }

        if(Event.current.type == EventType.MouseDown && Event.current.button == 1) {
            if (bgRect.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f), Mathf.Clamp(1.0f - (Event.current.mousePosition.y - bgRect.y) / bgRect.height, 0.0f, 1.0f)));
            else
                tf.colourControlPoints.Add(new TFColourControlPoint(Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f), Random.ColorHSV()));
            selectedColPointIndex = -1;
        }

        if(selectedColPointIndex != -1) {
            TFColourControlPoint colPoint = tf.colourControlPoints[selectedColPointIndex];
            colPoint.colourValue = EditorGUI.ColorField(new Rect(bgRect.x, bgRect.y + bgRect.height + 50, 100.0f, 40.0f), colPoint.colourValue);
            tf.colourControlPoints[selectedColPointIndex] = colPoint;
        }

        updateController();
        GUI.color = oldColour;
    }

    private void updateController() {
        tf.GenerateTexture();
        volRend.updateTF(tf.GetTexture());
    }

    public void OnInspectorUpdate() {
        Repaint();
    }
}
