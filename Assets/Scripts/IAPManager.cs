#if UNITY_IOS
using System;
using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using DG.Tweening;
//using Facebook.MiniJSON;
#if GAME_ANALYTICS_SDK
using GameAnalyticsSDK;
#endif

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager instance;


    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;


#if UNITY_IOS || UNITY_EDITOR

    string dolar50k = "com.drift.money.pack1"; // 2.99 USD    
    string dolar100k = "com.drift.money.pack2"; // 3.99 USD    
    string dolar150k = "com.drift.money.pack3"; // 5.99 USD    
    string dolar250k = "com.drift.money.pack4"; // 7.99 USD    

    string premiumCoin100 = "com.drift.gem.pack1"; // 8.99 USD     -   100 coins 
    string premiumCoin250 = "com.drift.gem.pack2"; // 13.99 USD    -   250 coins    
    string premiumCoin500 = "com.drift.gem.pack4"; // 19.99 USD    -   500 coins    
    string premiumCoin1000 = "com.drift.gem.pack3"; // 29.99 USD    -   1000 coins    

    string removeAds = "com.drift.removeads"; // 4.99 USD     -   removeAds

    string carIAP1 = "com.drift.car1";
    string carIAP2 = "com.drift.car2";
    string carIAP3 = "com.drift.car3";
    string carIAP4 = "com.drift.car4";

    string xpBoostPack1 = "com.drift.xpboostpack1"; // 0.99 USD     -   XP BATTLE PASS
    string xpBoostPack2 = "com.drift.xpboostpack2"; // 1.99 USD     -   XP BATTLE PASS
    string xpBoostPack3 = "com.drift.xpboostpack3"; // 2.99 USD     -   XP BATTLE PASS

    private string memberSubscription1 = "com.drift.membership"; // 4.99 USD     -   Membership Subscription
    
#endif
    
    [SerializeField] int membershipCarIndex = 0;

    [SerializeField] int carIAPIndex1 = 0;
    [SerializeField] int carIAPIndex2 = 0;
    [SerializeField] int carIAPIndex3 = 0;
    [SerializeField] int carIAPIndex4 = 0;

    //************************** Adjust these methods **************************************
    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        //Step 2 choose if your product is a consumable or non consumable

        builder.AddProduct(dolar50k, ProductType.Consumable);
        builder.AddProduct(dolar100k, ProductType.Consumable);
        builder.AddProduct(dolar150k, ProductType.Consumable);
        builder.AddProduct(dolar250k, ProductType.Consumable);

        builder.AddProduct(premiumCoin100, ProductType.Consumable);
        builder.AddProduct(premiumCoin250, ProductType.Consumable);
        builder.AddProduct(premiumCoin500, ProductType.Consumable);
        builder.AddProduct(premiumCoin1000, ProductType.Consumable);

        builder.AddProduct(xpBoostPack1, ProductType.Consumable);
        builder.AddProduct(xpBoostPack2, ProductType.Consumable);
        builder.AddProduct(xpBoostPack3, ProductType.Consumable);

        builder.AddProduct(removeAds, ProductType.NonConsumable);

        builder.AddProduct(carIAP1, ProductType.NonConsumable);
        builder.AddProduct(carIAP2, ProductType.NonConsumable);
        builder.AddProduct(carIAP3, ProductType.NonConsumable);
        builder.AddProduct(carIAP4, ProductType.NonConsumable);

        builder.AddProduct(memberSubscription1, UnityEngine.Purchasing.ProductType.Subscription, new StoreSpecificIds()
        {
            { memberSubscription1, AppleAppStore.Name },
            { memberSubscription1, GooglePlay.Name },
        });

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }


    //Step 3 Create methods

    public void buy50k()
    {
        BuyProductID(dolar50k);
    }

    public void buy100k()
    {
        BuyProductID(dolar100k);
    }

    public void buy150k()
    {
        BuyProductID(dolar150k);
    }

    public void buy250k()
    {
        BuyProductID(dolar250k);
    }

    public void buy100prem()
    {
        BuyProductID(premiumCoin100);
    }

    public void buy250prem()
    {
        BuyProductID(premiumCoin250);
    }

    public void buy500prem()
    {
        BuyProductID(premiumCoin500);
    }

    public void buy1000prem()
    {
        BuyProductID(premiumCoin1000);
    }

    public void buy45XP()
    {
        BuyProductID(xpBoostPack1);
    }

    public void buy150XP()
    {
        BuyProductID(xpBoostPack2);
    }

    public void buy350XP()
    {
        BuyProductID(xpBoostPack3);
    }

    public void RemoveADS()
    {
        BuyProductID(removeAds);
    }

    public void BuyCarIAP1()
    {
        BuyProductID(carIAP1);
    }

    public void BuyCarIAP2()
    {
        BuyProductID(carIAP2);
    }

    public void BuyCarIAP3()
    {
        BuyProductID(carIAP3);
    }

    public void BuyCarIAP4()
    {
        BuyProductID(carIAP4);
    }

    public void BuyMembershipSubscription()
    {
        BuyProductID(memberSubscription1);
    }

    //Step 4 modify purchasing

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP initialization failed: {error}. {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {

        Debug.Log("Geting the purchease");

#if UNITY_EDITOR
        DOVirtual.DelayedCall(0.5f, () =>
        {
            CloseInfoIfAvailable();
        });
#else
        DOVirtual.DelayedCall(0.5f, () =>
        {
            CloseInfoIfAvailable();
        });
#endif
        //OnProcessPurchase(args);

        TransactionAnalytic();

        if (String.Equals(args.purchasedProduct.definition.id, dolar50k, StringComparison.Ordinal))
        {
            AddCoins(50000);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, dolar100k, StringComparison.Ordinal))
        {
            AddCoins(100000);

        }
        else if (String.Equals(args.purchasedProduct.definition.id, dolar150k, StringComparison.Ordinal))
        {
            AddCoins(150000);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, dolar250k, StringComparison.Ordinal))
        {
            AddCoins(250000);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, premiumCoin100, StringComparison.Ordinal))
        {
            AddPremiumCoins(100);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, premiumCoin250, StringComparison.Ordinal))
        {
            AddPremiumCoins(250);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, premiumCoin500, StringComparison.Ordinal))
        {
            AddPremiumCoins(500);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, premiumCoin1000, StringComparison.Ordinal))
        {
            AddPremiumCoins(1000);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, xpBoostPack1, StringComparison.Ordinal))
        {
            AddXP(45);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, xpBoostPack2, StringComparison.Ordinal))
        {
            AddXP(150);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, xpBoostPack3, StringComparison.Ordinal))
        {
            AddXP(350);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, removeAds, StringComparison.Ordinal))
        {
            RemovingAds();
        }
        else if (String.Equals(args.purchasedProduct.definition.id, carIAP1, StringComparison.Ordinal))
        {
            AddCar1();
        }
        else if (String.Equals(args.purchasedProduct.definition.id, carIAP2, StringComparison.Ordinal))
        {
            AddCar2();
        }
        else if (String.Equals(args.purchasedProduct.definition.id, carIAP3, StringComparison.Ordinal))
        {
            AddCar3();
        }
        else if (String.Equals(args.purchasedProduct.definition.id, carIAP4, StringComparison.Ordinal))
        {
            AddCar4(); 
        }
        else if (String.Equals(args.purchasedProduct.definition.id, memberSubscription1, StringComparison.Ordinal))
        {
            MembershipSubscription1Granted();
        }
        else
        {
            Debug.Log("Purchase Failed");
            ShowInfoIfAvailable("Purchase Failed!", BD_InfoDisplayer.InfoType.Process);
        }

        return PurchaseProcessingResult.Complete;
    }


    private void AddCoins(int coins)
    {
        Debug.Log(" ADD " + coins + " DOLARS ");
        BD_API.AddSoftCurrency(coins);       
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "Dollar_Bought_" + coins);
#endif
    }


    private void AddPremiumCoins(int coins)
    {
        Debug.Log(" ADD " + coins + "PREMIUM COINS ");
        BD_API.AddHardCurrency(coins);
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "PremiumCoins_Bought_" + coins);
#endif

    }

    private void AddXP(int xp)
    {
        BD_PlayerProgression.AddExperience(xp);
        Debug.Log($"Added {xp} XP. Total XP: {BD_PlayerProgression.TotalExperience}");
    }

    private void RemovingAds()
    {
        Debug.Log("No ads Bought");
        PlayerPrefs.SetInt("ADSREMOVED", 1);
        Time.timeScale = 1;
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "ADREMOVED_BOUGHT");
#endif
    }

    private void AddCar1()
    {
        GrantCar(carIAPIndex1);
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "Car1_BOUGHT");
#endif

    }
    private void AddCar2()
    {
        GrantCar(carIAPIndex2);
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "Car2_BOUGHT");
#endif

    }
    private void AddCar3()
    {
        GrantCar(carIAPIndex3);
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "Car3_BOUGHT");
#endif

    }
    private void AddCar4()
    {
        GrantCar(carIAPIndex4);
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "Car4_BOUGHT");
#endif

    }

    private static void GrantCar(int carIndex)
    {
        BD_MainMenuManager menuManager = BD_MainMenuManager.Instance;
        if (menuManager == null)
        {
            Debug.LogError($"Can't grant IAP car index {carIndex}: main menu manager is unavailable.");
            return;
        }

        menuManager.BuyIAPCar(carIndex);
        menuManager.OpenMenu(menuManager._mainMenu);
    }

    private void MembershipSubscription1Granted()
    {
        Debug.Log("Membership granted");
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "MEMBERSHIP",
            "MEBERSHIP_PURCHASED_SUCCESFULL");
#endif
        PlayerPrefs.SetInt("Membership", 1);

        RemovingAds();
        BD_MainMenuManager menuManager = BD_MainMenuManager.Instance;
        if (menuManager != null)
        {
            menuManager.BuyIAPCar(membershipCarIndex);
            menuManager.OpenMenu(menuManager._mainMenu);
            menuManager.membershipBTN?.SetActive(false);
        }

        // Restoring a subscription can invoke ProcessPurchase again. Entitlements are
        // idempotent, but the welcome currencies must only be granted once.
        const string welcomeBonusKey = "MembershipWelcomeBonusGranted";
        if (PlayerPrefs.GetInt(welcomeBonusKey, 0) == 0)
        {
            BD_API.AddSoftCurrency(100000);
            BD_API.AddHardCurrency(20);
            PlayerPrefs.SetInt(welcomeBonusKey, 1);
        }

        PlayerPrefs.SetInt("XpMultiply", 1);
        PlayerPrefs.Save();
    }

    public void TransactionAnalytic()
    {
        int transactionTime = PlayerPrefs.GetInt("TransactionTimes");
        PlayerPrefs.SetInt("TransactionTimes", transactionTime + 1);
#if GAME_ANALYTICS_SDK
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "GAME_FLOW", "SHOP_PURCHASE", transactionTime);
#endif
    }


    #region TENJIN IMPLEMENTATION

//     public static void OnProcessPurchase(PurchaseEventArgs purchaseEventArgs)
//     {
//
//         string kProductIDConsumable = purchaseEventArgs.purchasedProduct.definition.id;
//
//         if (String.Equals(purchaseEventArgs.purchasedProduct.definition.id, kProductIDConsumable,
//                 StringComparison.Ordinal))
//         {
//             var product = m_StoreController.products.WithID(kProductIDConsumable);
//             string receipt = product.receipt;
//             string currency = product.metadata.isoCurrencyCode;
//             int amount = decimal.ToInt32(product.metadata.localizedPrice * 100);
// #if UNITY_ANDROID
//             Receipt receiptAndroid = JsonUtility.FromJson<Receipt>(receipt);
//             PayloadAndroid receiptPayload = JsonUtility.FromJson<PayloadAndroid>(receiptAndroid.Payload);
//             GameAnalytics.NewBusinessEventGooglePlay(currency, amount, "IAP", kProductIDConsumable, "IAP", receiptPayload.json, receiptPayload.signature);
// #endif
// #if UNITY_IPHONE
//             Receipt receiptiOS = JsonUtility.FromJson<Receipt>(receipt);
//             string receiptPayload = receiptiOS.Payload;
//             GameAnalytics.NewBusinessEventIOS(currency, amount, "IAP", kProductIDConsumable, "IAP", receiptPayload);
// #endif
//         }
//
//         var price = purchaseEventArgs.purchasedProduct.metadata.localizedPrice;
//         double lPrice = decimal.ToDouble(price);
//         var currencyCode = purchaseEventArgs.purchasedProduct.metadata.isoCurrencyCode;
//
//         var wrapper =
//             Json.Deserialize(purchaseEventArgs.purchasedProduct
//                 .receipt) as Dictionary<string, object>; // https://gist.github.com/darktable/1411710
//         if (null == wrapper)
//         {
//             return;
//         }
//
//         var payload = (string)wrapper["Payload"]; // For Apple this will be the base64 encoded ASN.1 receipt
//         var productId = purchaseEventArgs.purchasedProduct.definition.id;
//
// #if UNITY_ANDROID
//
//         var gpDetails = Json.Deserialize(payload) as Dictionary<string, object>;
//         var gpJson = (string)gpDetails["json"];
//         var gpSig = (string)gpDetails["signature"];
//
//         CompletedAndroidPurchase(productId, currencyCode, 1, lPrice, gpJson, gpSig);
//
// #elif UNITY_IOS
//
//         var transactionId = purchaseEventArgs.purchasedProduct.transactionID;
//
//         CompletedIosPurchase(productId, currencyCode, 1, lPrice, transactionId, payload);
//
// #endif
//
//     }
//
//     private static void CompletedAndroidPurchase(string ProductId, string CurrencyCode, int Quantity, double UnitPrice,
//         string Receipt, string Signature)
//     {
//         BaseTenjin instance = Tenjin.getInstance("WQYKOCJS31BW5JTCFZPMZREWMWSAIWHE");
//         instance.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, null, Receipt, Signature);
//     }
//
//     private static void CompletedIosPurchase(string ProductId, string CurrencyCode, int Quantity, double UnitPrice,
//         string TransactionId, string Receipt)
//     {
//         BaseTenjin instance = Tenjin.getInstance("WQYKOCJS31BW5JTCFZPMZREWMWSAIWHE");
//         instance.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, TransactionId, Receipt, null);
//     }
//

    #endregion


    #region IAP DO NOT TOUCH //**************************** Dont worry about these methods ***********************************

    private void Awake()
    {
        TestSingleton();
    }

    void Start()
    {
        InitializePurchasing();
    }

    private void TestSingleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                ShowInfoIfAvailable("Processing transaction...", BD_InfoDisplayer.InfoType.Process);
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log(
                    "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                DOVirtual.DelayedCall(3f, () =>
                {
                    CloseInfoIfAvailable();
                });
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
            DOVirtual.DelayedCall(3f, () =>
            {
                CloseInfoIfAvailable();
            });

        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            ShowInfoIfAvailable("Restoring purchases...", BD_InfoDisplayer.InfoType.Process);


            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, message) =>
            {
                Debug.Log("RestorePurchases continuing: " + result +
                          ". " + message);
                DOVirtual.DelayedCall(3f, () =>
                {
                    CloseInfoIfAvailable();
                });

            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            DOVirtual.DelayedCall(3f, () =>
            {
                CloseInfoIfAvailable();
            });

        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }



    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",
            product.definition.storeSpecificId, failureReason));
        DOVirtual.DelayedCall(3f, () =>
        {
            CloseInfoIfAvailable();
        });
    }

    private static void ShowInfoIfAvailable(string description, BD_InfoDisplayer.InfoType type)
    {
        BD_InfoDisplayer displayer = BD_InfoDisplayer.Instance;
        if (displayer)
            displayer.ShowInfo("WARNING", description, type);
        else
            Debug.LogWarning($"IAP message couldn't be displayed: {description}");
    }

    private static void CloseInfoIfAvailable()
    {
        BD_InfoDisplayer.Instance?.CloseInfo();
    }

    #endregion IAP DO NOT TOUCH

    public class Receipt
    {

        public string Store;
        public string TransactionID;
        public string Payload;

        public Receipt()
        {
            Store = TransactionID = Payload = "";
        }

        public Receipt(string store, string transactionID, string payload)
        {
            Store = store;
            TransactionID = transactionID;
            Payload = payload;
        }
    }

    public class PayloadAndroid
    {
        public string json;
        public string signature;

        public PayloadAndroid()
        {
            json = signature = "";
        }

        public PayloadAndroid(string _json, string _signature)
        {
            json = _json;
            signature = _signature;
        }
    }
}
#endif
