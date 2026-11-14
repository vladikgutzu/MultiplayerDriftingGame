using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarBtnUI: MonoBehaviour
{
    public TextMeshProUGUI _priceText;
    public TextMeshProUGUI _carName;
    
    public Button _carBtnMain;
    
    public Image _carBtnImage;
    public Image _carIcon;
    public Image _lockImage;

    public Sprite _boughtCarMainImage;
    public Sprite _boughtCarMainSelectedImage;

    public int _index;

    public GameObject priceGameObject;

}
