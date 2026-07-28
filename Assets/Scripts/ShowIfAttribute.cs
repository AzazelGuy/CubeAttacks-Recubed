using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public readonly string ConditionName;
    public readonly bool ExpectedValue;

    public ShowIfAttribute(string conditionName, bool expectedValue = true)
    {
        ConditionName = conditionName;
        ExpectedValue = expectedValue;
    }
}