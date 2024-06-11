using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using S_Potrykus_Zad_Rek_1.Modules;
using System.Data;
using System.Data.SqlClient;


namespace S_Potrykus_Zad_Rek_1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : Controller
    {
        // Passing the configuration
        public readonly IConfiguration _configuration;
        public readonly LoginController _loginController;

        public ContactsController(IConfiguration configuration, LoginController loginController)
        {
            _configuration = configuration;
            _loginController = loginController;
        }

        [HttpGet]
        [Route("GetAllContacts")]
        // Getting all the contacts from database
        public string GetContacts() 
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("S_Potrykus_Zad_Rek_1Connection".ToString())); // Setting up the conncection to database
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Contacts", con); // Creating a SQL query
            DataTable dataTable = new DataTable(); // Creating a data table
            adapter.Fill(dataTable); // Filling the table
            List<Contact> contacts = new(); // Creating a list of contacts
            Response response = new(); // Creating a new response
            if(dataTable.Rows.Count > 0)
            {
                for(int i = 0; i < dataTable.Rows.Count; i++)
                {
                    // Filling in the contact objects with data from db
                    Contact contact = new Contact();
                    contact.Name = Convert.ToString(dataTable.Rows[i]["FirstName"]);
                    contact.LastName = Convert.ToString(dataTable.Rows[i]["LastName"]);
                    contact.Email = Convert.ToString(dataTable.Rows[i]["email"]);
                    contact.Password = Convert.ToString(dataTable.Rows[i]["password"]);
                    contact.Category = Convert.ToString(dataTable.Rows[i]["category"]);
                    contact.CategorySecondary = Convert.ToString(dataTable.Rows[i]["categorySecondary"]);
                    contact.DateOfBirth = Convert.ToString(dataTable.Rows[i]["dateofbirth"]);
                    contact.Phone = Convert.ToInt32(dataTable.Rows[i]["phonenumber"]);
                    contacts.Add(contact);
                }
            }
            if(contacts.Count > 0)
            {
                return JsonConvert.SerializeObject(contacts); // Changing the object to string
            }
            else // Giving out an error in case of epmty database or failed connection
            { 
                response.StatusCode = 100;
                response.ErrorMessage = "No contacts found";
                return JsonConvert.SerializeObject(response);
            }
        }

        [Authorize] // Require authorization (being logged in) to execute
        [HttpPost]
        [Route("EditContact")]
        // Editing a contact

        public string EditContact(Contact contact)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("S_Potrykus_Zad_Rek_1Connection".ToString()));
            SqlCommand cmd = new SqlCommand($"UPDATE Contacts SET" +
                $" FirstName = '{contact.Name}', LastName = '{contact.LastName}', email = '{contact.Email}', password = '{_loginController.EncodePassword(contact.Password)}'," +
                $" category = '{contact.Category}', categorySecondary = '{contact.CategorySecondary}', phonenumber = '{contact.Phone}', dateofbirth = '{contact.DateOfBirth}' WHERE email = '{contact.oldEmail}'");
            cmd.Connection = con;
            con.Open();
            int i = cmd.ExecuteNonQuery(); // Excecute querry
            con.Close();
            if(i > 0) // Check if update was successful
            {
                return "Contact updated";
            }
            else
            {
                return "Unexpected error occured";
            }
            
        }
        [Authorize] // Require authorization (being logged in) to execute
        [HttpPost]
        [Route("AddContact")]
        // Adding a contact

        public string AddContact(Contact contact)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("S_Potrykus_Zad_Rek_1Connection".ToString()));
            SqlCommand cmd = new SqlCommand($"INSERT INTO Contacts " +
                $"(FirstName, LastName, email, password, category, categorySecondary, phonenumber, dateofbirth) VALUES " +
                $"('{contact.Name}','{contact.LastName}','{contact.Email}','{_loginController.EncodePassword(contact.Password)}','{contact.Category}', '{contact.CategorySecondary}', {contact.Phone}, '{contact.DateOfBirth}');");
            cmd.Connection = con;
            con.Open();
            try
            {
               cmd.ExecuteNonQuery(); // Excecute querry
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                con.Close();
            }
            return "Contact added";
        }
    }
}
