using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RimColorPicker : MonoBehaviour
{
    public float currentHue, currentSat, currentVal;

    [SerializeField] private RawImage hueImage, satValImage, outputImage;

    [SerializeField] private Slider hueSlider;

    private Texture2D hueTexture, svTexture, outputTexture;

    public BD_UIModificationWheelColor _ModificationColor;
    
    public TextMeshProUGUI priceText;

    public void SetPaintType(int index)
    {
        if (_ModificationColor == null)
            return;

        switch (index)
        {
            case 0:
                PlayerPrefs.SetString("WheelPaintType" + PlayerPrefs.GetString("SelectedCarName"), "Mate");
                break;
            case 1:
                PlayerPrefs.SetString("WheelPaintType" + PlayerPrefs.GetString("SelectedCarName"), "Metallic");
                break;
            case 2:
                PlayerPrefs.SetString("WheelPaintType" + PlayerPrefs.GetString("SelectedCarName"), "Chrome");
                break;
        }

        if (_ModificationColor.gameObject.activeInHierarchy)
        {
            _ModificationColor.PreviewImage();
        }
    }

    private void Start()
    {
        if (hueImage == null || satValImage == null || outputImage == null || hueSlider == null || _ModificationColor == null)
        {
            Debug.LogWarning($"{nameof(RimColorPicker)} on '{name}' is missing required references.", this);
            enabled = false;
            return;
        }

        Color.RGBToHSV(_ModificationColor != null &&
            RCCP_SceneManager.Instance != null &&
            RCCP_SceneManager.Instance.activePlayerVehicle != null &&
            RCCP_SceneManager.Instance.activePlayerVehicle._modApplier != null &&
            RCCP_SceneManager.Instance.activePlayerVehicle._modApplier.WheelManager != null
                ? RCCP_SceneManager.Instance.activePlayerVehicle._modApplier.WheelManager._color
                : BD_VehicleUpgrade_WheelManager.FactoryWheelColor,
            out float initialHue, out float initialSat, out float initialVal);
        currentHue = initialHue * 100f;
        currentSat = initialSat;
        currentVal = initialVal;

        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        hueSlider.SetValueWithoutNotify(currentHue);
        UpdateOutputImage();
        
        if (priceText != null)
            priceText.text = _ModificationColor.price.ToString();
    }

    
    private void CreateHueImage()
    {
        hueTexture = new Texture2D(16, 1);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";

        for (int i = 0; i < hueTexture.width; i++)
        {
            hueTexture.SetPixel(i,0,Color.HSVToRGB((float)i / hueTexture.width,1,1f));
        }
        
        hueTexture.Apply();
        hueImage.texture = hueTexture;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValueTexture";

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x,y, Color.HSVToRGB(currentHue/100, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }
        
        svTexture.Apply();
        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "OutputTexture";

        Color currentColor = Color.HSVToRGB(currentHue / 100f, currentSat, currentVal);
        
        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0,i, currentColor);
        }
        
        outputTexture.Apply();

        outputImage.texture = outputTexture;
    }

    private void UpdateOutputImage()
    {
        if (outputTexture == null)
            return;

        Color currentColor = Color.HSVToRGB(currentHue/100, currentSat, currentVal);
        
        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0,i, currentColor);
        }
        
        outputTexture.Apply();

        
        var sceneManager = RCCP_SceneManager.Instance;
        if (sceneManager != null && sceneManager.activePlayerVehicle != null && sceneManager.activePlayerVehicle._modApplier != null &&
            sceneManager.activePlayerVehicle._modApplier.WheelManager != null)
            sceneManager.activePlayerVehicle._modApplier.WheelManager.PreviewColor(currentColor);
    }

    public void SetSV(float S, float V)
    {
        currentSat = Mathf.Clamp01(S);
        currentVal = Mathf.Clamp01(V);
        
        UpdateOutputImage();
    }

    public void UpdateSVImage()
    {
        if (hueSlider == null || svTexture == null)
            return;

        currentHue = Mathf.Clamp(hueSlider.value, 0f, 100f);

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x,y,Color.HSVToRGB(currentHue/100, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }
        
        svTexture.Apply();
        
        UpdateOutputImage();
    }

    private void OnDisable()
    {
        var sceneManager = RCCP_SceneManager.Instance;
        if (sceneManager != null && sceneManager.activePlayerVehicle != null && sceneManager.activePlayerVehicle._modApplier != null &&
            sceneManager.activePlayerVehicle._modApplier.WheelManager != null)
            sceneManager.activePlayerVehicle._modApplier.WheelManager.Initialize();
    }
    private void OnDestroy()
    {
        if (hueTexture != null)
            Destroy(hueTexture);
        if (svTexture != null)
            Destroy(svTexture);
        if (outputTexture != null)
            Destroy(outputTexture);
    }
}
