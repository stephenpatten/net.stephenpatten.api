You *MUST* register ServiceStacks '/api' path by adding the lines below to MvcApplication.RegisterRoutes(RouteCollection) in the Global.asax:

	routes.IgnoreRoute("api/{*pathInfo}"); 
	routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" }); //Prevent exceptions for favicon

Place them before the current entries the method.

For MVC4 applications you also need to unregister WebApi, by commenting out this line:

        //WebApiConfig.Register(GlobalConfiguration.Configuration);




For more info on the MiniProfiler see v3.09 of the https://github.com/ServiceStack/ServiceStack/wiki/Release-Notes


The Urls for metadata page and included Services:

  * /api/metadata - Auto generated metadata pages
  * /api/hello - Simple Hello World Service see: http://www.servicestack.net/ServiceStack.Hello/
  * /api/todos - Simple REST Service see: http://www.servicestack.net/Backbone.Todos/

  * /default.htm - Backbone.js TODO application talking to the TODO REST service at /api/todos

