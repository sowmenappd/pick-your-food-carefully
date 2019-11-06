[System.Serializable]
public struct Wave{
  int bombCount;
  int foodCount;

  public Wave(int bombCount, int foodCount){
    this.bombCount = bombCount;
    this.foodCount = foodCount;
  }
}