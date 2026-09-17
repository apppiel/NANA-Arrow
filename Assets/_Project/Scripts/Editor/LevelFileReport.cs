using NanaArrow.Data;

namespace NanaArrow.Editor
{
    /// <summary>LevelValidatorWindow 의 파일 하나 검증 결과.</summary>
    public sealed class LevelFileReport
    {
        public string Path { get; }
        public LevelData Level { get; }
        public LevelValidationResult Result { get; }
        /// <summary>JSON 파싱 자체가 실패한 경우의 메시지, 아니면 null.</summary>
        public string ParseError { get; }

        public bool IsValid => ParseError == null && Result != null && Result.IsValid;

        public LevelFileReport(string path, LevelData level, LevelValidationResult result)
        {
            Path = path;
            Level = level;
            Result = result;
        }

        public LevelFileReport(string path, string parseError)
        {
            Path = path;
            ParseError = parseError;
        }
    }
}
