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
using System.Numerics;

namespace MapsetChecks.checks.standard.spread
{
    public class CheckCloseOverlap : BeatmapCheck
    {
        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Modes = new Beatmap.Mode[]
            {
                Beatmap.Mode.Standard
            },
            Difficulties = new Beatmap.Difficulty[]
            {
                Beatmap.Difficulty.Easy,
                Beatmap.Difficulty.Normal
            },
            Category = "Spread",
            Message = "Objects close in time not overlapping.",
            Author = "Naxess",

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    @"
                    Ensuring that objects close in time are indiciated as such in easy and normal difficulties.
                    <image>
                        https://i.imgur.com/rnIi6Pj.png
                        Right image is harder to distinguish time distance in, despite spacings still clearly being different.
                    </image>"
                },
                {
                    "Reasoning",
                    @"
                    Newer players often have trouble reading how far apart objects are in time, which is why enabling 
                    distance spacing for lower difficulties is often recommended. However, if two spacings for different 
                    snappings look similar, it's possible to confuse them. By forcing an overlap between objects close in 
                    time and discouraging it for objects further apart, the difference in snappings become more apparent."
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "Problem",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} {1} ms apart, should either be overlapped or at least {2} ms apart.",
                        "timestamp - ", "gap", "threshold")
                    .WithCause(
                        "Two objects with a time gap less than 125 ms (240 bpm 1/2) are not overlapping.") },

                { "Warning",
                    new IssueTemplate(Issue.Level.Warning,
                        "{0} {1} ms apart.",
                        "timestamp - ", "gap")
                    .WithCause(
                        "Two objects with a time gap less than 167 ms (180 bpm 1/2) are not overlapping.") }
            };
        }

        public override IEnumerable<Issue> GetIssues(Beatmap aBeatmap)
        {
            double problemThreshold = 125; // Shortest acceptable gap is 1/2 in 240 BPM, 125 ms.
            double warningThreshold = 188; // Shortest gap before warning is 1/2 in 160 BPM, 188 ms.

            for (int i = 0; i < aBeatmap.hitObjects.Count - 1; ++i)
            {
                HitObject hitObject     = aBeatmap.hitObjects[i];
                HitObject nextHitObject = aBeatmap.hitObjects[i + 1];

                // Slider ends do not need to overlap, same with spinners, spinners should be ignored overall.
                if (hitObject is Circle &&
                    !(nextHitObject is Spinner) &&
                    nextHitObject.time - hitObject.time < warningThreshold)
                {
                    double distance =
                        Math.Sqrt(
                            Math.Pow(hitObject.Position.X - nextHitObject.Position.X, 2) +
                            Math.Pow(hitObject.Position.Y - nextHitObject.Position.Y, 2));

                    // If the distance is larger or equal to two radiuses, then they're not overlapping.
                    float radius = aBeatmap.difficultySettings.GetCircleRadius();
                    if (distance >= radius * 2)
                    {
                        if (nextHitObject.time - hitObject.time < problemThreshold)
                            yield return new Issue(GetTemplate("Problem"), aBeatmap,
                                Timestamp.Get(hitObject, nextHitObject),
                                $"{nextHitObject.time - hitObject.time:0.##}",
                                problemThreshold);

                        else
                            yield return new Issue(GetTemplate("Warning"), aBeatmap,
                                Timestamp.Get(hitObject, nextHitObject),
                                $"{nextHitObject.time - hitObject.time:0.##}");
                    }
                }
            }
        }
    }
}
