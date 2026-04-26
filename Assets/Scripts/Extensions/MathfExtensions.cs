using UnityEngine;

public static class MathfExtensions
{
    public static int NumberLoop(int value, int min, int max) {
        if (value < min) {
            return max;
        } else if (value > max) {
            return min;
        } else {
            return value;
        }
    }
}
