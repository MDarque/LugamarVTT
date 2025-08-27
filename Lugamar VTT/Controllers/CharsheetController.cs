using LugamarVTT.Services;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Linq;

namespace LugamarVTT.Controllers
{
    /// <summary>
    /// Controller responsible for presenting a list of player characters to the
    /// user.  The characters are loaded from an XML database via
    /// <see cref="XmlDataService"/>.  The default action renders a table view
    /// showing basic Pathfinder 1e sheet information.
    /// </summary>
    public class CharsheetController : Controller
    {
        private readonly XmlDataService _service;

        public CharsheetController(XmlDataService service)
        {
            _service = service;
        }

        /// <summary>
        /// List all player characters.  This action calls
        /// <see cref="XmlDataService.GetCharacters"/> and passes the result to
        /// the view for rendering.  If the XML file cannot be found,
        /// an error view is returned instead.
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                var characters = _service.GetCharacters().ToList();
                return View(characters);
            }
            catch (Exception ex)
            {
                // Log the error and display an informative message
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}