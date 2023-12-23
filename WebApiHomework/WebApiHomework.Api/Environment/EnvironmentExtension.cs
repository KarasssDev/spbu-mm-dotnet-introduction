namespace WebApiHomework.Api.Environment;

public static class EnvironmentExtension
{
    public class UndefinedEnvironmentVariableException: Exception
    {
        public string Name { get; init; }

        public UndefinedEnvironmentVariableException(string name)
        {
            Name = name;
        }
    }
    
    public class InvalidEnvironmentVariableException: Exception
    {
        public string Name { get; init; }
        public string Value { get; init; }

        public InvalidEnvironmentVariableException(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public static string GetString(string name)
    {
        return System.Environment.GetEnvironmentVariable(name) 
               ?? throw new UndefinedEnvironmentVariableException(name);
    }

    public static int GetInt(string name)
    {
        var stringValue = GetString(name);
        var successful = Int32.TryParse(stringValue, out var result);
        if (!successful) throw new InvalidEnvironmentVariableException(name, stringValue);
        return result;
    }
}