namespace BetterOtherRoles.EnoFw.Modules.BorApi;

public class PresetEntry : ICustomOptionEntry
{
    public string Key { get; set; }
    public int Value { get; set; }
    public int DefaultValue;

    public PresetEntry(string key, int defaultValue)
    {
        Key = key;
        Value = DefaultValue = defaultValue;
    }
    
    public void SetValue(int value)
    {
        InternalSetValue(value);
        BorClient.Instance.ChangeCurrentPreset(value);
    }

    public void InternalSetValue(int value)
    {
        Value = value;
    }
}

public class CustomOptionEntry : ICustomOptionEntry
{
    public string Key { get; set; }
    public int Value { get; set; }
    
    public int DefaultValue;

    public CustomOptionEntry(string key, int defaultValue)
    {
        Key = key;
        Value = DefaultValue = defaultValue;
    }

    public void SetValue(int value)
    {
        InternalSetValue(value);
        BorClient.Instance.UpdateOption(Key, Value);
    }

    public void InternalSetValue(int value)
    {
        Value = value;
    }
}

public interface ICustomOptionEntry
{
    public string Key { get; protected set; }
    public int Value { get; protected set; }

    public void SetValue(int value);
    public void InternalSetValue(int value);
}