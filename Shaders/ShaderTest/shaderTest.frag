#version 330 core
out vec4 FragColor;

uniform float time;
uniform vec2 resolution;

in vec2 uv;

vec2 randomGradient(vec2 p) {
  p = p + 0.02;
  float x = dot(p, vec2(123.4, 234.5));
  float y = dot(p, vec2(234.5, 345.6));
  vec2 gradient = vec2(x, y);
  gradient = sin(gradient);
  gradient = gradient * 43758.5453;

  // part 4.5 - update noise function with time
  gradient = sin(gradient + time);
  return gradient;

  // gradient = sin(gradient);
  // return gradient;
}

vec3 pallete(float t)
{
  vec3 a = vec3(0.5, 0.5, 0.5);
  vec3 b = vec3(0.5, 0.5, 0.5);
  vec3 c = vec3(1.0, 1.0, 1.0);
  vec3 d = vec3(0.265, 0.416, 0.557);
  
  return a + b * cos(6.28318*(c*t+d));
}

void main()
{
    /*vec2 aUV = uv * 4.0;
    vec2 gridID = floor(aUV);
    vec2 gridUV = fract(aUV);
    
    vec2 bl = gridID + vec2(0.0, 0.0);
    vec2 br = gridID + vec2(1.0, 0.0);
    vec2 tl = gridID + vec2(0.0, 1.0);
    vec2 tr = gridID + vec2(1.0, 1.0);

    vec2 grad_bl = randomGradient(bl);
    vec2 grad_br = randomGradient(br);
    vec2 grad_tl = randomGradient(tl);
    vec2 grad_tr = randomGradient(tr);

    vec2 dist_bl = gridUV - vec2(0.0, 0.0);
    vec2 dist_br = gridUV - vec2(1.0, 0.0);
    vec2 dist_tl = gridUV - vec2(0.0, 1.0);
    vec2 dist_tr = gridUV - vec2(1.0, 1.0);

    float dot_bl = dot(grad_bl, dist_bl);
    float dot_br = dot(grad_br, dist_br);
    float dot_tl = dot(grad_tl, dist_tl);
    float dot_tr = dot(grad_tr, dist_tr);

    gridUV = smoothstep(0.0, 1.0, gridUV);

    float b = mix(dot_bl, dot_br, gridUV.x);
    float t = mix(dot_tl, dot_tr, gridUV.x);
    float perlin = mix(b, t, gridUV.y);
    
    vec3 color = vec3(perlin + 0.1);*/

    vec2 aUV = uv * 2.0 - 1.0;
    aUV.x *= resolution.x / resolution.y;

    float d = length(aUV);
    
    vec3 color = vec3(1.0, 0.20, 0.50);

    d = 0.02 / d;
    color *= d;
    FragColor = vec4(color, 1.0);
} 