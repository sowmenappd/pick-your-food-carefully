using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour {
  
  public bool IsAlive{
    get { return alive; }
  }
  bool alive = true;

  public int startingHealthPoints;
  
  [SerializeField] protected int healthPoints;
  public int HP{
    get { return healthPoints; }
  }

  public System.Action<int> OnHpChanged;
  public System.Action OnDeath;

  protected virtual void Awake () {
    healthPoints = startingHealthPoints;
  }

  public virtual void Heal(int amount){
    healthPoints += amount;
    if(OnHpChanged != null) OnHpChanged(healthPoints);
  }

  public virtual void Die () {
    print (name + " died.");
    alive = false;
    if(OnDeath != null) OnDeath();
  }

}