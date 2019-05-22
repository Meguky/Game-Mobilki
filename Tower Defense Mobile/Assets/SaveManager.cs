using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {

    [SerializeField] private EndlessBitDefense.GameManager gameManager;
    [SerializeField] private MapManager mapManager;
    //public static SaveManager Instance { set; get; }
    public SaveState state = new SaveState();

    private void Start() {

        /*(if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }*/
        Load();
    }

    public SaveState GetSavedState() {
        return state;
    }

    //save zapisany do player pref
    public void Save() {
        PlayerPrefs.DeleteKey("save");
        PlayerPrefs.SetString("save", SaveSerializer.Serialize<SaveState>(state));
        Debug.Log("Saved save: " + PlayerPrefs.GetString("save"));
    }

    public bool Load() {

        if (PlayerPrefs.HasKey("save")) {

            state = SaveSerializer.Deserialize<SaveState>(PlayerPrefs.GetString("save"));
            Debug.Log("Loaded save: " + PlayerPrefs.GetString("save"));
            PlayerPrefs.DeleteKey("save");
            mapManager.ReconstructMap(state);
            gameManager.simulateWaves(state.waveNumber);
            gameManager.money = state.money;
            return true;

        }
        else {
            state = new SaveState();
            return false;
        }
    }

    public void DeleteSaveData() {
        PlayerPrefs.DeleteAll();
    }

    /*void OnApplicationPause() {
        MapTile[,] map = mapManager.getMap();
        if(map != null){
            state.SetMapTiles(map);
        }
        state.money = gameManager.money;
        state.waveNumber = gameManager.waveNumber;
        Save();
    }*/

    void OnApplicationQuit() {
        MapTile[,] map = mapManager.getMap();
        if(map != null){
            state.SetMapTiles(map);
        }
        state.money = gameManager.money;
        state.waveNumber = gameManager.waveNumber;
        Save();
    }

}
