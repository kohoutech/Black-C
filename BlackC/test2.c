//calculates e raised to the power x
//by summing its Taylor series expansion
// e^x = 1 + x + x^2/2! + x^3/3! + x^4/4! + x^5/5! + ...

float test2 (float x) {

  float result = 1.0;
  float term = 1.0;

  for (int i = 1; i <= 20; i++) {
    term *= x;
    term /= i;
    result += term;
  }

  return result;
}
