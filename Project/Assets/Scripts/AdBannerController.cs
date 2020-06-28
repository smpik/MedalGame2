using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdBannerController : MonoBehaviour
{
	/********************************************************************************/
	/* 内部定数																		*/
	/********************************************************************************/
	private const string APP_ID = "ca-app-pub-3940256099942544~3347511713";//アプリID、テスト用はca-app-pub-3940256099942544~3347511713
	private const string AD_UNIT_ID = "ca-app-pub-3940256099942544/6300978111";// 広告ユニットID テスト用は:ca-app-pub-3940256099942544/6300978111

	/********************************************************************************/
	/* 内部変数																		*/
	/********************************************************************************/
	BannerView BannerViewInstance;

	private DisplayController DisplayControllerInstance;

	// Start is called before the first frame update
	void Start()
	{
		DisplayControllerInstance = GameObject.Find("Main Camera").GetComponent<DisplayController>();

		MobileAds.Initialize(APP_ID);

		// Create a 320x50 banner at the top of the screen.
		BannerViewInstance = new BannerView(AD_UNIT_ID, AdSize.Banner, AdPosition.Top);

		RequestBanner();
	}

	private void RequestBanner()
	{
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();

		// Load the banner with the request.
		BannerViewInstance.LoadAd(request);

		// Create a 320x50 banner at the top of the screen.
		//bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
	}

	public void StopBanner()
	{
		BannerViewInstance.Destroy();//インスタンス削除
		DisplayControllerInstance.InactivateBanner1();//バナー用ゲームオブジェクトを非表示
	}

	public void ReDispBanner()
	{
		BannerViewInstance = new BannerView(AD_UNIT_ID, AdSize.Banner, AdPosition.Top);
		RequestBanner();
	}
}
