﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;


//-----------
using Npgsql;//"Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Items;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true"
//-----------

//Username: Dan
//Password: AQ4736


namespace VendingMachine
{
    internal class Program
    {
        //-----------------------
        //GENERAL METHODS

        public static void Menu()
        {

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
                            conn.Close();
                            // Perform operations with the data
                            value = columnValue.ToString();
                        }
                    }
                }
                
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

                using (var cmd = new NpgsqlCommand("SELECT * FROM Admins WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {

                        return true;
                    }
                    else
                    {

                        return false;
                    }
                }
            }
        }

        public static bool AdminPasswordFetch(string cs, int password)
        {
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT * FROM Admins WHERE password = @password", conn))
                {
                    cmd.Parameters.AddWithValue("@password", password);

                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {

                        return true;
                    }
                    else
                    {

                        return false;
                    }
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
                //CALL ON MAIN MENUE to exit Admin
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
                //CALL ON MAIN MENUE to exit Admin
            }
            //-----------------------
            Console.WriteLine("\n--------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            if (pair.firstValue == true&&pair.secondValue == true)
            {
                //Call on Admin Menu
            }
            else
            {
                //Unreachable
            }
        }//Determines if they have the correct credentials


        //-----------------------

        public static string connString = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Items;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";
        public static string csAdmin = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Admins;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";

        //-----------------------

        static void Main(string[] args)
        {
            var connString = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Items;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";
            var csAdmin = "Host=ragged-mummy-11407.8nj.cockroachlabs.cloud;Port=26257;Database=Admins;Username=aq232596_aquinas_ac_;Password=72eg0Wd7zpeV1TLCwAqr2A;SSL Mode=Prefer;Trust Server Certificate=true";

            ListAllItems(connString);


            Console.ReadKey();

        }
    }
}
