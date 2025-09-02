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

    private bool isRecipeConsumer = false;

    private Animator animator;
    private NavMeshAgent agent;
    private Material material;
    private readonly float outlineWidth = 10;
    private readonly Color clickedOutlineColor = Color.cyan;
    private readonly Color unclickedOutlineColor = Color.white;

    private readonly int walkAnimId = Animator.StringToHash("Walk");
    
    public delegate void OnClickHandler();
    public event OnClickHandler OnClick;

    public delegate void OnDeselectHandler();
    public event OnDeselectHandler OnDeselect;

    public void OnDestroy()
    {
        // 메모리 누수 방지를 위한 인스턴스 material 삭제
        Destroy(material);
    }

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
        ShaderEffectHelper.SetOutlineColor(material, unclickedOutlineColor);

        // 레시피 손님이라면 처음부터 Outline을 켜도록 합니다.
        isRecipeConsumer = gameObject.GetComponentInParent<RecipeConsumer>() != null;
        ShaderEffectHelper.SetOutlineEnable(material, isRecipeConsumer);
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

        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);

        // 모든 Consumer 검사하여 다른 손님은 Click해제
        ConsumerManager.Instance.DeselectOtherConsumers();

        ShaderEffectHelper.SetOutlineColor(material, clickedOutlineColor);
        ShaderEffectHelper.SetOutlineEnable(material, true);
        StartCoroutine(ShaderEffectHelper.SpringAnimation(material));

        isClicked = true;

        OnClick?.Invoke();
    }

    public void OnSpriteDeselected()
    {
        ShaderEffectHelper.SetOutlineColor(material, unclickedOutlineColor);
        ShaderEffectHelper.SetOutlineEnable(material, isRecipeConsumer);

        isClicked = false;

        OnDeselect?.Invoke();
    }

    public void SetClickable(bool clickable)
    {
        isClickable = clickable;
        if(!clickable)
        {
            ShaderEffectHelper.SetOutlineEnable(material, isRecipeConsumer);
        }
    }
}
