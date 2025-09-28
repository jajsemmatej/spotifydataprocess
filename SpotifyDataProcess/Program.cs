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
        private static void processResults(List<Record> records)
        {
            long totalMinutes = records.Sum(x => x.ms_played ?? 0) /(1000 * 60);
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
            printSeparator();
            Console.WriteLine("Total Song Records: " + records.Count);
            Console.WriteLine("Total Listening Minutes: " + totalMinutes);
            Console.WriteLine("Total Unique Songs: " + groupedSongs.Count);
            printSeparator();
            int topSongCount = 200;
            Console.WriteLine("The list of top " + topSongCount + " songs:");
            Console.WriteLine();
            int topSongCount2 = topSongCount;
            foreach (var song in groupedSongs.OrderByDescending(x => x.Playtime).Take(topSongCount).ToList())
            {
                Console.WriteLine((topSongCount2 - topSongCount + 1) + ": " + song.Artist + " - " + song.Song + " (" + (song.Playtime / (1000 * 60)) + ")");
                topSongCount--;
            }
            printSeparator();
            int topArtistCount = 50;
            Console.WriteLine("The list of top " + topArtistCount + " artists:");
            Console.WriteLine();
            int topArtistCount2 = topArtistCount;
            foreach (var artist in groupedArtists.OrderByDescending(x => x.Playtime).Take(topArtistCount).ToList())
            {
                Console.WriteLine((topArtistCount2 - topArtistCount + 1) + ": " + artist.Artist + " (" + (artist.Playtime / (1000 * 60)) + ")");
                topArtistCount--;
            }
            printSeparator();
        }
        static void Main(string[] args)
        {
            string dir = Directory.GetCurrentDirectory();
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
