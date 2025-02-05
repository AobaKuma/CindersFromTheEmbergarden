using Verse;

namespace Embergarden
{
    public class HediffComp_StopMusicIfDowned : HediffComp_DownedAction
    {
        CompProperties_HediffStopMusicIfDowned Props => props as CompProperties_HediffStopMusicIfDowned;
        public override void Notify_Downed()
        {
            var music = Find.MusicManagerPlay;
            if (music.CurrentSong == Props.songDef) music.ForceTriggerNextSongOrSequence();
            Pawn.health.RemoveHediff(parent);
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            var music = Find.MusicManagerPlay;
            if (music.CurrentSong == Props.songDef) music.ForceTriggerNextSongOrSequence();
            Pawn.health.RemoveHediff(parent);
        }
    }

    public class CompProperties_HediffStopMusicIfDowned : HediffCompProperties
    {
        public CompProperties_HediffStopMusicIfDowned()
        {
            compClass = typeof(HediffComp_StopMusicIfDowned);
        }

        public SongDef songDef;
    }
}
