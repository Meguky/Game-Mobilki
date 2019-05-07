using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public static UIManager instance;

    [Header("Required References")]
    [SerializeField] private Text gameLog;
    [SerializeField] private Text moneyTextbox;

    // Start is called before the first frame update
    void Start() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }
        UpdateMoney();
    }

    // Update is called once per frame
    void Update() {

    }

    [Header("Game Log Parameters")]
    [SerializeField] private float logVisibilityTime = 2.0f;
    [SerializeField] private float logFadeoutTime = 1.0f;

    IEnumerator ShowMessageOnLog(string message) {

        gameLog.text = message;
        gameLog.color = new Color(gameLog.color.r, gameLog.color.g, gameLog.color.b, 255);
        yield return new WaitForSeconds(logVisibilityTime);

        float elapsedTime = 0;

        while (gameLog.color.a!=0) {

            float newAlpha = Mathf.Clamp(Mathf.Lerp(1, 0, elapsedTime/logFadeoutTime), 0, 1);
            gameLog.color = new Color(gameLog.color.r, gameLog.color.g, gameLog.color.b, newAlpha);

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void PrintToGameLog(string message) {
        StartCoroutine(ShowMessageOnLog(message));
    }

    public void UpdateMoney() {
        moneyTextbox.text = "Funds: " + TowerDefense.GameManager.instance.money + "$";
    }

}
