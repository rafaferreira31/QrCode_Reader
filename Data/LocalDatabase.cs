using CsvHelper;
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

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<dynamic>().ToList();
            foreach (var r in records)
            {
                int id = int.Parse(r.Id);
                string name = r.Nome;

                var client = new Client
                {
                    Id = id,
                    Name = name,
                    Delivered = false,
                    ProjectId = project.Id
                };
                await _db.InsertAsync(client);
            }
        }

        public async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _db.Table<Client>()
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _db.Table<Client>().ToListAsync();
        }

        public async Task MarkAsDeliveredAsync(int id)
        {
            var client = await GetClientByIdAsync(id);
            if (client != null)
            {
                client.Delivered = true;
                await _db.UpdateAsync(client);
            }
        }
    }

}
