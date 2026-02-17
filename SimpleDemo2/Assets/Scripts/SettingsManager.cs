using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Settings
{
    public GridPattern gridPattern;
    public int noOfMatches;
    public int noOfTurns;
    public bool gameIsStarted;
    public List<CardData> cardData = new();
}

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private string settingsFileName = "Settings.json";
    [SerializeField] public Settings settings;
    private string _settingsFilePath;

    public static event Action<Settings> OnSettingsLoaded;
    public static SettingsManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        _settingsFilePath = Path.Combine(Application.streamingAssetsPath, settingsFileName);
#if UNITY_ANDROID && !UNITY_EDITOR
        _settingsFilePath = Path.Combine(Application.persistentDataPath, settingsFileName);
#endif
    }

    private void Start()
    {
        if (settings != null)
            ReadSettingsFile();
        else
            Debug.LogError("Settings reference not assigned!");
    }

    private void ReadSettingsFile()
    {
        if (!File.Exists(_settingsFilePath))
        {
            Debug.Log("<color=red>Settings file read error.</color>");
            SaveSettingsFile();
        }

        using StreamReader r = new(_settingsFilePath);
        string text = r.ReadToEnd();
        r.Close();
        JsonUtility.FromJsonOverwrite(text, settings);

        if (settings != null)
            OnSettingsLoaded?.Invoke(settings);
    }

    public void SaveSettingsFile()
    {
        try
        {
            using StreamWriter w = new(_settingsFilePath);
            string json = JsonUtility.ToJson(settings, true);
            w.Write(json);
            Debug.Log("<color=green>Settings json saved.</color>");
        }
        catch (Exception ex)
        {
            Debug.Log("<color=red>Settings file save error: </color>" + ex);
        }
    }
}