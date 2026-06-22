Shader "Custom/Gaming"
{
    Properties
    {
        // コントロール用パラメータ
        _ColorCycle ("ColorCycle", float) = 0.5
        _BrightCycle ("BrightCycle", float) = 3.0
    }

// 共通関数
CGINCLUDE
uniform float _ColorCycle;
uniform float _BrightCycle;

// 指定レベルに応じた色を返す
float3 color(float level)
{
    // level: 0~6 -> rgb
    float r = float(level <= 2.0) + float(level > 4.0) * 0.5;       // 赤成分
    float g = max(1.0 - abs(level - 2.0) * 0.5, 0.0);               // 緑成分
    float b = (1.0 - (level - 4.0) * 0.5) * float(level >= 4.0);    // 青成分
    // return float3(r, g, b);                                         // 色を返す

    // 基本は灰色だが、途中途中白色を混ぜる
    float3 gray = float3(0.25, 0.25, 0.25);
    float3 white = float3(1.0, 1.0, 1.0);
    float factor = abs(frac(level) - 0.5) * 2.0;                     // 0~1の係数
    return lerp(white, gray + float3(r, g, b) * 0.7, factor);

}

// 滑らかな色変化を返す
float3 smoothColor(float x)
{
    float level1 =  floor(x * 6.0);                                 // レベル1
    float level2 = min(6.0, floor(x*6.0) + 1.0);                    // レベル2
    float3 a = color(level1);                                       // 色1
    float3 b = color(level2);                                       // 色2
    return lerp(a, b, frac(x * 6.0));                               // 補間して返す
}

// 描画色を返す
float4 paint(float2 uv)
{
    float repeat = abs(fmod(uv.y * _ColorCycle + _Time.y, 1));
    float3 col = smoothColor(repeat);
    float timeBrightness = sin(_Time.y * _BrightCycle) + 1.5;
    return float4(col * timeBrightness, 1);
}
ENDCG

    SubShader
    {
        Pass
        {
            // 頂点・フラグメントシェーダー
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 頂点・フラグメントシェーダー用構造体
            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            // 頂点・フラグメントシェーダー用構造体
            struct fin
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            // 頂点シェーダー
            fin vert(appdata v)
            {
                fin o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            // フラグメントシェーダー
            float4 frag(fin IN) : SV_TARGET
            {
                return paint(IN.texcoord.xy);
            }
            ENDCG
        }
    }
}