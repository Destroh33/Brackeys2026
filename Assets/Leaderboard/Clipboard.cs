using UnityEngine;

public static class Clipboard
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void CopyToClipboard(string text);
#endif

    public static void Copy(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        CopyToClipboard(text);
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}
