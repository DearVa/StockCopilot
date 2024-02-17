namespace StockCopilot.Abstractions.Extensions;

public static class ArrayExtension
{
    public static void Deconstruct<T>(this T[] array, out T a)
    {
        a = array[0];
    }
    
    public static void Deconstruct<T>(this T[] array, out T a, out T b)
    {
        a = array[0];
        b = array[1];
    }
    
    public static void Deconstruct<T>(this T[] array, out T a, out T b, out T c)
    {
        a = array[0];
        b = array[1];
        c = array[2];
    }
    
    public static void Deconstruct<T>(this T[] array, out T a, out T b, out T c, out T d)
    {
        a = array[0];
        b = array[1];
        c = array[2];
        d = array[3];
    }
}