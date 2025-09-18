using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using BlazorShared.Models;

namespace BlazorAdmin.Interfaces;

public interface ICatalogTypeService
{
    Task<List<CatalogType>> List();
}
