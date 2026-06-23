

using UltimateParser.Config;

namespace UltimateParser.Engines 
{
    public interface IParserEngine {
        event Action<List<Dictionary<string,string>>>? OnCheckpoint;
        Task<List<Dictionary<string,string>>> GetParse(ParserConfig config);

    }
}