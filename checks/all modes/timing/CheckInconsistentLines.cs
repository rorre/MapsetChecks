﻿using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using MapsetParser.objects.timinglines;
using MapsetParser.statics;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MapsetChecks.checks.timing
{
    public class CheckInconsistentLines : BeatmapSetCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Category = "Timing",
            Message = "Inconsistent uninherited lines, meter signatures or BPM.",
            Author = "Naxess",

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    @"
                    Ensuring that the song is timed consistently for all difficulties."
                },
                {
                    "Reasoning",
                    @"
                    Since all difficulties in a set are based around a single song, they should all use the same base timing, 
                    which is made from uninherited lines. Even if a line isn't used by some difficulty due to there being a 
                    break or similar, they still affect things like the main menu flashing and beats/snares/finishes in the 
                    nightcore mod.
                    <br \><br \>
                    Similar to metadata, timing (bpm/meter/offset of uninherited lines) should really just be global for the 
                    whole beatmapset rather than difficulty-specific."
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "Missing",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Missing uninherited line, see {1}.",
                        "timestamp - ", "difficulty")
                    .WithCause(
                        "A beatmap does not have an uninherited line which the reference beatmap does, or visa versa.") },

                { "Inconsistent Meter",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} Inconsistent meter signature, see {1}.",
                        "timestamp - ", "difficulty")
                    .WithCause(
                        "The meter signature of an uninherited timing line is different from the reference beatmap.") },

                { "Inconsistent BPM",
                    new IssueTemplate(Issue.Level.Problem,
                         "{0} Inconsistent BPM, see {1}.",
                        "timestamp - ", "difficulty")
                    .WithCause(
                        "Same as the meter check, except checks BPM instead.") }
            };
        }

        public override IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet)
        {
            Beatmap refBeatmap = aBeatmapSet.beatmaps[0];
            foreach (Beatmap beatmap in aBeatmapSet.beatmaps)
            {
                foreach (TimingLine line in refBeatmap.timingLines)
                {
                    if (line is UninheritedLine uninheritLine)
                    {
                        UninheritedLine otherUninheritLine =
                            beatmap.timingLines.OfType<UninheritedLine>().FirstOrDefault(
                                aLine => aLine.offset == uninheritLine.offset);
                        
                        double offset = Timestamp.Round(uninheritLine.offset);
                        
                        if (otherUninheritLine == null)
                            yield return new Issue(GetTemplate("Missing"), beatmap,
                                Timestamp.Get(offset), refBeatmap);
                        else
                        {
                            if (uninheritLine.meter != otherUninheritLine.meter)
                                yield return new Issue(GetTemplate("Inconsistent Meter"), beatmap,
                                    Timestamp.Get(offset), refBeatmap);

                            if (uninheritLine.msPerBeat != otherUninheritLine.msPerBeat)
                                yield return new Issue(GetTemplate("Inconsistent BPM"), beatmap,
                                    Timestamp.Get(offset), refBeatmap);
                        }
                    }
                }
                
                // Check the other way around as well, to make sure the reference map has all uninherited lines this map has.
                foreach (TimingLine line in beatmap.timingLines)
                {
                    if (line is UninheritedLine)
                    {
                        UninheritedLine otherLine =
                            refBeatmap.timingLines.OfType<UninheritedLine>().FirstOrDefault(
                                aLine => aLine.offset == line.offset);

                        double offset = (int)Math.Floor(line.offset);
                        
                        if (otherLine == null)
                            yield return new Issue(GetTemplate("Missing"), refBeatmap,
                                Timestamp.Get(offset), beatmap);
                    }
                }
            }
        }
    }
}
