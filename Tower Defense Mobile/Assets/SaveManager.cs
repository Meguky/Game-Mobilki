using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {

    public static SaveManager Instance { set; get; }
    public SaveState state = new SaveState();
    public bool saveLoaded = false;

    private void Start() {

        if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }

    }
    
    //save zapisany do player pref
    public void Save() {

        PlayerPrefs.SetString("save", SaveSerializer.Serialize<SaveState>(state));

    }

    public bool Load() {

        //PlayerPrefs.DeleteAll(); // ODKOMENTOWAĆ JEŻELI CHCESZ ODPALAĆ BEZ WCZYTANIA ZAPISU

        if (PlayerPrefs.HasKey("save")) {

            state = SaveSerializer.Deserialize<SaveState>(PlayerPrefs.GetString("save"));
            Debug.Log("Loaded save: " + PlayerPrefs.GetString("save"));
            saveLoaded = true;
            return true;

        }
        else {
            state = new SaveState();
            saveLoaded = false;
            return false;
        }
    }
}
