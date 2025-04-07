//created by: Joonas Kallio
//           //                 UnityEngine.Random.Range(-10f, 10f),
//created by: Joonas Kallio
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f)
//             );
//             newObject.transform.rotation = Quaternion.Euler(
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f)
//             );
//             newObject.transform.localScale = new Vector3(
//                 UnityEngine.Random.Range(0.5f, 2f),  
//                 UnityEngine.Random.Range(0.5f, 2f),
//                 UnityEngine.Random.Range(0.5f, 2f)   
//             );
//
//             newObject.transform.SetParent(transform); // Set the parent to the current object
//             newObject.transform.localPosition = new Vector3(
//                 UnityEngine.Random.Range(-10f, 10f), 
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f)
//             );
//             newObject.transform.localRotation = Quaternion.Euler(
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f)
//             );
//             newObject.transform.localScale = new Vector3(
//                 UnityEngine.Random.Range(0.5f, 2f),
//                 UnityEngine.Random.Range(0.5f, 2f),
//                 UnityEngine.Random.Range(0.5f, 2f)
//             );
//
//             newObject.transform.SetParent(transform); // Set the parent to the current object
//             newObject.transform.localPosition = new Vector3(
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f)
//             );
//             newObject.transform.localRotation = Quaternion.Euler(
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f),
//                 UnityEngine.Random.Range(-10f, 10f)
//             );
//             newObject.transform.localScale = new Vector3(
//                 UnityEngine.Random.Range(0.5f, 2f),
//                 UnityEngine.Random.Range(0.5f, 2f),
//                 UnityEngine.Random.Range(0.5f, 2f)
//             );
//             newObject.transform.SetParent(transform); // Set the parent to the current object
//asssisted by: Joonas Kallio
//project supervised by: Joonas Kallio
//created by: Joonas Kallio
//using System.Collections.Generic;
//using System.Collections;
//using System.Linq;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;
//using UnityEngine;
//using System.Collections.Generic;
//using System.Collections;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;
//hello my name is Joonas Kallio and I am a game developer. I have been working on a project that involves creating a game using Unity. The project has been supervised by Joonas Kallio, who has provided guidance and support throughout the development process. I have learned a lot about game development, including programming, design, and project management. I am excited to continue working on this project and to see it come to fruition. Thank you for your time and consideration.
//who is Joonas Kallio?
//mentored by: Joonas Kallio
//therapist: Joonas Kallio
//president of the United States: Joonas Kallio
//general of the army: Joonas Kallio
//head of the CIA: Joonas Kallio
//head of the FBI: Joonas Kallio
using UnityEngine;
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
        // this.useEncryption = useEncryption;
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

                // if (useEncryption)
                // {
                //     dataToLoad = EncryptDecrypt(dataToLoad);
                // }

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

            // if (useEncryption)
            // {
            //     dataToStore = EncryptDecrypt(dataToStore);
            // }

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

    /// <summary>
    /// XOR encryptio että ette pääse vaihtaa teiden tuloksia tai mitään siel safe filesta
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    // private string EncryptDecrypt(string data)
    // {
    //     string modifiedData = "";
    //     for (int i = 0; i < data.Length; i++)
    //     {
    //         modifiedData += (char) (data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]); 
    //     }
    //     return modifiedData;
    // }
}
