using System;
using UnityEngine;

/// <summary>Persistent player experience shared by gameplay, UI and store grants.</summary>
public static class BD_PlayerProgression
{
    public const string TotalExperienceKey = "PlayerTotalExperience";
    public const string CompletedRacesKey = "PlayerCompletedRacesV1";
    public static int CompletedRaces => Mathf.Max(0, PlayerPrefs.GetInt(CompletedRacesKey, 0));

    public static void RecordCompletedRace()
    {
        PlayerPrefs.SetInt(CompletedRacesKey, (int)Math.Min((long)CompletedRaces + 1, int.MaxValue));
        PlayerPrefs.Save();
    }

    public static event Action<int> ExperienceChanged;

    public static int TotalExperience => Mathf.Max(0, PlayerPrefs.GetInt(TotalExperienceKey, 0));

    public static void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        int updated = (int)Math.Min((long)TotalExperience + amount, int.MaxValue);
        PlayerPrefs.SetInt(TotalExperienceKey, updated);
        PlayerPrefs.Save();
        ExperienceChanged?.Invoke(updated);
    }

    public static void SetExperience(int amount)
    {
        int clamped = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(TotalExperienceKey, clamped);
        PlayerPrefs.Save();
        ExperienceChanged?.Invoke(clamped);
    }
}
