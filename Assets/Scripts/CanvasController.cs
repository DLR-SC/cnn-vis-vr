﻿using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * Author: Nadine Meißler
 * Date: June 2019
 * Description: Controlls the UI-Elements on the canvases used for the viusalization  
 */

public class CanvasController : MonoBehaviour
{
    // Array for the three different operation canvas, conv - pool - fc
    public Canvas[] canvasOperation;
    // Array for the input and output canvas, input - output
    public Canvas[] canvasInOut;
    // Canvas for layer name
    public Canvas canvasLayerName;
    // Canvas for filter details
    public Canvas canvasFilterDetails;
    // Canvas for feature map details on the right side
    public Canvas canvasFmDetails;
    // Canvas for feature map details on the left side
    public Canvas canvasFmDetailsLeft;
    // Canvas for input selection
    public Canvas canvasInputSelect;
    // Canvas for model input
    public Canvas canvasInput;
    // Canvas for filter-fm link Button
    public Canvas canvasLink;
    // Image background panels which have to change color
    public Image[] colorPanels;

    // Panels with UI components for input/output of different layers
    public Image[] inputPanels;
    public Image[] outputPanels;
    public Image[] fcOutputPanels;
    public Image[] modelOutputPanels;

    // Images in scene which display input image
    public Image[] inputImages;

    public Sprite openLock, closedLock;
    public Button infoBtn1, infoBtn2;

    // Array for GameObjects which have the layer buttons as children
    public GameObject[] layerObjs;
    // Array for GameObjects Detail 3D-Models
    public GameObject[] detailsObjs;

    SpriteLoader spriteLoader;

    // Array for buttons from 3D-Model
    Button[] layerBtns;
    // Array for layer instances
    Layer[] layers;

    Layer selectedLayer;
    int selectedInput, m_FilterID, m_FMID;
    Text filterNum, outNum, inNum, filterName, filterValuesTxt, fmName, fmDimensions;
    Image filterImg, fmImg;
    Image filterPanel, fmPanelIn, fmPanelOut, inputImgPanel;
    Button[] filterBtns, fmBtnsIn, fmBtnsOut;
    float[] colorbarMaxValues;

    // Colors for canvas
    Color32 convColor, poolColor, fcColor, normalColor, buttonColor;

    bool linked;

    private void Awake()
    {
        spriteLoader = this.gameObject.GetComponent<SpriteLoader>();

        // Get button components from canvas
        filterPanel = canvasOperation[0].transform.Find("Filter_Panel").GetComponent<Image>();
        fmPanelIn = canvasInOut[0].transform.Find("FM_Panel").GetComponent<Image>();
        fmPanelOut = canvasInOut[1].transform.Find("FM_Panel").GetComponent<Image>();

        // change the parameter to true if buttons are inactive on start
        filterBtns = filterPanel.GetComponentsInChildren<Button>(true);
        fmBtnsIn = fmPanelIn.GetComponentsInChildren<Button>(true);
        fmBtnsOut = fmPanelOut.GetComponentsInChildren<Button>(true);

        layerBtns = new Button[layerObjs.Length];
        layers = new Layer[layerBtns.Length];

        // get buttons from layer gameobjects
        for (int i = 0; i < layerObjs.Length; i++)
        {
            layerBtns[i] = layerObjs[i].GetComponentInChildren<Button>();
        }

        // reference layer scripts in Array
        for(int i = 0; i < layerBtns.Length; i++)
        {
            layers[i] = layerBtns[i].GetComponent<Layer>();
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        // Start with linked filter and fm details
        linked = true;

        // get text component for number of filters from canvas
        filterNum = canvasOperation[0].transform.Find("FilterNum_Text").GetComponent<Text>();
        // get text component for number of output fms from canvas
        outNum = canvasInOut[1].transform.Find("FeatureM_Text").GetComponent<Text>();
        // get text component for number of input fms from canvas
        inNum = fmPanelIn.transform.Find("FeatureM_Text").GetComponent<Text>();

        // get components from filter detail canvas
        filterName = canvasFilterDetails.transform.Find("Filter_Text").GetComponent<Text>();
        filterValuesTxt = canvasFilterDetails.transform.Find("FilterValues_Text").GetComponent<Text>();
        filterImg = canvasFilterDetails.transform.Find("Filter_Image").GetComponent<Image>();

        // get components from feature map detail canvas
        fmName = canvasFmDetails.transform.Find("FM_Text").GetComponent<Text>();
        fmDimensions = canvasFmDetails.transform.Find("Dimensions_Text").GetComponent<Text>();
        fmImg = canvasFmDetails.transform.Find("FM_Image").GetComponent<Image>();

        // get button component for input image button from canvas_middle_input
        inputImgPanel = canvasInOut[0].transform.Find("InputImg_Panel").GetComponent<Image>();

        // Set colors
        convColor = new Color32(226, 127, 42, 184);
        poolColor = new Color32(226, 56, 42, 184);
        fcColor = new Color32(65, 101, 195, 184);
        normalColor = new Color32(0, 211, 224, 184);
        buttonColor = new Color32(190, 190, 190, 255);

        // Set maximum values for feature map colorbars (minimum ist always 0.0)
        colorbarMaxValues = new float[] {0.89f, 0.92f, 0.91f, 0.96f, 0.91f, 1.14f};

        // set layer IDs, layer types and names
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].SetLayerID(i);

            switch (i)
            {
                case 0:
                    layers[i].SetLayerType("input");
                    layers[i].SetLayerName("Input");
                    break;
                case 1:
                    layers[i].SetLayerType("conv");
                    layers[i].SetLayerName("Convolution 1");
                    layers[i].SetInputOutputLength(1, 32);
                    break;
                case 2:
                    layers[i].SetLayerType("pool");
                    layers[i].SetLayerName("Pooling 1");
                    layers[i].SetInputOutputLength(32, 32);
                    break;
                case 3:
                    layers[i].SetLayerType("fc");
                    layers[i].SetLayerName("Fully Connected 1");
                    layers[i].SetInputOutputLength(32, 10);
                    break;
                case 4:
                    layers[i].SetLayerType("output");
                    layers[i].SetLayerName("Output");
                    break;
            }
        }       
    }

    /*
     * Functions called from buttons in the scene
     */ 

    // Called from layer buttons on 3D model
    // Handles the display of the details of the selected layer
    public void ShowLayerDetails()
    {
        var go = EventSystem.current.currentSelectedGameObject;

        selectedLayer = go.GetComponent<Layer>();

        // Set layer name on UI and show it
        canvasLayerName.GetComponentInChildren<Text>().text = selectedLayer.GetLayerName();
        canvasLayerName.gameObject.SetActive(true);

        // deactivate filter and feature map detail canvas
        canvasFilterDetails.gameObject.SetActive(false);
        canvasFmDetails.gameObject.SetActive(false);
        canvasFmDetailsLeft.gameObject.SetActive(false);

        string type = selectedLayer.GetLayerType();
        
        // Reset highlight color for layer buttons
        //ResetHighlightColor();

        // call next functions depending on layer type
        switch (type)
        {
            case "input":
                Debug.Log("Layer: " + type);
                // Add highlight Color for selected Layer Button
                //layerBtns[selectedLayer.GetLayerID()].GetComponent<Image>().color = normalColor;
                ShowInput();
                break;
            case "conv":
                // Add highlight Color for selected Layer Button
                //layerBtns[selectedLayer.GetLayerID()].GetComponent<Image>().color = convColor;
                ShowConv();
                break;
            case "pool":
                Debug.Log("Layer: " + type);
                // Add highlight Color for selected Layer Button
                //layerBtns[selectedLayer.GetLayerID()].GetComponent<Image>().color = poolColor;
                ShowPool();
                break;
            case "fc":
                Debug.Log("Layer: " + type);
                // Add highlight Color for selected Layer Button
                //layerBtns[selectedLayer.GetLayerID()].GetComponent<Image>().color = fcColor;
                ShowFC();
                break;
            case "output":
                Debug.Log("Layer: " + type);
                // Add highlight Color for selected Layer Button
                //layerBtns[selectedLayer.GetLayerID()].GetComponent<Image>().color = fcColor;
                ShowOutput();
                break;
            default:
                Debug.Log("CanvasController: Unknown layerType");
                break;
        }
    }

    // Called from Buttons which display filter images
    public void ShowFilterDetails()
    {
        // Get selected filter button
        var go = EventSystem.current.currentSelectedGameObject;
        Button btn = go.GetComponent<Button>();

        // get filter ID from sprite name
        string filterID = btn.image.sprite.name.Substring(btn.image.sprite.name.Length - 2);

        // change filter image on new canvas to selected filter image
        filterImg.sprite = btn.image.sprite;

        // Get index of Filter
        int index = -1;

        if (filterID.StartsWith("0"))
        {
            index = Convert.ToInt32(filterID.Substring(1));
        } else
        {
            index = Convert.ToInt32(filterID);
        }

        // change filter name on Canvas
        filterName.text = "Filter " + index.ToString();

        m_FilterID = index;

        // Show filter values on canvas
        ShowFilterValues(index);

        // Show canvas with details
        canvasFilterDetails.gameObject.SetActive(true);

        // Show canvas with link button
        canvasLink.gameObject.SetActive(true);

        if(linked)
        {
            ShowLinkedFM(index);
        }
    }

    // Called from buttons which display feature maps
    // on conv and pool layers
    public void ShowFmDetails()
    {
        // change image
        // change name
        // change dimensions

        // Get selected feature map button
        var go = EventSystem.current.currentSelectedGameObject;
        Button btn = go.GetComponent<Button>();

        string fmID = "00";

        // get feature map ID from sprite name
        if(btn.image.sprite.name.Length == 17)
        {
            fmID = btn.image.sprite.name.Substring(btn.image.sprite.name.Length - 6).Remove(2);
        } else if (btn.image.sprite.name.Length > 17)
        {
            fmID = btn.image.sprite.name.Substring(btn.image.sprite.name.Length - 7).Remove(2);
        }

        // get fmID as int
        int index = -1;

        if (fmID.StartsWith("0"))
        {
            index = Convert.ToInt32(fmID.Substring(1));
        }
        else
        {
            index = Convert.ToInt32(fmID);
        }

        // if button is on layer input canvas show canvasFmDetailsLeft and change fm name and image
        // else show canvasFmDetailsRight and change name, image and dimensions
        if (btn.transform.parent.parent.name.Equals("Canvas_Middle_LayerInput"))
        {
            // change feature map name on Canvas
            canvasFmDetailsLeft.transform.Find("FM_Text").GetComponent<Text>().text = "Feature Map " + index.ToString();
            // change feature map image on new canvas to selected feature map image
            canvasFmDetailsLeft.transform.Find("FM_Image").GetComponent<Image>().sprite = btn.image.sprite;

            switch(selectedLayer.GetLayerType())
            {
                case "pool":
                    canvasFmDetailsLeft.transform.Find("Dimensions_Text").GetComponent<Text>().text = "26x26";
                    break;
                case "fc":
                    canvasFmDetailsLeft.transform.Find("Dimensions_Text").GetComponent<Text>().text = "13x13";
                    break;
            }

            // Show canvas with details
            canvasFmDetailsLeft.gameObject.SetActive(true);
        } else
        {
            // change feature map name on Canvas
            fmName.text = "Feature Map " + index.ToString();

            // change feature map image on new canvas to selected feature map image
            fmImg.sprite = btn.image.sprite;

            // change dimensions text on canvas
            switch (selectedLayer.GetLayerType())
            {
                case "conv":
                    fmDimensions.text = "26x26";
                    // hide x button
                    canvasFmDetails.transform.Find("InteractiveButton").gameObject.SetActive(false);
                    // set m_fmID to index for right canvas
                    m_FMID = index;
                    // show canvas with filter details if is not active
                    // Show or Update Filter if linked
                    if (!canvasFilterDetails.gameObject.activeSelf || linked)
                    {
                        ShowLinkedFilter(index);
                    }
                    break;
                case "pool":
                    fmDimensions.text = "13x13";
                    // show x button
                    canvasFmDetails.transform.Find("InteractiveButton").gameObject.SetActive(true);
                    break;
            }
            // Show canvas with details
            canvasFmDetails.gameObject.SetActive(true); 
        }
    }

    // Called from filter-feature map link button if filter/fm Details are shown
    // Only on Convolutional Layer
    public void Link()
    {
        // Get selected button
        var go = EventSystem.current.currentSelectedGameObject;

        if (linked)
        {
            linked = false;
            go.GetComponent<Button>().transform.Find("Lock_Panel").GetComponent<Image>().sprite = openLock;
            
        } else
        {
            linked = true;
            go.GetComponent<Button>().transform.Find("Lock_Panel").GetComponent<Image>().sprite = closedLock;
            if(canvasFilterDetails.gameObject.activeSelf)
            {
                ShowLinkedFM(m_FilterID);
            }
        }
    }

    // Called from x buttons
    public void Hide()
    {
        var go = EventSystem.current.currentSelectedGameObject;

        go.transform.parent.gameObject.SetActive(false);

        if(go.transform.parent.name.Equals("Canvas_Filter_Details")) {
            canvasFmDetails.gameObject.SetActive(false);
        }

        if (go.name.Equals("XButton_inputSelect"))
        {
            filterPanel.gameObject.SetActive(true);
        }
    }

    public void HideDetailObj()
    {
        var go = EventSystem.current.currentSelectedGameObject;

        // Hide detail 3d Object
        go.transform.parent.parent.gameObject.SetActive(false);

        // reset color for info Buttons
        infoBtn1.image.color = Color.white;
        infoBtn2.image.color = Color.white;
    }

    // Called from buttons from Canvas_Middle_LayerInput
    public void ShowInputSelection()
    {
        // Show input selection canvas
        canvasInputSelect.gameObject.SetActive(true);
        filterPanel.gameObject.SetActive(false);
    }

    // Called from buttons from input selection canvas
    public void ChangeInputImg()
    {
        var go = EventSystem.current.currentSelectedGameObject;

        // Detect which image was selected and save ID in selectedInput
        switch (go.name)
        {
            case "Input_Image_0":
                selectedInput = 0;
                break;
            case "Input_Image_1":
                selectedInput = 1;
                break;
            case "Input_Image_2":
                selectedInput = 2;
                break;
            case "Input_Image_3":
                selectedInput = 3;
                break;
            case "Input_Image_4":
                selectedInput = 4;
                break;
            case "Input_Image_5":
                selectedInput = 5;
                break;
        }

        Debug.Log("New input img: "+selectedInput);

        // Change input images in scene to new input image
        if(spriteLoader.GetInputImages().Length > selectedInput)
        {
            for(int i = 0; i < inputImages.Length; i++)
            {
                inputImages[i].sprite = spriteLoader.GetInputImages()[selectedInput];
            }   
        }

        // change fms according to input selection if conv, pool or fc layer is open
        switch (selectedLayer.GetLayerType())
        {
            case "input":
            case "output":
                break;
            case "conv":
                UpdateFms("conv", "in");
                UpdateFms("conv", "out");
                break;
            case "pool":
                UpdateFms("pool", "in");
                UpdateFms("pool", "out");
                break;
            case "fc":
                UpdateFms("fc", "in");
                break;
            default:
                Debug.Log("CanvasController - Update(): Unknown layerType");
                break;
        }

        // Hide detail canvas for input fms
        //canvasFmDetails.gameObject.SetActive(false);
        canvasFmDetailsLeft.gameObject.SetActive(false);

        // update fm image for canvasFMDetails if the canvas is active
        if(canvasFmDetails.gameObject.activeSelf)
        {
            ShowLinkedFM(m_FMID);
        }

        // Change colorbar maximum values for feautre map detail canvas left and right according to input
        canvasFmDetailsLeft.transform.Find("Colorbar_Image/Max_Text").GetComponent<Text>().text = colorbarMaxValues[selectedInput].ToString(CultureInfo.InvariantCulture);
        canvasFmDetails.transform.Find("Colorbar_Image/Max_Text").GetComponent<Text>().text = colorbarMaxValues[selectedInput].ToString(CultureInfo.InvariantCulture);

        // Show Filter Panel
        filterPanel.gameObject.SetActive(true);

        // Hide input selection canvas
        // Add animation?
        canvasInputSelect.gameObject.SetActive(false);
        
    }

    public void ShowInfoModel()
    {
        var go = EventSystem.current.currentSelectedGameObject;

        switch(go.name) {
            case "Info_Button_1":
                detailsObjs[1].gameObject.SetActive(false);
                detailsObjs[0].gameObject.SetActive(true);
                go.GetComponent<Button>().image.color = buttonColor;
                infoBtn2.image.color = Color.white;
                break;
            case "Info_Button_2":
                detailsObjs[0].gameObject.SetActive(false);
                detailsObjs[1].gameObject.SetActive(true);
                go.GetComponent<Button>().image.color = buttonColor;
                infoBtn1.image.color = Color.white;
                break;
        }
      
    }

    /*
     * private functions
     */ 

    private void ResetHighlightColor()
    {
        // Reset Color for all Layer Name Buttons
        foreach (var tempBtn in layerBtns)
        {
            tempBtn.GetComponent<Image>().color = Color.white;
        }
    }

    // Show all UI elements for model input layer
    private void ShowInput()
    {
        // change color for background panels
        foreach (var img in colorPanels)
        {
            img.color = normalColor;
        }

        // hide other operation canvas if one is active
        foreach (var opcanvas in canvasOperation)
        {
            if (opcanvas.gameObject.activeSelf) { opcanvas.gameObject.SetActive(false); }
        }

        // hide layer input and layer output canvas
        foreach(var tempcanvas in canvasInOut)
        {
            if (tempcanvas.gameObject.activeSelf) { tempcanvas.gameObject.SetActive(false); }
        }

        // Show Canvas
        canvasInput.gameObject.SetActive(true);
    }

    // Show all UI elements for Conv layer
    private void ShowConv()
    {
        // Update input canvas for layer (input image or fms)

        // deactivate Panels for layer input
        foreach (var panel in inputPanels)
        {
            if (panel.gameObject.activeSelf) { panel.gameObject.SetActive(false); }
        }

        // deactivate Panels for layer output
        foreach (var panel in outputPanels)
        {
            if (panel.gameObject.activeSelf) { panel.gameObject.SetActive(false); }
        }

        // if selected layer is first layer after input
        // show input image else show fm buttons
        if (selectedLayer.GetLayerID() == 1)
        {
            // deactivate feature map buttons
            //fmPanelIn.gameObject.SetActive(false);

            // ativate input image buttons
            inputImgPanel.gameObject.SetActive(true);
        }
        else
        {
            // update input fm buttons for layer (feature maps from layer before)
            UpdateFms("conv", "in");

            // activate feature map buttons
            fmPanelIn.gameObject.SetActive(true);
            
            // deativate input image buttons
            //inputImgPanel.gameObject.SetActive(false);
        }

        // update filters for layer
        UpdateFilters();

        // update output for layer (new feature maps)
        UpdateFms("conv", "out");

        // activate output panel for conv layer
        outputPanels[1].gameObject.SetActive(true);

        // change color for background panels
        foreach (var img in colorPanels)
        {
            img.color = convColor;
        }

        // hide layer input and layer output canvas
        foreach (var tempcanvas in canvasInOut)
        {
            if (tempcanvas.gameObject.activeSelf) { tempcanvas.gameObject.SetActive(false); }
        }

        // hide other operation canvas if one is active
        foreach (var opcanvas in canvasOperation)
        {
            if (opcanvas.gameObject.activeSelf) { opcanvas.gameObject.SetActive(false); }
        }

        // show input canvas
        if (canvasInOut[0] != null && !canvasInOut[0].gameObject.activeSelf)
        {
            canvasInOut[0].gameObject.SetActive(true);
        }

        // show Operation Canvas
        if (canvasOperation[0] != null && !canvasOperation[0].gameObject.activeSelf)
        {
            canvasOperation[0].gameObject.SetActive(true);
        }

        // show output canvas
        if (canvasInOut[1] != null && !canvasInOut[1].gameObject.activeSelf)
        {
            canvasInOut[1].gameObject.SetActive(true);
        }
    }

    // show all UI elements for pooling layer
    private void ShowPool()
    {
        // Update input canvas for layer (feature maps from layer before)

        // deactivate Panels for layer input
        foreach (var panel in inputPanels)
        {
            if (panel.gameObject.activeSelf) { panel.gameObject.SetActive(false); }
        }

        // deactivate Panels for layer output
        foreach (var panel in outputPanels)
        {
            if (panel.gameObject.activeSelf) { panel.gameObject.SetActive(false); }
        }

        // activate feature map buttons
        fmPanelIn.gameObject.SetActive(true);

        // activate input panel for pool layer
        inputPanels[2].gameObject.SetActive(true);

        // deativate input image buttons
        //inputImgPanel.gameObject.SetActive(false);

        // activate output panel for pool layer
        outputPanels[0].gameObject.SetActive(true);

        // update sprites of fm buttons with right feature maps
        UpdateFms("pool", "in");

        // Update output canvas for layer (feature maps from pooling layer)
        UpdateFms("pool","out");

        // change color for background panels
        foreach (var img in colorPanels)
        {
            img.color = poolColor;
        }

        // hide layer input and layer output canvas
        foreach (var tempcanvas in canvasInOut)
        {
            if (tempcanvas.gameObject.activeSelf) { tempcanvas.gameObject.SetActive(false); }
        }

        // hide other operation canvas if one is active
        foreach (var opcanvas in canvasOperation)
        {
            if (opcanvas.gameObject.activeSelf) { opcanvas.gameObject.SetActive(false); }
        }

        // show input canvas
        if (canvasInOut[0] != null && !canvasInOut[0].gameObject.activeSelf)
        {
            canvasInOut[0].gameObject.SetActive(true);
        }

        // show Operation Canvas for active layer
        if (canvasOperation[1] != null && !canvasOperation[1].gameObject.activeSelf)
        {
            canvasOperation[1].gameObject.SetActive(true);
        }

        // show output canvas
        if (canvasInOut[1] != null && !canvasInOut[1].gameObject.activeSelf)
        {
            canvasInOut[1].gameObject.SetActive(true);
        }
    }

    // Show all UI elements for fully connected layer
    private void ShowFC()
    {
        // deactivate Panels for layer input
        foreach (var panel in inputPanels)
        {
            if (panel.gameObject.activeSelf) { panel.gameObject.SetActive(false); }
        }

        // deactivate Panels for layer output
        foreach (var panel in outputPanels)
        {
            if (panel.gameObject.activeSelf) { panel.gameObject.SetActive(false); }
        }

        // activate input panel for fc layer
        inputPanels[3].gameObject.SetActive(true);

        // update input fm buttons for layer (feature maps from layer before)
        UpdateFms("fc", "in");

        // activate feature map buttons
        fmPanelIn.gameObject.SetActive(true);

        // deativate input image buttons
        inputImgPanel.gameObject.SetActive(false);
    
        // set values for output canvas

        // change color for background panels
        foreach (var img in colorPanels)
        {
            img.color = fcColor;
        }

        // hide other operation canvas if one is active
        foreach (var opcanvas in canvasOperation)
        {
            if (opcanvas.gameObject.activeSelf) { opcanvas.gameObject.SetActive(false); }
        }

        // hide layer input and layer output canvas
        foreach (var tempcanvas in canvasInOut)
        {
            if (tempcanvas.gameObject.activeSelf) { tempcanvas.gameObject.SetActive(false); }
        }

        // hide all fc output panels
        foreach (var tempcanvas in fcOutputPanels)
        {
            if (tempcanvas.gameObject.activeSelf) { tempcanvas.gameObject.SetActive(false); }
        }

        // show fc output panel for selected input
        fcOutputPanels[selectedInput].gameObject.SetActive(true);

        // show input canvas
        canvasInOut[0].gameObject.SetActive(true);

        // show Operation Canvas for active layer
        if (canvasOperation[2] != null && !canvasOperation[1].gameObject.activeSelf)
        {
            canvasOperation[2].gameObject.SetActive(true);
        }

        // show output canvas
        if (!canvasInOut[2].gameObject.activeSelf)
        {
            canvasInOut[2].gameObject.SetActive(true);
        }
    }

    // Show all UI elements for model output layer
    private void ShowOutput()
    {
        // change color for background panels
        foreach (var img in colorPanels)
        {
            img.color = fcColor;
        }

        // hide other operation canvas if one is active
        foreach (var opcanvas in canvasOperation)
        {
            if (opcanvas.gameObject.activeSelf) { opcanvas.gameObject.SetActive(false); }
        }

        // hide layer input and layer output canvas
        foreach (var tempcanvas in canvasInOut)
        {
            if (tempcanvas.gameObject.activeSelf) { tempcanvas.gameObject.SetActive(false); }
        }

        // hide all model output panels
        foreach (var tempcanvas in modelOutputPanels)
        {
            if (tempcanvas.gameObject.activeSelf) { tempcanvas.gameObject.SetActive(false); }
        }

        // show model output panel for selected input
        modelOutputPanels[selectedInput].gameObject.SetActive(true);

        // Show Output Canvas
        canvasOperation[canvasOperation.Length - 1].gameObject.SetActive(true);
    }

    // Show filter details for filter linked to selected FM
    private void ShowLinkedFilter(int index)
    {
        // change filter name on Canvas
        filterName.text = "Filter " + index.ToString();

        // set m_FilterID to index
        m_FilterID = index;

        // change filter image on new canvas to selected filter image
        filterImg.sprite = filterBtns[index].image.sprite;

        // Show filter values on canvas
        ShowFilterValues(index);

        // Show canvas with filter details
        canvasFilterDetails.gameObject.SetActive(true);
    }

    // Show FM details for FM linked to selected filter
    private void ShowLinkedFM(int index)
    {
        // change feature map name on Canvas
        fmName.text = "Feature Map " + index.ToString();

        // change feature map image on new canvas to selected feature map image
        fmImg.sprite = fmBtnsOut[index].image.sprite;

        // Change dimensions text
        fmDimensions.text = "26x26";

        // set m_fmID to index
        m_FMID = index;

        // hide x button
        canvasFmDetails.transform.Find("InteractiveButton").gameObject.SetActive(false);

        // Show canvas with fm details
        canvasFmDetails.gameObject.SetActive(true);
    }

    // Load and show filter values for filter with given index
    private void ShowFilterValues(int index)
    {
        // load values for filter
        string[,] filterValues = spriteLoader.GetFilterValues(selectedLayer.GetLayerName());

        filterValuesTxt.text = "";

        // Add filter values to Text on filterdetails canvas
        if (index > -1 && index < filterValues.GetLength(0))
        {
            for (int i = 0; i < filterValues.GetLength(1); i++)
            {
                double temp = Convert.ToDouble(filterValues[index, i]);

                if (temp < 0)
                {
                    filterValuesTxt.text += filterValues[index, i].Remove(5) + " ";
                }
                else
                {
                    filterValuesTxt.text += filterValues[index, i].Remove(4) + " ";
                }

                switch (i)
                {
                    case 2:
                        filterValuesTxt.text += "\n";
                        break;
                    case 5:
                        filterValuesTxt.text += "\n";
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            Debug.Log("CanvasController - ShowFilterDetails: invalid index");
            return;
        }
    }

    // Update all filter data to show on conv operation canvas
    private void UpdateFilters()
    {
        // Load sprites from conv1 or conv2 filters depending on layer name
        Sprite[] filterSprites = spriteLoader.GetFilterSprites(selectedLayer.GetLayerName());

        // number of filter/fms
        // length of array with filters = number of filters/fms
        int filterLength = filterSprites.Length;

        if (filterLength < 2)
        {
            Debug.Log("CanvasController - UpdateFilters(): No filter sprites loaded");
            return;
        }

        // Set text for number of filters on canvas
        filterNum.text = filterLength.ToString() + " Filter";

        // Add loaded sprites from resources to buttons on canvas
        if (filterBtns.Length > 0)
        {
            for (int i = 0; i < filterLength; i++)
            {
                filterBtns[i].image.sprite = filterSprites[i];
                filterBtns[i].gameObject.SetActive(true);
            }

            // If there are more Buttons than sprites set remaining buttons to inactive
            if (filterBtns.Length > filterLength)
            {
                for (int i = filterLength; i < filterBtns.Length; i++)
                {
                    filterBtns[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // Update all FM data to show on input or output canvas from different layers
    private void UpdateFms(string layertype, string inOrOut)
    {
        switch (layertype)
        {
            case "conv":
            case "pool":
            case "fc":
                break;
            default:
                Debug.Log("CanvasController - UpdateFms: invalid layertype: " + layertype + ". Valid layertypes: conv, pool, fc");
                return;
        }

        Sprite[] fmSprites;
        Button[] fmBtns;
        int fmNum = 0;

        // Check if fms for input or output should be updated
        // load fms from Sprite Loader for input and/or output
        switch (inOrOut)
        {
            case "in":
                // if selectedLayer is first conv layer no fms have to be loaded
                // input is input image
                if (selectedLayer.GetLayerID() == 1) { return; }

                string layername = layers[selectedLayer.GetLayerID() - 1].GetLayerName();
                Debug.Log("Vorherige Layer: " + layername);

                fmSprites = spriteLoader.GetFmSprites(layername);
                fmBtns = fmBtnsIn;
                fmNum = selectedLayer.GetInputLength();

                // Set text for number of input fms
                inNum.text = fmNum.ToString() + " Feature Maps";
                break;
            case "out":
                fmSprites = spriteLoader.GetFmSprites(selectedLayer.GetLayerName());
                fmBtns = fmBtnsOut;
                fmNum = selectedLayer.GetOutputLength();

                // Set text for number of output fms
                outNum.text = fmNum.ToString() + " Feature Maps";
                break;
            default:
                fmSprites = new Sprite[1];
                fmBtns = new Button[1];
                break;
        }

        if (fmSprites.Length < 2)
        {
            Debug.Log("CanvasController - UpdateFms(): No fm sprites loaded");
            return;
        }
        if (fmBtns.Length < 2)
        {
            Debug.Log("CanvasController - UpdateFms(): No Buttons for fms found");
            return;
        }

        // Add loaded fm sprites from resources to buttons
        if (fmBtns != null)
        {
            // depending on input ID show the fms for selected input image
            switch (selectedInput)
            {
                case 0:
                    for (int i = 0; i < fmNum; i++)
                    {
                        fmBtns[i].image.sprite = fmSprites[i];
                        fmBtns[i].gameObject.SetActive(true);
                    }
                    break;
                case 1:
                    for (int i = fmNum, j = 0; i < (fmNum * 2); i++, j++)
                    {
                        fmBtns[j].image.sprite = fmSprites[i];
                        fmBtns[j].gameObject.SetActive(true);
                    }
                    break;
                case 2:
                    for (int i = (fmNum * 2), j = 0; i < (fmNum * 3); i++, j++)
                    {
                        fmBtns[j].image.sprite = fmSprites[i];
                        fmBtns[j].gameObject.SetActive(true);
                    }
                    break;
                case 3:
                    for (int i = (fmNum * 3), j = 0; i < (fmNum * 4); i++, j++)
                    {
                        fmBtns[j].image.sprite = fmSprites[i];
                        fmBtns[j].gameObject.SetActive(true);
                    }
                    break;
                case 4:
                    for (int i = (fmNum * 4), j = 0; i < (fmNum * 5); i++, j++)
                    {
                        fmBtns[j].image.sprite = fmSprites[i];
                        fmBtns[j].gameObject.SetActive(true);
                    }
                    break;
                case 5:
                    for (int i = (fmNum * 5), j = 0; i < (fmNum * 6); i++, j++)
                    {
                        fmBtns[j].image.sprite = fmSprites[i];
                        fmBtns[j].gameObject.SetActive(true);
                    }
                    break;
                default:
                    Debug.Log("No valid input index selected");
                    break;
            }

            // If there are more Buttons than sprites set remaining buttons to inactive
            if (fmBtns.Length > fmNum)
            {
                for (int i = fmNum; i < fmBtns.Length; i++)
                {
                    fmBtns[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
