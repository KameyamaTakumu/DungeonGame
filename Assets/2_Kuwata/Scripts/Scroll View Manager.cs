using UnityEngine;

public class ScrollViewManager:MonoBehaviour
{
    [SerializeField] GameObject GameObject;

    public void OnOptionButton()
    {
        if(GameObject != null)
        {
        GameObject.SetActive(!GameObject.activeSelf);
        }
          
    }
}
