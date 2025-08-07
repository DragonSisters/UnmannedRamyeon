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

    private Animator animator;
    private NavMeshAgent agent;
    private Material material;
    private readonly float outlineWidth = 10;
    private readonly Color outlineColor = Color.cyan;

    private readonly int walkAnimId = Animator.StringToHash("Walk");
    
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
        ShaderEffectHelper.SetOutlineWidth(material, outlineWidth);
        ShaderEffectHelper.SetOutlineColor(material, outlineColor);
        ShaderEffectHelper.SetOutlineEnable(material, false);
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

        ShaderEffectHelper.SetOutlineEnable(material, true);
        StartCoroutine(ShaderEffectHelper.SpringAnimation(material));

        isClicked = true;

        OnClick?.Invoke();
    }

    public void OnSpriteDeselected()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Unclick);

        ShaderEffectHelper.SetOutlineEnable(material, false);

        isClicked = false;

        OnDeselect?.Invoke();
    }
}
