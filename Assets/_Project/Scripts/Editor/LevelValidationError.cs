namespace NanaArrow.Editor
{
    public readonly struct LevelValidationError
    {
        public LevelRule Rule { get; }
        public string Message { get; }

        public LevelValidationError(LevelRule rule, string message)
        {
            Rule = rule;
            Message = message;
        }

        public override string ToString() => $"[{Rule}] {Message}";
    }
}
