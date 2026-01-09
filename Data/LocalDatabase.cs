using CsvHelper;
using CsvHelper.Configuration;
using QrCode_Reader.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace QrCode_Reader.Data
{
    public class LocalDatabase
    {
        private readonly SQLiteAsyncConnection _db;

        public LocalDatabase(string path)
        {
            _db = new SQLiteAsyncConnection(path);
            _db.CreateTableAsync<Client>().Wait();
        }



        /// <summary>
        /// Asynchronously deletes all client records from the data store.
        /// </summary>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeleteAllClientsAsync()
        {
            await _db.DeleteAllAsync<Client>();
        }



        /// <summary>
        /// Imports client data from a CSV file and creates a new project with the specified name. All existing clients
        /// are deleted before the import.
        /// </summary>
        /// <remarks>This method deletes all existing clients before importing new data. The CSV file must
        /// contain columns named "UNID", "NOME", and "COGNOME". Rows with missing or empty "UNID" values are skipped.
        /// The import is performed in batches for efficiency.</remarks>
        /// <param name="filePath">The path to the CSV file containing client data. The file must be encoded in UTF-8 and include a header row.</param>
        /// <param name="projectName">The name to assign to the new project that will be created during the import.</param>
        /// <returns>A task that represents the asynchronous import operation.</returns>
        public async Task ImportCsvAsNewProjectAsync(string filePath, string projectName)
        {
            await DeleteAllClientsAsync();

            var project = new Project { Name = projectName };
            await _db.InsertAsync(project);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(filePath, Encoding.UTF8);
            using var csv = new CsvReader(reader, config);

            var clientBatch = new List<Client>();
            const int BATCH_SIZE = 100;

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var unid = csv.GetField("UNID")?.Trim();

                if (string.IsNullOrWhiteSpace(unid))
                    continue;

                var client = new Client
                {
                    UNID = unid,
                    Name = csv.GetField("NOME")?.Trim() ?? "NO NAME",
                    LastName = csv.GetField("COGNOME")?.Trim() ?? "NO LAST NAME",
                    Delivered = false,
                    ProjectId = project.Id
                };

                clientBatch.Add(client);

                if (clientBatch.Count >= BATCH_SIZE)
                {
                    await _db.InsertAllAsync(clientBatch);
                    clientBatch.Clear();
                }
            }

            if (clientBatch.Count > 0)
            {
                await _db.InsertAllAsync(clientBatch);
            }
        }



        /// <summary>
        /// Asynchronously retrieves a client entity with the specified unique identifier.
        /// </summary>
        /// <param name="UNID">The unique identifier of the client to retrieve. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the client entity if found;
        /// otherwise, null.</returns>
        public async Task<Client?> GetClientByIdAsync(string UNID)
        {
            return await _db.Table<Client>()
                .Where(c => c.UNID == UNID)
                .FirstOrDefaultAsync();
        }



        /// <summary>
        /// Asynchronously retrieves all clients from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all <see
        /// cref="Client"/> objects in the database. The list will be empty if no clients are found.</returns>
        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _db.Table<Client>().ToListAsync();
        }



        /// <summary>
        /// Asynchronously updates the specified client in the data store.
        /// </summary>
        /// <param name="client">The client entity to update. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public async Task UpdateClientAsync(Client client)
        {
            await _db.UpdateAsync(client);
        }
    


        /// <summary>
        /// Copies the application's database file to the user's Downloads or Documents folder, overwriting any existing
        /// file with the same name.
        /// </summary>
        /// <remarks>On Android, the database is copied to the device's Downloads folder. On iOS, it is
        /// copied to the Documents folder. On other platforms, the file is copied to the application's data directory.
        /// If the database file does not exist, the operation completes without copying.</remarks>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        public async Task CopyDatabaseToDownloadsAsync()
        {
            try
            {
                // Caminho do banco dentro do app
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "delivery.db3");

                if (!File.Exists(dbPath))
                {
                    Console.WriteLine("Database not found!");
                    return;
                }

                #if ANDROID
                // Caminho da pasta Downloads no Android
                string downloadsPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Download";
                #elif IOS
                // Caminho da pasta Documents no iOS (simulador ou dispositivo)
                string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                #else
                string downloadsPath = FileSystem.AppDataDirectory;
                #endif

                string destinationPath = Path.Combine(downloadsPath, "delivery.db3");

                File.Copy(dbPath, destinationPath, overwrite: true);

                Console.WriteLine($"Database successfully copied to: {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying database: {ex.Message}");
            }
        }

    }
}