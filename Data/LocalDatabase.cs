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
            _db.CreateTableAsync<Project>().Wait();
            _db.CreateTableAsync<Client>().Wait();
        }

        public async Task<int> InsertProjectAsync(Project project)
        {
            return await _db.InsertAsync(project);
        }

        public async Task DeleteAllClientsAsync()
        {
            await _db.DeleteAllAsync<Client>();
            await _db.DeleteAllAsync<Project>();
        }

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
                    Name = csv.GetField("NOME")?.Trim() ?? "Sem Nome",
                    LastName = csv.GetField("COGNOME")?.Trim() ?? "Sem Apelido",
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


        public async Task<Client?> GetClientByIdAsync(string UNID)
        {
            return await _db.Table<Client>()
                .Where(c => c.UNID == UNID)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _db.Table<Client>().ToListAsync();
        }

        public async Task UpdateClientAsync(Client client)
        {
            await _db.UpdateAsync(client);
        }
    


        public async Task CopyDatabaseToDownloadsAsync()
        {
            try
            {
                // Caminho do banco dentro do app
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "delivery.db3");

                if (!File.Exists(dbPath))
                {
                    Console.WriteLine("Banco de dados não encontrado.");
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

                Console.WriteLine($"Banco copiado com sucesso para: {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao copiar banco: {ex.Message}");
            }
        }

    }
}
