using System.Security.Cryptography;
using System.Text;

var input = File.ReadAllLines("input.txt");
var instructions = input.Select(long.Parse).ToList();

var answer1 = Simulate(instructions[..32]);
Console.WriteLine($"Answer 1: {answer1}");
var hash1 = CalculateSHA256(answer1);
Console.WriteLine($"Hash 1: {hash1}");
Console.WriteLine();

var answer2 = Simulate(instructions);
Console.WriteLine($"Answer 2: {answer2}");
var hash2 = CalculateSHA256(answer2);
Console.WriteLine($"Hash 2: {hash2}");

string Simulate(List<long> instructions)
{
    long index = 0;
    List<char> letters = [];

    var tree = new Tree();

    foreach (var instruction in instructions)
    {
        index += instruction;

        letters.Add(tree.RemoveAt(index));
    }

    return string.Join("", letters);
}

string CalculateSHA256(string raw)
{
    var bytes = Encoding.UTF8.GetBytes(raw);
    var hash = SHA256.HashData(bytes);

    return Convert.ToHexString(hash).ToLower();
}

class Tree
{
    private Node root = new LetterNode();

    public char RemoveAt(long index)
    {
        while (index >= root.Size)
        {
            root = root.Expand();
        }

        return root.RemoveAt(index);
    }
}

interface Node
{
    public long DefaultSize { get; }
    public long Size { get; }
    public char RemoveAt(long index);

    public Node Expand()
    {
        return new IntermediateNode
        {
            Left = this,
            DefaultSize = 2 * DefaultSize,
            Size = Size + DefaultSize
        };
    }
}

class IntermediateNode : Node
{
    public Node? Left { get; set; }
    public Node? Right { get; set; }

    public long DefaultSize { get; set; }
    public long DefaultChildSize => DefaultSize / 2;
    public long Size { get; set; }

    public char RemoveAt(long index)
    {
        Size--;

        var (child, childIndex) = ChooseChild(index);

        return child.RemoveAt(childIndex);
    }

    private (Node, long) ChooseChild(long index)
    {
        var leftSize = Left?.Size ?? DefaultChildSize;

        if (index < leftSize)
        {
            Left ??= CreateChild();

            return (Left, index);
        }

        Right ??= CreateChild();

        return (Right, index - leftSize);
    }

    private Node CreateChild()
    {
        if (DefaultChildSize == LetterNode.InitialSize)
        {
            return new LetterNode();
        }

        return new IntermediateNode
        {
            DefaultSize = DefaultChildSize,
            Size = DefaultChildSize
        };
    }
}

class LetterNode : Node
{
    public const int InitialSize = 26;

    private List<char> letters = [.. Enumerable.Range(0, InitialSize).Select(x => (char)(x + 'A'))];

    public long DefaultSize => InitialSize;
    public long Size => letters.Count; 

    public char RemoveAt(long index)
    {
        var result = letters[(int)index];
        letters.RemoveAt((int)index);

        return result;
    }
}
