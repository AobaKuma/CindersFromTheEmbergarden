using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class HediffComp_StopMusicIfDowned : HediffComp_DownedAction
    {
        CompProperties_HediffStopMusicIfDowned Props => props as CompProperties_HediffStopMusicIfDowned;
        public override void Notify_Downed()
        {
            DoSomething();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            DoSomething();
        }

        public void DoSomething()
        {
            var music = Find.MusicManagerPlay;
            if (Props.songDefs.Contains(music.CurrentSong)) music.ForceTriggerNextSongOrSequence();
            Pawn.health.RemoveHediff(parent);
        }
    }

    public class CompProperties_HediffStopMusicIfDowned : HediffCompProperties
    {
        public CompProperties_HediffStopMusicIfDowned()
        {
            compClass = typeof(HediffComp_StopMusicIfDowned);
        }

        public List<SongDef> songDefs;
    }
}
