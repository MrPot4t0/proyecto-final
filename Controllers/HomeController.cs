using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Todo.Models;
using Todo.Models.ViewModels;

namespace Todo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var todoListViewModel = GetAllTodos();
            return View(todoListViewModel);
        }

        [HttpGet]
        public JsonResult PopulateForm(int id)
        {
            var todo = GetById(id);
            return Json(todo);
        }


        public TodoViewModel GetAllTodos()
        {
            List<TodoItem> todoList = new();

            using (SqliteConnection con =
                   new SqliteConnection("Data Source=db.sqlite"))
            {
                using (var tableCmd = con.CreateCommand())
                {
                    con.Open();
                    tableCmd.CommandText = "select rootpage, name from sqlite_schema where type ='table' and name not like 'sqlite_%'";

                    using (var reader = tableCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                todoList.Add(
                                    new TodoItem
                                    {
                                        Id = reader.GetInt32(0),
                                        Name = reader.GetString(1)
                                    });
                            }
                        }
                        else
                        {
                            return new TodoViewModel
                            {
                                TodoList = todoList
                            };
                        }
                    };
                }
            }

            return new TodoViewModel
            {
                TodoList = todoList
            };
        }

        internal TodoItem GetById(int id)
        {
            TodoItem todo = new();

            using (var connection =
                   new SqliteConnection("Data Source=db.sqlite"))
            {
                using (var tableCmd = connection.CreateCommand())
                {
                    connection.Open();
                    tableCmd.CommandText = $"SELECT * FROM sqlite_schema Where rootpage = '{id}'";

                    using (var reader = tableCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            todo.Id = reader.GetInt32(0);
                            todo.Name = reader.GetString(1);
                        }
                        else
                        {
                            return todo;
                        }
                    };
                }
            }

            return todo;
        }

        public RedirectResult Insert(TodoItem todo)
        {
            using (SqliteConnection con =
                   new SqliteConnection("Data Source=db.sqlite"))
            {
                using (var tableCmd = con.CreateCommand())
                {
                    con.Open();
                    tableCmd.CommandText = $"create table '{todo.Name}' ( Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT)";
                    try
                    {
                        tableCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return Redirect("https://localhost:7240");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            using (SqliteConnection con =
                   new SqliteConnection("Data Source=db.sqlite"))
            {
                using (var tableCmd = con.CreateCommand())
                {
                    con.Open();
                    tableCmd.CommandText = $"select name from sqlite_schema where rootpage = {id}";
                    SqliteDataReader reader = tableCmd.ExecuteReader();
                    string value = "";
                    while (reader.Read()) 
                    {
                    value = Convert.ToString(reader["name"]);
                    Console.WriteLine(value);
                    reader.Close();
                    break;
                    } 
                    con.Open();
                    tableCmd.CommandText = $"drop table '{value}'";
                    tableCmd.ExecuteNonQuery();
                }
            }

            return Json(new {});
        }
    
        public RedirectResult GetItems(TodoItem todo, RedirectResult items)
        {
            List<TodoItem> objeto = new List<TodoItem>();
            using (SqliteConnection con =
                   new SqliteConnection("Data Source=db.sqlite"))
            {
                using (var tableCmd = con.CreateCommand())
                {
                    con.Open();
                    tableCmd.CommandText = $"select * from '{todo.Name}'";
                    try
                    {
                        SqliteDataReader reader = tableCmd.ExecuteReader();
                        while (reader.Read()) {
                            int id = Convert.ToInt32(reader["Id"]);
                            string name = Convert.ToString(reader["Name"]);
                            TodoItem item = new TodoItem();
                            item.Id = id;
                            item.Name = name;
                            objeto.Add(item);
                        }
                      reader.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }   
            }return Redirect("Https://localhost:7240/GetItems");
        }
    }
}
