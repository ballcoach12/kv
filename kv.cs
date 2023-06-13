using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Text;

namespace kv
{
    public class kv
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
            }

            // Keys or Get
            else if (args.Length == 1)
            {
                if (isKeys(args[0]))
                {
                    Console.WriteLine(Keys().Message);
                }
                else if (isList(args[0]))
                {
                    Console.WriteLine(List().Message);
                }
                else
                {
                    Console.WriteLine(Get(args[0]).Message);
                }
            }

            // Delete or Upsert
            else if (args.Length == 2)
            {
                if (isDelete(args[0]))
                {
                    Console.WriteLine(Delete(args[1]).Message);
                }
                else
                {
                    string key = args[0];
                    string value = args[1];
                    var keyValue = new KeyValue(key, value);
                    Console.WriteLine(Upsert(keyValue).Message);
                }
            }

            // Append
            else if (args.Length == 3 && isAppend(args[0]))
            {
                Console.WriteLine(Append(args[1], args[2]).Message);
            }

            // Unrecognized input sequence
            else
            {
                Console.WriteLine("Syntax error");
                ShowUsage();
            }
        }

        private static bool isDelete(string arg)
        {
            return string.Compare(arg, "--delete") == 0 ||
                 string.Compare(arg, "-D") == 0;
        }

        private static bool isKeys(string arg)
        {
            return string.Compare(arg, "--keys") == 0 ||
                 string.Compare(arg, "-K") == 0;
        }

        private static bool isList(string arg)
        {
            return string.Compare(arg, "--list") == 0 ||
                 string.Compare(arg, "-L") == 0;
        }

        private static bool isAppend(string arg)
        {
            return string.Compare(arg, "--append") == 0 ||
                 string.Compare(arg, "-A") == 0;
        }

        private static string GetMongoConnectionString()
        {
            string mongoHost = EnvironmentVariables.MONGO_HOST;
            string mongoPort = EnvironmentVariables.MONGO_PORT;

            return $"mongodb://{mongoHost}:{mongoPort}";
        }

        private static void ShowUsage()
        {
            Console.WriteLine("kv: a key/value pair storage utility");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("usage:");
            Console.WriteLine("  kv: Shows this screen");
            Console.WriteLine("  kv [key]: Returns the value for the specified key, if it exists");
            Console.WriteLine("  kv [key] [value]: Stores the specified value under the specified key");
            Console.WriteLine("  kv --append (-A) [key] [value]: Appends the specified value to the specified key, delimited by a comma");
            Console.WriteLine("  kv --delete (-D) [key]: Deletes the value stored under the specified key, if it exists");
            Console.WriteLine("  kv --keys (-K): Lists all the keys in the store");
            Console.WriteLine("  kv --list (-L): Lists all the key/value pairs in the store");
        }

        private static OperationResult List()
        {
            try
            {
                MongoClient client = new MongoClient(GetMongoConnectionString());
                var database = client.GetDatabase("kv");
                var collection = database.GetCollection<KeyValue>("store");
                List<KeyValue> items = collection.Find(Builders<KeyValue>.Filter.Empty).ToList();
                if (items != null && items.Count > 0)
                {
                    StringBuilder keys = new StringBuilder();
                    foreach (KeyValue item in items)
                    {
                        keys.AppendLine(item.Key + "-> " + item.Value);
                    }
                    return new OperationResult(keys.ToString(), ResultCode.Success);
                }
                else
                {
                    return new OperationResult($"No key/value pairs have been stored.", ResultCode.Fail);
                }
            }
            catch (Exception ex)
            {
                {
                    return new OperationResult($"Error: {ex.Message}", ResultCode.Fail);
                }
            }
        }
        private static OperationResult Keys()
        {
            try
            {
                MongoClient client = new MongoClient(GetMongoConnectionString());
                var database = client.GetDatabase("kv");
                var collection = database.GetCollection<KeyValue>("store");
                List<KeyValue> items = collection.Find(Builders<KeyValue>.Filter.Empty).ToList();
                if (items != null && items.Count > 0)
                {
                    StringBuilder keys = new StringBuilder();
                    foreach (KeyValue item in items)
                    {
                        keys.AppendLine(item.Key);
                    }
                    return new OperationResult(keys.ToString(), ResultCode.Success);
                }
                else
                {
                    return new OperationResult($"No keys have been stored.", ResultCode.Fail);
                }
            }
            catch (Exception ex)
            {
                {
                    return new OperationResult($"Error: {ex.Message}", ResultCode.Fail);
                }
            }
        }

        private static OperationResult Append(string key, string value)
        {
            try
            {
                MongoClient client = new MongoClient(GetMongoConnectionString());
                var database = client.GetDatabase("kv");
                var collection = database.GetCollection<KeyValue>("store");
                var filter = Builders<KeyValue>.Filter.Eq("Key", key);
                var document = collection.Find(filter).FirstOrDefault();
                if (document != null)
                {
                    document.Value = document.Value + "," + value;
                    Console.WriteLine(document.Key + "->" + document.Value);
                    var rslt = Upsert(document, false);
                    if (rslt.Code == ResultCode.Success)
                    {
                        return new OperationResult($"Value appended to key {key}", ResultCode.Success);
                    }
                    return rslt;
                }
                else
                {
                    return new OperationResult($"Unable to find value for key '{key}'.", ResultCode.Success);
                }
            }
            catch (Exception ex)
            {
                {
                    return new OperationResult($"Error: {ex.Message}", ResultCode.Fail);
                }
            }
        }

        private static OperationResult Delete(string key)
        {
            try
            {
                MongoClient client = new MongoClient(GetMongoConnectionString());
                var database = client.GetDatabase("kv");
                var collection = database.GetCollection<KeyValue>("store");
                var filter = Builders<KeyValue>.Filter.Eq("Key", key);
                var document = collection.Find(filter).FirstOrDefault();

                if (document != null)
                {
                    Console.WriteLine($"Are you sure you want to delete key '{key}' (y/N)?");
                    var answer = Console.ReadLine();
                    if (string.Compare(answer?.ToLower(), "y") == 0)
                    {
                        collection.DeleteOne(filter);
                        return new OperationResult($"Value deleted for key '{key}'.", ResultCode.Success);
                    }
                    else
                    {
                        return new OperationResult($"Key/value pair NOT deleted.", ResultCode.Fail);
                    }
                }
                else
                {
                    return new OperationResult($"No value found for key '{key}'.", ResultCode.Fail);
                }
            }
            catch (Exception ex)
            {

                return new OperationResult($"Error: {ex.Message}", ResultCode.Fail);

            }
        }

        private static OperationResult Get(string key)
        {
            try
            {
                MongoClient client = new MongoClient(GetMongoConnectionString());
                var database = client.GetDatabase("kv");
                var collection = database.GetCollection<KeyValue>("store");
                var filter = Builders<KeyValue>.Filter.Eq("Key", key);
                var document = collection.Find(filter).FirstOrDefault();
                if (document != null)
                {
                    return new OperationResult(document.Value, ResultCode.Success);
                }
                else
                {
                    return new OperationResult($"Unable to find value for key '{key}'.", ResultCode.Fail);
                }
            }
            catch (Exception ex)
            {
                {
                    return new OperationResult($"Error: {ex.Message}", ResultCode.Fail);
                }
            }
        }

        private static OperationResult Upsert(KeyValue kv, bool warn = true)
        {
            try
            {
                MongoClient client = new MongoClient(GetMongoConnectionString());
                var database = client.GetDatabase("kv");
                var collection = database.GetCollection<KeyValue>("store");
                var filter = Builders<KeyValue>.Filter.Eq("Key", kv.Key);
                var document = collection.Find(filter).FirstOrDefault();
                if (document != null)
                {
                    if (warn)
                    {
                        Console.WriteLine($"Key '{kv.Key}' already exists. Replace (y/N)?");
                        var answer = Console.ReadLine();
                        if (string.Compare(answer?.ToLower(), "y") == 0)
                        {
                            document.Value = kv.Value;
                            collection.ReplaceOne(filter, document);
                            return new OperationResult($"Value replaced for key '{kv.Key}'.", ResultCode.Success);
                        }
                        else
                        {
                            return new OperationResult($"Value NOT replaced for key '{kv.Key}'.", ResultCode.Fail);
                        }
                    }
                    else
                    {
                        document.Value = kv.Value;
                        collection.ReplaceOne(filter, document);
                        return new OperationResult($"Value replaced for key '{kv.Key}'.", ResultCode.Success);
                    }

                }
                else
                {
                    collection.InsertOne(kv);
                    return new OperationResult($"Value saved for key '{kv.Key}'.", ResultCode.Success);
                }
            }
            catch (Exception ex)
            {

                return new OperationResult($"Error: {ex.Message}", ResultCode.Fail);
            }
        }
    }

    public class OperationResult
    {
        public OperationResult(string message, ResultCode code)
        {
            this.Message = message;
            this.Code = Code;
        }
        public OperationResult()
        {
            this.Message = String.Empty;
        }
        public ResultCode Code { get; set; }

        public string Message { get; set; }

    }

    public enum ResultCode
    {
        Success, Fail
    }

    public class KeyValue
    {
        public KeyValue(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }
    }

    static class EnvironmentVariables
    {
        public static string MONGO_HOST
        {
            get
            {
                return Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";
            }
            set
            {
                Environment.SetEnvironmentVariable("MONGO_HOST", value);
            }
        }

        public static string MONGO_PORT
        {
            get
            {
                return Environment.GetEnvironmentVariable("MONGO_PORT") ?? "27017";
            }
            set
            {
                Environment.SetEnvironmentVariable("MONGO_PORT", value);
            }
        }
    }
}