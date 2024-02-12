using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;

internal class MyErrorStrategy : DefaultErrorStrategy
{
    protected override void NotifyErrorListeners([NotNull] Parser recognizer, string message, RecognitionException e)
    {
        Console.WriteLine($"NotifyErrorListenersMYYYY {message}");
        recognizer.NotifyErrorListeners(e.OffendingToken, message, e);
    }
}
