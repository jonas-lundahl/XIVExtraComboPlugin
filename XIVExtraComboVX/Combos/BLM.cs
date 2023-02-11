namespace XIVExtraComboVX.Combos;

using Dalamud.Game.ClientState.JobGauge.Types;

internal static class BLM {
	public const byte JobID = 25;

	public const uint
		Fire = 141,
		Blizzard = 142,
		Thunder = 144,
		Fire2 = 147,
		Transpose = 149,
		Fire3 = 152,
		Thunder3 = 153,
		Blizzard3 = 154,
		Scathe = 156,
		Freeze = 159,
		Flare = 162,
		LeyLines = 3573,
		Blizzard4 = 3576,
		Fire4 = 3577,
		BetweenTheLines = 7419,
		Despair = 16505,
		UmbralSoul = 16506,
		Xenoglossy = 16507,
		Blizzard2 = 25793,
		HighFire2 = 25794,
		HighBlizzard2 = 25795,
		Paradox = 25797;

	public static class Buffs {
		public const ushort
			Thundercloud = 164,
			Firestarter = 165,
			LeyLines = 737,
			EnhancedFlare = 2960;
	}

	public static class Debuffs {
		public const ushort
			Thunder = 161,
			Thunder3 = 163;
	}

	public static class Levels {
		public const byte
			Fire3 = 35,
			Blizzard3 = 35,
			Freeze = 40,
			Thunder3 = 45,
			Flare = 50,
			Blizzard4 = 58,
			Fire4 = 60,
			BetweenTheLines = 62,
			Despair = 72,
			UmbralSoul = 76,
			Xenoglossy = 80,
			HighFire2 = 82,
			HighBlizzard2 = 82,
			Paradox = 90;
	}
}

internal class BlackFireBlizzard4: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlmAny;
	public override uint[] ActionIDs => new[] { BLM.Fire4, BLM.Blizzard4 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		BLMGauge gauge = GetJobGauge<BLMGauge>();

		if (IsEnabled(CustomComboPreset.BlackUmbralSoulFeature)) {
			if (actionID is BLM.Blizzard4 && level >= BLM.Levels.UmbralSoul && gauge.InUmbralIce && !HasTarget)
				return BLM.UmbralSoul;
		}

		if (actionID is BLM.Fire4 or BLM.Blizzard4) {

			if (IsEnabled(CustomComboPreset.BlackEnochianFeature)) {

				if (gauge.InAstralFire) {

					if (IsEnabled(CustomComboPreset.BlackEnochianDespairFeature) && level >= BLM.Levels.Despair && LocalPlayer?.CurrentMp < 2400)
						return BLM.Despair;

					if (IsEnabled(CustomComboPreset.BlackEnochianNoSyncFeature) || level >= BLM.Levels.Fire4)
						return BLM.Fire4;

					return BLM.Fire;
				}

				if (gauge.InUmbralIce) {

					if (IsEnabled(CustomComboPreset.BlackEnochianNoSyncFeature) || level >= BLM.Levels.Blizzard4)
						return BLM.Blizzard4;

					return BLM.Blizzard;
				}
			}
		}

		return actionID;
	}
}

internal class BlackTranspose: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlackManaFeature;
	public override uint[] ActionIDs => new[] { BLM.Transpose };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		BLMGauge gauge = GetJobGauge<BLMGauge>();

		if (level >= BLM.Levels.UmbralSoul && gauge.IsEnochianActive && gauge.InUmbralIce)
			return BLM.UmbralSoul;

		return actionID;
	}
}

internal class BlackLeyLines: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlackLeyLinesFeature;
	public override uint[] ActionIDs => new[] { BLM.LeyLines };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= BLM.Levels.BetweenTheLines && SelfHasEffect(BLM.Buffs.LeyLines))
			return BLM.BetweenTheLines;

		return actionID;
	}
}

internal class BlackFireFeature: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlmAny;
	public override uint[] ActionIDs { get; } = new[] { BLM.Fire };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		BLMGauge gauge = GetJobGauge<BLMGauge>();

		if (level >= BLM.Levels.Paradox && gauge.IsParadoxActive && (gauge.InUmbralIce || LocalPlayer?.CurrentMp >= 1600))
			return BLM.Paradox;

		if (level >= BLM.Levels.Fire3
			&& (
				(IsEnabled(CustomComboPreset.BlackFireAstralFeature) && gauge.AstralFireStacks <= 1)
				|| (IsEnabled(CustomComboPreset.BlackFireProcFeature) && SelfHasEffect(BLM.Buffs.Firestarter))
			)
		) {
			return BLM.Fire3;
		}

		return OriginalHook(BLM.Fire);
	}
}

internal class BlackBlizzard: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlmAny;
	public override uint[] ActionIDs => new[] { BLM.Blizzard, BLM.Blizzard3 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		BLMGauge gauge = GetJobGauge<BLMGauge>();

		if (IsEnabled(CustomComboPreset.BlackUmbralSoulFeature)) {
			if (level >= BLM.Levels.UmbralSoul && gauge.InUmbralIce && !HasTarget)
				return BLM.UmbralSoul;
		}

		if (IsEnabled(CustomComboPreset.BlackBlizzardFeature)) {

			if (level >= BLM.Levels.Paradox && gauge.IsParadoxActive && (gauge.InUmbralIce || LocalPlayer?.CurrentMp >= 1600))
				return BLM.Paradox;

			if (level >= BLM.Levels.Blizzard3)
				return BLM.Blizzard3;

			return BLM.Blizzard;
		}

		return actionID;
	}
}

internal class BlackFreezeFlare: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlmAny;
	public override uint[] ActionIDs => new[] { BLM.Freeze, BLM.Flare };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		BLMGauge gauge = GetJobGauge<BLMGauge>();

		if (actionID is BLM.Freeze) {
			if (IsEnabled(CustomComboPreset.BlackUmbralSoulFeature)) {
				if (level >= BLM.Levels.UmbralSoul && gauge.InUmbralIce && !HasTarget)
					return BLM.UmbralSoul;
			}
		}

		if (IsEnabled(CustomComboPreset.BlackFreezeFlareFeature)) {
			if (level >= BLM.Levels.Freeze && gauge.InUmbralIce)
				return BLM.Freeze;

			if (level >= BLM.Levels.Flare && gauge.InAstralFire)
				return BLM.Flare;
		}

		return actionID;
	}
}

internal class BlackFire2: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlackFire2Feature;
	public override uint[] ActionIDs => new[] { BLM.Fire2, BLM.HighFire2 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		BLMGauge gauge = GetJobGauge<BLMGauge>();

		if (IsEnabled(CustomComboPreset.BlackFireBlizzard2Option)) {
			if (gauge.AstralFireStacks < 3)
				return actionID;
		}

		if (level >= BLM.Levels.Flare
			&& gauge.InAstralFire &&
			(gauge.UmbralHearts == 1 || LocalPlayer?.CurrentMp < 3800 || SelfHasEffect(BLM.Buffs.EnhancedFlare))
		) {
			return BLM.Flare;
		}

		return actionID;
	}
}

internal class BlackBlizzard2: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlmAny;
	public override uint[] ActionIDs => new[] { BLM.Blizzard2, BLM.HighBlizzard2 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		BLMGauge gauge = GetJobGauge<BLMGauge>();

		if (IsEnabled(CustomComboPreset.BlackUmbralSoulFeature)) {
			if (level >= BLM.Levels.UmbralSoul && gauge.InUmbralIce && !HasTarget)
				return BLM.UmbralSoul;
		}

		if (IsEnabled(CustomComboPreset.BlackBlizzard2Feature)) {
			if (IsEnabled(CustomComboPreset.BlackFireBlizzard2Option)) {
				if (gauge.UmbralIceStacks < 3)
					return actionID;
			}

			if (level >= BLM.Levels.Freeze && gauge.InUmbralIce)
				return BLM.Freeze;
		}

		return actionID;
	}
}

internal class BlackScathe: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BlackScatheFeature;
	public override uint[] ActionIDs => new[] { BLM.Scathe };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= BLM.Levels.Xenoglossy && GetJobGauge<BLMGauge>().PolyglotStacks > 0)
			return BLM.Xenoglossy;

		return actionID;
	}
}
