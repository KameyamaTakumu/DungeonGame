using UnityEngine;
using System.Collections;
using System.Net;

public class BattleManager : MonoBehaviour
{
    public bool isPlayerTurn;
    public int playerHP = 100;
    public int enemyHP = 100;
    public int attack = 20;

    public void Start()
    {
        isPlayerTurn = true;
        //StartCoroutine(TurnCalc());
    }

    public void Update()
    {

    }

    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを逆にする
        //StartCoroutine(TurnCalc());   // ターンを相手に回す
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
        if (!isPlayerTurn) return;

        Debug.Log("プレイヤーのターン ");
        //プレイヤーの処理
        enemyHP -= attack;
        Debug.Log("敵に" + attack + "のダメージを与えた。敵の残りHPは" + enemyHP);
        //ChangeTurn();

        Debug.Log("プレイヤーターン終了 > 敵ターンへ");
        isPlayerTurn = false;

        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        //Debug.Log("敵のターン ");
        //敵の処理
        //ChangeTurn();

        yield return new WaitForSeconds(1f); // n秒待つ

        Debug.Log("敵ターン");

        // 敵の処理
        
        int x = Random.Range(-1, 2); // -1, 0, 1 のいずれか
        int y = Random.Range(-1, 2); // -1, 0, 1 のいずれか
        while (!(x == 0 && y != 0) || (x != 0 && y == 0))
        {
            x = Random.Range(-1, 2);
            y = Random.Range(-1, 2);
        }
            


        Test_PlayerMovement.instance.TryMove(x,y); // 左に1マス移動

        // 敵ターン終了 > プレイヤーの行動待ち
        isPlayerTurn = true;

        Debug.Log("敵ターン終了 > 次のプレイヤー行動待ち");
        yield return null;
    }


}
