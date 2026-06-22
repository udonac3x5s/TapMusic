using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Light : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField] private Color glowColor = Color.cyan;  // 光る色
    [Min(0f)] private float peakIntensity = 2f; // ピカッの強さ（HDR）
    [SerializeField, Min(0.01f)] private float fadeTime = 0.12f; // 減衰時間（秒）

    private Renderer rend;
    private MaterialPropertyBlock mpb;

    // URP/Lit の発光色プロパティ
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private float t;          // 0..fadeTime
    private bool glowing;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();          // マテリアルプロパティブロックとは、個別オブジェクトのマテリアルプロパティを制御するための仕組み

        // 初期：発光なし
        SetEmission(0f);
    }

    private void UpdateEmission()
    {
        if (!glowing) return;

        t += Time.deltaTime;
        float k = 1f - Mathf.Clamp01(t / fadeTime);   // 1→0

        // “一瞬ピカッ”寄りにしたいなら二乗で落とす（好み）
        k = k * k;

        SetEmission(peakIntensity * k);

        if (k <= 0.001f)
        {
            glowing = false;
            SetEmission(0f);
        }
    }

    /// <summary>タップ時に呼ぶ</summary>
    public void TriggerGlow()
    {
        // t = 0f;
        // glowing = true;
        // SetEmission(peakIntensity);
        Grow();
    }

    private void SetEmission(float intensity)
    {
        // オブジェクト単位で色を上書き（マテリアル複製しない）
        rend.GetPropertyBlock(mpb);                             // 既存のプロパティを取得
        mpb.SetColor(EmissionColorId, glowColor * intensity);   // 発光色を設定
        rend.SetPropertyBlock(mpb);                             // プロパティを反映
    }







    [Header("Emission Fade")]
    Renderer myRenderer;


    private Color emissionColor = Color.gray;  // 発光する色
    private float emissionIntensity = 1.6f;    // 発光の強度
    private float emissionFadeSpeed = 0.2f;    // 発光の減衰速度



    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            Material material = myRenderer.material;

            // エミッションを有効化
            material.EnableKeyword("_EMISSION");
        }
    }



    void Grow()
    {
        // myRenderer = GetComponent<Renderer>();
        // if (myRenderer != null)
        // {
        //     Material material = myRenderer.material;
        //     // 発光色を設定
        //     Color finalColor = emissionColor * emissionIntensity;
        //     material.SetColor("_EmissionColor", finalColor);
        // }

        // オブジェクト単位で色を上書き（マテリアル複製しない）
        rend.GetPropertyBlock(mpb);                             // 既存のプロパティを取得
        mpb.SetColor(EmissionColorId, emissionColor * emissionIntensity);   // 発光色を設定
        rend.SetPropertyBlock(mpb);                             // プロパティを反映


    }

    void Fade()
    {
        // if (myRenderer != null)
        // {
        //     Material material = myRenderer.material;

        //     // 現在の発光色を取得
        //     Color currentColor = material.GetColor("_EmissionColor");

        //     // 発光色を徐々に減衰させる
        //     Color fadedColor = Color.Lerp(currentColor, Color.black, emissionFadeSpeed);
        //     material.SetColor("_EmissionColor", fadedColor);
        // }

        // オブジェクト単位で色を上書き（マテリアル複製しない）
        rend.GetPropertyBlock(mpb);                             // 既存のプロパティを取得
        Color currentColor = mpb.GetColor(EmissionColorId);
        Color fadedColor = Color.Lerp(currentColor, Color.black, emissionFadeSpeed);
        mpb.SetColor(EmissionColorId, fadedColor);   // 発光色を設定
        rend.SetPropertyBlock(mpb);                             // プロパティを反映
    }

    void FixedUpdate()
    {
        Fade();
    }



}
