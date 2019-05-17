using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance {set; get;}

    [SerializeField] private TowerDefense.GameManager gameManager;
    [SerializeField] private MapManager mapManager;
    public SaveState state;
    private void Awake(){
        DontDestroyOnLoad(gameObject);
        Instance = this;
        PlayerPrefs.DeleteAll(); // ODKOMENTOWAĆ JEŻELI CHCESZ ODPALAĆ BEZ WCZYTANIA ZAPISU
        Load();
        
    }
    //save zapisany do player pref
    public void Save(){
        PlayerPrefs.SetString("save",SaveSerializer.Serialize<SaveState>(state));
    }

    public void Load(){
        if(PlayerPrefs.HasKey("save")){
            state = SaveSerializer.Deserialize<SaveState>(PlayerPrefs.GetString("save"));
        }else{
            state = new SaveState();
        }
    }
    void OnApplicationQuit()
    {    
        state.money = gameManager.money;
        state.waveNumber = gameManager.waveNumber;
        state.setMapTiles(mapManager.getMap());
        Debug.Log(SaveSerializer.Serialize<SaveState>(state));
        Save();
    }
}
