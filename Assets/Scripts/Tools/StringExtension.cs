using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Found at https://forum.unity.com/threads/easy-text-format-your-debug-logs-rich-text-format.906464/

// This is the extension method.
// The first parameter takes the "this" modifier
// and specifies the type for which the method is defined.
public static class StringExtension
{
    public static string Bold(this string str) => "<b>" + str + "</b>";
    public static string Color(this string str, string clr) => string.Format("<color={0}>{1}</color>", clr, str);
    public static string Italic(this string str) => "<i>" + str + "</i>";
    public static string Size(this string str, int size) => string.Format("<size={0}>{1}</size>", size, str);
}