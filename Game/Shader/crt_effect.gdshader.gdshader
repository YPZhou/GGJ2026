shader_type canvas_item;


// 扫描线数量
uniform float scanline_count : hint_range(0, 1080) = 100.0;
// 扫描线透明度
uniform float scanline_opacity : hint_range(0, 1) = 0.3; // 稍微降低一点，不要太黑
// 噪点强度
uniform float static_intensity : hint_range(0, 1) = 0.05;
// 颜色亮度增强 (用于配合发光)
uniform float boost : hint_range(1.0, 5.0) = 1.6;

void fragment() {
    // 获取原图颜色
    vec4 color = texture(TEXTURE, UV);
    
    // --- 1. 扫描线效果 ---
    // 使用正弦波根据UV纵坐标生成条纹
    float s = sin(UV.y * scanline_count * PI * 2.0);
    s = (s * 0.5 + 0.5) * 0.9 + 0.1; 
    vec4 scanline_color = vec4(0.0, 0.0, 0.0, 1.0);
    // 混合黑色条纹
    color = mix(color, scanline_color, (1.0 - s) * scanline_opacity);
    
    // --- 2. 噪点/雪花效果 ---
    // 基于时间生成随机噪点
    float noise = fract(sin(dot(UV * TIME, vec2(12.9898, 78.233))) * 43758.5453);
    // 仅在暗部或整体混合噪点
    color.rgb = mix(color.rgb, vec3(noise), static_intensity);
    
    // --- 3. 亮度增强(为了发光) ---
    color.rgb *= boost;
    
    COLOR = color;
}