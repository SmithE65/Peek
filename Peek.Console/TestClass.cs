namespace Peek.Console;

public class TestClass
{
    public int Int1 { get; set; }
    public int Int2 { get; set; }
    public string MyProperty { get; set; } = string.Empty;

    public string Useless() => $"{MyProperty} and {Int1} + {Int2} = {Int1 + Int2}";
}
