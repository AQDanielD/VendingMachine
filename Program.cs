using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;


//-----------
using Npgsql;//"Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Items;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true"
//-----------

//Type 4736 on Menu to access Admin

//Very bad Host for DB
/*
 * Table name --> Products, Admins
 * 
 * Prodcuts --> [int PRIMARY id, string name, int stock, decimal price]
 * Admins --> [int PRIMARY id, string username, TIMESPAMTZ lastsignin, VARCHAR Key] 
 * 
 * 
 * 
 */

//Username: Dan
//Password: AQ4736


namespace VendingMachine
{
    internal class Program
    {
        //-----------------------
        //Basket

        public class Basket
        {
            public static decimal total;
            public static List<int> items = new List<int>();

            public static void Show()
            {
                for(int i = 0; i < items.Count();i++)
                {
                    Console.WriteLine($"ID: {items[i]}, Product: {ReadName(connString, items[i])}");
                }
            }
            public static void Add(string cs, int id)
            {
                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand($"SELECT price FROM Products WHERE id = {id}", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnValue = reader.GetDecimal(0);
                            total += columnValue;
                        }
                    }

                    conn.Close();
                }

                items.Add(id);
            }
            public static void Remove(string cs, int id)
            {
                using (var conn = new NpgsqlConnection(cs))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand($"SELECT price FROM Products WHERE id = {id}", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnValue = reader.GetDecimal(0);
                            total -= columnValue;
                        }
                    }

                    conn.Close();
                }

                items.Remove(id);
            }
            public static void Clear()
            {
                total = 0;
                items.Clear();
            }
        }
    
        //-----------------------
        //GENERAL METHODS

        public static void Menu()
        {
            Console.Write($"Options:     ");BasketContents(connString);
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT * FROM Products", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string itemName = reader.GetString(1);
                        int stock = reader.GetInt32(2);
                        Console.WriteLine($"\nID: {id}, Item: {itemName}, Stock: {stock}");
                    }
                }
            }
            TakeOrder(connString);
        }

        public static void Exit()
        {
            Console.Clear();
            Basket.Clear();
            Menu();
        }

        public static void TakeOrder(string cs)
        {
            Console.Write("Order ID: ");
            List<int> items = GetItemsID(connString,"id");
            int order = IntegerValidation();
            while (items.Contains(order)==false&&GetStock(connString,order)==0)
            {
                if (order == 4736)
                {
                    AdminValidation();
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: invalid ID");
                Console.ForegroundColor = ConsoleColor.White;
                order = IntegerValidation();
            }
            Basket.Add(connString, order);
            Console.Clear();
            Console.WriteLine("Options:");
            Console.WriteLine("1 - Menu");
            Console.WriteLine("2 - Basket");
            Console.WriteLine("3 - Exit");
            Console.Write("--> ");
            int Choice = IntegerValidation();
            switch (Choice)
            {
                case 1:
                    Menu();
                    break;
                case 2:
                    BasketView();
                    break;
                case 3:
                    Exit();
                    break;
            }
            Menu();
        }

        public static void BasketView()
        {
            Console.Clear();
            Console.WriteLine($"Basket: £{Basket.total}");
            Basket.Show();
            Console.WriteLine("Options:");
            Console.WriteLine("1 - Menu");
            Console.WriteLine("2 - Checkout");
            Console.WriteLine("3 - Remove Item");
            Console.WriteLine("4 - Exit");
            int Choice = IntegerValidation();
            switch (Choice)
            {
                case 1:
                    Menu();
                    break;
                case 2:
                    Checkout();
                    break;
                case 3:
                    RemoveItem();
                    break;
                default:
                    Exit();
                    break;
            }
            Menu();
        }

        public static void Checkout()
        {
            Console.Clear();
            Console.WriteLine($"Total: £{Basket.total}");
            BasketContents(connString);
            Console.WriteLine("1 - Pay\n2 - Basket\n3 - Exit\n");
            Console.Write("--> "); int choice = IntegerValidation();
            switch (choice)
            {
                case 1:
                    Pay();
                    break;
                case 2:
                    BasketView();
                    break;
                case 3:
                    Exit();
                    break;
                default:
                    Checkout();
                    break;
            }
        }

        public static void Pay()
        {
            Console.WriteLine("Enter Coins: ");
            while (Basket.total > 0)
            {
                try
                {
                    decimal coin = decimal.Parse(Console.ReadLine());
                    Basket.total -= coin;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Rejected");
                        Console.ForegroundColor = ConsoleColor.White;
                }
            }

            for (int i = 0; i < Basket.items.Count(); i++)
            {
                PurchaseProduct(connString, Basket.items[i]);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Returned --> £{Basket.total}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
            Exit();
        }

        public static void RemoveItem()
        {
            Console.WriteLine("Remove item: ");
            List<int> items = GetItemsID(connString, "id");
            int item = IntegerValidation();
            while (items.Contains(item) == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: invalid ID");
                Console.ForegroundColor = ConsoleColor.White;
                item = IntegerValidation();
            }
            Basket.Remove(connString, item);
            BasketView();
        }

        public static void BasketContents(string cs)
        {
            switch (Basket.items.Count)
            {
                case 0:
                    Console.Write("[    ]");
                    break;
                case 1:
                    Console.Write($"[ {Basket.items[0]} ]");
                    break;
                default:
                    Console.Write("[ ");
                    for (int i = 0; i < Basket.items.Count; i++)
                    {
                        Console.Write($"{Basket.items[i]}");

                        if (i < Basket.items.Count - 1)
                        {
                            Console.Write(", ");
                        }
                    }
                    Console.Write("]\n");
                    break;
            }
        }

        public static void PurchaseProduct(string connString, int id)
        {
            int currentStock = GetStock(connString, id);

            if (currentStock > 0)
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand("UPDATE Products SET stock = @value1 WHERE id = @value2", conn))
                    {

                        cmd.Parameters.AddWithValue("value1", currentStock - 1);
                        cmd.Parameters.AddWithValue("value2", id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {

            }
        }


        public static List<int> GetItemsID(string cs,string column)
        {
            List<int> items = new List<int>();
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT * FROM Products", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnValue = reader.GetInt32(0);
                            items.Add(columnValue);
                        }
                    }
                }

            }
            return items;
        }

        public static int IntegerValidation()
        {
            bool flag = false;
            int id = 0;
            do
            {
                try
                {
                    id = int.Parse(Console.ReadLine());
                    flag = true;
                }
                catch
                {
                    flag = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Must be of data type Integer!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } while (flag == false);

            return id;
        }//Infinetly loops until Integer input

        public static decimal DecimalValidation()
        {
            bool flag = false;
            decimal id = 0;
            do
            {
                try
                {
                    id = decimal.Parse(Console.ReadLine());
                    flag = true;
                }
                catch
                {
                    flag = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Must be of data type Decimal!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } while (flag == false);

            return id;
        }//Infinetly loops until Decimal input

        public static string ReadName(string cs,int id)
        {
            var value = "";
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT * FROM Products WHERE id = {id}", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Access and process the data from the query result
                            var columnValue = reader["name"];
                            // Perform operations with the data
                            value = columnValue.ToString();
                        }
                    }
                }
                conn.Close();
            }
            return value;
        }//Given the id and cs it will find the name of the product with the matching id in the table Prodcuts

        public static void ListAllItems(string cs)
        {
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT * FROM Products", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnValue = reader["name"];
                            Console.WriteLine(columnValue.ToString());
                        }
                    }
                }

            }
        }//List all product name

        public static int RowCount(string cs)
        {
            int rows = 0;
            using(var conn = new NpgsqlConnection(cs))
            {
                using(var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Products",conn))
                {
                    rows = (int)cmd.ExecuteScalar();
                }
            }
            return rows;
        }

        public static int GetStock(string cs, int id)
        {
            int value = 0;
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT * FROM Products WHERE id = {id}", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Access and process the data from the query result
                            int columnValue = reader.GetInt16(2);
                            // Perform operations with the data
                            value = columnValue;
                        }
                    }
                }
                conn.Close();
            }
            return value;
        }


        //-----------------------
        //ADMIN METHODS

        public static void InsertProduct(string cs)
        {
            using (var conn = new NpgsqlConnection(cs))
            {


                using (var cmd = new NpgsqlCommand("INSERT INTO Products(id,name,stock,price) VALUES(@value1,@value2,@value3,@value4)", conn))
                {
                    Console.Write("ID: "); cmd.Parameters.AddWithValue("value1", int.Parse(Console.ReadLine()));
                    Console.Write("Name: "); cmd.Parameters.AddWithValue("value2", Console.ReadLine());
                    Console.Write("Stock: "); cmd.Parameters.AddWithValue("value3", int.Parse(Console.ReadLine()));
                    Console.Write("Price: "); cmd.Parameters.AddWithValue("value4", decimal.Parse(Console.ReadLine()));

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

        }

        public static void AdminMenu()
        {
            Console.Clear();
            Console.Write("1 - Insert\n2 - Update\n3 - Delete\n4 - Exit\n");
            Console.Write("--> "); int choice = IntegerValidation();
            switch (choice)
            {
                case 1:
                    InsertProduct(connString);
                    break;
                case 2:
                    UpdateValues(connString);
                    break;
                case 3:
                    DeleteProduct(connString);
                    break;
                case 4:
                    Exit();
                    break;
            }
        }

        public static void UpdateValues(string cs)
        {
            string i = "y";
            while (i.ToLower() == "y")
            {

                Console.Write("ID: "); int id = IntegerValidation();
                string o = "y";
                while(o.ToLower() == "y")
                { 
                    using (var conn = new NpgsqlConnection(cs))
                    {
                        Console.Write("Column (1-4): ");

                        int column = IntegerValidation();

                        switch (column)
                        {
                            case 1:
                                using (var cmd = new NpgsqlCommand("UPDATE Products SET id = @new_value1 WHERE id = @condition_value"))
                                {
                                    Console.Write("New ID: "); cmd.Parameters.AddWithValue("new_value1", IntegerValidation());
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                }
                                break;
                            case 2:
                                using (var cmd = new NpgsqlCommand("UPDATE Prodcuts SET name = @newvalue1 WHERE id = @condition_value"))
                                {
                                    Console.Write("New Name: "); cmd.Parameters.AddWithValue("new_value1", Console.ReadLine().Trim());
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                }
                                break;
                            case 3:
                                using (var cmd = new NpgsqlCommand("UPDATE Prodcuts SET stock = @newvalue1 WHERE id = @condition_value"))
                                {
                                    Console.Write("New Stock: "); cmd.Parameters.AddWithValue("new_value1", IntegerValidation());
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                }
                                break;
                            case 4:
                                using (var cmd = new NpgsqlCommand("UPDATE Prodcuts SET price = @newvalue1 WHERE id = @condition_value"))
                                {
                                    Console.Write("New Price: "); cmd.Parameters.AddWithValue("new_value1", DecimalValidation());
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                }
                                break;
                        }
                    }
                    Console.Write($"Would you to to continue editing Product_ID: {id} (Y/N)");o=Console.ReadLine();
                }
                Console.WriteLine("Would you like to exit editor mode (Y/N)");i=Console.ReadLine();
            }
        }//used for restocks

        public static void DeleteProduct(string cs)
        {
            Console.Write("ID: "); int id = IntegerValidation();

            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("DELETE FROM Products WHERE id = @condition_value", conn))
                {
                    cmd.Parameters.AddWithValue("condition_value", id);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public static bool AdminUsernameFetch(string cs, string username)
        {
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Admins WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    // Use ExecuteScalar to get the count of records.
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    // Check if the count is greater than 0 to determine if the username exists.
                    return count > 0;
                }
            }
        }


        public static bool AdminPasswordFetch(string cs, int password)
        {
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT * FROM Admins WHERE key = @password", conn))
                {
                    cmd.Parameters.AddWithValue("@password", password);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        public static void AdminValidation()
        {
            (bool firstValue, bool secondValue) pair = (false, false);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------");
            Console.WriteLine("ADMIN LOGIN");


            //-----------------------
            Console.Write("Username: ");
            string username = Console.ReadLine();
            if (AdminUsernameFetch(csAdmin,username))
            {
                pair.firstValue = true;
            }
            else
            {
                Menu();
            }
            //-----------------------
            Console.Write("Password: ");

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.White;


            int password = Console.ReadLine().GetHashCode();

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;

            if (AdminPasswordFetch(csAdmin, password))
            {
                pair.secondValue = true;
            }
            else
            {
                Menu();
            }
            //-----------------------
            Console.WriteLine("\n--------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            if (pair.firstValue == true&&pair.secondValue == true)
            {
                AdminMenu();
            }
            else
            {
                //Unreachable
            }
        }//Determines if they have the correct credentials#

        //Add menu for Admin


        //-----------------------

        public static string connString = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Items;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";
        public static string csAdmin = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Items;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";

        //-----------------------

        public static void Main(string[] args)
        {
            var connString = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Items;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";
            var csAdmin = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Admins;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";

            Menu();



            Console.ReadKey();

        }
    }
}
