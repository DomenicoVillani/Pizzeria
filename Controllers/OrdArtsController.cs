using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pizzeria.Models;

namespace Pizzeria.Controllers
{
    public class OrdArtsController : Controller
    {
        private DBContext db = new DBContext();

        // GET: OrdArts
        [Authorize(Roles = "Amministratore")]
        public ActionResult Index()
        {
            var ordArt = db.OrdArt.Include(o => o.Articoli).Include(o => o.Ordini);
            return View(ordArt.ToList());
        }

        // GET: OrdArts/Details/5
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ordineWithArticoli = db.OrdArt
                .Include(o => o.Ordini)
                .Include(o => o.Ordini.Users)
                .Include(o => o.Articoli)
                .Where(o => o.Ordine_ID == id).ToList();

            if (ordineWithArticoli == null)
            {
                return HttpNotFound();
            }

            return View(ordineWithArticoli);

            //var ArtOrderId = db.OrdArt.Where(u => u.Ordine_ID == orderId).ToList();
            //Ordini ordini = db.Ordini.Find(orderId);
            //if (ArtOrderId == null || ordini == null)
            //{
            //    return HttpNotFound();
            //}
            //TempData["ordineDetails"] = ordini;
            //return View(ArtOrderId);
        }

        // GET: OrdArts/Create
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Create()
        {
            ViewBag.Articolo_ID = new SelectList(db.Articoli, "Articolo_ID", "Nome");
            ViewBag.Ordine_ID = new SelectList(db.Ordini, "Ordine_ID", "Indirizzo");
            return View();
        }

        // POST: OrdArts/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente,Amministratore")]
        public ActionResult Create([Bind(Include = "Articolo_ID,Ordine_ID,Quantita")] OrdArt ordArt)
        {
            var ControlloOrdine = db.OrdArt.Where(o => o.Articolo_ID == ordArt.Articolo_ID).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (ControlloOrdine == null)
                {
                    db.OrdArt.Add(ordArt);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ControlloOrdine.Quantita += ordArt.Quantita;
                    db.Entry(ControlloOrdine).State = EntityState.Modified;
                }
            }
            ViewBag.Articolo_ID = new SelectList(db.Articoli, "Articolo_ID", "Nome", ordArt.Articolo_ID);
            ViewBag.Ordine_ID = new SelectList(db.Ordini, "Ordine_ID", "Indirizzo", ordArt.Ordine_ID);
            return View(ordArt);
        }

        // GET: OrdArts/Edit/5
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Edit(int? articoloId, int? ordineId)
        {
            // Verifica se gli ID sono nulli
            if (articoloId == null || ordineId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Eseguo una conversione dei valori nullable in tipi int
            int articoloIdValue = articoloId.Value;
            int ordineIdValue = ordineId.Value;

            // Query per trovare l'ordArt corrispondente
            OrdArt ordArt = db.OrdArt
                .Include(o => o.Articoli)
                .Where(o => o.Ordine_ID == ordineIdValue && o.Articolo_ID == articoloIdValue)
                .FirstOrDefault();

            // Se l'ordArt non è trovato, restituisci un errore 404
            if (ordArt == null)
            {
                return HttpNotFound();
            }

            // Popola i ViewBag per il dropdownlist
            ViewBag.Articolo_ID = new SelectList(db.Articoli, "Articolo_ID", "Nome", ordArt.Articolo_ID);
            ViewBag.Ordine_ID = new SelectList(db.Ordini, "Ordine_ID", "Indirizzo", ordArt.Ordine_ID);

            // Ritorna la vista con l'ordArt trovato
            return View(ordArt);
        }

        // POST: OrdArts/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Edit([Bind(Include = "Articolo_ID,Ordine_ID,Quantita")] OrdArt ordArt)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ordArt).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Articolo_ID = new SelectList(db.Articoli, "Articolo_ID", "Nome", ordArt.Articolo_ID);
            ViewBag.Ordine_ID = new SelectList(db.Ordini, "Ordine_ID", "Indirizzo", ordArt.Ordine_ID);
            return View(ordArt);
        }

        // GET: OrdArts/Delete/5
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrdArt ordArt = db.OrdArt.Find(id);
            if (ordArt == null)
            {
                return HttpNotFound();
            }
            return View(ordArt);
        }

        // POST: OrdArts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult DeleteConfirmed(int id)
        {
            OrdArt ordArt = db.OrdArt.Find(id);
            db.OrdArt.Remove(ordArt);
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
