// See https://aka.ms/new-console-template for more information
using Peek;

var hello = "Hello, World!";
Console.WriteLine(hello);

var t1 = new Test() { MyString = hello, Prop1 = 0xABA, Prop2 = 0xABCD };
var t2 = new Test() { MyString = hello, Prop1 = 0xDEF, Prop2 = 0x2468 };

//Console.WriteLine(ToHex(Inspector.GetAddress(t1)));
//Console.WriteLine(ToHex(Inspector.GetAddress(t2)));

lock (t2)
{
    Inspect(t1, nameof(t1));
    Inspect(t2, nameof(t2)); 
}

Console.WriteLine();
Console.WriteLine("Method addresses...");
unsafe
{
    delegate*<object, int> test = &GC.GetGeneration;
    int gen = test(t1);
    Console.WriteLine(ToHex((long)test));

    delegate*<int, int> doSomethingPtr = &Inspector.DoSomething;
    int i = doSomethingPtr(14);
    Console.WriteLine($"{nameof(doSomethingPtr)}(14) = {i}");
    Console.WriteLine(ToHex((long)doSomethingPtr));

    byte* jmp = (byte*)doSomethingPtr;
    if (*jmp == 0xE9) // Is 32-bit near jump?
    {
        int offset = *(int*)(jmp + 1);
        var method = jmp + offset + 5;
    }

    Console.WriteLine("Break here.");
}

Console.WriteLine("Done");

string ToHex(long l) => $"{l:X16}";

static void Inspect(object o, string varName)
{
    Console.WriteLine();
    Console.WriteLine($"Inspecting {varName}...");

    // Get instance data
    var f = Inspector.GetFields(o);

    // Get type info
    var header = Inspector.Peek(f.MinBy(x => x.Address).Address - 16, 8);
    Console.WriteLine($"{string.Concat(header.Select(x => $"{x:X2}"))} - Header");

    // Get type info
    var typeRef = Inspector.Peek(f.MinBy(x => x.Address).Address - 8, 8);
    Console.WriteLine($"{string.Concat(typeRef.Select(x => $"{x:X2}"))} - Method Table Pointer");

    foreach (var (Name, Address, Size, Value) in f.OrderBy(x => x.Address))
    {
        var t = Value.GetType();
        var bytes = Inspector.Peek(Address, Size);
        Console.WriteLine($"{Address:X16} - {string.Join(' ', bytes.Select(x => $"{x:X2}"))} - {Name} - {Value}");
    }
}
