using System.Reflection;
using System.Reflection.Emit;

namespace Peek;

public static class Inspector
{
    public static long GetAddress<T>(IPinnable<T> o) where T : unmanaged
    {
        long result;
        unsafe
        {
            fixed (T* p = &o.GetPinnableReference())
            {
                result = (long)p;
            }
        }

        return result;
    }
    public static long GetAddress<T>(T o) where T : unmanaged
    {
        long result;
        unsafe
        {
            result = (long)&o;
        }

        return result;
    }

    public static byte[] Peek(long address, int bytes = 4)
    {
        var result = new byte[bytes];

        unsafe
        {
            byte* bob = (byte*)address;

            for (int i = 0; i < bytes; i++)
            {
                result[i] = *(bob + i);
            }
        }

        return result;
    }

    public static (string Name, long Address, int size, object? Value)[] GetFields(object o)
    {
        var type = o.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var result = new List<(string, long, int, object?)>();
        foreach (var field in fields)
        {
            var s = field.FieldType.IsValueType ? GetFieldSize(field) : 8;
            result.Add((field.Name, GetFieldAddress(o, field), GetFieldSize(field), field.GetValue(o)));
        }
        return result.ToArray();
    }

    private static long GetFieldAddress(object o, FieldInfo field)
    {
        var method = new System.Reflection.Emit.DynamicMethod("GetField", typeof(long), new Type[] { typeof(object) });
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldflda, field);
        il.Emit(OpCodes.Ret);

        var del = method.CreateDelegate<GetFieldAddrDelegate>();
        return del(o);
    }

    private static int GetFieldSize(FieldInfo field)
    {
        var method = new System.Reflection.Emit.DynamicMethod("GetField", typeof(int), null);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Sizeof, field.FieldType);
        il.Emit(OpCodes.Ret);

        var del = method.CreateDelegate<GetFieldSizeDelegate>();
        return del();
    }

    private delegate long GetFieldAddrDelegate(object o);
    private delegate int GetFieldSizeDelegate();

    public static int DoSomething(int i) => i + i;
}

public interface IPinnable<T> where T : unmanaged
{
    ref T GetPinnableReference();
}

public class Test : IPinnable<int>
{
    private int _pin = 0x12345678;
    public int Prop1 { get; set; }
    public int Prop2 { get; set; }
    public string MyString { get; set; } = string.Empty;
    public Test2 MyTest2 { get; set; } = new();

    ref int IPinnable<int>.GetPinnableReference() => ref _pin;
}

public class Test2
{
    public int MyProperty { get; set; } = 0x09ABCDEF;
}
