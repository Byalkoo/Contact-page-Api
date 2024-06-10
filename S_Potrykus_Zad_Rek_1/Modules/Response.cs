namespace S_Potrykus_Zad_Rek_1.Modules
{
    //Creating a response module, defining its variables, used for catching error when getting data from db
    public class Response
    {
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
