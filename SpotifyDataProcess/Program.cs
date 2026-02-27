using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text.Json;
using System.Globalization;
using System.Diagnostics.Metrics;

namespace SpotifyDataProcess
{
    internal class Program
    {
        private static List<Record> filterResults(List<Record> records)
        {
            return records.Where(x =>
                x.episode_name == null &&
                x.episode_show_name == null &&
                x.spotify_episode_uri == null &&
                x.spotify_track_uri != null).ToList();
        }
        private static void printSeparator()
        {
            Console.WriteLine("=========================");
        }
        private static void processSongs(List<Record> records, int limit)
        {
            var groupedSongs = records.Select(x => new { x.ms_played, x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .GroupBy(x => new { x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .Select(x => new
                                        {
                                            Song = x.Key.master_metadata_track_name,
                                            Artist = x.Key.master_metadata_album_artist_name,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).ToList();
            Console.WriteLine("Total Unique Songs: " + groupedSongs.Count);
            Console.WriteLine("The list of top " + limit + " songs:");
            Console.WriteLine();
            int topSongCount2 = limit;
            foreach (var song in groupedSongs.OrderByDescending(x => x.Playtime).Take(limit).ToList())
            {
                Console.WriteLine((topSongCount2 - limit + 1) + ": " + song.Artist + " - " + song.Song + " (" + (song.Playtime / (1000 * 60)) + ")");
                limit--;
            }
        }
        private static List<SongData> getTopSongs(List<Record> records, int limit)
        {
            var groupedSongs = records.Select(x => new { x.ms_played, x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .GroupBy(x => new { x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .Select(x => new SongData
                                        {
                                            Song = x.Key.master_metadata_track_name,
                                            Artist = x.Key.master_metadata_album_artist_name,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).ToList();
            return groupedSongs.OrderByDescending(x => x.Playtime).Take(limit).ToList();
        }
        private static void processTopArtists(List<Record> records, int limit)
        {
            var groupedSongs = records.Select(x => new { x.ms_played, x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .GroupBy(x => new { x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .Select(x => new
                                        {
                                            Song = x.Key.master_metadata_track_name,
                                            Artist = x.Key.master_metadata_album_artist_name,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).ToList();
            var groupedArtists = groupedSongs.GroupBy(x => x.Artist)
                                        .Select(x => new
                                        {
                                            Artist = x.Key,
                                            Playtime = x.Sum(s => s.Playtime)
                                        }).ToList();
            Console.WriteLine("The list of top " + limit + " artists:");
            Console.WriteLine();
            int topArtistCount2 = limit;
            foreach (var artist in groupedArtists.OrderByDescending(x => x.Playtime).Take(limit).ToList())
            {
                Console.WriteLine((topArtistCount2 - limit + 1) + ": " + artist.Artist + " (" + (artist.Playtime / (1000 * 60)) + ")");
                limit--;
            }
        }
        private static List<Record> ListDataRange(List<Record> records, string fromDate, string toDate)
        {
            DateTime from = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime to = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            return records.Where(x => x.ts.Date >= from && x.ts.Date <= to).ToList();
        }
        private static void processTopSongsByArtist(List<Record> records, int limit, string artist)
        {
            var groupedSongs = records.Select(x => new { x.ms_played, x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .Where(x => x.master_metadata_album_artist_name == artist)
                                        .GroupBy(x => new { x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .Select(x => new
                                        {
                                            Song = x.Key.master_metadata_track_name,
                                            Artist = x.Key.master_metadata_album_artist_name,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).ToList();
            Console.WriteLine("The list of top " + limit + " songs from " + artist + ":");
            Console.WriteLine();
            int topSongCount2 = limit;
            foreach (var song in groupedSongs.OrderByDescending(x => x.Playtime).Take(limit).ToList())
            {
                Console.WriteLine((topSongCount2 - limit + 1) + ": " + song.Artist + " - " + song.Song + " (" + (song.Playtime / (1000 * 60)) + ")");
                limit--;
            }
        }
        private static void processTopDays(List<Record> records, int limit)
        {
            var groupedDays = records.Select(x => new { x.ts.Date, x.ms_played })
                                        .GroupBy(x => new { x.Date })
                                        .Select(x => new
                                        {
                                            Date = x.Key.Date,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).ToList();
            Console.WriteLine("The list of top " + limit + " days:");
            Console.WriteLine();
            int limit2 = limit;
            foreach (var day in groupedDays.OrderByDescending(x => x.Playtime).Take(limit).ToList())
            {
                Console.WriteLine((limit2 - limit + 1) + ": " + day.Date.ToString("dd-MM-yyyy") + " (" + (day.Playtime / (1000 * 60)) + ")");
                limit--;
            }
        }
        private static void processDaysInWeek(List<Record> records)
        {
            var groupedDays = records.Select(x => new { x.ts.Date, x.ms_played })
                                        .GroupBy(x => new { x.Date })
                                        .Select(x => new
                                        {
                                            Date = x.Key.Date,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).ToList();
            int dayNumber = 1;
            var groupedWeekDays = groupedDays.Select(x => new { x.Date, x.Playtime, dayNumber })
                                        .GroupBy(x => x.Date.DayOfWeek)
                                        .Select(x => new
                                        {
                                            DayWeek = x.Key,
                                            AvgPlaytime = x.Sum(s => s.Playtime) / x.Sum(s => s.dayNumber)
                                        }).ToList();
            Console.WriteLine("The list of avg listening minutes in days of week:");
            Console.WriteLine();
            int limit = 0;
            foreach (var day in groupedWeekDays.OrderByDescending(x => x.AvgPlaytime).ToList())
            {
                Console.WriteLine((limit + 1) + ": " + day.DayWeek.ToString() + " (" + (day.AvgPlaytime / (1000 * 60)) + ")");
                limit++;
            }
        }
        private static List<GraphData> smoothGraphData(List<GraphData> data, int sumDays)
        {
            List<GraphData> smoothData = new List<GraphData>();
            foreach (var d in data)
            {
                smoothData.Add(new GraphData { Date = d.Date, AvgPlaytime = data.Where(x => x.Date >= d.Date.AddDays(-1 * (sumDays / 2 + sumDays % 2)) && x.Date <= d.Date.AddDays(sumDays / 2)).Sum(y => y.AvgPlaytime) / (double)(sumDays + 1) });
            }
            return smoothData;
        }
        private static void processGraphSum(List<Record> records, string filename = "graphData", List<int>? sumDays = null, Func<Record, bool>? selection = null)
        {
            if (selection == null)
                selection = x => true;
            var selected = records.Where(selection).ToList();
            var groupedDays = selected.Select(x => new { x.ts.Date, x.ms_played })
                                        .GroupBy(x => new { x.Date })
                                        .Select(x => new
                                        {
                                            Date = x.Key.Date,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).OrderBy(x => x.Date).ToList();

            var dates = groupedDays.Select(x => x.Date).OrderBy(x => x).ToList();
            var dateMin = dates[0];
            var dateMax = dates[dates.Count - 1];
            List<GraphData> smoothedDays = new List<GraphData>();
            for (DateTime i = dateMin; i <= dateMax; i = i.AddDays(1))
            {
                smoothedDays.Add(new GraphData { Date = i, AvgPlaytime = groupedDays.FirstOrDefault(x => x.Date == i)?.Playtime ?? 0 });
            }

            if (sumDays != null)
                foreach (var sumValue in sumDays)
                {
                    smoothedDays = smoothGraphData(smoothedDays, sumValue);
                }

            string json = JsonSerializer.Serialize(smoothedDays);
            if (!Directory.Exists("graph_data"))
                Directory.CreateDirectory("graph_data");
            File.WriteAllText($"graph_data/{filename}.json", json);
        }
        private static Func<Record, bool> getSelector(string? artist, string? song)
        {
            Func<Record, bool> selector = x => true;
            if (artist != null && song != null)
                selector = x => x.master_metadata_album_artist_name?.ToLower()?.Contains(artist.ToLower()) == true && x.master_metadata_track_name?.ToLower() == song.ToLower();
            if (artist != null && song == null)
                selector = x => x.master_metadata_album_artist_name?.ToLower()?.Contains(artist.ToLower()) == true;
            if (artist == null && song != null)
                selector = x => x.master_metadata_track_name?.ToLower() == song.ToLower();
            return selector;
        }
        private static void processGraphSumSimplified(List<Record> records, string filename, List<int> sumDays, string? artist, string? song)
        {
            processGraphSum(records, filename, sumDays, getSelector(artist, song));
        }
        private static void myGraphs(List<Record> records)
        {
            var smallSmoothing = new List<int> { 30, 20 };
            processGraphSumSimplified(records, "total", smallSmoothing, null, null);
            processGraphSumSimplified(records, "roxette", smallSmoothing, "roxette", null);
            processGraphSumSimplified(records, "abba", smallSmoothing, "abba", null);
            processGraphSumSimplified(records, "tobu", smallSmoothing, "tobu", null);
            processGraphSumSimplified(records, "pitbull", smallSmoothing, "pitbull", null);
            processGraphSumSimplified(records, "green_day", smallSmoothing, "green day", null);
            processGraphSumSimplified(records, "david_guetta", smallSmoothing, "david guetta", null);
            processGraphSumSimplified(records, "alan_walker", smallSmoothing, "alan walker", null);
            processGraphSumSimplified(records, "avicii", smallSmoothing, "avicii", null);
            processGraphSumSimplified(records, "linkin_park", smallSmoothing, "linkin park", null);
            processGraphSumSimplified(records, "lady_gaga", smallSmoothing, "lady gaga", null);
            processGraphSumSimplified(records, "imagine_dragons", smallSmoothing, "imagine dragons", null);
            processGraphSumSimplified(records, "sia", smallSmoothing, "sia", null);
            processGraphSumSimplified(records, "taylor_swift", smallSmoothing, "taylor swift", null);
            processGraphSumSimplified(records, "martin_garrix", smallSmoothing, "martin garrix", null);
            processGraphSumSimplified(records, "emily_justice", smallSmoothing, "emily & justice", null);
            processGraphSumSimplified(records, "horkyze_slize", smallSmoothing, "horkýže slíže", null);
            processGraphSumSimplified(records, "kabat", smallSmoothing, "kabát", null);
            processGraphSumSimplified(records, "bon_jovi", smallSmoothing, "bon jovi", null);

            processGraphSumSimplified(records, "roxette_sleeping_in_my_car", smallSmoothing, "Roxette", "sleeping in my car");
            processGraphSumSimplified(records, "roxette_joyride", smallSmoothing, "Roxette", "joyride");
            processGraphSumSimplified(records, "roxette_dangerous", smallSmoothing, "Roxette", "dangerous");
            processGraphSumSimplified(records, "roxette_fading_like_a_flower", smallSmoothing, "Roxette", "Fading like a flower (every time you leave)");
            processGraphSumSimplified(records, "roxette_the_look", smallSmoothing, "Roxette", "the look");

            processGraphSumSimplified(records, "i_wanna_dance_with_somebody", smallSmoothing, null, "i wanna dance with somebody (who loves me)");
            processGraphSumSimplified(records, "nicki_minaj_starships", smallSmoothing, "Nicki minaj", "starships");
            processGraphSumSimplified(records, "aha_take_on_me", smallSmoothing, "a-ha", "take on me");
            processGraphSumSimplified(records, "the_fox", smallSmoothing, null, "The Fox (What Does the Fox Say?)");
            processGraphSumSimplified(records, "chinaski_kazdy_rano", smallSmoothing, "chinaski", "každý ráno");
            processGraphSumSimplified(records, "kesha_tik_tok", smallSmoothing, "kesha", "tik tok");
            processGraphSumSimplified(records, "zombie", smallSmoothing, "Lucie Vondráčková", "Zombie");
            processGraphSumSimplified(records, "titanium", smallSmoothing, "David Guetta", "Titanium (feat. sia)");

            var bigSmoothing = new List<int> { 180, 60 };
            processGraphSumSimplified(records, "total_smooth", bigSmoothing, null, null);
            processGraphSumSimplified(records, "roxette_smooth", bigSmoothing, "roxette", null);
            processGraphSumSimplified(records, "abba_smooth", bigSmoothing, "abba", null);
            processGraphSumSimplified(records, "tobu_smooth", bigSmoothing, "tobu", null);
            processGraphSumSimplified(records, "pitbull_smooth", bigSmoothing, "pitbull", null);
            processGraphSumSimplified(records, "green_day_smooth", bigSmoothing, "green day", null);
            processGraphSumSimplified(records, "david_guetta_smooth", bigSmoothing, "david guetta", null);
            processGraphSumSimplified(records, "alan_walker_smooth", bigSmoothing, "alan walker", null);
            processGraphSumSimplified(records, "avicii_smooth", bigSmoothing, "avicii", null);
            processGraphSumSimplified(records, "linkin_park_smooth", bigSmoothing, "linkin park", null);
            processGraphSumSimplified(records, "lady_gaga_smooth", bigSmoothing, "lady gaga", null);
            processGraphSumSimplified(records, "imagine_dragons_smooth", bigSmoothing, "imagine dragons", null);
            processGraphSumSimplified(records, "taylor_swift_smooth", bigSmoothing, "taylor swift", null);
            processGraphSumSimplified(records, "martin_garrix_smooth", bigSmoothing, "martin garrix", null);
            processGraphSumSimplified(records, "horkyze_slize_smooth", bigSmoothing, "horkýže slíže", null);
            processGraphSumSimplified(records, "kabat_smooth", bigSmoothing, "kabát", null);
            processGraphSumSimplified(records, "bon_jovi_smooth", bigSmoothing, "bon jovi", null);

            processGraphSumSimplified(records, "titanium_smooth", bigSmoothing, "David Guetta", "Titanium (feat. sia)");


        }
        private static List<DateInterval> songFavoriteTimes(List<Record> records, string? artist, string? song)
        {
            var selected = records.Where(getSelector(artist, song)).ToList();
            var groupedDays = selected.Select(x => new { x.ts.Date, x.ms_played })
                                        .GroupBy(x => new { x.Date })
                                        .Select(x => new
                                        {
                                            Date = x.Key.Date,
                                            Playtime = x.Sum(s => s.ms_played)
                                        }).OrderBy(x => x.Date).ToList();

            var dates = groupedDays.Select(x => x.Date).OrderBy(x => x).ToList();
            var dateMin = dates[0];
            var dateMax = dates[dates.Count - 1];
            List<GraphData> smoothedDays = new List<GraphData>();
            for (DateTime i = dateMin; i <= dateMax; i = i.AddDays(1))
            {
                smoothedDays.Add(new GraphData { Date = i, AvgPlaytime = groupedDays.FirstOrDefault(x => x.Date == i)?.Playtime ?? 0 });
            }
            var sumDays = new List<int> { 90, 60, 30 };
            if (sumDays != null)
                foreach (var sumValue in sumDays)
                {
                    smoothedDays = smoothGraphData(smoothedDays, sumValue);
                }
            var lowCutAvg = smoothedDays.Where(x => x.AvgPlaytime >= smoothedDays.OrderByDescending(y => y.AvgPlaytime).First().AvgPlaytime / 2).Average(x => x.AvgPlaytime);
            List<DateInterval> result = new List<DateInterval>();
            bool gap = true;
            DateTime from = dateMin;
            DateTime to = dateMin;
            for (DateTime i = dateMin; i <= dateMax; i = i.AddDays(1))
            {
                if ((smoothedDays.FirstOrDefault(x => x.Date == i)?.AvgPlaytime ?? 0) >= lowCutAvg)
                {
                    if (gap == true)
                    {
                        gap = false;
                        from = i;
                        to = i;
                    }
                    else
                    {
                        to = i;
                    }
                }
                else
                {
                    if (gap == false)
                    {
                        gap = true;
                        result.Add(new DateInterval(from, to));
                    }
                }
            }
            if (gap == false)
            {
                gap = true;
                result.Add(new DateInterval(from, to));
            }
            return result;
        }
        private static List<SongData> getBindedSongs(List<Record> records, string? artist, string? song, List<SongData> songList, double overlap)
        {
            var result = new List<SongData>();
            var refIntervals = songFavoriteTimes(records, artist, song);
            foreach (var refInterval in refIntervals)
            {
                result.AddRange(getSongsInPeakInterval(records, songList, overlap, refInterval));
            }
            return result;
        }
        private static List<SongData> getSongsInPeakInterval(List<Record> records, List<SongData> songList, double overlap, DateInterval refInterval)
        {
            List<SongData> result = new List<SongData>();
            foreach (var songData in songList)
            {
                var intervals = songFavoriteTimes(records, songData.Artist, songData.Song);
                bool match = false;
                foreach (var interval in intervals)
                {
                    if (interval.OverlapDays(refInterval) >= refInterval.Duration.TotalDays * overlap)
                        match = true;
                }
                if (match)
                {
                    result.Add(new SongData { Artist = songData.Artist, Song = songData.Song });
                }
            }
            return result;
        }
        private static void printMatches(List<SongData> results, string message)
        {
            Console.WriteLine(message);
            long counter = 1;
            foreach (var match in results)
            {
                Console.WriteLine($" - match found ({counter++}): {match.Artist} - {match.Song}");
            }
        }
        private static void songDiscovery(List<Record> records)
        {
            var groupedSongs = records.Select(x => new { x.ts, x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .GroupBy(x => new { x.master_metadata_track_name, x.master_metadata_album_artist_name })
                                        .Select(x => new
                                        {
                                            Song = x.Key.master_metadata_track_name,
                                            Artist = x.Key.master_metadata_album_artist_name,
                                            FirstSeen = x.Min(s => s.ts).Date
                                        }).OrderBy(x => x.FirstSeen).ToList();
            List<GraphData> discovery = new List<GraphData>();
            int idx = 0;
            for(DateTime day = records.OrderBy(x => x.ts).First().ts.Date; day <= records.OrderBy(x => x.ts).Last().ts.Date; day = day.AddDays(1))
            {
                discovery.Add(new GraphData{Date = day, AvgPlaytime = 0});
                while(idx < groupedSongs.Count() && groupedSongs[idx].FirstSeen == day)
                {
                    discovery.Last().AvgPlaytime += 1;
                    idx ++;
                }
            }
            discovery = smoothGraphData(discovery, 60);
            discovery = smoothGraphData(discovery, 20);
            string json = JsonSerializer.Serialize(discovery);
            if (!Directory.Exists("graph_data"))
                Directory.CreateDirectory("graph_data");
            File.WriteAllText($"graph_data/song_discovery.json", json);
        }
        private static void processResults(List<Record> records)
        {
            long totalMinutes = records.Sum(x => x.ms_played ?? 0) / (1000 * 60);
            printSeparator();
            Console.WriteLine("Total Song Records: " + records.Count);
            Console.WriteLine("Total Listening Minutes: " + totalMinutes);
            Console.WriteLine("Date Interval: " + 
                records.Select(x => x.ts).Min().ToString("dd-MM-yyyy") + 
                " - " + 
                records.Select(x => x.ts).Max().ToString("dd-MM-yyyy"));
            printSeparator();
            processSongs(records, 200);
            printSeparator();
            processTopArtists(records, 50);
            printSeparator();
            processTopSongsByArtist(records, 50, "Roxette");
            printSeparator();
            processTopDays(records, 20);
            printSeparator();
            processDaysInWeek(records);
            printSeparator();
            processTopArtists(ListDataRange(records, "01-01-2025", "01-02-2026"), 20);
            printSeparator();
            myGraphs(records);
            songDiscovery(records);
            //getBindedSongs(records, "David Guetta", "Titanium (feat. Sia)", getTopSongs(records, 2000), 0.66);
            //var intervals = songFavoriteTimes(records, "a-ha", "take on me");
            //foreach (var i in intervals)
            //    Console.WriteLine(i.ToString());
            //printMatches(getBindedSongs(records, "a-ha", "take on me", getTopSongs(records, 1000), 0.66), $"Matching songs for: a-ha - take on me:");
        }
        static void Main(string[] args)
        {
            string dir = Directory.GetCurrentDirectory();
            if (args.Count() == 1)
                dir += "/" + args[0];
            string[] files = Directory.GetFiles(dir, "Streaming_History_Audio*.json");
            Console.WriteLine("Files:");
            int totalRecords = 0;
            List<Record> database = new List<Record>();
            foreach (string file in files)
            {
                Console.WriteLine("Processing: " + file);
                var records = JsonSerializer.Deserialize<List<Record>>(File.ReadAllText(file));
                if (records == null)
                    throw new Exception("error parsing file: " + file);
                totalRecords += records.Count;
                database.AddRange(records);
            }
            Console.WriteLine("Total Records: " + totalRecords);
            if (totalRecords != database.Count)
                throw new Exception("wtf1");
            processResults(filterResults(database));
        }
    }
}
