using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


public class FileDataHandler
{
    private string DataDirPath = "";
    private string DataFileName = "";


    public FileDataHandler(string DataDirPath, string DataFileName)
    {
        this.DataDirPath = DataDirPath;
        this.DataFileName = DataFileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(DataDirPath, DataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

            }
            catch (Exception e)
            {
                Debug.Log("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }

        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath = Path.Combine(DataDirPath, DataFileName);
        
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error occured when trying to save data to file:" + fullPath + "\n" + e);
        }
    }
}
