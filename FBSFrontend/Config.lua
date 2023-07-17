return {
    Title = "FBS Test Mod",
    ["BackendPlugins"] = { "FBSBackend.dll" },
    ["FrontendPlugins"] = {
        "FBSFrontend.dll" },
    Author = "saveroo",
    Description = "Test"
    Source = 1,
    DefaultSettings = {
    [1] =   {
    			Description = "No Desc",
    			SettingType = "Toggle",
    			DefaultValue = true,
    			Key = "upgradeMax",
    			DisplayName = "Upgrade Max"
    		}
    }
}
