using UnityEngine;

[System.Serializable]
public struct Window
{
  public Transform leftPos, rightPos, midPos;

  public Window(Transform l, Transform r, Transform m){
    this.leftPos = l;
    this.rightPos = r;
    this.midPos = m;
  }
}

[System.Serializable]
public struct Balcony{
  public Transform entry, outside;

  public Balcony(Transform e, Transform o){
    entry = e;
    outside = o;
  }
}
