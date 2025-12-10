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
            // 1. Limpeza inicial
            await DeleteAllClientsAsync();

            var project = new Project { Name = projectName };
            await _db.InsertAsync(project);

            // 2. Configuração de leitura
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Lista temporária para guardar um lote de clientes
            var clientBatch = new List<Client>();
            const int BATCH_SIZE = 50; // Processa 500 registos de cada vez para poupar RAM

            // Ler o cabeçalho
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                // Leitura manual é mais rápida e segura que dynamic
                // Tenta obter o ID, se falhar ou for nulo, ignora ou trata
                if (int.TryParse(csv.GetField("Id"), out int id))
                {
                    var client = new Client
                    {
                        Id = id,
                        // Certifique-se que "Nome" corresponde ao cabeçalho no CSV. 
                        // Se no CSV for "Name", altere aqui.
                        Name = csv.GetField("Nome") ?? "Sem Nome",
                        Delivered = false,
                        ProjectId = project.Id
                    };

                    clientBatch.Add(client);
                }

                // 3. Quando o lote atingir o tamanho definido, grava na base de dados
                if (clientBatch.Count >= BATCH_SIZE)
                {
                    await _db.InsertAllAsync(clientBatch); // Grava 500 de uma vez
                    clientBatch.Clear(); // Limpa a memória
                }
            }

            // 4. Grava os registos restantes (o que sobrou do último lote)
            if (clientBatch.Count > 0)
            {
                await _db.InsertAllAsync(clientBatch);
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


        public async Task<string> GetProjectNameByClientIdAsync(int clientId)
        {
            var client = await _db.Table<Client>()
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
                return string.Empty;

            var project = await _db.Table<Project>()
                .FirstOrDefaultAsync(p => p.Id == client.ProjectId);

            return project?.Name ?? string.Empty;
        }

    }

}
