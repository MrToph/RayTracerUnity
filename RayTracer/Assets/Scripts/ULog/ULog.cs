using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class ULog {
    private static ILogger log;
    public static LogColor colorBodyText = LogColor.black;
    public static LogColor colorBodyValues = LogColor.blue;
    public static LogColor colorArrayDelimiter = LogColor.white;
    public static bool prefixClassMethods = true;
    public static bool prefixAutoColor = true;
    public static string objectDelimiter = " ";



    public static void SetLogger(ILogger logger)
    {
        log = logger;
    }

    static ULog()
    {
        log = UnityEngine.Debug.logger;
    }

    public static void Log(params object[] args)
    {
        log.Log(CreateMessage(args));
    }

    public static void LogWarning(string tag, params object[] args)
    {
        log.LogWarning(tag, CreateMessage(args));
    }

    public static void LogError(string tag, params object[] args)
    {
        log.LogError(tag, CreateMessage(args));
    }

    private static string CreateMessage(params object[] args)
    {
        StringBuilder sb = new StringBuilder();
        CreateMessagePrefix(ref sb, args);
        CreateMessageBody(ref sb, args);
        CreateMessageSuffix(ref sb, args);
        sb.Append(System.Environment.NewLine);
        return sb.ToString();
    }

    private static void CreateMessagePrefix(ref StringBuilder sb, params object[] args)
    {
        if (prefixClassMethods)
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame frame = stackTrace.GetFrame(3); // The caller frame (we got called from *Intersting* => Log[Error|Warning] => CreateMessage)

            string methodName = frame.GetMethod().Name;    // Method name
            string className = frame.GetMethod().DeclaringType.Name;
            string prefix = string.Format("{0}.{1}: ", className, methodName);
            if (prefixAutoColor)
            {
                int index = className.GetHashCode();
                index = (index < 0 ? -index : index) % darkColors.Length;
                sb.Append(ColorString(prefix, (LogColor)(darkColors[index])));
            }
            else
            {
                sb.Append(prefix);
            }
        }
    }

    private static void CreateMessageBody(ref StringBuilder sb, params object[] args)
    {
        for(int i = 0; i < args.Length; i++)
        {
            FormatObject(ref sb, args[i]);
            if (i < args.Length - 1)
                sb.Append(objectDelimiter);
        }
    }

    private static void FormatObject(ref StringBuilder sb, object v)
    {
        //if (v is object[])
        //{
        //    sb.Append(ColorString("{", colorArrayDelimiter));
        //    CreateMessageBody(ref sb, v as object[]);
        //    sb.Append(ColorString("}", colorArrayDelimiter));
        //}
        if (v is string)
            sb.Append(ColorString(v.ToString(), colorBodyText));
        else if (v is IEnumerable)
        {
            sb.Append(ColorString("{", colorArrayDelimiter));
            IEnumerator enumerator = (v as IEnumerable).GetEnumerator();
            enumerator.MoveNext();
            CreateMessageBody(ref sb, enumerator.Current);
            while (enumerator.MoveNext())
            {
                sb.Append(objectDelimiter);
                CreateMessageBody(ref sb, enumerator.Current);
            } 
            sb.Append(ColorString("}", colorArrayDelimiter));
        }
        else
            sb.Append(ColorString(v.ToString(), colorBodyValues));
    }

    private static void CreateMessageSuffix(ref StringBuilder sb, params object[] args)
    {
        return;
    }

    public static string ColorString(string message, LogColor color)
    {
        return string.Format("<color={0}>{1}</color>", color.ToString(), message);
    }

    private static int[] darkColors = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15};

    public enum LogColor
    {
        aqua,
        black,
        blue,
        brown,
        cyan,
        darkblue,
        fuchsia,
        green,
        grey,
        lightblue, // 9
        lime,
        magenta,
        maroon,
        navy,
        olive,
        purple,
        red,  // 16
        silver,
        teal,
        white,
        yellow
    }
}
