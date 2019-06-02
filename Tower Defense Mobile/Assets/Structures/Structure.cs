using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Structure : MonoBehaviour, IDamageable<float> {

    [Header("Inherited Parameters")]    
    [SerializeField] protected string structureName;

    protected int structureLevel = 1;

    [SerializeField] protected float buildingCost;
    [SerializeField] protected float upgradeCost;

    [SerializeField] Text levelIndicator;

    [SerializeField] protected float maxHealth;
    protected float health;

    [SerializeField] protected GameObject healthBar;
    [SerializeField] protected Image healthBarFilling;
    [SerializeField] protected Text healthValue;

    public void ReplenishHealth() {
        health = maxHealth;
        StartCoroutine(UpdateHealthbar());
    }

    public float GetBuildingCost() {
        return buildingCost;
    }

    public float GetUpgradeCost() {
        return upgradeCost;
    }

    public float GetHealth() {
        return health;
    }

    public string GetStructureName() {
        return structureName;
    }

    public int GetStructureLevel() {
        return structureLevel;
    }

    public virtual void TakeDamage(float dmg) {
        health -= dmg;
        StartCoroutine(UpdateHealthbar());
        if (health <= 0) {
            Die();
        }
    }

    public IEnumerator UpdateHealthbar() {

        if (!healthBar.activeInHierarchy) {
            healthBar.SetActive(true);
        }

        healthValue.text = Mathf.Round(health).ToString();

        float elapsedTime = 0;
        float healthbarScalingTime = 0.1f;
        float startFillAmount = healthBarFilling.fillAmount;
        float targetFillAmount = health / maxHealth;

        while (healthBarFilling.fillAmount != targetFillAmount) {

            healthBarFilling.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, Mathf.Clamp01(elapsedTime / healthbarScalingTime));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }

    }

    public abstract void Upgrade(int levels = 1);

    public void UpdateLevelIndicator() {

        levelIndicator.text = "lvl." + structureLevel;

    }

    public void Sell() {
        EndlessBitDefense.GameManager.instance.EarnMoney(buildingCost * 0.8f);
        Die();
    }

    public virtual void Die() {
        //W przyszłości kwestie graficzne umierania (animacje/eksplozje/particle etc.)
        Destroy(gameObject);
    }

}
