# kv.net

`kv` is a commandline tool that allows you to conveniently store key/value pairs so that you don't have to remember things. 

## Installation

Follow these steps to install on Windows:

1. Install the .NET CLR (see https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net70)
2. Clone this repo
3. Open a Powershell command prompt and switch to the directory where the `.csproj` file is.
4. Run the following command: `dotnet build`.
5. Add the location of the compiled `kv.exe` file to your `PATH` environment variable.
6. Get the latest `MongoDb` image: `docker run --name mongodb -d -p 27017:27017 mongo:latest`
6. Open a new command prompt and type `kv`.  You should see the Usage screen.

## Usage

```
  kv: Shows the usage screen
  kv [key]: Returns the value for the specified key, if it exists
  kv [key] [value]: Stores the specified value under the specified key
  kv --append (-A) [key] [value]: Appends the specified value to the specified key, delimited by a comma
  kv --delete (-D) [key]: Deletes the value stored under the specified key, if it exists
  kv --keys (-K): Lists all the keys in the store
  kv --list (-L): Lists all the key/value pairs in the store
```
