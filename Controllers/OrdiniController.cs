using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using Pizzeria.Models;

namespace Pizzeria.Controllers
{
    public class OrdiniController : Controller
    {
        private DBContext db = new DBContext();

        // GET: Ordini
        [Authorize(Roles = "Amministratore")]
        public ActionResult Index()
        {
            var ordini = db.Ordini.Include(o => o.Users);
            return View(ordini.ToList());
        }

        // GET: Ordini/Details/5
        [Authorize(Roles = "Cliente, Amministratore")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var idInt = Convert.ToInt32(id);
            var order = db.Ordini
                        .Where(o => o.User_ID == idInt).ToList();
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Ordini/Create
        [Authorize(Roles = "Cliente, Amministratore")]
        public ActionResult Create()
        {
            if (User.IsInRole("Cliente"))
            {
                ViewBag.User_ID = User.Identity.Name; ;
            }
            else if (User.IsInRole("Amministratore"))
            {
                ViewBag.UserID = new SelectList(db.Users, "User_ID", "Nome");
            }
            return View();
        }

        // POST: Ordini/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OrdArt ordArt)
        {
            if (ModelState.IsValid)
            {
                // Salva l'ordine nella tabella Ordini
                db.Ordini.Add(ordArt.Ordini);
                db.SaveChanges();

                // Ottieni l'Ordine_ID appena creato
                int newOrdineID = ordArt.Ordini.Ordine_ID;

                // Assegna l'Ordine_ID appena creato al record della tabella OrdArt
                ordArt.Ordine_ID = newOrdineID;

                // Aggiungi il record della tabella OrdArt
                db.OrdArt.Add(ordArt);
                db.SaveChanges();

                return RedirectToAction("Details", "OrdArts", new { id = newOrdineID });
            }

            return View(ordArt);
        }


        // GET: Ordini/Edit/5
        [Authorize(Roles = "Amministratore")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordini ordini = db.Ordini.Find(id);
            if (ordini == null)
            {
                return HttpNotFound();
            }
            ViewBag.User_ID = new SelectList(db.Users, "User_ID", "Nome", ordini.User_ID);
            return View(ordini);
        }

        // POST: Ordini/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Amministratore")]
        public ActionResult Edit([Bind(Include = "Ordine_ID,Indirizzo,Note,CostoCons,User_ID")] Ordini ordini)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ordini).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.User_ID = new SelectList(db.Users, "User_ID", "Nome", ordini.User_ID);
            return View(ordini);
        }

        // GET: Ordini/Delete/5
        [Authorize(Roles = "Amministratore")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordini ordini = db.Ordini.Find(id);
            if (ordini == null)
            {
                return HttpNotFound();
            }
            return View(ordini);
        }

        // POST: Ordini/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Amministratore")]
        public ActionResult DeleteConfirmed(int id)
        {
            Ordini ordini = db.Ordini.Find(id);
            db.Ordini.Remove(ordini);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
