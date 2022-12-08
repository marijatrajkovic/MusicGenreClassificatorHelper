using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenresClassificator
{
    class Program
    {
        static void Main(string[] args)
        {
            var basePath = "D:\\MusicGenreClassificator\\";

            //var dataSetSourcePath = $"{basePath}fma_small\\";
            //var dataSetDestinationPath = $"{ basePath}dataset\\";

            var dataSetSourcePath = $"D:\\MusicGenreClassificator\\fma_small\\";
            var dataSetDestinationPath = $"D:\\MusicGenreClassificator\\dataset\\";
            var metaDataTracksPath = $"{basePath}fma_metadata\\tracks.csv";
            var metaDataGenresPath = $"{basePath}fma_metadata\\genres.csv";

            var tracks = GetAllTracks(metaDataTracksPath);
            var genres = GetAllGenres(metaDataGenresPath);

            var finaltracks = (from t in tracks
                               join g in genres on t.genre_all equals g.genre_id
                               select new Tracks()
                               {
                                   Id = t.track_id.PadLeft(6, '0'),
                                   Genre = (t.genre_top == "" || t.genre_top == null) ? g?.title.Replace("/","-").Replace(":","-") : t.genre_top.Replace("/", "-").Replace(":", "-")
                               }).ToList();


            var directories = finaltracks.GroupBy(x => x.Genre).Select(x => x.Key).ToList();

            foreach (var d in directories)
            {
                var path = dataSetDestinationPath + d;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            foreach (var ft in finaltracks)
            {
                string sourceFile = $"{dataSetSourcePath}{ft.Id.Substring(0,3)}\\{ft.Id}.mp3";
                string destinationFile = $"{dataSetDestinationPath}{ft.Genre}\\{ft.Id}.mp3";

                if(File.Exists(sourceFile)) File.Move(sourceFile, destinationFile);
            }
        }

        public static List<TracksBaseModel> GetAllTracks(string path)
        {
            var tracks = new List<TracksBaseModel>();

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.Read();

                while (csv.Read())
                {
                    var record = csv.GetRecord<TracksBaseModel>();
                    var gt = record.genre_all.TrimStart('[').TrimEnd(']');
                    var position = gt.IndexOf(',');

                    if(position!= -1)
                        gt = gt.Substring(0, position);

                    record.genre_all = gt.TrimStart().TrimEnd();

                    tracks.Add(record);
                }
            }

            return tracks;
        }

        public static List<GenreBaseModel> GetAllGenres(string path)
        {
            var genres = new List<GenreBaseModel>();

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                while (csv.Read())
                {
                    var record = csv.GetRecord<GenreBaseModel>();
                    genres.Add(record);
                }
            }
            return genres;
        }
    }
    public class TracksBaseModel
    {
        [Index(0)]
        public string track_id { get; set; }

        [Index(40)]
        public string genre_top { get; set; }

        [Index(41)]
        public string genre_all { get; set; }
    }
    public class GenreBaseModel
    {
        [Index(0)]
        public string genre_id { get; set; }

        [Index(3)]
        public string title { get; set; }
    }
    public class Tracks
    {
        public string Id { get; set; }
        public string Genre { get; set; }

    }
}
