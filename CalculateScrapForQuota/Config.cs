using BepInEx.Configuration;
using UnityEngine;

namespace CalculateScrapForQuota;

public class Config
{
    public bool isVerbose => _isVerbose.Value;
    private readonly ConfigEntry<bool> _isVerbose;
    public readonly Color highlightColor;
    private readonly ConfigEntry<string> _highlightColor;

    public Config(ConfigFile cfg)
    {
        _isVerbose = cfg.Bind(
            "General.Debug",
            "isVerbose",
            false,
            "To display plugin logs in console."
        );
        
        _highlightColor = cfg.Bind(
            "General.Settings", 
            "highlightColor", 
            "#00FF00", 
            "The hex color of the highlight material.");
        
        ColorUtility.TryParseHtmlString(_highlightColor.Value, out highlightColor);
    }
}