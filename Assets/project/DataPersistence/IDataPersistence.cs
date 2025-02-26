using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public interface IDataPersistence 
{
    void LoadData( GameData data);

    void SaveData( ref GameData data);
    
}
