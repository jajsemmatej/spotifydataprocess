using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text.Json;
using System.Globalization;

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
        private static void processGraphSum(List<Record> records, string filename = "graphData", int sumDays = 0, Func<Record, bool>? selection = null)
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
            var dateMin = dates[0].AddDays(-1 * (sumDays / 2));
            var dateMax = dates[dates.Count - 1];
            List<GraphData> smoothedDays = new List<GraphData>();
            for (DateTime i = dateMin; i <= dateMax; i = i.AddDays(1))
            {
                smoothedDays.Add(new GraphData { Date = i, AvgPlaytime = (groupedDays.Where(x => x.Date >= i.AddDays(-1 * (sumDays / 2)) && x.Date <= i.AddDays(sumDays / 2)).Sum(d => d.Playtime) ?? 0) / (double)(sumDays - (sumDays % 2) + 1) });
            }

            string json = JsonSerializer.Serialize(smoothedDays);
            if (!Directory.Exists("graph_data"))
                Directory.CreateDirectory("graph_data");
            File.WriteAllText($"graph_data/{filename}.json", json);
        }
        private static void processGraphSumSimplified(List<Record> records, string filename, int sumDays, string? artist, string? song)
        {
            Func<Record, bool> selector = x => true;
            int selection = (artist == null ? 0 : 1) + (song == null ? 0 : 2);
            if (selection == 3)
                selector = x => x.master_metadata_album_artist_name?.ToLower()?.Contains(artist.ToLower()) == true && x.master_metadata_track_name?.ToLower() == song.ToLower();
            if (selection == 1)
                selector = x => x.master_metadata_album_artist_name?.ToLower()?.Contains(artist.ToLower()) == true;
            if(selection == 2)
                selector = x => x.master_metadata_track_name?.ToLower() == song.ToLower();
            processGraphSum(records, filename, sumDays, selector);
        }
        private static void myGraphs(List<Record> records)
        {
            int smallSmoothing = 20;
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

            int bigSmoothing = 180;
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
        }
        private static void processResults(List<Record> records)
        {
            long totalMinutes = records.Sum(x => x.ms_played ?? 0) / (1000 * 60);
            printSeparator();
            Console.WriteLine("Total Song Records: " + records.Count);
            Console.WriteLine("Total Listening Minutes: " + totalMinutes);
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
            processTopArtists(ListDataRange(records, "01-05-2024", "01-01-2025"), 20);
            printSeparator();
            myGraphs(records);
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
