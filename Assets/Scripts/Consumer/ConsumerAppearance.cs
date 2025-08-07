using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ConsumerAppearance : MonoBehaviour, IClickableSprite
{
    public bool IsClickable => isClickable;
    private bool isClickable = true;

    /// <summary>
    /// 클릭 된 상태인지 반환합니다.
    /// </summary>
    public bool IsClicked => isClicked;
    private bool isClicked = false;

    private bool isSpringWorking = false;

    private Animator animator;
    private NavMeshAgent agent;
    private Material material;

    private readonly int walkAnimId = Animator.StringToHash("Walk");
    private readonly int outlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
    private readonly int springScaleId = Shader.PropertyToID("_SpringScale");

    private const float SPRING_ORIGIN_SCALE = 1f;

    public delegate void OnClickHandler();
    public event OnClickHandler OnClick;

    public delegate void OnDeselectHandler();
    public event OnDeselectHandler OnDeselect;

    public void Initialize()
    {
        animator = gameObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"손님 외형 프리팹에서 애니메이터를 찾지 못했습니다. 원본 프리팹을 확인해주세요.");
        }
        agent = gameObject.GetComponentInParent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"손님에게서 NavMeshAgent를 찾지 못했습니다. 원본 프리팹을 확인해주세요.");
        }
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.LogError($"손님 외형 프리팹에서 SpriteRenderer를 찾지 못했습니다. 원본 프리팹을 확인해주세요.");
        }
        material = spriteRenderer.material;
        SetMaterialSettingUnclicked();
    }

    private void SetMaterialSettingUnclicked()
    {
        material.SetFloat(outlineEnabledId, 0f);
        material.SetFloat(springScaleId, SPRING_ORIGIN_SCALE);
    }

    public void OnUpdate()
    {
        // agent가 움직이는 상태라면 애니메이션을 Walk로 바꿉니다.
        animator.SetBool(walkAnimId, agent.velocity.magnitude > 0);
    }

    public void OnSpriteClicked()
    {
        if (!isClickable)
        {
            return;
        }

        Debug.Log($"손님{gameObject.GetInstanceID()}가 클릭되었습니다.");
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);

        // 모든 Consumer 검사하여 다른 손님은 Click해제
        ConsumerManager.Instance.DeselectOtherConsumers();

        material.SetFloat(outlineEnabledId, 1f);

        if(!isSpringWorking) // 중복방지
        {
            StartCoroutine(SpringAnimation());
        }

        isClicked = true;

        OnClick?.Invoke();
    }

    public void OnSpriteDeselected()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Unclick);

        SetMaterialSettingUnclicked();

        isClicked = false;

        OnDeselect?.Invoke();
    }

    private IEnumerator SpringAnimation()
    {
        isSpringWorking = true;

        float duration = 0.6f;

        // 랜덤값 설정
        float amplitude = Random.Range(0.2f, 0.25f); // 진폭
        float frequency = Random.Range(15f, 20f);    // 진동수
        float damping = Random.Range(3f, 6f);        // 감쇠 계수

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed;

            // 감쇠 진동 함수 (스프링 수식)
            float scale = 1f + amplitude * Mathf.Exp(-damping * t) * Mathf.Sin(frequency * t);
            material.SetFloat(springScaleId, scale);

            yield return null;
        }

        // 애니메이션 끝나면 원래 크기로
        material.SetFloat(springScaleId, SPRING_ORIGIN_SCALE);
        isSpringWorking = false;
    }
}
