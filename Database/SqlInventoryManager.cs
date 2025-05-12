using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite; // Change to MySqlConnection if using MySQL
using UnifiedInventory.Database;

namespace UnifiedInventory.Database
{
    public static class SqlInventoryManager
    {
        private static SQLiteConnection connection;

        public static void Initialize(string dbPath)
        {
            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            using var cmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS shared_inventory (
                    slot_index INTEGER PRIMARY KEY,
                    item_id INTEGER,
                    item_stack INTEGER,
                    item_prefix INTEGER
                )", connection);
            cmd.ExecuteNonQuery();
        }

        public static void SaveInventory(List<InventorySlotData> slots)
        {
            using var tx = connection.BeginTransaction();

            foreach (var slot in slots)
            {
                using var cmd = new SQLiteCommand(@"
                    INSERT OR REPLACE INTO shared_inventory (slot_index, item_id, item_stack, item_prefix)
                    VALUES (@slot, @id, @stack, @prefix)", connection);

                cmd.Parameters.AddWithValue("@slot", slot.SlotIndex);
                cmd.Parameters.AddWithValue("@id", slot.ItemID);
                cmd.Parameters.AddWithValue("@stack", slot.Stack);
                cmd.Parameters.AddWithValue("@prefix", slot.Prefix);

                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        public static List<InventorySlotData> LoadInventory()
        {
            var result = new List<InventorySlotData>();

            using var cmd = new SQLiteCommand("SELECT * FROM shared_inventory ORDER BY slot_index", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new InventorySlotData
                {
                    SlotIndex = Convert.ToInt32(reader["slot_index"]),
                    ItemID = Convert.ToInt32(reader["item_id"]),
                    Stack = Convert.ToInt32(reader["item_stack"]),
                    Prefix = Convert.ToInt32(reader["item_prefix"])
                });
            }

            return result;
        }
    }
}
