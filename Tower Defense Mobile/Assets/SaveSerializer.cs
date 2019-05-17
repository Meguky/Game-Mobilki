using System.IO;
using System.Xml.Serialization;
using UnityEngine;
public static class SaveSerializer
{
    public static string Serialize<T>(this T @object){
        return JsonUtility.ToJson(@object);
    }
    public static T Deserialize<T>(this string @string){
        return JsonUtility.FromJson<T>(@string);
    }
}
