using UnityEngine;

public static class DifficultyAdjuster
{

  const float a1 = 1.2f;
  const float c1 = 0.13f;

  const float a2 = 1.1f;
  const float c2 = 0.2f;

  const float c3 = -5f;
  
  const float c4 = 6f;

  static int[] levelHealCount = new int[36]
  { 
    0, 0, 1, 0, 1, 0, 0, 1, 0, 0,
    1, 0, 0, 0, 1, 0, 0, 0, 0, 1,
    0, 0, 0, 1, 0, 0, 1, 0, 0, 1,
    0, 1, 1, 0, 1, 2
  };
  public static int[] LevelHealCount{
    get { return levelHealCount; }
  }

  public static int GetBombCount(int level){
    return Mathf.RoundToInt(Mathf.Pow(c1, 2) * Mathf.Pow(level, 2) + Mathf.Pow(a1, 6));
  }

  // public static int GetFoodCount(int level){
  //   return Mathf.RoundToInt(Mathf.Pow(c2, 2) * Mathf.Pow(level, 2) - Mathf.Pow(a2, 2) + 1.2f);
  // }

  public static int GetHealdropCount(int level){
    float num = 0.1f * Mathf.Pow(level, 0.7f);
    float denum = 0.1f + Mathf.Exp(-(level + c3));
    return Mathf.RoundToInt(num / denum);
  }

  public static float GetBombFuseTime(int level){
    return (c4 / (Mathf.Pow(level, 0.5f) + 1f));
  }

}
