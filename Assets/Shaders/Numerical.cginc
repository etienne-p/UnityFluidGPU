// sum cell neighbors, useful for poisson solver
#define FLUID_SUM_NEIGHBORS(v, t, uv, txsize) v = \
    tex2D(t, uv - float2(1, 0) * txsize) + \
    tex2D(t, uv + float2(1, 0) * txsize) + \
    tex2D(t, uv - float2(0, 1) * txsize) + \
    tex2D(t, uv + float2(0, 1) * txsize);

// gradient using centered finite difference
#define FLUID_SCALAR_GRADIENT(v, t, uv, txsize) v = float2( \
    tex2D(t, uv + float2(1, 0) * txsize).x - tex2D(t, uv - float2(1, 0) * txsize).x, \
    tex2D(t, uv + float2(0, 1) * txsize).x - tex2D(t, uv - float2(0, 1) * txsize).x);

#define FLUID_VECTOR2_GRADIENT(v, t, uv, txsize) v = float2( \
    tex2D(t, uv + float2(1, 0) * txsize).x - tex2D(t, uv - float2(1, 0) * txsize).x, \
    tex2D(t, uv + float2(0, 1) * txsize).y - tex2D(t, uv - float2(0, 1) * txsize).y);