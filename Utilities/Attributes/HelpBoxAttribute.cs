using UnityEngine;

public class HelpBoxAttribute : PropertyAttribute
{
    public string Text { get; }

    public HelpBoxAttribute(string text)
    {
        Text = text;
    }
}
