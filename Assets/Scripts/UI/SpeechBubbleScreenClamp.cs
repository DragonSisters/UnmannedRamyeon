using UnityEngine;

public class SpeechBubbleScreenClamp : MonoBehaviour
{
    private Vector2 padding = new Vector2(80f, 50f); // 화면 경계 여백
    private Vector3 initialLocalPosition; // 초기 localPosition 저장
    private float lerpSpeed = 10f; // 위치 보간 속도

    void Start()
    {
        initialLocalPosition = transform.localPosition; // 초기 위치 저장
    }

    void LateUpdate()
    {
        // 초기 위치 기준으로 월드 좌표 계산
        Vector3 targetWorldPos = transform.parent.TransformPoint(initialLocalPosition);

        // 월드 → 스크린 변환
        Vector2 screenPos = InputUtility.WorldToScreenPoint(targetWorldPos);

        // 경계에 닿았는지 확인
        bool clamped =
            screenPos.x < padding.x ||
            screenPos.x > Screen.width - padding.x ||
            screenPos.y < padding.y ||
            screenPos.y > Screen.height - padding.y;

        Vector3 targetLocalPos;

        if (clamped)
        {
            // 화면 경계 클램핑
            screenPos.x = Mathf.Clamp(screenPos.x, padding.x, Screen.width - padding.x);
            screenPos.y = Mathf.Clamp(screenPos.y, padding.y, Screen.height - padding.y);

            // 다시 스크린 → 월드 변환
            var clampedWorldPos = InputUtility.ScreenToWorldPoint(screenPos);

            // 부모 기준 localPosition으로 변환해서 적용
            targetLocalPos = transform.parent.InverseTransformPoint(clampedWorldPos);
        }
        else
        {
            // 경계에 닿지 않으면 초기 위치로 복귀
            targetLocalPos = initialLocalPosition;
        }

        // 현재 위치에서 목표 위치로 서서히 이동
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * lerpSpeed);
    }
}
