using UnityEngine;

// Single source of truth for all persistent data.
// Static — no MonoBehaviour needed, callable from anywhere.
public static class SaveManager
{
    private const string KEY_HIGHSCORE   = "highscore";
    private const string KEY_BGM_VOLUME  = "bgm_volume";
    private const string KEY_SFX_VOLUME  = "sfx_volume";

    // ── Highscore ─────────────────────────────────────────────

    public static int LoadHighscore() => PlayerPrefs.GetInt(KEY_HIGHSCORE, 0);

    // Only saves if score beats the current record. Returns true if a new record was set.
    public static bool TrySaveHighscore(int score)
    {
        if (score <= LoadHighscore()) return false;
        PlayerPrefs.SetInt(KEY_HIGHSCORE, score);
        PlayerPrefs.Save();
        return true;
    }

    // ── Volume ────────────────────────────────────────────────

    public static float LoadBGMVolume() => PlayerPrefs.GetFloat(KEY_BGM_VOLUME, 1f);
    public static float LoadSFXVolume() => PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);

    public static void SaveBGMVolume(float volume)
    {
        PlayerPrefs.SetFloat(KEY_BGM_VOLUME, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public static void SaveSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    // ── Utility ───────────────────────────────────────────────

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
