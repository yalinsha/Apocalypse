using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class XmlDataManager
{
    static XmlDataManager instance = new XmlDataManager();
    public static XmlDataManager Instance => instance;
    XmlDataManager() { }
    public void Save(object obj, string filename, bool saveToStreamingAssets = false)
    {
        XmlSerializer serializer = new XmlSerializer(obj.GetType());
        string path = Application.persistentDataPath + "/" + filename + ".xml";
        if (saveToStreamingAssets)
        {
            path = Application.streamingAssetsPath + "/" + filename + ".xml";
        }
        using (StreamWriter sw = new StreamWriter(path))
        {
            serializer.Serialize(sw, obj);
        }
    }
    public object Load(Type type, string filename)
    {
        XmlSerializer serializer = new XmlSerializer(type);
        string path = Application.persistentDataPath + "/" + filename + ".xml";
        if (!File.Exists(path))
        {
            path = Application.streamingAssetsPath + "/" + filename + ".xml";
            //Debug.Log(path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
        }
        using (StreamReader sr = new StreamReader(path))
        {
            return serializer.Deserialize(sr);
        }
    }
    public T Load<T>(string filename)
    {
        return (T)Load(typeof(T), filename);
    }
}