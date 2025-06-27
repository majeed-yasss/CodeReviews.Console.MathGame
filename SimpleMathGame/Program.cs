// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Diagnostics.Metrics;

SimpleMathGame game = new();
game.Run();

enum OperationType { Random = 1, Addition, Subtraction, Multiplication, Division }
class SimpleMathGame
{
    public readonly string[] Operations;
    public readonly PlayerInput Input;
    public readonly Player CurrentPlayer;

    public SimpleMathGame()
    {
        Operations = Enum.GetNames(typeof(OperationType));
        CurrentPlayer = new();  Input = new();
    }
    public void Run()
    {
        while(true)
        {
            GameUI.List("Choose option number:", Operations);
            GameUI.Prompt("(Enter 0 for additional options)");

            int choice = Input.ReadInt(0, Operations.Length);
            MathOperation? question =  SelectOption(choice);
            if (question == null) continue;

            Stopwatch stopwatch = Stopwatch.StartNew();
            int answer = AnswerQuestion(question);
            stopwatch.Stop();

            GiveResult(question, answer, stopwatch.ElapsedMilliseconds);
        }
    }
    public MathOperation? SelectOption(int choice)
    {
        if (choice == 0)
        {
            AdditionalOptions();
            return null;
        }
        return QuestionMaker((OperationType)choice);
    }
    public MathOperation? QuestionMaker(OperationType choice)
    {
        return choice switch
        {
            OperationType.Random => 
            QuestionMaker((OperationType)Randomizer.Generate.Next(1, Operations.Length)),

            OperationType.Addition => new Add(),
            OperationType.Subtraction => new Subtract(),
            OperationType.Multiplication => new Multiply(),
            OperationType.Division => new Divide(),
            _ => null
        };
    }
    private void AdditionalOptions()
    {
        string[] options = ["History"];
        GameUI.List("options", options);
        int choice = Input.ReadInt(options.Length);

        switch (choice)
        {
            case 1: GameUI.ShowRecord(CurrentPlayer.Record); break;
            default: break;
        }
    }
    private int AnswerQuestion(MathOperation question)
    {
        GameUI.Prompt("what's the answer?");
        GameUI.ShowMathOperation(question);
        return Input.ReadInt();
    }
    private void GiveResult(MathOperation question, int answer, long time)
    {
        bool evaluation = question.Evaluate(answer);
        Result result = new(question, answer, evaluation, time);
        GameUI.ShowResult(result);

        CurrentPlayer.Record.Add(result);
    }
}
abstract class MathOperation
{
    protected int n1, n2;
    public readonly int RangeFrom, RangeTo;
    public MathOperation(int From, int To)
    {
        RangeFrom = From;
        RangeTo = To;

        n1 = Randomizer.Generate.Next(From, To);
        n2 = Randomizer.Generate.Next(From, To);
    }
    public MathOperation() : this(1, 101) { }
    public bool Evaluate(int answer) => (this.Result() == answer);
    public abstract int Result();
    public abstract override string ToString();
}
class Add : MathOperation
{
    public override int Result() => base.n1 + base.n2;
    public override string ToString() => $"{n1} + {n2}";
}
class Subtract : MathOperation
{
    public override int Result() =>base.n1 - base.n2;
    public override string ToString() => $"{n1} - {n2}";
}
class Multiply : MathOperation
{
    public override int Result() => base.n1 * base.n2;
    public override string ToString() => $"{n1} x {n2}";
}
class Divide : MathOperation
{
    public Divide() => ValidateNumbers();
    private void ValidateNumbers()
    {
        while (n2 == 0 || base.n1 % base.n2 != 0)
        {
            n2 = Randomizer.Generate.Next(1, 11);
            int quotient = Randomizer.Generate.Next(1, 11);
            n1 = n2 * quotient;
        }
    }
    public override int Result() => base.n1 / base.n2;
    public override string ToString() => $"{n1} / {n2}";
}

class PlayerInput
{
    public int ReadInt(int From, int To)
    {
        while (true)
        {
            bool parsed = int.TryParse(Console.ReadLine(), out int n);
            if (parsed && n >= From && n <= To) return n;
            else Console.WriteLine(
            $"Unvalid input: enter an integer between {From} and {To} inclusive");
        }
    }
    public int ReadInt(int To) => ReadInt(1, To);
    public int ReadInt() => ReadInt(int.MinValue, int.MaxValue);
}
class GameUI
{
    public static void List(string msg, string[] names)
    {
        Console.WriteLine(msg);
        for (int i = 0; i < names.Length; ++i)
            Console.WriteLine($"{i+1}) {names[i]}");
    }
    public static void Prompt(string msg)
    {
        Console.WriteLine(msg);
    }
    public static void ShowMathOperation(MathOperation? question)
    {
        if (question == null) Console.WriteLine("Error: question is null");
        else Console.WriteLine($"{question} =");
    }
    public static void ShowResult(Result result)
    {
        Console.WriteLine(result);
    }
    public static void ShowRecord(List<Result> Record)
    {
        int correct = 0;
        int wrong = 0;

        Console.WriteLine("Questions/Results Record:");
        foreach (Result result in Record)
        {
            Console.WriteLine(result);
            if (result.Evaluation) ++correct; else ++wrong;
        }

        Console.WriteLine();
        Console.WriteLine($"Correct answers: {correct}");
        Console.WriteLine($"Wrong answers: {wrong}");
        Console.WriteLine();
    }
}
class Result(MathOperation question, int answer, bool evaluation, long time)
{
    public readonly MathOperation Question = question;
    public readonly int Answer = answer;
    public readonly bool Evaluation = evaluation;
    public readonly long Time = time;

    public override string ToString() =>
        $"{Question} = {Answer}: {Evaluation}.\n" +
        $"{Question.Result()} is the correct answer.\n" +
        $"The time you took to answer: {Time / 1000}s";
}
class Player
{
    public List<Result> Record { get; set; }
    public Player() => Record = [];
}
class Randomizer
{ 
    public static readonly Random Generate = new();
}