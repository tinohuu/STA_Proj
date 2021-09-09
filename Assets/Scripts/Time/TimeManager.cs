using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public bool IsGettingTime = false;
	[SerializeField] Vector2 _difference = new Vector2();
	[SerializeField] string _systemTime = "";
	[SerializeField] string _realTime = "";

	string[] sites =
{
			"https://www.baidu.com",
			"https://www.tencent.com",
			"https://www.apple.com",
			"https://github.com",
			"https://www.microsoft.com",
			"https://www.google.com",
		};

	public static TimeManager Instance = null;

	private void Awake()
    {
		if (!Instance) Instance = this;
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
			GetTime(TimeAuthenticity.FirstTime);
			//RefreshTime();
		}
		else if (Data.CheckedSource == TimeSource.System)
        {
			GetTime(Data.CheckedAuthenticity);
		}
	}

    private void Update()
    {
		// Debug view
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
		// 1st verification: boot time check
		bool _isAuthentic = VerifyTimeSpan(RealNow - Data.CheckedDateTime, TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan(), ThresholdMinutes);

		// debug view
		_difference = new Vector2((float)(RealNow - Data.CheckedDateTime).TotalMinutes, (float)(TimeSinceBoot - Data.CheckedBootTime.ToTimeSpan()).TotalMinutes);

		if (!_isAuthentic)
        {
			TimeDebugText.Text.text += "\nUnauthentic during 1st verificaion: " + RealNow.ToString();
			GetTime(TimeAuthenticity.Unauthentic);
		}
		else
        {
			TimeDebugText.Text.text += "\nAuthentic during 1st verificaion: " + RealNow.ToString();
			Data.CheckedAuthenticity = TimeAuthenticity.Authentic;
			RefreshTime();
		}
	}

	public void GetTime(TimeAuthenticity timeAuthenticity) { StopCoroutine("IGetTime"); StartCoroutine(IGetTime(timeAuthenticity)); }

	IEnumerator IGetTime(TimeAuthenticity firstStepAuth)
	{
		IsGettingTime = true;
		LoadingWindow.Play(true);

		// Getting internet time
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
					resHeaders.TryGetValue(key, out value);
				if (value == null) { Debug.LogWarning("DATE is null"); continue; }

				// Record new internet time
				TimeSpan oldOffset = Data.CheckedSystemOffset.ToTimeSpan();
				RecordTime(TimeSource.Internet, String2DateTime(value));
				TimeSpan newOffset = SystemNow - Data.CheckedDateTime;
				TimeDebugText.Text.text += "\nRecorded from " + url;

				// 2nd verification: system-world time offset
				if (firstStepAuth == TimeAuthenticity.Unauthentic)
				{
					if (VerifyTimeSpan(newOffset, oldOffset, ThresholdMinutes)) // Authentic
					{
						Data.CheckedAuthenticity = TimeAuthenticity.Authentic;
						TimeDebugText.Text.text += "\nNot punish as passing 2nd verification.";
					}
					else // Not authentic
					{
						Data.CheckedAuthenticity = TimeAuthenticity.Unauthentic;
						TimeDebugText.Text.text += "\nPunush!";
					}
				}

				RefreshTime();

				IsGettingTime = false;
				LoadingWindow.Play(false);

				yield break;
			}
		}

		// Record system time
		RecordTime(TimeSource.System, RealNow);
		TimeDebugText.Text.text += "\nRecorded system time.";

		if (firstStepAuth == TimeAuthenticity.Unauthentic)
		{
			TimeDebugText.Text.text += "\nPunush!";
			Data.CheckedAuthenticity = TimeAuthenticity.Unauthentic;
		}

		RefreshTime();

		IsGettingTime = false;
		LoadingWindow.Play(false);
	}

	void RecordTime(TimeSource source, DateTime time)
	{
		Data.CheckedSource = source;
		Data.CheckedDateTime = time;
		Data.CheckedBootTime = TimeSinceBoot.TotalMilliseconds;
		Data.CheckedSystemOffset = (SystemNow - time).TotalMilliseconds;
	}

	void RefreshTime()
	{
		// Get and execute all interfaces
		var refreshers = FindObjectsOfType<MonoBehaviour>().OfType<ITimeRefreshable>();
		foreach (var refresher in refreshers)
		{
			refresher.RefreshTime(RealNow, Data.CheckedSource, Data.CheckedAuthenticity);
		}
		Debug.Log("TimeManager:: Refreshed as " + Data.CheckedAuthenticity.ToString());
		Data.CheckedAuthenticity = TimeAuthenticity.Authentic;
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

public interface ITimeRefreshable
{
	void RefreshTime(DateTime now, TimeSource source, TimeAuthenticity timeAuthenticity);

	void ResetTime(DateTime now);
}