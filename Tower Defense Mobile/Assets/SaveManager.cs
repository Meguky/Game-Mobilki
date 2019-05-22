using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {

    public static SaveManager Instance { set; get; }
    public SaveState state = new SaveState();

    private void Start() {

        if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }

    }

    public SaveState GetSavedState() {
        return state;
    }

    //save zapisany do player pref
    public void Save() {

        PlayerPrefs.SetString("save", SaveSerializer.Serialize<SaveState>(state));

    }

    public bool Load() {

        if (PlayerPrefs.HasKey("save")) {

            state = SaveSerializer.Deserialize<SaveState>(PlayerPrefs.GetString("save"));
            Debug.Log("Loaded save: " + PlayerPrefs.GetString("save"));
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

}
