using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace NezarkaBookstore
{
    //
    // Model
    //

    class ModelStore
    {
        private List<Book> books = new List<Book>();
        private List<Customer> customers = new List<Customer>();

        public IList<Book> GetBooks()
        {
            return books;
        }
        public IList<Customer> GetCustomers()
        {
            return customers;
        }
        public Book GetBook(int id)
        {
            return books.Find(b => b.Id == id);
        }

        public Customer GetCustomer(int id)
        {
            return customers.Find(c => c.Id == id);
        }

        public static ModelStore LoadFrom(TextReader reader, ModelStore store)
        {
           

            try
            {
                if (reader.ReadLine() != "DATA-BEGIN")
                {
                    return null;
                }
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        return null;
                    }
                    else if (line == "DATA-END")
                    {
                        break;
                    }

                    string[] tokens = line.Split(';');
                    switch (tokens[0])
                    {
                        case "BOOK":
                            store.books.Add(new Book
                            {
                                Id = int.Parse(tokens[1]),
                                Title = tokens[2],
                                Author = tokens[3],
                                Price = decimal.Parse(tokens[4])
                            });
                            break;
                        case "CUSTOMER":
                            store.customers.Add(new Customer
                            {
                                Id = int.Parse(tokens[1]),
                                FirstName = tokens[2],
                                LastName = tokens[3]
                            });
                            break;
                        case "CART-ITEM":
                            var customer = store.GetCustomer(int.Parse(tokens[1]));
                            if (customer == null)
                            {
                                return null;
                            }
                            var BookID = int.Parse(tokens[2]);
                            var CounT = int.Parse(tokens[3]);
                            customer.ShoppingCart.Items.Add(new ShoppingCartItem
                            {
                                BookId = BookID,
                                Count = CounT
                            });
                            break;
                        default:
                            return null;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is IndexOutOfRangeException)
                {
                    return null;
                }
                throw;
            }

            return store;
        }
    }

    class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
    }

    class Customer
    {
        private ShoppingCart shoppingCart;

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ShoppingCart ShoppingCart
        {
            get
            {
                if (shoppingCart == null)
                {
                    shoppingCart = new ShoppingCart();
                }
                return shoppingCart;
            }
            set
            {
                shoppingCart = value;
            }
        }
    }

    class ShoppingCartItem
    {
        public int BookId { get; set; }
        public int Count { get; set; }
    }

    class ShoppingCart
    {
        public int CustomerId { get; set; }
        public List<ShoppingCartItem> Items = new List<ShoppingCartItem>();
    }
    //
    //View
    //

     class View
    {
        public void Head()
        {
            Console.WriteLine("<!DOCTYPE html>");
            Console.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
            Console.WriteLine("<head>");
            Console.WriteLine("\t<meta charset=\"utf-8\" />");
            Console.WriteLine("\t<title>Nezarka.net: Online Shopping for Books</title>");
            Console.WriteLine("</head>");
            Console.WriteLine("<body>");
        }
        public void Foot()
        {
            Console.WriteLine("</body>");
            Console.WriteLine("</html>");
        }
        public void End()
        {
            Console.WriteLine("====");
        }
        public void Header(string FirstName, int NumberOfItems)
        {
            Console.WriteLine("\t<h1><pre>  v,<br />Nezarka.NET: Online Shopping for Books</pre></h1>");
            Console.WriteLine("\t{0}, here is your menu:", FirstName);
            Console.WriteLine("\t<table>");
            Console.WriteLine("\t\t<tr>");
            Console.WriteLine("\t\t\t<td><a href=\"/Books\">Books</a></td>");
            Console.WriteLine("\t\t\t<td><a href=\"/ShoppingCart\">Cart ({0})</a></td>", NumberOfItems);
            Console.WriteLine("\t\t</tr>");
            Console.WriteLine("\t</table>");
        }
        public void Invalid()
        {
            Head();
            Console.WriteLine("<p>Invalid request.</p>");
            Foot();
            End();
        }
        public void Style()
        {
            Console.WriteLine("\t<style type=\"text/css\">");
            Console.WriteLine("\t\ttable, th, td {");
            Console.WriteLine("\t\t\tborder: 1px solid black;");
            Console.WriteLine("\t\t\tborder-collapse: collapse;");
            Console.WriteLine("\t\t}");
            Console.WriteLine("\t\ttable {");
            Console.WriteLine("\t\t\tmargin-bottom: 10px;");
            Console.WriteLine("\t\t}");
            Console.WriteLine("\t\tpre {");
            Console.WriteLine("\t\t\tline-height: 70%;");
            Console.WriteLine("\t\t}");
            Console.WriteLine("\t</style>");
        }
        public void AllBooks(ModelStore model)
        {
            int i = 0;
            Console.WriteLine("\tOur books for you:");
            Console.WriteLine("\t<table>");
            if (model.GetBooks().Count > 0) {
            Console.WriteLine("\t\t<tr>");
            foreach (Book book in model.GetBooks())
            {
                    if (i % 3 == 0 && i!=0)
                    {
                        Console.WriteLine("\t\t</tr>");
                        Console.WriteLine("\t\t<tr>");
                    }
            
            
            Console.WriteLine("\t\t\t<td style=\"padding: 10px;\">");
            Console.WriteLine("\t\t\t\t<a href=\"/Books/Detail/{0}\">{1}</a><br />", book.Id, book.Title);
            Console.WriteLine("\t\t\t\tAuthor: {0}<br />", book.Author );
            Console.WriteLine("\t\t\t\tPrice: {0} EUR &lt;<a href=\"/ShoppingCart/Add/{1}\">Buy</a>&gt;", book.Price, book.Id);
            Console.WriteLine("\t\t\t</td>");
                    i++;
            }
                Console.WriteLine("\t\t</tr>");
           }
            Console.WriteLine("\t</table>");
        }
        public void ShopingCart(int CustID,ModelStore model)
        {
            decimal price=0;
            
            Console.WriteLine("\tYour shopping cart:");
            Console.WriteLine("\t<table>");


            Console.WriteLine("\t\t<tr>");
            Console.WriteLine("\t\t\t<th>Title</th>");
            Console.WriteLine("\t\t\t<th>Count</th>");
            Console.WriteLine("\t\t\t<th>Price</th>");
            Console.WriteLine("\t\t\t<th>Actions</th>");
            Console.WriteLine("\t\t</tr>");

            foreach(ShoppingCartItem item in model.GetCustomer(CustID).ShoppingCart.Items) {
                
            
            Console.WriteLine("\t\t<tr>");

            Console.WriteLine("\t\t\t<td><a href=\"/Books/Detail/{0}\">{1}</a></td>", item.BookId, model.GetBook(item.BookId).Title );
            Console.WriteLine("\t\t\t<td>{0}</td>", item.Count);


            if (item.Count > 1)
            { Console.WriteLine("\t\t\t<td>{0} * {1} = {2} EUR</td>", item.Count, model.GetBook(item.BookId).Price, (model.GetBook(item.BookId).Price * item.Count)); }
            else
            { Console.WriteLine("\t\t\t<td>{0} EUR</td>", model.GetBook(item.BookId).Price); }

            Console.WriteLine("\t\t\t<td>&lt;<a href=\"/ShoppingCart/Remove/{0}\">Remove</a>&gt;</td>", item.BookId);
            Console.WriteLine("\t\t</tr>");
                price += (item.Count*model.GetBook(item.BookId).Price);
            }


            Console.WriteLine("\t</table>");
            Console.WriteLine("\tTotal price of all items: {0} EUR", price);


        }
        public void Detail(int bookID, ModelStore model)
        {
            Console.WriteLine("\tBook details:");

            Console.WriteLine("\t<h2>{0}</h2>", model.GetBook(bookID).Title);
            Console.WriteLine("\t<p style=\"margin-left: 20px\">");
            Console.WriteLine("\tAuthor: {0}<br />", model.GetBook(bookID).Author);
            Console.WriteLine("\tPrice: {0} EUR<br />", model.GetBook(bookID).Price);
            Console.WriteLine("\t</p>");

            Console.WriteLine("\t<h3>&lt;<a href=\"/ShoppingCart/Add/{0}\">Buy this book</a>&gt;</h3>", bookID);
        }
        public void Empty()
        {
            Console.WriteLine("\tYour shopping cart is EMPTY.");
        }
        public void Books(int CustID, ModelStore model)
        {
            Head();
            Style();
            Header(model.GetCustomer(CustID).FirstName, model.GetCustomer(CustID).ShoppingCart.Items.Count);
            AllBooks(model);
            Foot();
            End();
        }
        public void BooksDetail(int CustID, int bookID, ModelStore model)
        {
            Head();
            Style();
            Header(model.GetCustomer(CustID).FirstName, model.GetCustomer(CustID).ShoppingCart.Items.Count);
            Detail(bookID, model);
            Foot();
            End();
        }
        public void ShoppingCart(int CustID, ModelStore model)
        {
            Head();
            Style();
            Header(model.GetCustomer(CustID).FirstName, model.GetCustomer(CustID).ShoppingCart.Items.Count);
            ShopingCart(CustID, model);
            Foot();
            End();
        }
        public void ShoppingCartEmpty(int CustID, ModelStore model)
        {
            Head();
            Style();
            Header(model.GetCustomer(CustID).FirstName, model.GetCustomer(CustID).ShoppingCart.Items.Count);
            Empty();
            Foot();
            End();
        }
        

    }
    //
    //Controller
    //
    class Controller
    {
        private string[] prikaz;
        private string[] Url;
        private int CustomerId;
        private ModelStore model;
        private View view;
        public Controller(ModelStore model)
        {
            this.model = model;
            view = new View();
        }
        public bool Split(string line)
        {            
            prikaz = line.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            try {
                if (prikaz[0] == "GET")
                {
                    
                        CustomerId = Int32.Parse(prikaz[1]);
                        string name = model.GetCustomer(CustomerId).FirstName;
                        return true;
                    
                }
                else
                {
                    return false;

                }
            }
            catch
            {
                return false;
            }
        }

        public bool ControlURL()
        { if (prikaz.Length == 3) {
                string[] Url2 = prikaz[2].Split(new Char[]  { '/' });

                if (Url2.Length == 6) { }
                else if (Url2.Length == 4) { }
                    else return false;

                if (Url2[1] != "") return false;
                int a=0;
                foreach(string str in Url2)
                {
                    if (str == "" && a != 1) { return false; }
                        a++;
                }
                
                Url = prikaz[2].Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (Url[0] == "http:" && Url[1] == "www.nezarka.net" && (Url[2] == "Books" || Url[2] == "ShoppingCart")) return true;
            else return false;
            }
            return false;
        }
        public int IdOfBook = -1;
        public void Check()
        {
            try {
            
            if (ControlURL())
            {
                if (Url.Length > 3 && Url[2]=="Books")
                {
                    if(Url[3]== "Detail")
                    {
                        
                        Int32.TryParse(Url[4], out IdOfBook);
                        if(IdOfBook!= -1)
                        {
                            if (model.GetBooks().Contains(model.GetBook(IdOfBook)))
                            {
                                view.BooksDetail(CustomerId, IdOfBook, model);
                            }
                            else
                            {
                                view.Invalid();
                            }
                        }

                    }
                    else
                    {
                        view.Invalid();
                    }

                }
                else if (Url.Length > 3 && Url[2] == "ShoppingCart")
                {
                    if (Url[3] == "Add")
                    {
                        
                        Int32.TryParse(Url[4], out IdOfBook);
                        if (IdOfBook != -1)
                        {
                            if (!model.GetBooks().Contains(model.GetBook(IdOfBook)))
                            {
                                view.Invalid();
                                return;
                            }
                                if (model.GetCustomer(CustomerId).ShoppingCart.Items.Contains(model.GetCustomer(CustomerId).ShoppingCart.Items.Find(id => id.BookId == IdOfBook)))
                            {
                                model.GetCustomer(CustomerId).ShoppingCart.Items.Find(id => id.BookId == IdOfBook).Count++;
                            }
                            else
                            {
                                model.GetCustomer(CustomerId).ShoppingCart.Items.Add(new ShoppingCartItem());
                                model.GetCustomer(CustomerId).ShoppingCart.Items[model.GetCustomer(CustomerId).ShoppingCart.Items.Count - 1].BookId = IdOfBook;
                                model.GetCustomer(CustomerId).ShoppingCart.Items[model.GetCustomer(CustomerId).ShoppingCart.Items.Count - 1].Count = 1;
                            }
                            view.ShoppingCart(CustomerId, model);
                        }

                    }
                    else if(Url[3] == "Remove")
                    {
                        
                        Int32.TryParse(Url[4], out IdOfBook);
                        if (IdOfBook != -1)
                        {
                            if (!model.GetBooks().Contains(model.GetBook(IdOfBook)))
                            {
                                view.Invalid();
                                return;
                            }
                            if (model.GetCustomer(CustomerId).ShoppingCart.Items.Contains(model.GetCustomer(CustomerId).ShoppingCart.Items.Find(id => id.BookId == IdOfBook))) { 
                                if (model.GetCustomer(CustomerId).ShoppingCart.Items.Find(id => id.BookId == IdOfBook).Count >= 2)
                            {
                                model.GetCustomer(CustomerId).ShoppingCart.Items.Find(id => id.BookId == IdOfBook).Count--;
                            }
                            else if(model.GetCustomer(CustomerId).ShoppingCart.Items.Find(id => id.BookId == IdOfBook).Count == 1)
                            {
                                model.GetCustomer(CustomerId).ShoppingCart.Items.Remove(model.GetCustomer(CustomerId).ShoppingCart.Items.Find(id => id.BookId == IdOfBook));
                            }

                                    if (model.GetCustomer(CustomerId).ShoppingCart.Items.Count == 0)
                                    {
                                        view.ShoppingCartEmpty(CustomerId, model);
                                    }
                                    else { view.ShoppingCart(CustomerId, model); }
                                }
                            else { view.Invalid(); }
                        }

                    }
                    else
                    {
                        view.Invalid();
                    }

                }
                else
                {
                    if(Url[2] == "Books")
                    {
                        
                        if (model.GetCustomers().Count == 0)
                        {
                            view.Invalid();
                            return;
                        }
                        view.Books(CustomerId,model);
                    }
                    else if (Url[2] == "ShoppingCart")
                    {

                        if (model.GetCustomers().Count == 0)
                        {
                            view.Invalid();
                            return;
                        }
                        if (model.GetCustomer(CustomerId).ShoppingCart.Items.Count == 0)
                        {
                            view.ShoppingCartEmpty(CustomerId,model);
                        }
                        else { view.ShoppingCart(CustomerId, model); }
                        
                    }
                    else
                    {
                        view.Invalid();
                    }
                }
            }
            else
            {
                view.Invalid();
            }
            }
            catch { view.Invalid(); }
        }
        public void All(string line)
        {
            if (Split(line))
            {
                Check();
            }
            else
            {
                view.Invalid();
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {

            ModelStore model = new ModelStore();
            Controller prikazy = new Controller(model);
           
           if (ModelStore.LoadFrom(Console.In, model)== null)
            {
                Console.WriteLine("Data error.");
                
                return;
            }
            
            while (true)
            {
                string line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }
                prikazy.All(line);
            }
           
        }
    }
}
