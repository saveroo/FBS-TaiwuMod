return {
    Title = "FBS Test Mod",
    ["BackendPlugins"] = { "FBSBackend.dll" },
    ["FrontendPlugins"] = {
        "FBSFrontend.dll" },
    Author = "saveroo",
    Description = [[ >Self use mod<
        - Upgrade Max 
        - Upgrade Now
        - Build Now
        - Build Resource
        - Practice Today
        - Instant Practice
        - Free Move
        - No CD Profession
        - Profession XP Mult
    ]],
    DefaultSettings = {
		[1] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = false,
			Key = "ModEnabled",
			DisplayName = "Enable Mod"
		},
		[2] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "upgradeMax",
			DisplayName = "Upgrade Max"
		},
		[3] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "upgradeNow",
			DisplayName = "Upgrade Now"
		},
		[4] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "buildNow",
			DisplayName = "Build Now"
		},
		[5] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "BuildResource",
			DisplayName = "Build Resource"
		},
		[6] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "NoDurability",
			DisplayName = "No Durability"
		},
		[7] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "PracticeNoAdvanceInMonth",
			DisplayName = "Practice Today"
		},
		[8] =   {
			Description = "Instant 100% practice",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "InstantPractice",
			DisplayName = "Instant Practice"
		},
		[9] =   {
			Description = "No cost when moving",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "FreeMove",
			DisplayName = "Free Move"
		},
		[10] =   {
			Description = "No Desc",
			SettingType = "Toggle",
			DefaultValue = true,
			Key = "FreeProfession",
			DisplayName = "No CD Profession"
		},
		[11] =   {
			Description = "Multiply XP percentage get, if 0.4% then it will be multiplied 0.4 * (this value)",
			SettingType = "Slider",
			MinValue = 1,
			MaxValue = 90,
			Key = "ProfessionExpMultiplier",
			DisplayName = "Profession XP Mult"
		}
    }
}
