﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Structure : MonoBehaviour, IDamageable<float> {

    [Header("Inherited Parameters")]
    [SerializeField] protected float buildingCost;
    [SerializeField] protected float upgradeCost;
    [SerializeField] protected float maxHealth = 100;
    protected float health;

    [SerializeField] protected GameObject healthBar;
    [SerializeField] protected Image healthBarFilling;
    [SerializeField] protected Text healthValue;

    protected void InitialiseValues() {
        health = maxHealth;
    }

    public void replenishHealth(){
        health = maxHealth;
    }

    public float GetBuildingCost() {
        return buildingCost;
    }
    public float GetUpgradeCost() {
        return upgradeCost;
    }

    public float GetHealth(){
        return health;
    }
    public void TakeDamage(float dmg) {
        health -= dmg;
        StartCoroutine(FluentlyUpdateHealthbar());
        if (health <= 0) {
            Die();
        }
    }
    IEnumerator FluentlyUpdateHealthbar() {

        if (!healthBar.activeInHierarchy) {
            healthBar.SetActive(true);
        }

        healthValue.text = health.ToString();

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

    public abstract void Upgrade();

    public void Sell() {
        TowerDefense.GameManager.instance.EarnMoney(buildingCost * 0.8f);
        Die();
    }

    public virtual void Die() {
        //W przyszłości kwestie graficzne umierania (animacje/eksplozje/particle etc.)
        gameObject.SetActive(false);
    }

}
