using ACMESharp.Enrollment;
using Newtonsoft.Json;

namespace ACMEWeb
{
    public class FileStorage : IStorage
    {
        private readonly string _rootDir;
        public FileStorage()
        {
            _rootDir = new FileInfo(typeof(FileStorage).Assembly.Location).Directory + "/temp/";
            if (!Directory.Exists(_rootDir))
            {
                Directory.CreateDirectory(_rootDir);
            }
        }

        public T Load<T>(string key) where T : class
        {
            // load from file by key

            var file = $"{_rootDir}/{key}";

            if (File.Exists(file))
            {

                var text = File.ReadAllText(file);
                if (text == null)
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<T>(text);
            }
            else
            {
                return null;
            }


        }

        public void Save<T>(string key, T value) where T : class
        {
            // save to file by key

            var file = $"{_rootDir}/{key}";

            var text = JsonConvert.SerializeObject(value);
            File.WriteAllText(file, text);
        }
    }
}
