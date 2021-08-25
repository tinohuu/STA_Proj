using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
	[Header("Config")]
	public int ThresholdMinutes = 30;
	public TimeData Data = new TimeData();
	[Header("Debug")]
	public bool IsChecking = false;
	[SerializeField] Vector2 _difference = new Vector2();
	[SerializeField] string _source = "None";
	[SerializeField] string _systemTime = "";
	[SerializeField] string _realTime = "";
	[SerializeField] bool _verificationResult = true;

	public delegate void TimeHandler(bool isAuthentic);
	public event TimeHandler TimeRefresher = null; // Remove punishment
	public event TimeHandler OnGetTime = null;
	public static TimeManager Instance = null;
	
	private void Awake()
    {
		Instance = this;
		
		Data = SaveManager.Bind(new TimeData());

	}
    private void Start()
    {
		InitialiseTimeData();
	}

    void InitialiseTimeData()
    {
		if (Data.CheckedSystemOffset == default)
        {
			Data.CheckedDateTime = RealNow;
			Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
			TimeRefresher.Invoke(true);
			GetTime();

		}
		else
        {
			VerifyTime();
			//GetTime(false);
		}
	}

    private void Update()
    {
		_systemTime = SystemNow.ToString();
		_realTime = RealNow.ToString();
		/*TST_CheckedText.text =
			"Checked\n" +
			Data.CheckedDateTime.ToString() +
			"\nLocal: " + Data.CheckedDateTime.ToLocalTime().ToString() +
			"\nSince Boot: " + Data.CheckedBootTime.ToTimeSpan().ToString(@"dd\:hh\:mm\:ss") +
			"\nSystem Offset: " + Data.CheckedSystemOffset.ToTimeSpan().ToString(@"dd\:hh\:mm\:ss") +
			"\nFrom " + SiteUrl;
		TST_CheckedImage.color = SiteUrl == "System" ? new Color(1, 0.5f, 0) : new Color(0, 0.5f, 0);

		TST_CurrentText.text =
			"System\n" +
			SystemNow +
			"\nReal: " + RealNow +
			"\nLocal: " + DateTime.Now +
			"\nSince Boot: " + TimeSinceBoot.ToString(@"dd\:hh\:mm\:ss");

		TimeSpan timeSpan;
		timeSpan = Data.TST_LastHarvestTime + TimeSpan.FromHours(1) - SystemNow;
		TST_HarvestText.text = timeSpan.TotalSeconds > 0 ? "Harvest...\n" + timeSpan.ToString(@"dd\:hh\:mm\:ss") : "Harvest!";
		TST_HarvestImage.color = timeSpan.TotalSeconds > 0 ? Color.white : Color.green;
		TST_HarvestButton.interactable = timeSpan.TotalSeconds <= 0;

		timeSpan = Data.TST_LastQuestUpdateTime + TimeSpan.FromDays(1) - SystemNow;
		if (timeSpan.TotalSeconds < 0) Data.TST_LastQuestUpdateTime = SystemNow.Date + TimeSpan.FromHours(10);
		TST_QuestText.text = "Quest...\n" + timeSpan.ToString(@"dd\:hh\:mm\:ss");
		
		timeSpan = Data.TST_LastDailyRewardTime + TimeSpan.FromDays(1) - SystemNow;
		if (timeSpan.TotalSeconds < 0) Data.TST_LastDailyRewardTime = SystemNow.Date + TimeSpan.FromHours(16);
		TST_DailyText.text = "Daily...\n" + timeSpan.ToString(@"dd\:hh\:mm\:ss");
		//TST_DailyImage.color = IsAuthentic ? Color.white : Color.red;*/
	}

    private void OnApplicationPause(bool pause)
    {
		if (Data.CheckedSystemOffset != default) // Skip if not initialized
			VerifyTime();
	}
	public void GetTime(bool saveSystemTime = true)
    {
		StartCoroutine(IGetTime(saveSystemTime));
	}

	public void VerifyTime()
    {
		bool _isAuthentic = true;
		if (Data.CheckedDateTime.Year < 2000) _isAuthentic = false;
		else _isAuthentic = VerifyTimeSpan(RealNow - Data.CheckedDateTime, TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan(), ThresholdMinutes);

		_difference = new Vector2((float)(RealNow - Data.CheckedDateTime).TotalMinutes, (float)(TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan()).TotalMinutes);
		//IsAuthentic = _isAuthentic;
		if (!_isAuthentic)
        {
			Debug.LogWarning("Your time is not authentic!");
			//StartCoroutine(TST_IPunishColor());
			StartCoroutine(IGetTime(false, true));
		}
		_verificationResult = _isAuthentic;
	}
	public DateTime SystemNow => DateTime.Now.ToUniversalTime();
	public DateTime RealNow => DateTime.Now.ToUniversalTime() - Data.CheckedSystemOffset.ToTimeSpan();
	public static TimeSpan TimeSinceBoot
	{
		get
		{
			TimeSpan timeSpan = new TimeSpan();
			if (Application.platform == RuntimePlatform.Android)
			{
				var clock = new AndroidJavaClass("android.os.SystemClock");
				long time = clock.CallStatic<long>("elapsedRealtime");
				timeSpan = TimeSpan.FromMilliseconds(time);
			}
			else timeSpan = TimeSpan.FromMilliseconds(Environment.TickCount);
			return timeSpan;
		}
	}
	IEnumerator IGetTime(bool saveSystemTime, bool enablePunish = false)
	{
		OnGetTime?.Invoke(true);
		string[] sites =
		{
			"https://www.baidu.com",
			"https://www.tencent.com",
			"https://www.apple.com",
			"https://github.com",
			"https://www.microsoft.com",
			"https://www.google.com",
		};

		foreach (string url in sites)
		{
			UnityWebRequest www = new UnityWebRequest(url);
			yield return www.SendWebRequest();
			if (www.isDone && string.IsNullOrEmpty(www.error))
			{
				Dictionary<string, string> resHeaders = www.GetResponseHeaders();
				string key = "DATE";
				string value = null;
				if (resHeaders != null && resHeaders.ContainsKey(key))
				{
					resHeaders.TryGetValue(key, out value);
				}

				if (value == null)
				{
					Debug.Log("DATE is null");
					continue;
				}

				_source = url;
				Data.CheckedDateTime = String2DateTime(value);
				Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
				TimeSpan curOffset = SystemNow - Data.CheckedDateTime;
				TimeSpan oldOffset = Data.CheckedSystemOffset.ToTimeSpan();
				//IsAuthentic = true;
				Data.CheckedSystemOffset = curOffset.TotalMilliseconds;
				if (enablePunish && curOffset.Minutes > ThresholdMinutes && !VerifyTimeSpan(curOffset, oldOffset, ThresholdMinutes))
                {
					TimeRefresher(false);
                }
				OnGetTime?.Invoke(false);
				yield break;
			}
		}
		Debug.Log("Can't connect to any sites");
		if (enablePunish) TimeRefresher(false);
        if (saveSystemTime)
        {
			_source = "System";
			Data.CheckedDateTime = RealNow;
			Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
			Data.CheckedSystemOffset = (RealNow - Data.CheckedDateTime).TotalMilliseconds;
		}
		OnGetTime?.Invoke(false);
	}

	public DateTime String2DateTime(string gmt)
	{
		DateTime dt = DateTime.MinValue;
		try
		{
			string pattern = "";
			if (gmt.IndexOf("+0") != -1)
			{
				gmt = gmt.Replace("GMT", "");
				pattern = "ddd, dd MMM yyyy HH':'mm':'ss zzz";
			}
			if (gmt.ToUpper().IndexOf("GMT") != -1)
			{
				pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
			}
			if (pattern != "")
			{
				dt = DateTime.ParseExact(gmt, pattern, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal);
				//dt = dt.ToLocalTime();
			}
			else
			{
				dt = Convert.ToDateTime(gmt);
			}
		}
		catch
		{
		}
		return dt;
	}

    bool VerifyTimeSpan(TimeSpan toBeVerified, TimeSpan verifier, int thresholdMinutes)
    {
		return toBeVerified.TotalMinutes < verifier.TotalMinutes + thresholdMinutes;
	}
	/*
	public void TST_Harvest()
	{
		Data.TST_LastHarvestTime = SystemNow;
	}
	public void TST_Save()
	{
		Save save = new Save();
		save.Set(Data);
		SaveSystem.Save(save);
	}
	public void TST_Reset()
	{
		SaveSystem.Clear();
		SceneManager.LoadScene("TimeTest");
	}
	public void TST_Punish()
	{
		TimeRefresher?.Invoke(false);
	}
	void TST_UpdateNextTime(bool _isAuthentic = true)
	{
		Data.TST_LastQuestUpdateTime = SystemNow.Date + TimeSpan.FromDays(SystemNow.Hour >= 10 ? 0 : -1) + TimeSpan.FromHours(10);
		//Data.TST_LastDailyRewardTime = Now.Date + TimeSpan.FromDays(Now.Hour >= 16 ? 0 : -1) + TimeSpan.FromHours(16) + TimeSpan.FromDays(_isAuthentic ? 0 : 1);

		if (_isAuthentic)
		{
			Data.TST_LastHarvestTime = (Data.TST_LastHarvestTime - SystemNow).TotalHours < 1 ? Data.TST_LastHarvestTime : SystemNow;// Data.TST_LastHarvestTime > Now - TimeSpan.FromHours(1) ? Data.TST_LastHarvestTime : Now;
			Data.TST_LastDailyRewardTime = SystemNow.Date + TimeSpan.FromDays(SystemNow.Hour >= 16 ? 0 : -1) + TimeSpan.FromHours(16);// Data.TST_LastDailyRewardTime > Now - TimeSpan.FromDays(1) ? Data.TST_LastDailyRewardTime : Now.Date + TimeSpan.FromDays(Now.Hour >= 16 ? 0 : -1) + TimeSpan.FromHours(16);
		}
		else
		{
			if ((SystemNow - Data.TST_LastDailyRewardTime).TotalHours < 1)
			{
				Data.TST_LastDailyRewardTime = SystemNow;
			}
			Data.TST_LastHarvestTime = SystemNow;
			//Data.TST_LastDailyRewardTime = SystemNow.Date + TimeSpan.FromDays(SystemNow.Hour >= 16 ? 0 : -1) + TimeSpan.FromHours(16) + TimeSpan.FromDays(1);
			if ((SystemNow - Data.TST_LastDailyRewardTime).TotalDays < 1)
			{
				//Data.TST_LastDailyRewardTime += TimeSpan.FromDays(SystemNow.Hour >= 16 ? 2 : 1);
				Data.TST_LastDailyRewardTime = SystemNow.Date + TimeSpan.FromDays(SystemNow.Hour >= 16 ? 0 : -1) + TimeSpan.FromHours(16) + TimeSpan.FromDays(1);
			}

		}
	}
	IEnumerator TST_IPunishColor()
    {
		TST_PunishImage.color = Color.yellow;
		yield return new WaitForSeconds(3);
		TST_PunishImage.color = Color.white;
	}*/
}

[System.Serializable]
public class TimeData
{
	public DateTime CheckedDateTime = new DateTime();

	// Declear DateTime as TimeSpan is not serializable
	public double CheckedBootTime = 0;
	public double CheckedSystemOffset = 0;

	/*
	public DateTime TST_LastDailyRewardTime = new DateTime();
	public DateTime TST_LastQuestUpdateTime = new DateTime();
	public DateTime TST_LastHarvestTime = new DateTime();*/
}

public static class TimeExtension
{
	public static TimeSpan ToTimeSpan(this double ms)
	{
		return TimeSpan.FromMilliseconds(ms);
	}
}