float2 rotate(float2 v, float a) {
	float s = sin(a);
	float c = cos(a);
	float2x2 m = float2x2(c, -s, s, c);
	return mul(v, m);
}