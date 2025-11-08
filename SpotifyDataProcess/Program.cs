using System.Collections.Generic;
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
        private static void processGraphSum(List<Record> records, int sumDays = 0, Func<Record, bool>? selection = null)
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
            var dateMax = dates[dates.Count - 1].AddDays(sumDays / 2);
            List<GraphData> smoothedDays = new List<GraphData>();
            for (DateTime i = dateMin; i <= dateMax; i = i.AddDays(1))
            {
                smoothedDays.Add(new GraphData { Date = i, AvgPlaytime = (groupedDays.Where(x => x.Date >= i.AddDays(-1 * (sumDays / 2)) && x.Date <= i.AddDays(sumDays / 2)).Sum(d => d.Playtime) ?? 0) / (double)(sumDays - (sumDays % 2) + 1) });
            }

            string json = JsonSerializer.Serialize(smoothedDays);
            File.WriteAllText("graphData.json", json);
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
            processGraphSum(records, 40, x => /*x.master_metadata_album_artist_name?.Contains("David Guetta") == true/* && /**/
                                                x.master_metadata_track_name == "TiK ToK" /**/);
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
