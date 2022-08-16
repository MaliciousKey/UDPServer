using System;
using System.Collections;
using System.Text;

public class PlayerProfile
{
    public string name;
    public Guid playerGuid;

    public PlayerProfile(string name, Guid guid)
    {
        this.name = name;
        playerGuid = guid;
    }

    public static PlayerProfile fromProperties(Property property)
    {
        return new PlayerProfile(property.getProperties()[2], Guid.Parse(property.getProperties()[3]));
    }
}