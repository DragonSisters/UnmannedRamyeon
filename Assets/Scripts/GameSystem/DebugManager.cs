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
            FinanceManager.Instance.OnGameEnter();
            StartCoroutine(UpdateClick());
            ConsumerManager.Instance.InitializeConsumerManagerSetting();
            IngredientManager.Instance.CreateIngredientObjOnPosition();
            SoundManager.Instance.PlayBgmSound(BgmSoundType.InGame);
        }

        // 손님시스템
        if (GUI.Button(new Rect(10, CountUpY(), 150, 30), "Consumer StartSpawn"))
        {
            ConsumerManager.Instance.StartSpawn(true); // @anditsoon TODO: 수정 후 에러 떠서 일단은 임의로 하드모드로 설정합니다. 어차피 디버그 매니저라 나중에 지울 거 같아서요.
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
