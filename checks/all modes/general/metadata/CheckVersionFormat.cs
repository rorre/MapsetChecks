﻿using MapsetParser.objects;
using MapsetVerifier;
using MapsetVerifier.objects;
using MapsetVerifier.objects.metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MapsetChecks.checks.general.metadata
{
    public class CheckVersionFormat : GeneralCheck
    {
        public override CheckMetadata GetMetadata() => new CheckMetadata()
        {
            Category = "Metadata",
            Message = "Incorrect format of (TV Size) / (Game Ver.) / (Short Ver.) in title.",
            Author = "Naxess"
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "TV Size",
                    new IssueTemplate(Issue.Level.Unrankable,
                        "{0} title field; \"{1}\" incorrect format of (TV Size).",
                        "Romanized/unicode", "field")
                    .WithCause(
                        "The format of \"(TV Size)\" in either the romanized or unicode title is incorrect.") },

                { "Game Ver",
                    new IssueTemplate(Issue.Level.Unrankable,
                        "{0} title field; \"{1}\" incorrect format of (Game Ver.).",
                        "Romanized/unicode", "field")
                    .WithCause(
                        "The format of \"(Game Ver.)\" in either the romanized or unicode title is incorrect.") },

                { "Short Ver",
                    new IssueTemplate(Issue.Level.Unrankable,
                        "{0} title field; \"{1}\" incorrect format of (Short Ver.).",
                        "Romanized/unicode", "field")
                    .WithCause(
                        "The format of \"(Short Ver.)\" in either the romanized or unicode title is incorrect.") }
            };
        }

        public override IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet)
        {
            Beatmap beatmap = aBeatmapSet.beatmaps[0];

            // Matches any string containing some form of TV Size but not exactly "(TV Size)".
            Regex tvSizeRegex = new Regex("(?i)(tv.(size|ver))");
            Regex tvSizeExactRegex = new Regex("\\(TV Size\\)");

            foreach (Issue issue in GetIssuesFromRegex(beatmap, tvSizeRegex, tvSizeExactRegex, "TV Size"))
                yield return issue;
            
            Regex gameVerRegex = new Regex("(?i)(game.(size|ver))");
            Regex gameVerExactRegex = new Regex("\\(TV Size\\)");

            foreach (Issue issue in GetIssuesFromRegex(beatmap, tvSizeRegex, tvSizeExactRegex, "Game Ver"))
                yield return issue;
            
            Regex shortVerRegex = new Regex("(?i)((short|cut).(size|ver))");
            Regex shortVerExactRegex = new Regex("\\(TV Size\\)");

            foreach (Issue issue in GetIssuesFromRegex(beatmap, tvSizeRegex, tvSizeExactRegex, "Short Ver"))
                yield return issue;
        }

        /// <summary> Returns issues wherever the romanized or unicode title contains the regular regex but not the exact regex. </summary>
        private IEnumerable<Issue> GetIssuesFromRegex(Beatmap aBeatmap, Regex aRegex, Regex anExactRegex, string aTemplateName)
        {
            if (aRegex.IsMatch(aBeatmap.metadataSettings.title) &&
                !anExactRegex.IsMatch(aBeatmap.metadataSettings.title))
            {
                yield return new Issue(GetTemplate(aTemplateName), null,
                    "Romanized", aBeatmap.metadataSettings.title);
            }

            // Unicode fields do not exist in file version 9.
            if (aBeatmap.metadataSettings.titleUnicode != null
                && aRegex.IsMatch(aBeatmap.metadataSettings.titleUnicode) &&
                !anExactRegex.IsMatch(aBeatmap.metadataSettings.titleUnicode))
            {
                yield return new Issue(GetTemplate(aTemplateName), null,
                    "Unicode", aBeatmap.metadataSettings.titleUnicode);
            }
        }
    }
}