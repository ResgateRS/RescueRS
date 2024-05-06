
using System.Text.Json.Serialization;
using ResgateRS.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ResgateRS.Presenter.Controllers;

public class BaseController<Service, IServiceProvider> : ControllerBase
{
    protected readonly Service mainService;
    protected readonly IServiceProvider serviceProvider;


    public BaseController(Service service, IServiceProvider serviceProvider)
    {
        this.mainService = service;
        this.serviceProvider = serviceProvider;
    }

}