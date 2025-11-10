using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text.Json;

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
        private static void processTopArtistsLastYears(List<Record> records, int limit, int lastYears)
        {
            var maxdate = records.OrderByDescending(x => x.ts).FirstOrDefault().ts.Date;
            var selectedRecords = records.Where(x => x.ts > maxdate.AddYears((-1) * lastYears)).ToList();
            var groupedSongs = selectedRecords.Select(x => new { x.ms_played, x.master_metadata_track_name, x.master_metadata_album_artist_name })
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
            Console.WriteLine("The list of top " + limit + " artists in last " + lastYears + " years:");
            Console.WriteLine();
            int topArtistCount2 = limit;
            foreach (var artist in groupedArtists.OrderByDescending(x => x.Playtime).Take(limit).ToList())
            {
                Console.WriteLine((topArtistCount2 - limit + 1) + ": " + artist.Artist + " (" + (artist.Playtime / (1000 * 60)) + ")");
                limit--;
            }
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
            int smoothing = 20;
            processGraphSumSimplified(records, "total", smoothing, null, null);
            processGraphSumSimplified(records, "roxette", smoothing, "roxette", null);
            processGraphSumSimplified(records, "abba", smoothing, "abba", null);
            processGraphSumSimplified(records, "tobu", smoothing, "tobu", null);
            processGraphSumSimplified(records, "pitbull", smoothing, "pitbull", null);
            processGraphSumSimplified(records, "green_day", smoothing, "green day", null);
            processGraphSumSimplified(records, "david_guetta", smoothing, "david guetta", null);
            processGraphSumSimplified(records, "alan_walker", smoothing, "alan walker", null);
            processGraphSumSimplified(records, "avicii", smoothing, "avicii", null);
            processGraphSumSimplified(records, "linkin_park", smoothing, "linkin park", null);
            processGraphSumSimplified(records, "lady_gaga", smoothing, "lady gaga", null);
            processGraphSumSimplified(records, "imagine_dragons", smoothing, "imagine dragons", null);
            processGraphSumSimplified(records, "sia", smoothing, "sia", null);
            processGraphSumSimplified(records, "taylor_swift", smoothing, "taylor swift", null);
            processGraphSumSimplified(records, "martin_garrix", smoothing, "martin garrix", null);
            processGraphSumSimplified(records, "emily_justice", smoothing, "emily & justice", null);
            processGraphSumSimplified(records, "horkyze_slize", smoothing, "horkýže slíže", null);
            processGraphSumSimplified(records, "kabat", smoothing, "kabát", null);

            processGraphSumSimplified(records, "roxette_sleeping_in_my_car", smoothing, "Roxette", "sleeping in my car");
            processGraphSumSimplified(records, "roxette_joyride", smoothing, "Roxette", "joyride");
            processGraphSumSimplified(records, "roxette_dangerous", smoothing, "Roxette", "dangerous");
            processGraphSumSimplified(records, "roxette_fading_like_a_flower", smoothing, "Roxette", "Fading like a flower (every time you leave)");
            processGraphSumSimplified(records, "roxette_the_look", smoothing, "Roxette", "the look");

            processGraphSumSimplified(records, "i_wanna_dance_with_somebody", smoothing, null, "i wanna dance with somebody (who loves me)");
            processGraphSumSimplified(records, "nicki_minaj_starships", smoothing, "Nicki minaj", "starships");
            processGraphSumSimplified(records, "aha_take_on_me", smoothing, "a-ha", "take on me");
            processGraphSumSimplified(records, "the_fox", smoothing, null, "The Fox (What Does the Fox Say?)");
            processGraphSumSimplified(records, "chinaski_kazdy_rano", smoothing, "chinaski", "každý ráno");
            processGraphSumSimplified(records, "kesha_tik_tok", smoothing, "kesha", "tik tok");

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
            processTopArtistsLastYears(records, 30, 2);
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
