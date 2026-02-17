using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class RaceResultHandler
{
    private string dataDirPath;
    private string dataFileName;

    public RaceResultHandler(string dataDirPath, string dataFileName)
    {
        // Use Unity's persistent data path if no specific path is provided
        if (string.IsNullOrEmpty(dataDirPath))
        {
            this.dataDirPath = Application.persistentDataPath;
        }
        else
        {
            this.dataDirPath = dataDirPath;
        }
        
        this.dataFileName = dataFileName;
        
        Debug.Log($"RaceResultHandler initialized. Data will be saved to: {Path.Combine(this.dataDirPath, this.dataFileName)}");
    }

    public void Save(RaceResultData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Load existing results or create new collection
            RaceResultCollection collection = Load();
            if (collection == null)
            {
                collection = new RaceResultCollection();
            }

            // Add new result to collection
            collection.results.Add(data);

            string dataToStore = JsonUtility.ToJson(collection, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            Debug.Log($"Race result saved to: {fullPath} (Total results: {collection.results.Count})");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving race result to file: " + fullPath + "\n" + e);
        }
    }

    public RaceResultCollection Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        RaceResultCollection loadedData = null;

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

                loadedData = JsonUtility.FromJson<RaceResultCollection>(dataToLoad);
                
                // Check if results list is null after deserialization
                if (loadedData != null && loadedData.results == null)
                {
                    loadedData.results = new List<RaceResultData>();
                }
                
                Debug.Log($"Successfully loaded {loadedData?.results?.Count ?? 0} race results from: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading race result from file: " + fullPath + "\n" + e);
            }
        }
        else
        {
            Debug.Log("No race result file found at: " + fullPath);
            //jotta ei koita lataa tietoja kun niitä ei ole!!!
            loadedData = null;
        }

        return loadedData;
    }

    public int GetNextRacerNumber()
    {
        RaceResultCollection collection = Load();
        if (collection == null || collection.results == null || collection.results.Count == 0)
        {
            return 1;
        }
        return collection.results.Count + 1;
    }
}
