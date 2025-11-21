using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private bool isPlayerTurn;
    public int playerHP = 100;
    public int enemyHP  = 100;
    public int attack   = 20;

    public void Start()
    {
        isPlayerTurn = true;
        StartCoroutine(TurnCalc());
    }

    public void Update()
    {
        
    }

    public void ChangeTurn() 
    {
        isPlayerTurn = !isPlayerTurn; // ターンを逆にする
        StartCoroutine(TurnCalc());   // ターンを相手に回す
    }

    // ターン処理を待機してから実行するコルーチン
    public System.Collections.IEnumerator TurnCalc()
    {
        yield return new WaitForSeconds(3f); // n秒待つ

        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            EnemyTurn();
        }
    }

    public void PlayerTurn()
    {
        Debug.Log("プレイヤーのターン ");
        //プレイヤーの処理
        enemyHP -= attack;
        Debug.Log("敵に" + attack + "のダメージを与えた。敵の残りHPは" + enemyHP);
        ChangeTurn();
        
    }

    public void EnemyTurn()
    {
        Debug.Log("敵のターン ");
        //敵の処理


        ChangeTurn();
    }
}
