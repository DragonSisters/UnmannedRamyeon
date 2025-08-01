using System.Collections;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
#if UNITY_EDITOR
    private bool open = false;
    private int currentY = 10;
    private const int Y_SPACE_SIZE = 30;

    private void OnGUI()
    {
        currentY = 10;
        if (GUI.Button(new Rect(10, currentY, 100, 30), open ? "🔽 Close" : "▶️ Open"))
        {
            open = !open;
        }
        if (!open)
        {
            return;
        }

        if (GUI.Button(new Rect(10, CountUpY(), 150, 30), "Game Initialize"))
        {
            MoveManager.Instance.OnGameEnter();
            StartCoroutine(UpdateClick());
            ConsumerManager.Instance.InitializePools();
            IngredientManager.Instance.CreateIngredientObjOnPosition();
        }

        // 손님시스템
        if (GUI.Button(new Rect(10, CountUpY(), 150, 30), "Consumer StartSpawn"))
        {
            ConsumerManager.Instance.StartSpawn();
        }
        if (GUI.Button(new Rect(10, CountUpY(), 150, 30), "Consumer StopSpawn"))
        {
            ConsumerManager.Instance.StopSpawn();
        }
    }

    private IEnumerator UpdateClick()
    {
        while (true)
        {
            SpriteClickHandler.Instance.UpdateHandler();
            yield return null;
        }
    }

    private int CountUpY()
    {
        currentY += Y_SPACE_SIZE;
        return currentY;
    }
#endif
}
