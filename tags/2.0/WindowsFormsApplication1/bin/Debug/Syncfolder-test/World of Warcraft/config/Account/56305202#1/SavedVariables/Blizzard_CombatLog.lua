
Blizzard_CombatLog_Filters = {
	["currentFilter"] = 1,
	["filters"] = {
		{
			["quickButtonName"] = "Self",
			["onQuickBar"] = true,
			["quickButtonDisplay"] = {
				["party"] = true,
				["solo"] = true,
				["raid"] = true,
			},
			["tooltip"] = "Show messages of actions done by you and done to you.",
			["name"] = "Self",
			["colors"] = {
				["schoolColoring"] = {
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					}, -- [1]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.9,
						["b"] = 0.5,
					}, -- [2]
					nil, -- [3]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 0,
					}, -- [4]
					[16] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 1,
						["b"] = 1,
					},
					[64] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 1,
					},
					[0] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					[8] = {
						["a"] = 1,
						["r"] = 0.3,
						["g"] = 1,
						["b"] = 0.3,
					},
					[32] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 0.5,
						["b"] = 1,
					},
				},
				["unitColoring"] = {
					[17681] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32334] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[2147483648] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.75,
						["b"] = 0.75,
					},
					[12561] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32078] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[32542] = {
						["a"] = 1,
						["r"] = 0.34,
						["g"] = 0.64,
						["b"] = 1,
					},
					[32558] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
				},
				["eventColoring"] = {
				},
				["defaults"] = {
					["spell"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					["damage"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					},
				},
				["highlightedEvents"] = {
					["PARTY_KILL"] = true,
				},
			},
			["hasQuickButton"] = true,
			["settings"] = {
				["abilityHighlighting"] = true,
				["lineColoring"] = true,
				["spellBraces"] = false,
				["sourceColoring"] = true,
				["showHistory"] = true,
				["destColoring"] = true,
				["amountHighlighting"] = true,
				["schoolNameColoring"] = false,
				["abilitySchoolColoring"] = false,
				["sourceBraces"] = true,
				["missColoring"] = true,
				["itemBraces"] = true,
				["abilityColoring"] = false,
				["amountActorColoring"] = false,
				["textMode"] = "A",
				["destBraces"] = true,
				["timestamp"] = false,
				["actionColoring"] = false,
				["fullText"] = true,
				["lineColorPriority"] = 1,
				["lineHighlighting"] = true,
				["abilityActorColoring"] = false,
				["actionHighlighting"] = false,
				["hideBuffs"] = false,
				["schoolNameHighlighting"] = true,
				["unitBraces"] = true,
				["braces"] = false,
				["unitIcons"] = true,
				["unitColoring"] = false,
				["amountSchoolColoring"] = false,
				["amountColoring"] = false,
				["hideDebuffs"] = false,
				["noMeleeSwingColoring"] = false,
				["timestampFormat"] = "%H:%M:%S",
				["schoolNameActorColoring"] = false,
				["actionActorColoring"] = false,
			},
			["filters"] = {
				{
					["sourceFlags"] = {
						[17681] = true,
						[12561] = true,
					},
					["eventList"] = {
						["SPELL_PERIODIC_MISSED"] = true,
						["SPELL_INTERRUPT"] = true,
						["UNIT_DESTROYED"] = true,
						["SPELL_LEECH"] = true,
						["SPELL_AURA_BROKEN"] = true,
						["UNIT_DIED"] = true,
						["SPELL_PERIODIC_ENERGIZE"] = true,
						["UNIT_DISSIPATES"] = true,
						["SPELL_INSTAKILL"] = true,
						["SPELL_PERIODIC_DAMAGE"] = true,
						["SPELL_PERIODIC_HEAL"] = true,
						["PARTY_KILL"] = true,
						["SPELL_DAMAGE"] = true,
						["RANGE_DAMAGE"] = true,
						["ENVIRONMENTAL_DAMAGE"] = true,
						["SPELL_DISPEL"] = true,
						["SPELL_EXTRA_ATTACKS"] = true,
						["SPELL_MISSED"] = true,
						["SPELL_STOLEN"] = true,
						["SPELL_ENERGIZE"] = true,
						["SWING_MISSED"] = true,
						["SPELL_AURA_REFRESH"] = true,
						["SPELL_AURA_REMOVED_DOSE"] = true,
						["SPELL_DISPEL_FAILED"] = true,
						["SPELL_AURA_APPLIED"] = true,
						["ENCHANT_REMOVED"] = true,
						["ENCHANT_APPLIED"] = true,
						["SWING_DAMAGE"] = true,
						["SPELL_AURA_BROKEN_SPELL"] = true,
						["SPELL_PERIODIC_LEECH"] = true,
						["RANGE_MISSED"] = true,
						["SPELL_HEAL"] = true,
						["SPELL_AURA_REMOVED"] = true,
						["SPELL_AURA_APPLIED_DOSE"] = true,
						["SPELL_PERIODIC_DRAIN"] = true,
						["SPELL_DRAIN"] = true,
					},
				}, -- [1]
				{
					["destFlags"] = {
						[17681] = true,
						[12561] = true,
					},
					["eventList"] = {
						["SPELL_EXTRA_ATTACKS"] = true,
						["SPELL_MISSED"] = true,
						["SPELL_STOLEN"] = true,
						["SPELL_ENERGIZE"] = true,
						["SPELL_INTERRUPT"] = true,
						["UNIT_DESTROYED"] = true,
						["SPELL_LEECH"] = true,
						["UNIT_DIED"] = true,
						["ENCHANT_APPLIED"] = true,
						["SPELL_DISPEL_FAILED"] = true,
						["SPELL_INSTAKILL"] = true,
						["UNIT_DISSIPATES"] = true,
						["SWING_MISSED"] = true,
						["SWING_DAMAGE"] = true,
						["PARTY_KILL"] = true,
						["SPELL_DAMAGE"] = true,
						["RANGE_MISSED"] = true,
						["RANGE_DAMAGE"] = true,
						["ENCHANT_REMOVED"] = true,
						["SPELL_HEAL"] = true,
						["SPELL_DISPEL"] = true,
						["SPELL_DRAIN"] = true,
					},
				}, -- [2]
			},
		}, -- [1]
		{
			["quickButtonName"] = "Everything",
			["onQuickBar"] = true,
			["quickButtonDisplay"] = {
				["party"] = true,
				["solo"] = true,
				["raid"] = true,
			},
			["tooltip"] = "Show all combat messages.",
			["name"] = "Everything",
			["colors"] = {
				["schoolColoring"] = {
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					}, -- [1]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.9,
						["b"] = 0.5,
					}, -- [2]
					nil, -- [3]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 0,
					}, -- [4]
					[16] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 1,
						["b"] = 1,
					},
					[64] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 1,
					},
					[0] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					[8] = {
						["a"] = 1,
						["r"] = 0.3,
						["g"] = 1,
						["b"] = 0.3,
					},
					[32] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 0.5,
						["b"] = 1,
					},
				},
				["unitColoring"] = {
					[17681] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32334] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[2147483648] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.75,
						["b"] = 0.75,
					},
					[12561] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32078] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[32542] = {
						["a"] = 1,
						["r"] = 0.34,
						["g"] = 0.64,
						["b"] = 1,
					},
					[32558] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
				},
				["eventColoring"] = {
				},
				["defaults"] = {
					["spell"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					["damage"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					},
				},
				["highlightedEvents"] = {
					["PARTY_KILL"] = true,
				},
			},
			["hasQuickButton"] = true,
			["settings"] = {
				["abilityHighlighting"] = true,
				["lineColoring"] = true,
				["spellBraces"] = false,
				["sourceColoring"] = true,
				["showHistory"] = true,
				["destColoring"] = true,
				["amountHighlighting"] = true,
				["schoolNameColoring"] = false,
				["abilitySchoolColoring"] = false,
				["sourceBraces"] = true,
				["missColoring"] = true,
				["itemBraces"] = true,
				["abilityColoring"] = false,
				["amountActorColoring"] = false,
				["textMode"] = "A",
				["destBraces"] = true,
				["timestamp"] = false,
				["actionColoring"] = false,
				["fullText"] = true,
				["lineColorPriority"] = 1,
				["lineHighlighting"] = true,
				["abilityActorColoring"] = false,
				["actionHighlighting"] = false,
				["hideBuffs"] = false,
				["schoolNameHighlighting"] = true,
				["unitBraces"] = true,
				["braces"] = false,
				["unitIcons"] = true,
				["unitColoring"] = false,
				["amountSchoolColoring"] = false,
				["amountColoring"] = false,
				["hideDebuffs"] = false,
				["noMeleeSwingColoring"] = false,
				["timestampFormat"] = "%H:%M:%S",
				["schoolNameActorColoring"] = false,
				["actionActorColoring"] = false,
			},
			["filters"] = {
				{
					["sourceFlags"] = {
						[17681] = true,
						[32334] = true,
						[2147483648] = true,
						[12561] = true,
						[32078] = true,
						[32542] = true,
						[32558] = true,
					},
					["eventList"] = {
						["SPELL_PERIODIC_MISSED"] = true,
						["SPELL_SUMMON"] = true,
						["SPELL_INTERRUPT"] = true,
						["UNIT_DESTROYED"] = true,
						["SPELL_RESURRECT"] = true,
						["DAMAGE_SHIELD_MISSED"] = true,
						["SPELL_LEECH"] = true,
						["SPELL_AURA_BROKEN"] = true,
						["SPELL_BUILDING_DAMAGE"] = true,
						["SPELL_PERIODIC_ENERGIZE"] = true,
						["DAMAGE_SPLIT"] = true,
						["UNIT_DISSIPATES"] = true,
						["SPELL_INSTAKILL"] = true,
						["SPELL_PERIODIC_DAMAGE"] = true,
						["SPELL_PERIODIC_HEAL"] = true,
						["PARTY_KILL"] = true,
						["SPELL_DAMAGE"] = true,
						["RANGE_DAMAGE"] = true,
						["ENVIRONMENTAL_DAMAGE"] = true,
						["SPELL_DISPEL"] = true,
						["SPELL_EXTRA_ATTACKS"] = true,
						["DAMAGE_SHIELD"] = true,
						["SPELL_STOLEN"] = true,
						["SPELL_ENERGIZE"] = true,
						["UNIT_DIED"] = true,
						["SPELL_AURA_APPLIED"] = true,
						["SWING_MISSED"] = true,
						["ENCHANT_REMOVED"] = true,
						["SPELL_BUILDING_HEAL"] = true,
						["SPELL_DURABILITY_DAMAGE_ALL"] = true,
						["SPELL_AURA_REMOVED_DOSE"] = true,
						["SPELL_CAST_SUCCESS"] = true,
						["SPELL_PERIODIC_LEECH"] = true,
						["SPELL_DURABILITY_DAMAGE"] = true,
						["SPELL_CAST_FAILED"] = true,
						["ENCHANT_APPLIED"] = true,
						["SPELL_DISPEL_FAILED"] = true,
						["SPELL_AURA_REFRESH"] = true,
						["SPELL_AURA_APPLIED_DOSE"] = true,
						["SPELL_AURA_REMOVED"] = true,
						["SWING_DAMAGE"] = true,
						["SPELL_AURA_BROKEN_SPELL"] = true,
						["SPELL_CREATE"] = true,
						["RANGE_MISSED"] = true,
						["SPELL_HEAL"] = true,
						["SPELL_CAST_START"] = true,
						["SPELL_MISSED"] = true,
						["SPELL_PERIODIC_DRAIN"] = true,
						["SPELL_DRAIN"] = true,
					},
				}, -- [1]
				{
					["destFlags"] = {
						[17681] = true,
						[32334] = true,
						[2147483648] = true,
						[12561] = true,
						[32078] = true,
						[32542] = true,
						[32558] = true,
					},
					["eventList"] = {
						["SPELL_PERIODIC_MISSED"] = true,
						["SPELL_SUMMON"] = true,
						["SPELL_INTERRUPT"] = true,
						["UNIT_DESTROYED"] = true,
						["SPELL_RESURRECT"] = true,
						["DAMAGE_SHIELD_MISSED"] = true,
						["SPELL_LEECH"] = true,
						["SPELL_AURA_BROKEN"] = true,
						["SPELL_BUILDING_DAMAGE"] = true,
						["SPELL_PERIODIC_ENERGIZE"] = true,
						["DAMAGE_SPLIT"] = true,
						["UNIT_DISSIPATES"] = true,
						["SPELL_INSTAKILL"] = true,
						["SPELL_PERIODIC_DAMAGE"] = true,
						["SPELL_PERIODIC_HEAL"] = true,
						["PARTY_KILL"] = true,
						["SPELL_DAMAGE"] = true,
						["RANGE_DAMAGE"] = true,
						["ENVIRONMENTAL_DAMAGE"] = true,
						["SPELL_DISPEL"] = true,
						["SPELL_EXTRA_ATTACKS"] = true,
						["DAMAGE_SHIELD"] = true,
						["SPELL_STOLEN"] = true,
						["SPELL_ENERGIZE"] = true,
						["UNIT_DIED"] = true,
						["SPELL_AURA_APPLIED"] = true,
						["SWING_MISSED"] = true,
						["ENCHANT_REMOVED"] = true,
						["SPELL_BUILDING_HEAL"] = true,
						["SPELL_DURABILITY_DAMAGE_ALL"] = true,
						["SPELL_AURA_REMOVED_DOSE"] = true,
						["SPELL_CAST_SUCCESS"] = true,
						["SPELL_PERIODIC_LEECH"] = true,
						["SPELL_DURABILITY_DAMAGE"] = true,
						["SPELL_CAST_FAILED"] = true,
						["ENCHANT_APPLIED"] = true,
						["SPELL_DISPEL_FAILED"] = true,
						["SPELL_AURA_REFRESH"] = true,
						["SPELL_AURA_APPLIED_DOSE"] = true,
						["SPELL_AURA_REMOVED"] = true,
						["SWING_DAMAGE"] = true,
						["SPELL_AURA_BROKEN_SPELL"] = true,
						["SPELL_CREATE"] = true,
						["RANGE_MISSED"] = true,
						["SPELL_HEAL"] = true,
						["SPELL_CAST_START"] = true,
						["SPELL_MISSED"] = true,
						["SPELL_PERIODIC_DRAIN"] = true,
						["SPELL_DRAIN"] = true,
					},
				}, -- [2]
			},
		}, -- [2]
		{
			["quickButtonName"] = "What happened to me?",
			["onQuickBar"] = true,
			["quickButtonDisplay"] = {
				["party"] = true,
				["solo"] = true,
				["raid"] = true,
			},
			["tooltip"] = "Show everything done to me.",
			["name"] = "What happened to me?",
			["colors"] = {
				["schoolColoring"] = {
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					}, -- [1]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.9,
						["b"] = 0.5,
					}, -- [2]
					nil, -- [3]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 0,
					}, -- [4]
					[16] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 1,
						["b"] = 1,
					},
					[64] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 1,
					},
					[0] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					[8] = {
						["a"] = 1,
						["r"] = 0.3,
						["g"] = 1,
						["b"] = 0.3,
					},
					[32] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 0.5,
						["b"] = 1,
					},
				},
				["unitColoring"] = {
					[17681] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32334] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[2147483648] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.75,
						["b"] = 0.75,
					},
					[12561] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32078] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[32542] = {
						["a"] = 1,
						["r"] = 0.34,
						["g"] = 0.64,
						["b"] = 1,
					},
					[32558] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
				},
				["eventColoring"] = {
				},
				["defaults"] = {
					["spell"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					["damage"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					},
				},
				["highlightedEvents"] = {
					["PARTY_KILL"] = true,
				},
			},
			["hasQuickButton"] = true,
			["settings"] = {
				["abilityHighlighting"] = true,
				["lineColoring"] = true,
				["spellBraces"] = false,
				["sourceColoring"] = true,
				["showHistory"] = true,
				["destColoring"] = true,
				["amountHighlighting"] = true,
				["schoolNameColoring"] = false,
				["abilitySchoolColoring"] = false,
				["sourceBraces"] = true,
				["missColoring"] = true,
				["itemBraces"] = true,
				["abilityColoring"] = false,
				["amountActorColoring"] = false,
				["textMode"] = "A",
				["destBraces"] = true,
				["timestamp"] = false,
				["actionColoring"] = false,
				["fullText"] = true,
				["lineColorPriority"] = 1,
				["lineHighlighting"] = true,
				["abilityActorColoring"] = false,
				["actionHighlighting"] = false,
				["hideBuffs"] = false,
				["schoolNameHighlighting"] = true,
				["unitBraces"] = true,
				["braces"] = false,
				["unitIcons"] = true,
				["unitColoring"] = false,
				["amountSchoolColoring"] = false,
				["amountColoring"] = false,
				["hideDebuffs"] = false,
				["noMeleeSwingColoring"] = false,
				["timestampFormat"] = "%H:%M:%S",
				["schoolNameActorColoring"] = false,
				["actionActorColoring"] = false,
			},
			["filters"] = {
				{
					["sourceFlags"] = {
						[17681] = false,
						[32334] = false,
						[2147483648] = false,
						[12561] = false,
						[32078] = false,
						[32542] = false,
						[32558] = false,
					},
					["eventList"] = {
						["SPELL_PERIODIC_MISSED"] = true,
						["SPELL_SUMMON"] = true,
						["SPELL_INTERRUPT"] = true,
						["UNIT_DESTROYED"] = true,
						["SPELL_RESURRECT"] = true,
						["DAMAGE_SHIELD_MISSED"] = true,
						["SPELL_LEECH"] = true,
						["SPELL_AURA_BROKEN"] = true,
						["SPELL_BUILDING_DAMAGE"] = true,
						["SPELL_PERIODIC_ENERGIZE"] = true,
						["DAMAGE_SPLIT"] = true,
						["UNIT_DISSIPATES"] = true,
						["SPELL_INSTAKILL"] = true,
						["SPELL_PERIODIC_DAMAGE"] = true,
						["SPELL_PERIODIC_HEAL"] = true,
						["PARTY_KILL"] = true,
						["SPELL_DAMAGE"] = true,
						["RANGE_DAMAGE"] = true,
						["ENVIRONMENTAL_DAMAGE"] = true,
						["SPELL_DISPEL"] = true,
						["SPELL_EXTRA_ATTACKS"] = true,
						["DAMAGE_SHIELD"] = true,
						["SPELL_STOLEN"] = true,
						["SPELL_ENERGIZE"] = true,
						["UNIT_DIED"] = true,
						["SPELL_AURA_APPLIED"] = true,
						["SWING_MISSED"] = true,
						["ENCHANT_REMOVED"] = true,
						["SPELL_BUILDING_HEAL"] = true,
						["SPELL_DURABILITY_DAMAGE_ALL"] = true,
						["SPELL_AURA_REMOVED_DOSE"] = true,
						["SPELL_CAST_SUCCESS"] = true,
						["SPELL_PERIODIC_LEECH"] = true,
						["SPELL_DURABILITY_DAMAGE"] = true,
						["SPELL_CAST_FAILED"] = true,
						["ENCHANT_APPLIED"] = true,
						["SPELL_DISPEL_FAILED"] = true,
						["SPELL_AURA_REFRESH"] = true,
						["SPELL_AURA_APPLIED_DOSE"] = true,
						["SPELL_AURA_REMOVED"] = true,
						["SWING_DAMAGE"] = true,
						["SPELL_AURA_BROKEN_SPELL"] = true,
						["SPELL_CREATE"] = true,
						["RANGE_MISSED"] = true,
						["SPELL_HEAL"] = true,
						["SPELL_CAST_START"] = true,
						["SPELL_MISSED"] = true,
						["SPELL_PERIODIC_DRAIN"] = true,
						["SPELL_DRAIN"] = true,
					},
				}, -- [1]
				{
					["destFlags"] = {
						[17681] = true,
						[12561] = true,
					},
					["eventList"] = {
						["SPELL_PERIODIC_MISSED"] = true,
						["SPELL_SUMMON"] = true,
						["SPELL_INTERRUPT"] = true,
						["UNIT_DESTROYED"] = true,
						["SPELL_RESURRECT"] = true,
						["DAMAGE_SHIELD_MISSED"] = true,
						["SPELL_LEECH"] = true,
						["SPELL_AURA_BROKEN"] = true,
						["SPELL_BUILDING_DAMAGE"] = true,
						["SPELL_PERIODIC_ENERGIZE"] = true,
						["DAMAGE_SPLIT"] = true,
						["UNIT_DISSIPATES"] = true,
						["SPELL_INSTAKILL"] = true,
						["SPELL_PERIODIC_DAMAGE"] = true,
						["SPELL_PERIODIC_HEAL"] = true,
						["PARTY_KILL"] = true,
						["SPELL_DAMAGE"] = true,
						["RANGE_DAMAGE"] = true,
						["ENVIRONMENTAL_DAMAGE"] = true,
						["SPELL_DISPEL"] = true,
						["SPELL_EXTRA_ATTACKS"] = true,
						["DAMAGE_SHIELD"] = true,
						["SPELL_STOLEN"] = true,
						["SPELL_ENERGIZE"] = true,
						["UNIT_DIED"] = true,
						["SPELL_AURA_APPLIED"] = true,
						["SWING_MISSED"] = true,
						["ENCHANT_REMOVED"] = true,
						["SPELL_BUILDING_HEAL"] = true,
						["SPELL_DURABILITY_DAMAGE_ALL"] = true,
						["SPELL_AURA_REMOVED_DOSE"] = true,
						["SPELL_CAST_SUCCESS"] = true,
						["SPELL_PERIODIC_LEECH"] = true,
						["SPELL_DURABILITY_DAMAGE"] = true,
						["SPELL_CAST_FAILED"] = true,
						["ENCHANT_APPLIED"] = true,
						["SPELL_DISPEL_FAILED"] = true,
						["SPELL_AURA_REFRESH"] = true,
						["SPELL_AURA_APPLIED_DOSE"] = true,
						["SPELL_AURA_REMOVED"] = true,
						["SWING_DAMAGE"] = true,
						["SPELL_AURA_BROKEN_SPELL"] = true,
						["SPELL_CREATE"] = true,
						["RANGE_MISSED"] = true,
						["SPELL_HEAL"] = true,
						["SPELL_CAST_START"] = true,
						["SPELL_MISSED"] = true,
						["SPELL_PERIODIC_DRAIN"] = true,
						["SPELL_DRAIN"] = true,
					},
				}, -- [2]
			},
		}, -- [3]
		{
			["quickButtonName"] = "Kills",
			["onQuickBar"] = false,
			["quickButtonDisplay"] = {
				["party"] = true,
				["solo"] = true,
				["raid"] = true,
			},
			["tooltip"] = "Show all deaths and kills.",
			["name"] = "Kills",
			["colors"] = {
				["schoolColoring"] = {
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					}, -- [1]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.9,
						["b"] = 0.5,
					}, -- [2]
					nil, -- [3]
					{
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 0,
					}, -- [4]
					[16] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 1,
						["b"] = 1,
					},
					[64] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 0.5,
						["b"] = 1,
					},
					[0] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					[8] = {
						["a"] = 1,
						["r"] = 0.3,
						["g"] = 1,
						["b"] = 0.3,
					},
					[32] = {
						["a"] = 1,
						["r"] = 0.5,
						["g"] = 0.5,
						["b"] = 1,
					},
				},
				["unitColoring"] = {
					[17681] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32334] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[2147483648] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.75,
						["b"] = 0.75,
					},
					[12561] = {
						["a"] = 1,
						["r"] = 0.7,
						["g"] = 0.7,
						["b"] = 0.7,
					},
					[32078] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
					[32542] = {
						["a"] = 1,
						["r"] = 0.34,
						["g"] = 0.64,
						["b"] = 1,
					},
					[32558] = {
						["a"] = 1,
						["r"] = 0.75,
						["g"] = 0.05,
						["b"] = 0.05,
					},
				},
				["eventColoring"] = {
				},
				["defaults"] = {
					["spell"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 1,
					},
					["damage"] = {
						["a"] = 1,
						["r"] = 1,
						["g"] = 1,
						["b"] = 0,
					},
				},
				["highlightedEvents"] = {
					["PARTY_KILL"] = true,
				},
			},
			["hasQuickButton"] = false,
			["settings"] = {
				["abilityHighlighting"] = true,
				["lineColoring"] = true,
				["spellBraces"] = false,
				["sourceColoring"] = true,
				["showHistory"] = true,
				["destColoring"] = true,
				["amountHighlighting"] = true,
				["schoolNameColoring"] = false,
				["abilitySchoolColoring"] = false,
				["sourceBraces"] = true,
				["missColoring"] = true,
				["itemBraces"] = true,
				["abilityColoring"] = false,
				["amountActorColoring"] = false,
				["textMode"] = "A",
				["destBraces"] = true,
				["timestamp"] = false,
				["actionColoring"] = false,
				["fullText"] = true,
				["lineColorPriority"] = 1,
				["lineHighlighting"] = true,
				["abilityActorColoring"] = false,
				["actionHighlighting"] = false,
				["hideBuffs"] = false,
				["schoolNameHighlighting"] = true,
				["unitBraces"] = true,
				["braces"] = false,
				["unitIcons"] = true,
				["unitColoring"] = false,
				["amountSchoolColoring"] = false,
				["amountColoring"] = false,
				["hideDebuffs"] = false,
				["noMeleeSwingColoring"] = false,
				["timestampFormat"] = "%H:%M:%S",
				["schoolNameActorColoring"] = false,
				["actionActorColoring"] = false,
			},
			["filters"] = {
				{
					["sourceFlags"] = {
						[17681] = true,
						[32334] = true,
						[2147483648] = true,
						[12561] = true,
						[32078] = true,
						[32542] = true,
						[32558] = true,
					},
					["eventList"] = {
						["PARTY_KILL"] = true,
						["UNIT_DESTROYED"] = true,
						["UNIT_DISSIPATES"] = true,
						["UNIT_DIED"] = true,
					},
				}, -- [1]
				{
					["destFlags"] = {
						[17681] = true,
						[32334] = true,
						[2147483648] = true,
						[12561] = true,
						[32078] = true,
						[32542] = true,
						[32558] = true,
					},
					["eventList"] = {
						["PARTY_KILL"] = true,
						["UNIT_DESTROYED"] = true,
						["UNIT_DISSIPATES"] = true,
						["UNIT_DIED"] = true,
					},
				}, -- [2]
			},
		}, -- [4]
	},
}
Blizzard_CombatLog_Filter_Version = 4.1
