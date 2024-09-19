/*******************************************************
Nom......... : DisplayTime.cs
Role........ : Override of DisplayStatistics for a custom 
text for the time

Auteur...... : Edouard MORDANT
Version..... : V1.0 du 19/09/2024
********************************************************/
using System;

public class DisplayTime : DisplayStatistics
{
    #region HANDLE
    protected override void OnUpdate(int obj)
    {
        TimeSpan time = TimeSpan.FromSeconds(obj);

        //here backslash is must to tell that colon is
        //not the part of format, it just a character that we want in output
        string str = time.ToString(@"mm\:ss");

        var text = $"{_prefix}{str}{_suffix}";
        _text.text = text;
    }
    #endregion
}