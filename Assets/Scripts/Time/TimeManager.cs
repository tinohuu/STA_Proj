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
	[SavedData] public TimeData Data = new TimeData();
	[Header("Debug")]
	public bool IsChecking = false;
	[SerializeField] Vector2 _difference = new Vector2();
	[SerializeField] string _systemTime = "";
	[SerializeField] string _realTime = "";
	//[SerializeField] public bool Authenticity = true;
	public bool IsGettingTime = false;

	public delegate void TimeHandler();
	public event TimeHandler Refresher;
	public event TimeHandler OnGetTime;

	public static TimeManager Instance = null;

	private void Awake()
    {
		if (!Instance) Instance = this;
		//OnGetTime += (bool on) => isGettingTime = on;
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
			//TimeRefresherOld.Invoke(false);
			GetTime(TimeAuthenticity.FirstTime);
			Refresher?.Invoke();
		}
	}

    private void Update()
    {
		_systemTime = SystemNow.ToString();
		_realTime = RealNow.ToString();
	}

    private void OnApplicationPause(bool pause)
    {
		if (pause || Data.CheckedSource == TimeSource.Unknown) return;
		VerifyTime();
	}


	public void VerifyTime()
    {
		TimeDebugText.Text.text += "\nVerifying Time...";
		bool _isAuthentic = true;
		_isAuthentic = VerifyTimeSpan(RealNow - Data.CheckedDateTime, TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan(), ThresholdMinutes);

		_difference = new Vector2((float)(RealNow - Data.CheckedDateTime).TotalMinutes, (float)(TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan()).TotalMinutes);
		if (!_isAuthentic)
        {
			TimeDebugText.Text.text += "\nYour time is not authentic during verificaion." + RealNow.ToString();
			GetTime(TimeAuthenticity.Unauthentic);
		}
		else
        {
			TimeDebugText.Text.text += "\nYour time is authentic during verificaion: " + RealNow.ToString();
			Data.CheckedAuthenticity = TimeAuthenticity.Authentic;
			Refresher?.Invoke();
		}
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

	public void GetTime(TimeAuthenticity timeAuthenticity) { StopCoroutine("IGetTime"); StartCoroutine(IGetTime(timeAuthenticity)); }

	IEnumerator IGetTime(TimeAuthenticity timeAuthenticity)
	{
		IsGettingTime = true;
		OnGetTime?.Invoke();

		TimeAuthenticity timeAuth = timeAuthenticity;

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

				// Record new internet time
				Data.CheckedSource = TimeSource.Internet;
				Data.CheckedDateTime = String2DateTime(value);
				Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
				Data.CheckedSource = TimeSource.Internet;
				TimeDebugText.Text.text += "\nRecorded time from " + url;

				TimeSpan newOffset = SystemNow - Data.CheckedDateTime;
				TimeSpan oldOffset = Data.CheckedSystemOffset.ToTimeSpan();
				Data.CheckedSystemOffset = newOffset.TotalMilliseconds;

				// 2nd verification: system-world time offset
				if (timeAuth == TimeAuthenticity.Unauthentic)
                {
					if (VerifyTimeSpan(newOffset, oldOffset, ThresholdMinutes))
                    {
						Data.CheckedAuthenticity = TimeAuthenticity.Authentic;
						Refresher?.Invoke();
						TimeDebugText.Text.text += "\nNot punish as passing 2nd verification.";
					}
					else
                    {
						Data.CheckedAuthenticity = TimeAuthenticity.Unauthentic;
						Refresher?.Invoke();
						TimeDebugText.Text.text += "\nPunush!";
					}
				}

				if (timeAuth == TimeAuthenticity.FirstTime)
				{
					Debug.Log("FirstTime");
					Refresher?.Invoke();
				}

				IsGettingTime = false;
				OnGetTime?.Invoke();
				yield break;
			}
		}

		if (timeAuth == TimeAuthenticity.Unauthentic)
		{
			Data.CheckedAuthenticity = TimeAuthenticity.Unauthentic;
			Refresher?.Invoke();
			TimeDebugText.Text.text += "\nPunush!";
		}

		// Record system time

		Data.CheckedSource = TimeSource.Unknown;
		Data.CheckedDateTime = RealNow;
		Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
		Data.CheckedSystemOffset = (RealNow - Data.CheckedDateTime).TotalMilliseconds;
		TimeDebugText.Text.text += "\nRecorded system time.";

		if (timeAuth == TimeAuthenticity.FirstTime)
        {
			Debug.Log("FirstTime");
			Refresher?.Invoke();
		}

		IsGettingTime = false;
		OnGetTime?.Invoke();
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
	public TimeSource CheckedSource = TimeSource.Unknown;
	public TimeAuthenticity CheckedAuthenticity = TimeAuthenticity.FirstTime;
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

[Serializable]
public enum TimeSource
{
	Unknown,
	System,
	Internet
}

[Serializable]
public enum TimeAuthenticity
{
	FirstTime,
	Authentic,
	Unauthentic
}