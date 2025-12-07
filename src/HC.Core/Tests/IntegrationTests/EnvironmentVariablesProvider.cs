using System;

namespace HC.Core.IntegrationTests;
public static class EnvironmentVariablesProvider
{
    public static string? GetVariable(string variableName)
    {
        string? environmentVariable = Environment.GetEnvironmentVariable(variableName);
        if (!string.IsNullOrEmpty(environmentVariable))
            return environmentVariable;
        environmentVariable = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
        if (!string.IsNullOrEmpty(environmentVariable))
            return environmentVariable;
        return Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);
    }
}
