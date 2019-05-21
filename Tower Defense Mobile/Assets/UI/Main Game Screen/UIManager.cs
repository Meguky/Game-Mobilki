using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public static UIManager instance;

    [Header("Required References")]
    [SerializeField] private Text gameLog;
    [SerializeField] private Text moneyTextbox;
    [SerializeField] private RectTransform bottomLeftPanel;
    [SerializeField] private RectTransform hidePanelButton;

    private EndlessBitDefense.GameManager gameManager;

    private bool bottomPanelShown = true;

    // Start is called before the first frame update
    void Start() {

        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }

        gameManager = EndlessBitDefense.GameManager.instance;

        UpdateMoney();
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

    [Header("Panel Parameters")]
    [SerializeField] private Vector2 panelRevealedPosition;
    [SerializeField] private Vector2 panelHiddenPosition;
    [SerializeField] private float panelTransitionTime;

    IEnumerator MovePanelToPosition() {

        float elapsedTime = 0;
        Vector2 startPosition, targetPosition;
        float buttonStartRotation, buttonTragetRotation;

        if (bottomPanelShown) {

            targetPosition = panelRevealedPosition;
            startPosition = panelHiddenPosition;

            buttonStartRotation = 180;
            buttonTragetRotation = 0;

        }
        else {

            targetPosition = panelHiddenPosition;
            startPosition = panelRevealedPosition;

            buttonStartRotation = 0;
            buttonTragetRotation = 180;

        }

        while (bottomLeftPanel.anchoredPosition!=targetPosition) {

            bottomLeftPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, Mathf.Clamp01(elapsedTime / panelTransitionTime));
            hidePanelButton.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(buttonStartRotation, buttonTragetRotation, Mathf.Clamp01(elapsedTime / panelTransitionTime)));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }

    }

    public void ToggleBottomLeftPanel() {

        bottomPanelShown = !bottomPanelShown;

        StartCoroutine(MovePanelToPosition());

    }

    public void UpdateMoney() {
        moneyTextbox.text = "Funds: " + Mathf.Round(gameManager.GetMoney()) + "$";
    }

}
