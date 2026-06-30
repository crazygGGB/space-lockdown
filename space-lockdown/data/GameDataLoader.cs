using Godot;

namespace SpaceLockdown;

public static class GameDataLoader
{
    public static Godot.Collections.Dictionary LoadConfig()
    {
        var file = FileAccess.Open("res://data/space_escape_config.json", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError("无法打开配置文件: res://data/space_escape_config.json");
            return new Godot.Collections.Dictionary();
        }

        var jsonStr = file.GetAsText();
        var json = new Json();
        var parseErr = json.Parse(jsonStr);
        if (parseErr != Error.Ok)
        {
            GD.PushError("JSON 解析错误: " + json.GetErrorMessage());
            return new Godot.Collections.Dictionary();
        }

        return (Godot.Collections.Dictionary)json.Data;
    }

    public static Godot.Collections.Dictionary GetSpaceById(Godot.Collections.Dictionary config, string spaceId)
    {
        if (!config.ContainsKey("spaces"))
            return new Godot.Collections.Dictionary();

        var spaces = (Godot.Collections.Array)config["spaces"];
        foreach (var space in spaces)
        {
            var spaceDict = (Godot.Collections.Dictionary)space;
            if (spaceDict.ContainsKey("id") && (string)spaceDict["id"] == spaceId)
                return spaceDict;
        }
        return new Godot.Collections.Dictionary();
    }

    public static Godot.Collections.Dictionary GetPortalById(Godot.Collections.Dictionary config, string portalId)
    {
        if (!config.ContainsKey("portalNodes"))
            return new Godot.Collections.Dictionary();

        var portals = (Godot.Collections.Array)config["portalNodes"];
        foreach (var portal in portals)
        {
            var portalDict = (Godot.Collections.Dictionary)portal;
            if (portalDict.ContainsKey("id") && (string)portalDict["id"] == portalId)
                return portalDict;
        }
        return new Godot.Collections.Dictionary();
    }
}
