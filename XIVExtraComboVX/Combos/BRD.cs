namespace XIVExtraComboVX.Combos;

using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;

internal static class BRD {
	public const byte JobID = 23;

	public const uint
		HeavyShot = 97,
		StraightShot = 98,
		VenomousBite = 100,
		RagingStrikes = 101,
		QuickNock = 106,
		Barrage = 107,
		Bloodletter = 110,
		Windbite = 113,
		RainOfDeath = 117,
		BattleVoice = 118,
		EmpyrealArrow = 3558,
		WanderersMinuet = 3559,
		IronJaws = 3560,
		Sidewinder = 3562,
		PitchPerfect = 7404,
		CausticBite = 7406,
		Stormbite = 7407,
		RefulgentArrow = 7409,
		BurstShot = 16495,
		ApexArrow = 16496,
		Shadowbite = 16494,
		Ladonsbite = 25783,
		BlastArrow = 25784,
		RadiantFinale = 25785;

	public static class Buffs {
		public const ushort
			StraightShotReady = 122,
			BlastShotReady = 2692,
			ShadowbiteReady = 3002;
	}

	public static class Debuffs {
		public const ushort
			VenomousBite = 124,
			Windbite = 129,
			CausticBite = 1200,
			Stormbite = 1201;
	}

	public static class Levels {
		public const byte
			StraightShot = 2,
			RagingStrikes = 4,
			VenomousBite = 6,
			Bloodletter = 12,
			Windbite = 30,
			RainOfDeath = 45,
			BattleVoice = 50,
			PitchPerfect = 52,
			EmpyrealArrow = 54,
			IronJaws = 56,
			Sidewinder = 60,
			BiteUpgrade = 64,
			RefulgentArrow = 70,
			Shadowbite = 72,
			BurstShot = 76,
			ApexArrow = 80,
			Ladonsbite = 82,
			BlastShot = 86,
			RadiantFinale = 90;
	}
}

internal class BardStraightShotUpgradeFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.BrdAny;
	public override uint[] ActionIDs { get; } = new[] { BRD.HeavyShot, BRD.BurstShot };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.BardApexFeature)) {

			if (level >= BRD.Levels.BlastShot && SelfHasEffect(BRD.Buffs.BlastShotReady))
				return BRD.BlastArrow;

			if (level >= BRD.Levels.ApexArrow && GetJobGauge<BRDGauge>().SoulVoice == 100)
				return BRD.ApexArrow;

		}

		if (IsEnabled(CustomComboPreset.BardStraightShotUpgradeFeature)) {

			if (level >= BRD.Levels.StraightShot && SelfHasEffect(BRD.Buffs.StraightShotReady))
				return OriginalHook(BRD.StraightShot);

		}

		return actionID;
	}
}

internal class BardIronJawsFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.BardIronJawsFeature;
	public override uint[] ActionIDs { get; } = new[] { BRD.IronJaws };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level < BRD.Levels.Windbite)
			return BRD.VenomousBite;

		if (level < BRD.Levels.IronJaws) {

			Status? venomous = TargetFindOwnEffect(BRD.Debuffs.VenomousBite);
			Status? windbite = TargetFindOwnEffect(BRD.Debuffs.Windbite);

			if (venomous is null)
				return BRD.VenomousBite;

			if (windbite is null)
				return BRD.Windbite;

			if (venomous.RemainingTime < windbite.RemainingTime)
				return BRD.VenomousBite;

			return BRD.Windbite;
		}

		if (level < BRD.Levels.BiteUpgrade) {

			bool venomous = TargetHasOwnEffect(BRD.Debuffs.VenomousBite);
			bool windbite = TargetHasOwnEffect(BRD.Debuffs.Windbite);

			if (venomous && windbite)
				return BRD.IronJaws;

			if (windbite)
				return BRD.VenomousBite;

			return BRD.Windbite;
		}

		bool caustic = TargetHasOwnEffect(BRD.Debuffs.CausticBite);
		bool stormbite = TargetHasOwnEffect(BRD.Debuffs.Stormbite);

		if (caustic && stormbite)
			return BRD.IronJaws;

		if (stormbite)
			return BRD.CausticBite;

		return BRD.Stormbite;
	}
}

internal class BardShadowbiteFeature: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BrdAny;
	public override uint[] ActionIDs { get; } = new[] { BRD.QuickNock, BRD.Ladonsbite };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.BardApexFeature)) {

			if (level >= BRD.Levels.ApexArrow && GetJobGauge<BRDGauge>().SoulVoice == 100)
				return BRD.ApexArrow;

			if (level >= BRD.Levels.BlastShot && SelfHasEffect(BRD.Buffs.BlastShotReady))
				return BRD.BlastArrow;

		}

		if (IsEnabled(CustomComboPreset.BardShadowbiteFeature)) {
			if (level >= BRD.Levels.Shadowbite && SelfHasEffect(BRD.Buffs.ShadowbiteReady))
				return BRD.Shadowbite;
		}

		return actionID;
	}
}

internal class BardBloodletterFeature: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BardBloodletterFeature;
	public override uint[] ActionIDs { get; } = new[] { BRD.Bloodletter };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.BardBloodletterFeature)) {

			if (level >= BRD.Levels.Sidewinder)
				return PickByCooldown(actionID, BRD.Bloodletter, BRD.EmpyrealArrow, BRD.Sidewinder);

			if (level >= BRD.Levels.EmpyrealArrow)
				return PickByCooldown(actionID, BRD.Bloodletter, BRD.EmpyrealArrow);

		}

		if (IsEnabled(CustomComboPreset.BardBloodRainFeature)) {
			if (level >= BRD.Levels.RainOfDeath
				&& !TargetHasOwnEffect(BRD.Debuffs.CausticBite)
				&& !TargetHasOwnEffect(BRD.Debuffs.Stormbite)
				&& !TargetHasOwnEffect(BRD.Debuffs.Windbite)
				&& !TargetHasOwnEffect(BRD.Debuffs.VenomousBite)
			) {
				return BRD.RainOfDeath;
			}
		}

		return actionID;
	}
}

internal class BardRainOfDeathFeature: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BardRainOfDeathFeature;
	public override uint[] ActionIDs { get; } = new[] { BRD.RainOfDeath };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= BRD.Levels.Sidewinder)
			return PickByCooldown(actionID, BRD.RainOfDeath, BRD.EmpyrealArrow, BRD.Sidewinder);

		if (level >= BRD.Levels.EmpyrealArrow)
			return PickByCooldown(actionID, BRD.RainOfDeath, BRD.EmpyrealArrow);

		return actionID;
	}
}

internal class BardSidewinder: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BardSidewinderFeature;
	public override uint[] ActionIDs { get; } = new[] { BRD.Sidewinder };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= BRD.Levels.Sidewinder)
			return PickByCooldown(actionID, BRD.EmpyrealArrow, BRD.Sidewinder);

		return actionID;
	}
}

internal class BardBarrage: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BardBarrageFeature;
	public override uint[] ActionIDs { get; } = new[] { BRD.Barrage };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= BRD.Levels.StraightShot
			&& SelfHasEffect(BRD.Buffs.StraightShotReady)
			&& !SelfHasEffect(BRD.Buffs.ShadowbiteReady)
		) {
			return OriginalHook(BRD.StraightShot);
		}

		return actionID;
	}
}

internal class BardRadiantFinale: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.BrdAny;
	public override uint[] ActionIDs { get; } = new[] { BRD.RadiantFinale };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.BardRadiantStrikesFeature)) {
			if (level >= BRD.Levels.RagingStrikes && IsOffCooldown(BRD.RagingStrikes))
				return BRD.RagingStrikes;
		}

		if (IsEnabled(CustomComboPreset.BardRadiantVoiceFeature)) {
			if (level >= BRD.Levels.BattleVoice && IsOffCooldown(BRD.BattleVoice))
				return BRD.BattleVoice;
		}

		if (IsEnabled(CustomComboPreset.BardRadiantStrikesFeature)) {
			if (level < BRD.Levels.RadiantFinale)
				return BRD.RagingStrikes;
		}

		if (IsEnabled(CustomComboPreset.BardRadiantVoiceFeature)) {
			if (level < BRD.Levels.RadiantFinale)
				return BRD.BattleVoice;
		}

		return actionID;
	}
}
