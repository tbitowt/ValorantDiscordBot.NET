using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Discord;
using DiscordBot.Models.Database;
using ScottPlot.Drawing;
using ScottPlot.Drawing.Colorsets;
using Color = Discord.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace DiscordBot.Services
{
    public class PlotService
    {
        private String[] _rank_map = new String[]
        {
            "Unrated", "Unknown", "Unknown", "Iron 1", "Iron 2", "Iron 3", "Bronze 1", "Bronze 2", "Bronze 3",
            "Silver 1", "Silver 2", "Silver 3", "Gold 1", "Gold 2", "Gold 3", "Platinum 1", "Platinum 2",
            "Platinum 3", "Diamond 1", "Diamond 2", "Diamond 3", "Immortal 1", "Immortal 2", "Immortal 3",
            "Radiant"
        };

        class ValorantColorset : HexColorset, IColorset
        {
            public override string[] hexColors => new string[]
            {
                "#111111", "#000000", "#FFFFFF"
            };
        }

        public Stream GetHistoryPlot(ValorantAccount account)
        {
            if (account.RankInfos.Count == 0)
                return null;

            var plt = new ScottPlot.Plot(600, 400);
            var colors = new ValorantColorset();
            
            plt.Style(ColorTranslator.FromHtml("#111111"), ColorTranslator.FromHtml("#111111"), ColorTranslator.FromHtml("#22F9F9F9"), ColorTranslator.FromHtml("#FF4654"), ColorTranslator.FromHtml("#FF4654"), ColorTranslator.FromHtml("#F9F9F9"));
            

            double[] values = account.RankInfos.OrderBy(r => r.DateTime).Select(info => (double) info.RankInt * 100 + info.Progress).ToArray();
            double minValue = (double) ((int) values.Min() / 100 - 1) * 100;
            double maxValue = (double) ((int) values.Max() / 100 + 1) * 100;

            double[] dates = account.RankInfos.OrderBy(r => r.DateTime).Select(r => r.DateTime.ToOADate()).ToArray();

            plt.PlotScatter(dates, values, ColorTranslator.FromHtml("#FF4654"));
            plt.Legend();

            plt.Axis(y1: minValue, y2: maxValue);
            plt.Title(account.DisplayName);
            plt.XLabel("Date");
            plt.YLabel("Rank");
            
            double[] positions = _rank_map.Select((s, i) => (double)i * 100).ToArray();
            plt.YTicks(positions, _rank_map);
            plt.Ticks(dateTimeX: true);

            MemoryStream stream = new MemoryStream();
            plt.GetBitmap().Save(stream, ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        
    }
}