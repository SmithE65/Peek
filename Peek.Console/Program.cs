// See https://aka.ms/new-console-template for more information
using Peek;

var hello = "Hello, World!";
Console.WriteLine(hello);

var t1 = new Test() { MyString = hello, Prop1 = 0xABA, Prop2 = 0xABCD };
var t2 = new Test() { MyString = hello, Prop1 = 0xDEF, Prop2 = 0x2468 };

Console.WriteLine("A string...");
Inspect("This is a string", "myString");

lock (t2)
{
    Inspect(t1, nameof(t1));
    Inspect(t2, nameof(t2));
}

Console.WriteLine("Done");

static void Inspect(object o, string varName)
{
    var type = o.GetType();
    Console.WriteLine();
    Console.WriteLine($"Inspecting {type.Name} ({varName})...");

    var methods = type.GetMethods();
    var properties = type.GetProperties();

    // Get instance data
    var f = Inspector.GetFields(o);
    var root = f.MinBy(x => x.Address).Address;

    // Get type info
    var header = Inspector.Peek(root - 16, 8);
    Console.WriteLine($"{(root - 16):X16} - {string.Join(' ', header.Select(x => $"{x:X2}"))} - Header");

    // Get type info
    var typeRef = Inspector.Peek(root - 8, 8);
    Console.WriteLine($"{(root - 8):X16} - {string.Join(' ', typeRef.Select(x => $"{x:X2}"))} - Method Table Pointer");

    foreach (var (Name, Address, Size, Value) in f.OrderBy(x => x.Address))
    {
        var t = Value.GetType();
        var bytes = Inspector.Peek(Address, Size);
        Console.WriteLine($"{Address:X16} - {string.Join(' ', bytes.Select(x => $"{x:X2}"))} - {Name} - {Value}");
    }
}

Console.ReadLine();

//Console.WriteLine();
//Console.WriteLine("Method addresses...");
//unsafe
//{
//    delegate*<object, int> test = &GC.GetGeneration;
//    int gen = test(t1);
//    Console.WriteLine(ToHex((long)test));

//    delegate*<int, int> doSomethingPtr = &Inspector.DoSomething;
//    int i = doSomethingPtr(14);
//    Console.WriteLine($"{nameof(doSomethingPtr)}(14) = {i}");
//    Console.WriteLine(ToHex((long)doSomethingPtr));

//    byte* jmp = (byte*)doSomethingPtr;
//    if (*jmp == 0xE9) // Is 32-bit near jump?
//    {
//        int offset = *(int*)(jmp + 1);
//        var method = jmp + offset + 5;
//    }

//    Console.WriteLine("Break here.");
//}
