using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour, IDamageable<float> {

    [System.Serializable] public class EnemyEvent : UnityEvent<Enemy> { }
    [HideInInspector] public EnemyEvent OnDeath = new EnemyEvent();

    [Header("Inherited values")]
    [SerializeField] protected float maxHealth;
    protected float health;
    [SerializeField] protected float defaultDamage;
    protected float damage;
    [SerializeField] protected float killingReward;
    [SerializeField] protected float movementSpeed;

    [SerializeField] protected GameObject healthBar;
    [SerializeField] protected Image healthBarFilling;
    [SerializeField] protected Text healthValue; 

    protected void InitialiseValues() {
        health = maxHealth;
        damage = defaultDamage;
    }

    public void ScaleParameters(float healthMultiplier, float damageMultiplier, float rewardMultiplier) {
        //Debug.Log(healthMultiplier);
        maxHealth += maxHealth * healthMultiplier;
        defaultDamage += defaultDamage * damageMultiplier;
        killingReward += killingReward * rewardMultiplier;
    }

    IEnumerator UpdateHealthbar() {

        if (!healthBar.activeInHierarchy) {
            healthBar.SetActive(true);
        }

        if (health < 0) {
            healthBar.SetActive(false);
            yield break;
        }
        else {
            healthValue.text = Mathf.Round(health).ToString();
        }

        float elapsedTime = 0;
        float healthbarScalingTime = 0.1f;
        float startFillAmount = healthBarFilling.fillAmount;
        float targetFillAmount = health/maxHealth;

        while (healthBarFilling.fillAmount != targetFillAmount) {

            healthBarFilling.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, Mathf.Clamp01(elapsedTime / healthbarScalingTime));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }

    }

    protected LinkedList<Vector3> currentPath = new LinkedList<Vector3>();
    protected LinkedListNode<Vector3> targetIterator;

    Vector3 movementDirection;
    Vector3 currentDestination;

    int currentTargetIndex = 0;
    protected float distanceFromNextWaypoint;

    //zwraca pozycję na ścieżce waypointów i odległość od obecnego celu
    public float[] DistanceToBase() {
        return new[] { currentPath.Count - currentTargetIndex, distanceFromNextWaypoint };
    }

    public void TakeDamage(float dmg) {
        health -= dmg;
        StartCoroutine(UpdateHealthbar());
        if (health <= 0) {
            Die();
        }
    }

    private void GetNewPath() {
        currentPath = MapManager.instance.FindGroundPathToBaseFrom(transform.position);
        targetIterator = currentPath.First;
        currentTargetIndex = 0;
        GetNextTarget();
    }

    public void RegisterAsMapchangeListener() {
        MapManager.instance.OnMapChange.AddListener(GetNewPath);
    }

    IEnumerator DeathFadeout() {

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        float elapsedTime = 0f;
        float deathFadeoutTime = 0.5f;

        while (renderer.color.a > 0) {

            float newAlpha = Mathf.Clamp(Mathf.Lerp(1, 0, elapsedTime / deathFadeoutTime), 0, 1);
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, newAlpha);

            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    public virtual void Die() {
        Die(true);
    }

    public virtual void Die(bool earnReward = true) {

        OnDeath.Invoke(this);

        if (earnReward) {
            EndlessBitDefense.GameManager.instance.EarnMoney(killingReward);
        }

        StartCoroutine(DeathFadeout());

    }

    protected virtual void GetNextTarget() {

        if (targetIterator == currentPath.Last) {
            return;
        }
        else {
            movementDirection = (targetIterator.Value - transform.position).normalized * movementSpeed;
            currentDestination = targetIterator.Value;
            targetIterator = targetIterator.Next;
            currentTargetIndex++;
        }

    }

    protected virtual void Move() {
        if (health>0) {

            Vector3 expectedMovement = movementDirection * Time.deltaTime;
            distanceFromNextWaypoint = Vector3.Distance(transform.position, currentDestination);

            //check if we wont overstep
            if (expectedMovement.magnitude > distanceFromNextWaypoint) {
                transform.Translate(currentDestination - transform.position, Space.World);
            }
            else {
                transform.Translate(expectedMovement, Space.World);
            }

            if (currentDestination == transform.position) {
                GetNextTarget();
            }
        }
    }

}
