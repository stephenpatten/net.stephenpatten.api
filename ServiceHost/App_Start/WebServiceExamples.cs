using System.Collections.Generic;
using System.Linq;
using Raven.Client.Document;
using ServiceStack.Common;
using ServiceStack.Northwind.Tests.Entities;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;

namespace ServiceHost
{
	//Request DTO
	public class Northwind
	{
		public string CustomerName { get; set; }
	}

	//Response DTO
	public class NorthwindResponse
	{
        public Order[] Result { get; set; }
		public ResponseStatus ResponseStatus { get; set; } //Where Exceptions get auto-serialized
	}

	public class NorthwindService : Service
	{
		public object Any(Northwind request)
		{
		    Order[] orders;
		    using (var store = new DocumentStore { ConnectionStringName = "RavenDB.Northwind" })
		    {
                store.Initialize();

		        using (var session = store.OpenSession())
		        {
                    var results = from order in session.Query<Order>()
                                  where order.Customer.Id == request.CustomerName
                                  select order;

		            orders = results.ToArray();
		        }
		    }
            return new NorthwindResponse { Result = orders };
		    
		}
	}

    //REST Resource DTO
    [Route("/todos")]
    [Route("/todos/{Ids}")]
    public class Todos : IReturn<List<Todo>>
    {
        public long[] Ids { get; set; }
        public Todos(params long[] ids)
        {
            this.Ids = ids;
        }
    }

    [Route("/todos", "POST")]
    [Route("/todos/{Id}", "PUT")]
    public class Todo : IReturn<Todo>
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public bool Done { get; set; }
    }

    public class TodosService : Service
    {
        public TodoRepository Repository { get; set; }  //Injected by IOC

        public object Get(Todos request)
        {
            return request.Ids.IsEmpty()
                ? Repository.GetAll()
                : Repository.GetByIds(request.Ids);
        }

        public object Post(Todo todo)
        {
            return Repository.Store(todo);
        }

        public object Put(Todo todo)
        {
            return Repository.Store(todo);
        }

        public void Delete(Todos request)
        {
            Repository.DeleteByIds(request.Ids);
        }
    }
    
    public class TodoRepository
    {
        List<Todo> todos = new List<Todo>();
        
        public List<Todo> GetByIds(long[] ids)
        {
            return todos.Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<Todo> GetAll()
        {
            return todos;
        }

        public Todo Store(Todo todo)
        {
            var existing = todos.FirstOrDefault(x => x.Id == todo.Id);
            if (existing == null)
            {
                var newId = todos.Count > 0 ? todos.Max(x => x.Id) + 1 : 1;
                todo.Id = newId;
            }
            todos.Add(todo);
            return todo;
        }

        public void DeleteByIds(params long[] ids)
        {
            todos.RemoveAll(x => ids.Contains(x.Id));
        }
    }


/*  Example calling above Service with ServiceStack's C# clients:

	var client = new JsonServiceClient(BaseUri);
	List<Todo> all = client.Get(new Todos());           // Count = 0

	var todo = client.Post(
	    new Todo { Content = "New TODO", Order = 1 });      // todo.Id = 1
	all = client.Get(new Todos());                      // Count = 1

	todo.Content = "Updated TODO";
	todo = client.Put(todo);                            // todo.Content = Updated TODO

	client.Delete(new Todos(todo.Id));
	all = client.Get(new Todos());                      // Count = 0

*/

}

//http://www.headspring.com/alonso/demystifying-ravendb-queries-and-dynamic-indexes/