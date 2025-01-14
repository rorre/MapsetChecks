﻿using MapsetParser.objects;
using MapsetParser.settings;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MapsetChecks.checks.general.metadata
{
    public class CheckMarkerFormat : GeneralCheck
    {
        public override CheckMetadata GetMetadata() => new CheckMetadata()
        {
            Category = "Metadata",
            Message = "Incorrect marker format.",
            Author = "Naxess",

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    @"
                    Standardizing the way metadata is written for ranked content.
                    <image>
                        https://i.imgur.com/e5mHEan.png
                        An example of ""featured by"", which should be replaced by ""feat."".
                    </image>"
                },
                {
                    "Reasoning",
                    @"
                    Small deviations in metadata or obvious mistakes in its formatting or capitalization are for the 
                    most part eliminated through standardization. Standardization also reduces confusion in case of 
                    multiple correct ways to write certain fields and contributes to making metadata more consistent 
                    across official content."
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "Wrong Format",
                    new IssueTemplate(Issue.Level.Problem,
                        "{0} {1} field, \"{2}\".",
                        "Romanized/unicode", "artist/title", "field")
                    .WithCause(
                        "The artist or title field of a difficulty includes an incorrect format of \"CV:\", \"vs.\" or \"feat.\".") }
            };
        }

        public override IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet)
        {
            Beatmap refBeatmap = aBeatmapSet.beatmaps[0];

            // Matches strings which contain some version of "vs.", "CV:" or "feat." markers but not exactly.
            Regex regexVs   = new Regex("(?i)( vs.)");
            Regex regexCV   = new Regex("(?i)((\\(| )cv(:|：)?.)");
            Regex regexFeat = new Regex("(?i)((\\(| )(ft|feat)(\\.)?.)");

            Regex regexVsExact   = new Regex("vs\\.");
            Regex regexCVExact   = new Regex("CV(:|：)");
            Regex regexFeatExact = new Regex("feat\\.");

            foreach (Issue issue in GetFormattingIssues(refBeatmap.metadataSettings, aField =>
                    regexVs.IsMatch(aField) && !regexVsExact.IsMatch(aField) ||
                    regexCV.IsMatch(aField) && !regexCVExact.IsMatch(aField) ||
                    regexFeat.IsMatch(aField) && !regexFeatExact.IsMatch(aField)))
                yield return issue;
        }

        /// <summary> Applies a predicate to all artist and title metadata fields. Yields an issue wherever the predicate is true. </summary>
        private IEnumerable<Issue> GetFormattingIssues(MetadataSettings aSettings, Func<string, bool> aFunc)
        {
            if (aFunc(aSettings.artist))
                yield return new Issue(GetTemplate("Wrong Format"), null,
                    "Romanized", "artist", aSettings.artist);

            // Unicode fields do not exist in file version 9.
            if (aSettings.artistUnicode != null && aFunc(aSettings.artistUnicode))
                yield return new Issue(GetTemplate("Wrong Format"), null,
                    "Unicode", "artist", aSettings.artistUnicode);

            if (aFunc(aSettings.title))
                yield return new Issue(GetTemplate("Wrong Format"), null,
                    "Romanized", "title", aSettings.title);

            if (aSettings.titleUnicode != null && aFunc(aSettings.titleUnicode))
                yield return new Issue(GetTemplate("Wrong Format"), null,
                    "Unicode", "title", aSettings.titleUnicode);
        }
    }
}
