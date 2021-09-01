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
	public Text DebugText;
	[SavedData] public TimeData Data = new TimeData();
	[Header("Debug")]
	public bool IsChecking = false;
	[SerializeField] Vector2 _difference = new Vector2();
	[SerializeField] string _systemTime = "";
	[SerializeField] string _realTime = "";
	[SerializeField] public bool Authenticity = true;
	bool isGettingTime = false;
	public delegate void TimeHandler(bool isAuthentic);
	public event TimeHandler TimeRefresher = null; // Remove punishment
	public event TimeHandler OnGetTime = null;
	public static TimeManager Instance = null;
	
	private void Awake()
    {
		if (!Instance) Instance = this;
		OnGetTime += (bool on) => isGettingTime = on;
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
			TimeRefresher.Invoke(false);
			GetTime(true, false);
		}
	}

    private void Update()
    {
		_systemTime = SystemNow.ToString();
		_realTime = RealNow.ToString();
	}

    private void OnApplicationPause(bool pause)
    {
		if (pause || Data.CheckedSource == "None") return;
		VerifyTime();
	}
	public void GetTime(bool saveSystemTime, bool enablePunish)
    {
		DebugText.text += "\nGetting Time...";
		StartCoroutine(IGetTime(saveSystemTime, enablePunish));
	}

	public void VerifyTime()
    {
		DebugText.text += "\nVerifying Time...";
		bool _isAuthentic = true;
		_isAuthentic = VerifyTimeSpan(RealNow - Data.CheckedDateTime, TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan(), ThresholdMinutes);

		_difference = new Vector2((float)(RealNow - Data.CheckedDateTime).TotalMinutes, (float)(TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan()).TotalMinutes);
		if (!_isAuthentic)
        {
			DebugText.text += "\nYour time is not authentic during verificaion." + RealNow.ToString();
			GetTime(true, true);
		}
		else
        {
			DebugText.text += "\nYour time is authentic during verificaion: " + RealNow.ToString();
			TimeRefresher?.Invoke(true);
		}
		Authenticity = _isAuthentic;
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
	IEnumerator IGetTime(bool saveSystemTime, bool enablePunish)
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

				Data.CheckedSource = url;
				Data.CheckedDateTime = String2DateTime(value);
				Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
				DebugText.text += "\nRecorded time from " + Data.CheckedSource;
				TimeSpan curOffset = SystemNow - Data.CheckedDateTime;
				TimeSpan oldOffset = Data.CheckedSystemOffset.ToTimeSpan();
				//IsAuthentic = true;
				Data.CheckedSystemOffset = curOffset.TotalMilliseconds;
				if (enablePunish)
                {
					if (!VerifyTimeSpan(curOffset, oldOffset, ThresholdMinutes))
                    {
						TimeRefresher(false);
						DebugText.text += "\nPunushed";
					}
					else
                    {
						TimeRefresher?.Invoke(true);
						DebugText.text += "\nNot punished as passing the internet time.";
					}
					Authenticity = true;
				}
				OnGetTime?.Invoke(false);
				yield break;
			}
		}
		Debug.Log("Can't connect to any sites");
		if (enablePunish)
        {
			TimeRefresher(false);
			DebugText.text += "\nPunushed";
			Authenticity = true;
		}
        if (saveSystemTime)
        {
			Data.CheckedSource = "System";
			Data.CheckedDateTime = RealNow;
			Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
			Data.CheckedSystemOffset = (RealNow - Data.CheckedDateTime).TotalMilliseconds;
			DebugText.text += "\nRecorded time from " + Data.CheckedSource;
		}
		OnGetTime?.Invoke(false);
	}

	static DateTime String2DateTime(string gmt)
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

	static bool VerifyTimeSpan(TimeSpan toBeVerified, TimeSpan verifier, int thresholdMinutes)
    {
		return toBeVerified.TotalMinutes < verifier.TotalMinutes + thresholdMinutes;
	}
}

[System.Serializable]
public class TimeData
{
	public DateTime CheckedDateTime = new DateTime();
	public string CheckedSource = "None";
	// Declear DateTime as TimeSpan is not serializable
	public double CheckedBootTime = 0;
	public double CheckedSystemOffset = 0;
}

public static class TimeExtension
{
	public static TimeSpan ToTimeSpan(this double ms)
	{
		return TimeSpan.FromMilliseconds(ms);
	}
}