public class Property
{
    string propertyName;
    public List<string> properties;

    public Property(string propertyName)
    {
        this.propertyName = propertyName;
        properties = new List<string>();
    }

    public void AddProperty(string property)
    {
        properties.Add(property);
    }

    public void RemoveProperty(string property)
    {
        properties.Remove(property);
    }

    public List<string> getProperties()
    {
        return properties;
    }

    public Array getPropertiesAsArray()
    {
        return properties.ToArray();
    }
}