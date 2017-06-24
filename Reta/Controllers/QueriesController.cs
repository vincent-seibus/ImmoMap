using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Reta.Models;
using log4net;

namespace Reta.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class QueriesController : Controller
    {
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();
        private static readonly ILog logger = LogManager.GetLogger(typeof(QueriesController));

        // GET: Queries/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Queries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QueryString")] Query query)
        {
            query.QueryID = db.Database.ExecuteSqlCommand(query.QueryString).ToString();

            return View(query);
        }

      
       
    }
}
